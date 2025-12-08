# VMHelper 类库

一个基于 .NET 8.0 的 TCP 通讯服务类库，专为视觉服务通讯而设计。

## 📋 功能特性

- **TCP 通讯服务**：提供稳定的 TCP 客户端连接管理
- **异步操作**：支持异步连接、发送和接收操作
- **线程安全**：内置信号量确保多线程环境下的安全使用
- **自动重连**：连接异常时自动重连机制
- **灵活配置**：支持自定义服务器地址、端口、超时时间和结束符
- **数据解析**：内置响应数据解析功能，支持 `name:value` 格式，多个数据用`,`分隔
- **类型安全解析**：新增 `ParseResult` 类，提供类型安全的数据访问方法，支持多种数据类型（int、double、string、bool等）
- **严格的错误处理**：Get 方法在键不存在或转换失败时抛出明确的异常，确保数据完整性
- **灵活的访问模式**：提供 Try 方法用于安全访问，支持严格模式和宽松模式两种使用方式
- **自定义分隔符**：支持自定义键值对分隔符和键值分隔符，适应不同的数据格式
- **路径管理**：提供视觉服务相关的路径管理工具

## 🚀 快速开始

### 基本用法

```csharp
using VMHelper;
using VMHelper.Interfaces;

// 创建 TCP 通讯服务实例
ITcpCommunicationService tcpService = new TcpCommunicationService();

// 配置服务器连接参数
tcpService.SetServerConnection("127.0.0.1", 7920, 3000);

// 配置结束符（可选）
tcpService.SetTerminatorConfig("\r", "\r");

// 连接到服务器
bool connected = await tcpService.ConnectAsync();
if (connected)
{
    Console.WriteLine("连接成功！");
    
    // 发送命令并接收响应
    string response = await tcpService.SendCommandAsync("GET_STATUS");
    Console.WriteLine($"服务器响应: {response}");
    
    // 发送命令并解析响应数据（返回字典）
    var data = await tcpService.SendCommandAndParseAsync("GET_DATA");
    foreach (var kvp in data)
    {
        Console.WriteLine($"{kvp.Key}: {kvp.Value}");
    }
    
    // 使用新的解析结果类（推荐）
    var result = await tcpService.SendCommandAndParseResultAsync("GET_DATA");
    
    try
    {
        int x = result.GetInt("X");
        double y = result.GetDouble("Y");
        string status = result.GetString("STATUS");
        bool enabled = result.GetBool("ENABLED");
        
        Console.WriteLine($"X: {x}, Y: {y}, Status: {status}, Enabled: {enabled}");
    }
    catch (KeyNotFoundException ex)
    {
        Console.WriteLine($"缺少必要数据: {ex.Message}");
    }
    catch (FormatException ex)
    {
        Console.WriteLine($"数据格式错误: {ex.Message}");
    }
}

// 记得释放资源
tcpService.Dispose();
```

### 使用路径管理工具

```csharp
using VMHelper;

// 创建默认目录（如果不存在）
PathSD.CreateDirectoriesIfNotExist();

// 使用预定义的路径常量
string inputPath = PathSD.DefaultImageInputPath;
string outputPath = PathSD.DefaultImageOutputPath;
string inputDir = PathSD.DefaultImageInputDirectory;
string outputDir = PathSD.DefaultImageOutputDirectory;

Console.WriteLine($"图像输入路径: {inputPath}");
Console.WriteLine($"图像输出路径: {outputPath}");
```

### 使用 ParseResult 类解析数据

```csharp
using VMHelper;

// 解析服务器响应数据
string responseData = "X:123.45,Y:67.89,STATUS:OK,ENABLED:1,COUNT:10";
var result = new ParseResult(responseData);

try
{
    // 类型安全地获取数据（如果键不存在或转换失败会抛出异常）
    double x = result.GetDouble("X");           // 123.45
    double y = result.GetDouble("Y");           // 67.89
    string status = result.GetString("STATUS"); // "OK"
    bool enabled = result.GetBool("ENABLED");   // true
    int count = result.GetInt("COUNT");         // 10
    
    Console.WriteLine($"位置: ({x}, {y}), 状态: {status}, 启用: {enabled}, 计数: {count}");
}
catch (KeyNotFoundException ex)
{
    Console.WriteLine($"缺少必要数据: {ex.Message}");
}
catch (FormatException ex)
{
    Console.WriteLine($"数据格式错误: {ex.Message}");
}

// 使用Try方法处理可能不存在的键
if (result.TryGetInt("TIMEOUT", out int timeout))
{
    Console.WriteLine($"超时时间: {timeout}");
}
else
{
    Console.WriteLine("未设置超时时间");
}
```

