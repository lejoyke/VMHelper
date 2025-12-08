using System;
using System.Net.Sockets;
using System.Text;
using VMHelper.Interfaces;

namespace VMHelper;

/// <summary>
/// TCP通讯服务实现
/// </summary>
public class TcpService : ITcpService
{
    private string _serverHost = "127.0.0.1"; // 默认服务器地址
    private int _serverPort = 7930; // 默认服务器端口
    private int _timeoutMs = 3000; // 超时时间

    private string? _sendTerminator; // 发送数据时使用的结束符
    private string? _receiveTerminator; // 接收数据时检测的结束符

    private TcpClient? _tcpClient;
    private NetworkStream? _networkStream;
    private readonly SemaphoreSlim _connectionSemaphore = new(1, 1);
    private readonly SemaphoreSlim _communicationSemaphore = new(1, 1);
    private bool _disposed = false;

    /// <summary>
    /// 连接状态
    /// </summary>
    public bool IsConnected => _tcpClient?.Connected == true && _networkStream != null;

    /// <summary>
    /// 获取TcpClient对象
    /// </summary>
    public TcpClient? TcpClient => _tcpClient;

    /// <summary>
    /// 设置服务器连接参数
    /// </summary>
    /// <param name="host">服务器地址</param>
    /// <param name="port">服务器端口</param>
    /// <param name="timeoutMs">超时时间（毫秒）</param>
    public void SetServerConnection(string host, int port, int timeoutMs)
    {
        _serverHost = host;
        _serverPort = port;
        _timeoutMs = timeoutMs;

        // 参数变更后断开连接，下次使用时会用新参数重连
        DisposeConnection();
    }

    /// <summary>
    /// 设置结束符配置
    /// </summary>
    /// <param name="sendTerminator">发送数据时使用的结束符，null表示不使用结束符</param>
    /// <param name="receiveTerminator">接收数据时检测的结束符，null表示不检测结束符</param>
    public void SetTerminatorConfig(string? sendTerminator = "\r", string? receiveTerminator = "\r")
    {
        _sendTerminator = sendTerminator;
        _receiveTerminator = receiveTerminator;
    }

