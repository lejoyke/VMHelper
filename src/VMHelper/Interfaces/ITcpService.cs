using System.Net.Sockets;
namespace VMHelper.Interfaces;


/// <summary>
/// TCP通讯服务接口
/// </summary>
public interface ITcpService : IDisposable
{
    /// <summary>
    /// 连接状态
    /// </summary>
    bool IsConnected { get; }

    /// <summary>
    /// 获取TcpClient对象
    /// </summary>
    TcpClient? TcpClient { get; }

    /// <summary>
    /// 设置服务器连接参数
    /// </summary>
    /// <param name="host">服务器地址</param>
    /// <param name="port">服务器端口</param>
    /// <param name="timeoutMs">超时时间（毫秒）</param>
    void SetServerConnection(string host, int port, int timeoutMs);

    /// <summary>
    /// 设置结束符配置
    /// </summary>
    /// <param name="sendTerminator">发送数据时使用的结束符，null表示不使用结束符</param>
    /// <param name="receiveTerminator">接收数据时检测的结束符，null表示不检测结束符</param>
    void SetTerminatorConfig(string? sendTerminator = "\r", string? receiveTerminator = "\r");

    /// <summary>
    /// 连接到服务器
    /// </summary>
    /// <returns>连接是否成功</returns>
    Task<bool> ConnectAsync();

    /// <summary>
    /// 断开连接
    /// </summary>
    void Disconnect();

    /// <summary>
    /// 发送命令并接收响应
    /// </summary>
    /// <param name="command">要发送的命令</param>
    /// <returns>服务器响应字符串</returns>
    Task<string> SendCommandAsync(string command);

    /// <summary>
    /// 发送命令并接收响应，返回解析结果对象
    /// </summary>
    /// <param name="command">要发送的命令</param>
    /// <param name="pairSeparator">键值对之间的分隔符，默认为逗号</param>
    /// <param name="keyValueSeparator">键和值之间的分隔符，默认为冒号</param>
    /// <returns>解析结果对象</returns>
    Task<ParseResult> SendCommandAndParseResultAsync(string command, string pairSeparator = ",", string keyValueSeparator = ":");

    /// <summary>
    /// 解析服务器响应字符串为解析结果对象
    /// </summary>
    /// <param name="responseString">服务器响应字符串</param>
    /// <param name="pairSeparator">键值对之间的分隔符，默认为逗号</param>
    /// <param name="keyValueSeparator">键和值之间的分隔符，默认为冒号</param>
    /// <returns>解析结果对象</returns>
    ParseResult ParseToResult(string responseString, string pairSeparator = ",", string keyValueSeparator = ":");
}