## 🔧 API 参考

### ITcpCommunicationService 接口

#### 属性
- `bool IsConnected`: 获取当前连接状态

#### 方法
- `void SetServerConnection(string host, int port, int timeoutMs)`: 设置服务器连接参数
- `void SetTerminatorConfig(string? sendTerminator, string? receiveTerminator)`: 配置数据结束符
- `Task<bool> ConnectAsync()`: 异步连接到服务器
- `void Disconnect()`: 断开连接
- `Task<string> SendCommandAsync(string command)`: 发送命令并接收原始响应
- `Task<Dictionary<string, double>> SendCommandAndParseAsync(string command)`: 发送命令并解析响应数据（返回字典）
- `Task<ParseResult> SendCommandAndParseResultAsync(string command, string pairSeparator = ",", string keyValueSeparator = ":")`: 发送命令并解析响应数据（返回解析结果对象）
- `Dictionary<string, double> ParseResponse(string responseString)`: 解析响应字符串为字典
- `ParseResult ParseToResult(string responseString, string pairSeparator = ",", string keyValueSeparator = ":")`: 解析响应字符串为解析结果对象

### PathSD 类

#### 常量
- `DefaultImageInputPath`: 默认图像输入路径
- `DefaultImageOutputPath`: 默认图像输出路径
- `DefaultImageInputDirectory`: 默认图像输入目录
- `DefaultImageOutputDirectory`: 默认图像输出目录

#### 方法
- `CreateDirectoriesIfNotExist()`: 创建默认目录（如果不存在）

### ParseResult 类

用于解析服务器响应数据的结果类，提供类型安全的数据访问方法。

#### 设计理念

- **快速失败原则**：`Get` 方法在遇到问题时立即抛出异常，避免静默的错误传播
- **类型安全**：所有数据访问都是强类型的，编译时就能发现类型错误
- **灵活性**：提供两套API（Get/Try），满足不同的使用场景

#### 属性
- `IEnumerable<string> Keys`: 获取所有键名
- `int Count`: 获取数据项数量

#### 构造函数
- `ParseResult(string responseString, string pairSeparator = ",", string keyValueSeparator = ":")`: 解析响应字符串

#### 类型安全的获取方法
- `int GetInt(string key)`: 获取整数值，键不存在或转换失败时抛出异常
- `double GetDouble(string key)`: 获取双精度浮点数值，键不存在或转换失败时抛出异常
- `float GetFloat(string key)`: 获取单精度浮点数值，键不存在或转换失败时抛出异常
- `long GetLong(string key)`: 获取长整数值，键不存在或转换失败时抛出异常
- `decimal GetDecimal(string key)`: 获取十进制数值，键不存在或转换失败时抛出异常
- `string GetString(string key)`: 获取字符串值，键不存在时抛出异常
- `bool GetBool(string key)`: 获取布尔值，键不存在或转换失败时抛出异常

#### 安全获取方法
- `bool TryGetInt(string key, out int value)`: 尝试获取整数值，成功返回true
- `bool TryGetDouble(string key, out double value)`: 尝试获取双精度浮点数值，成功返回true
- `bool TryGetFloat(string key, out float value)`: 尝试获取单精度浮点数值，成功返回true
- `bool TryGetLong(string key, out long value)`: 尝试获取长整数值，成功返回true
- `bool TryGetDecimal(string key, out decimal value)`: 尝试获取十进制数值，成功返回true
- `bool TryGetBool(string key, out bool value)`: 尝试获取布尔值，成功返回true

#### 辅助方法
- `bool HasKey(string key)`: 检查是否包含指定键
- `bool TryGetValue(string key, out string value)`: 尝试获取原始字符串值
- `IEnumerable<KeyValuePair<string, string>> GetAllPairs()`: 获取所有键值对
- `Dictionary<string, string> ToDictionary()`: 转换为字典

## 📖 详细示例

### 高级使用示例