    /// <summary>
    /// 连接到服务器
    /// </summary>
    /// <returns>连接是否成功</returns>
    public async Task<bool> ConnectAsync()
    {
        try
        {
            await EnsureConnectedAsync();
            return IsConnected;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 断开连接
    /// </summary>
    public void Disconnect()
    {
        DisposeConnection();
    }

    /// <summary>
    /// 发送命令并接收响应
    /// </summary>
    /// <param name="command">要发送的命令</param>
    /// <returns>服务器响应字符串</returns>
    public async Task<string> SendCommandAsync(string command)
    {
        // 确保连接可用
        await EnsureConnectedAsync();

        using var cancelToken = new CancellationTokenSource(_timeoutMs);
        await _communicationSemaphore.WaitAsync(cancelToken.Token);
        try
        {
            if (!IsConnected)
                throw new InvalidOperationException("TCP连接未建立");

            // 清空缓冲区
            await ClearReceiveBufferAsync(cancelToken.Token);

            // 发送命令，根据配置添加结束符
            var commandToSend = !string.IsNullOrEmpty(_sendTerminator) ? command + _sendTerminator : command;
            var commandBytes = Encoding.UTF8.GetBytes(commandToSend);
            await _networkStream!.WriteAsync(commandBytes, 0, commandBytes.Length, cancelToken.Token);

            // 接收响应
            var response = await ReceiveResponseAsync(cancelToken.Token);

            return response;
        }
        catch
        {
            // 通信异常时断开连接，下次会自动重连
            DisposeConnection();
            throw;
        }
        finally
        {
            _communicationSemaphore.Release();
        }
    }


    /// <summary>
    /// 发送命令并接收响应，返回解析结果对象
    /// </summary>
    /// <param name="command">要发送的命令</param>
    /// <param name="pairSeparator">键值对之间的分隔符，默认为逗号</param>
    /// <param name="keyValueSeparator">键和值之间的分隔符，默认为冒号</param>
    /// <returns>解析结果对象</returns>
    public async Task<ParseResult> SendCommandAndParseResultAsync(string command, string pairSeparator = ",", string keyValueSeparator = ":")
    {
        var responseString = await SendCommandAsync(command);
        return ParseToResult(responseString, pairSeparator, keyValueSeparator);
    }

    /// <summary>
    /// 解析服务器响应字符串为解析结果对象
    /// </summary>
    /// <param name="responseString">服务器响应字符串</param>
    /// <param name="pairSeparator">键值对之间的分隔符，默认为逗号</param>
    /// <param name="keyValueSeparator">键和值之间的分隔符，默认为冒号</param>
    /// <returns>解析结果对象</returns>
    public ParseResult ParseToResult(string responseString, string pairSeparator = ",", string keyValueSeparator = ":")
    {
        return new ParseResult(responseString, pairSeparator, keyValueSeparator);
    }

    /// <summary>
    /// 接收响应数据，处理结束符
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>完整的响应字符串</returns>
    private async Task<string> ReceiveResponseAsync(CancellationToken cancellationToken)
    {
        var buffer = new byte[1024];
        var responseBuilder = new StringBuilder();
        bool foundTerminator = false;

        // 如果没有配置接收结束符，只读取一次
        if (string.IsNullOrEmpty(_receiveTerminator))
        {
            var bytesRead = await _networkStream!.ReadAsync(buffer, 0, buffer.Length, cancellationToken);

            if (bytesRead == 0)
                throw new Exception("服务器关闭了连接");

            return Encoding.UTF8.GetString(buffer, 0, bytesRead);
        }

        // 有配置接收结束符，持续读取直到找到结束符
        while (!foundTerminator && !cancellationToken.IsCancellationRequested)
        {
            var bytesRead = await _networkStream!.ReadAsync(buffer, 0, buffer.Length, cancellationToken);

            if (bytesRead == 0)
                throw new Exception("服务器关闭了连接");

            var chunk = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            responseBuilder.Append(chunk);

            // 检查是否包含配置的结束符
            if (chunk.Contains(_receiveTerminator))
            {
                foundTerminator = true;
            }
        }

        //去除结束符
        if (foundTerminator)
        {
            var terminatorIndex = responseBuilder.ToString().IndexOf(_receiveTerminator, StringComparison.Ordinal);
            if (terminatorIndex >= 0)
            {
                responseBuilder.Remove(terminatorIndex, _receiveTerminator.Length);
            }
        }
        return responseBuilder.ToString();
    }

    /// <summary>
    /// 确保连接有效
    /// </summary>
    private async Task EnsureConnectedAsync()
    {
        if (IsConnected)
            return;

        await _connectionSemaphore.WaitAsync(TimeSpan.FromSeconds(4));
        try
        {
            // 双重检查
            if (IsConnected)
                return;

            await ConnectInternalAsync();
        }
        finally
        {
            _connectionSemaphore.Release();
        }
    }

    private async Task ClearReceiveBufferAsync(CancellationToken token)
    {
        if (_networkStream == null || !_networkStream.DataAvailable)
            return;

        var buffer = new byte[1024];
        while (_networkStream.DataAvailable && !token.IsCancellationRequested)
        {
            await _networkStream.ReadAsync(buffer, 0, buffer.Length,token);
        }
    }

    /// <summary>
    /// 建立连接
    /// </summary>
    private async Task ConnectInternalAsync()
    {
        // 清理旧连接
        DisposeConnection();

        _tcpClient = new TcpClient();
        _tcpClient.ReceiveTimeout = _timeoutMs;
        _tcpClient.SendTimeout = _timeoutMs;

        await _tcpClient.ConnectAsync(_serverHost, _serverPort);
        _networkStream = _tcpClient.GetStream();
    }

    /// <summary>
    /// 清理连接资源
    /// </summary>
    private void DisposeConnection()
    {
        try
        {
            _networkStream?.Close();
            _networkStream?.Dispose();
            _networkStream = null;

            _tcpClient?.Close();
            _tcpClient?.Dispose();
            _tcpClient = null;
        }
        catch
        {
            // 忽略清理时的异常
        }
    }

    /// <summary>
    /// 释放资源
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;

        DisposeConnection();

        _connectionSemaphore.Dispose();
        _communicationSemaphore.Dispose();

        GC.SuppressFinalize(this);
    }
}