```csharp
using VMHelper;
using VMHelper.Interfaces;

public class VisionServiceClient
{
    private readonly ITcpCommunicationService _tcpService;
    
    public VisionServiceClient()
    {
        _tcpService = new TcpCommunicationService();
        
        // 配置连接参数
        _tcpService.SetServerConnection("192.168.1.100", 7920, 5000);
        
        // 配置结束符
        _tcpService.SetTerminatorConfig("\r\n", "\r\n");
    }
    
    public async Task<bool> InitializeAsync()
    {
        // 确保目录存在
        PathSD.CreateDirectoriesIfNotExist();
        
        // 连接到服务器
        return await _tcpService.ConnectAsync();
    }
    
    public async Task<Dictionary<string, double>> GetMeasurementDataAsync()
    {
        try
        {
            // 发送测量命令
            var data = await _tcpService.SendCommandAndParseAsync("MEASURE");
            return data;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"测量数据获取失败: {ex.Message}");
            return new Dictionary<string, double>();
        }
    }
    
    public async Task<ParseResult> GetMeasurementResultAsync()
    {
        try
        {
            // 使用新的解析结果类
            var result = await _tcpService.SendCommandAndParseResultAsync("MEASURE");
            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"测量数据获取失败: {ex.Message}");
            return new ParseResult(""); // 返回空解析结果
        }
    }
    
    public async Task<bool> ProcessImageAsync(string imagePath)
    {
        try
        {
            // 发送图像处理命令
            string command = $"PROCESS_IMAGE:{imagePath}";
            string response = await _tcpService.SendCommandAsync(command);
            
            return response.Contains("SUCCESS");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"图像处理失败: {ex.Message}");
            return false;
        }
    }
    
    public void Dispose()
    {
        _tcpService?.Dispose();
    }
}
```

### 错误处理示例

```csharp
using VMHelper;
using VMHelper.Interfaces;

public async Task SafeCommunicationExample()
{
    ITcpCommunicationService tcpService = new TcpCommunicationService();
    
    try
    {
        // 配置连接
        tcpService.SetServerConnection("127.0.0.1", 7920, 3000);
        
        // 尝试连接
        if (!await tcpService.ConnectAsync())
        {
            Console.WriteLine("无法连接到服务器");
            return;
        }
        
        // 发送命令
        string response = await tcpService.SendCommandAsync("GET_STATUS");
        
        // 解析响应
        var data = tcpService.ParseResponse(response);
        
        if (data.Count > 0)
        {
            Console.WriteLine("解析成功:");
            foreach (var kvp in data)
            {
                Console.WriteLine($"  {kvp.Key} = {kvp.Value}");
            }
        }
        else
        {
            Console.WriteLine("没有解析到有效数据");
        }
    }
    catch (TimeoutException)
    {
        Console.WriteLine("操作超时");
    }
    catch (SocketException ex)
    {
        Console.WriteLine($"网络错误: {ex.Message}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"未知错误: {ex.Message}");
    }
    finally
    {
        tcpService.Dispose();
    }
}
```

### ParseResult 使用示例

```csharp
using VMHelper;

public async Task ParseResultExample()
{
    // 模拟服务器响应数据
    string responseData = "X:123.45,Y:67.89,Z:12.34,STATUS:OK,ENABLED:1,COUNT:10";
    
    // 创建解析结果对象
    var result = new ParseResult(responseData);
    
    try
    {
        // 类型安全地获取各种数据类型（如果键不存在或转换失败会抛出异常）
        double x = result.GetDouble("X");           // 123.45
        double y = result.GetDouble("Y");           // 67.89  
        double z = result.GetDouble("Z");           // 12.34
        string status = result.GetString("STATUS"); // "OK"
        bool enabled = result.GetBool("ENABLED");   // true (因为值为"1")
        int count = result.GetInt("COUNT");         // 10
        
        Console.WriteLine($"位置: ({x}, {y}, {z}), 状态: {status}, 启用: {enabled}, 计数: {count}");
    }
    catch (KeyNotFoundException ex)
    {
        Console.WriteLine($"缺少必要数据: {ex.Message}");
    }
    catch (FormatException ex)
    {
        Console.WriteLine($"数据格式错误: {ex.Message}");
    }
    
    // 使用Try方法处理可能不存在的键
    if (result.TryGetInt("TIMEOUT", out int timeout))
    {
        Console.WriteLine($"超时时间: {timeout}");
    }
    else
    {
        Console.WriteLine("未设置超时时间");
    }
    
    // 检查键是否存在
    if (result.HasKey("ERROR"))
    {
        string error = result.GetString("ERROR");
        Console.WriteLine($"错误信息: {error}");
    }
    
    // 遍历所有数据
    Console.WriteLine($"解析到 {result.Count} 个数据项:");
    foreach (var kvp in result.GetAllPairs())
    {
        Console.WriteLine($"  {kvp.Key} = {kvp.Value}");
    }
    
    // 使用自定义分隔符解析
    string customData = "name1=value1;name2=value2;name3=value3";
    var customResult = new ParseResult(customData, ";", "=");
    
    Console.WriteLine($"自定义分隔符解析结果: {customResult}");
}

// 在TCP通讯中使用
public async Task TcpParseResultExample()
{
    ITcpCommunicationService tcpService = new TcpCommunicationService();
    
    try
    {
        // 配置并连接
        tcpService.SetServerConnection("127.0.0.1", 7920, 3000);
        await tcpService.ConnectAsync();
        
        // 发送命令并获取解析结果
        var result = await tcpService.SendCommandAndParseResultAsync("GET_SENSOR_DATA");
        
        try
        {
            // 类型安全地获取传感器数据
            double temperature = result.GetDouble("TEMP");
            double humidity = result.GetDouble("HUMIDITY");
            bool alertActive = result.GetBool("ALERT");
            int sensorId = result.GetInt("SENSOR_ID");
            
            Console.WriteLine($"传感器 {sensorId}:");
            Console.WriteLine($"  温度: {temperature}°C");
            Console.WriteLine($"  湿度: {humidity}%");
            Console.WriteLine($"  警报状态: {(alertActive ? "激活" : "正常")}");
        }
        catch (KeyNotFoundException ex)
        {
            Console.WriteLine($"缺少必要的传感器数据: {ex.Message}");
        }
        catch (FormatException ex)
        {
            Console.WriteLine($"传感器数据格式错误: {ex.Message}");
        }
        
        // 处理可能的错误状态
        if (result.HasKey("ERROR_CODE"))
        {
            try
            {
                int errorCode = result.GetInt("ERROR_CODE");
                string errorMsg = result.TryGetValue("ERROR_MSG", out var msg) ? msg : "未知错误";
                Console.WriteLine($"错误 {errorCode}: {errorMsg}");
            }
            catch (FormatException)
            {
                Console.WriteLine("错误代码格式不正确");
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"通讯错误: {ex.Message}");
    }
    finally
    {
        tcpService.Dispose();
    }
}
```

## 🔍 响应数据格式

服务器响应数据应遵循以下格式：

```
name1:value1,name2:value2,name3:value3
```

示例：
```
X:123.45,Y:67.89,Z:12.34,STATUS:1
```

解析后的结果：
```csharp
// 使用传统的字典方式
{
    "X": 123.45,
    "Y": 67.89,
    "Z": 12.34,
    "STATUS": 1.0
}

// 使用新的 ParseResult 类（推荐）
var result = new ParseResult("X:123.45,Y:67.89,Z:12.34,STATUS:OK,ENABLED:1");
try
{
    double x = result.GetDouble("X");        // 123.45
    double y = result.GetDouble("Y");        // 67.89
    double z = result.GetDouble("Z");        // 12.34
    string status = result.GetString("STATUS"); // "OK"
    bool enabled = result.GetBool("ENABLED");   // true
    
    Console.WriteLine($"位置: ({x}, {y}, {z}), 状态: {status}, 启用: {enabled}");
}
catch (KeyNotFoundException ex)
{
    Console.WriteLine($"缺少必要数据: {ex.Message}");
}
catch (FormatException ex)
{
    Console.WriteLine($"数据格式错误: {ex.Message}");
}
```

### 布尔值处理

`ParseResult` 类的 `GetBool` 方法支持多种布尔值格式：

**真值（不区分大小写）**：
- `true`
- `1`
- `yes`
- `on`
- `enabled`

**假值（不区分大小写）**：
- `false`
- `0`
- `no`
- `off`
- `disabled`

### 自定义分隔符

```csharp
// 使用分号和等号作为分隔符
string data = "name1=value1;name2=value2;name3=value3";
var result = new ParseResult(data, ";", "=");

// 使用竖线和冒号作为分隔符
string data2 = "temp:25.5|humidity:60.2|status:active";
var result2 = new ParseResult(data2, "|", ":");
```

### 安全获取方法示例

```csharp
using VMHelper;

public void SafeParseExample()
{
    string responseData = "X:123.45,Y:invalid,STATUS:OK,ENABLED:1";
    var result = new ParseResult(responseData);
    
    // 使用 Try 方法安全地获取数据
    if (result.TryGetDouble("X", out double x))
    {
        Console.WriteLine($"X 坐标: {x}");
    }
    
    if (result.TryGetDouble("Y", out double y))
    {
        Console.WriteLine($"Y 坐标: {y}");
    }
    else
    {
        Console.WriteLine("Y 坐标数据无效或不存在");
    }
    
    // 检查可选字段
    if (result.TryGetInt("TIMEOUT", out int timeout))
    {
        Console.WriteLine($"超时设置: {timeout}ms");
    }
    else
    {
        Console.WriteLine("使用默认超时设置");
    }
    
    // 混合使用：必须字段抛出异常，可选字段使用 Try 方法
    try
    {
        string status = result.GetString("STATUS"); // 必须存在
        bool enabled = result.GetBool("ENABLED");   // 必须存在
        
        Console.WriteLine($"状态: {status}, 启用: {enabled}");
        
        // 可选字段
        if (result.TryGetInt("ERROR_CODE", out int errorCode))
        {
            Console.WriteLine($"错误代码: {errorCode}");
        }
    }
    catch (KeyNotFoundException ex)
    {
        Console.WriteLine($"缺少必要字段: {ex.Message}");
    }
    catch (FormatException ex)
    {
        Console.WriteLine($"数据格式错误: {ex.Message}");
    }
}
```

## ⚠️ 错误处理

### 异常类型

`ParseResult` 类的 `Get` 方法可能抛出以下异常：

1. **KeyNotFoundException**: 当请求的键不存在时抛出
2. **FormatException**: 当值无法转换为请求的数据类型时抛出

### 错误处理策略

```csharp
// 策略1：严格模式 - 所有错误都抛出异常
try
{
    int value = result.GetInt("REQUIRED_VALUE");
    // 处理成功获取的值
}
catch (KeyNotFoundException ex)
{
    Console.WriteLine($"缺少必要参数: {ex.Message}");
    // 处理缺少键的情况
}
catch (FormatException ex)
{
    Console.WriteLine($"数据格式错误: {ex.Message}");
    // 处理格式错误的情况
}

// 策略2：宽松模式 - 使用 Try 方法避免异常
if (result.TryGetInt("OPTIONAL_VALUE", out int value))
{
    // 处理成功获取的值
}
else
{
    // 使用默认值或跳过处理
    int defaultValue = 100;
    // 使用 defaultValue 继续处理
}

// 策略3：混合模式 - 必要字段抛出异常，可选字段使用 Try 方法
try
{
    // 必要字段
    string status = result.GetString("STATUS");
    
    // 可选字段
    if (result.TryGetInt("TIMEOUT", out int timeout))
    {
        Console.WriteLine($"超时设置: {timeout}");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"处理必要字段时发生错误: {ex.Message}");
}
```

### 最佳实践

1. **必要字段使用 Get 方法**：对于业务逻辑必须的字段，使用 `Get` 方法让异常提前暴露问题
2. **可选字段使用 Try 方法**：对于可选字段，使用 `Try` 方法避免异常并提供默认行为
3. **集中错误处理**：在调用层面统一处理 `KeyNotFoundException` 和 `FormatException`
4. **详细的错误信息**：异常信息包含具体的键名和值，便于调试

### 使用场景对比

| 场景 | 推荐方法 | 原因 |
|------|----------|------|
| 必须存在的配置参数 | `Get` 方法 | 缺少参数应该立即失败，不应该继续执行 |
| 可选的配置参数 | `Try` 方法 | 允许使用默认值或跳过处理 |
| 数据验证 | `Get` 方法 | 数据格式错误应该被明确识别 |
| 向后兼容性 | `Try` 方法 | 新增字段可能在旧版本中不存在 |
| 批量处理 | `Try` 方法 | 避免单个错误中断整个处理流程 |
| 调试模式 | `Get` 方法 | 让问题快速暴露，便于调试 |

## ⚙️ 配置说明

### 连接参数
- **host**: 服务器地址（默认：127.0.0.1）
- **port**: 服务器端口（默认：7920）
- **timeoutMs**: 超时时间毫秒（默认：3000）

### 结束符配置
- **sendTerminator**: 发送数据时添加的结束符（默认："\r"）
- **receiveTerminator**: 接收数据时检测的结束符（默认："\r"）
