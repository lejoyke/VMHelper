using System.Globalization;
using System.Text;

namespace VMHelper;

/// <summary>
/// 解析结果类，用于解析服务器响应数据并提供类型安全的访问方法
/// 支持普通键值对和数组格式的数据解析
/// </summary>
public class ParseResult
{
    private readonly Dictionary<string, string> _data;
    private readonly Dictionary<string, string[]> _arrayData;
    private readonly string _responseString;
    
    /// <summary>
    /// 获取解析后的普通键集合
    /// </summary>
    public IEnumerable<string> Keys => _data.Keys;
    
    /// <summary>
    /// 获取解析后的数组键集合
    /// </summary>
    public IEnumerable<string> ArrayKeys => _arrayData.Keys;
    
    /// <summary>
    /// 获取所有键集合（包括普通键和数组键）
    /// </summary>
    public IEnumerable<string> AllKeys => Keys.Concat(ArrayKeys);
    
    /// <summary>
    /// 获取解析后的普通数据项数量
    /// </summary>
    public int Count => _data.Count;
    
    /// <summary>
    /// 获取解析后的数组数据项数量
    /// </summary>
    public int ArrayCount => _arrayData.Count;
    
    /// <summary>
    /// 获取总数据项数量（包括普通数据和数组数据）
    /// </summary>
    public int TotalCount => Count + ArrayCount;
    
    /// <summary>
    /// 初始化解析结果
    /// </summary>
    /// <param name="responseString">要解析的原始字符串</param>
    /// <param name="pairSeparator">键值对之间的分隔符，默认为逗号</param>
    /// <param name="keyValueSeparator">键和值之间的分隔符，默认为冒号</param>
    public ParseResult(string responseString, string pairSeparator = ",", string keyValueSeparator = ":")
    {
        _data = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        _arrayData = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
        _responseString = responseString;
        ParseData(responseString, pairSeparator, keyValueSeparator);
    }
    
    /// <summary>
    /// 获取整数值
    /// </summary>
    /// <param name="key">键名</param>
    /// <returns>整数值</returns>
    /// <exception cref="KeyNotFoundException">当键不存在时抛出</exception>
    /// <exception cref="FormatException">当值无法转换为整数时抛出</exception>
    public int GetInt(string key)
    {
        if (!_data.TryGetValue(key, out var value))
            throw new KeyNotFoundException($"键 '{key}' 不存在,responseString:{_responseString}");
            
        if (!int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
            throw new FormatException($"无法将键 '{key}' 的值 '{value}' 转换为整数");
            
        return result;
    }
    
    /// <summary>
    /// 获取双精度浮点数值
    /// </summary>
    /// <param name="key">键名</param>
    /// <returns>双精度浮点数值</returns>
    /// <exception cref="KeyNotFoundException">当键不存在时抛出</exception>
    /// <exception cref="FormatException">当值无法转换为双精度浮点数时抛出</exception>
    public double GetDouble(string key)
    {
        if (!_data.TryGetValue(key, out var value))
            throw new KeyNotFoundException($"键 '{key}' 不存在,responseString:{_responseString}");
            
        if (!double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
            throw new FormatException($"无法将键 '{key}' 的值 '{value}' 转换为双精度浮点数");
            
        return result;
    }
    
    /// <summary>
    /// 获取单精度浮点数值
    /// </summary>
    /// <param name="key">键名</param>
    /// <returns>单精度浮点数值</returns>
    /// <exception cref="KeyNotFoundException">当键不存在时抛出</exception>
    /// <exception cref="FormatException">当值无法转换为单精度浮点数时抛出</exception>
    public float GetFloat(string key)
    {
        if (!_data.TryGetValue(key, out var value))
            throw new KeyNotFoundException($"键 '{key}' 不存在,responseString:{_responseString}");
            
        if (!float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
            throw new FormatException($"无法将键 '{key}' 的值 '{value}' 转换为单精度浮点数");
            
        return result;
    }
    
    /// <summary>
    /// 获取字符串值
    /// </summary>
    /// <param name="key">键名</param>
    /// <returns>字符串值</returns>
    /// <exception cref="KeyNotFoundException">当键不存在时抛出</exception>
    public string GetString(string key)
    {
        if (!_data.TryGetValue(key, out var value))
            throw new KeyNotFoundException($"键 '{key}' 不存在,responseString:{_responseString}");
            
        return value;
    }
    
    /// <summary>
    /// 获取布尔值
    /// </summary>
    /// <param name="key">键名</param>
    /// <returns>布尔值</returns>
    /// <exception cref="KeyNotFoundException">当键不存在时抛出</exception>
    /// <exception cref="FormatException">当值无法转换为布尔值时抛出</exception>
    /// <remarks>
    /// 支持的真值：true, 1, yes, on, enabled（不区分大小写）
    /// 支持的假值：false, 0, no, off, disabled（不区分大小写）
    /// </remarks>
    public bool GetBool(string key)
    {
        if (!_data.TryGetValue(key, out var value))
            throw new KeyNotFoundException($"键 '{key}' 不存在,responseString:{_responseString}");
            
        var lowerValue = value.ToLowerInvariant();
        
        // 真值
        if (lowerValue is "true" or "1" or "yes" or "on" or "enabled")
            return true;
            
        // 假值
        if (lowerValue is "false" or "0" or "no" or "off" or "disabled")
            return false;
            
        throw new FormatException($"无法将键 '{key}' 的值 '{value}' 转换为布尔值。支持的值：true/false, 1/0, yes/no, on/off, enabled/disabled");
    }
    
    /// <summary>
    /// 获取长整数值
    /// </summary>
    /// <param name="key">键名</param>
    /// <returns>长整数值</returns>
    /// <exception cref="KeyNotFoundException">当键不存在时抛出</exception>
    /// <exception cref="FormatException">当值无法转换为长整数时抛出</exception>
    public long GetLong(string key)
    {
        if (!_data.TryGetValue(key, out var value))
            throw new KeyNotFoundException($"键 '{key}' 不存在,responseString:{_responseString}");
            
        if (!long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
            throw new FormatException($"无法将键 '{key}' 的值 '{value}' 转换为长整数");
            
        return result;
    }
    
    /// <summary>
    /// 获取十进制数值
    /// </summary>
    /// <param name="key">键名</param>
    /// <returns>十进制数值</returns>
    /// <exception cref="KeyNotFoundException">当键不存在时抛出</exception>
    /// <exception cref="FormatException">当值无法转换为十进制数时抛出</exception>
    public decimal GetDecimal(string key)
    {
        if (!_data.TryGetValue(key, out var value))
            throw new KeyNotFoundException($"键 '{key}' 不存在,responseString:{_responseString}");
            
        if (!decimal.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
            throw new FormatException($"无法将键 '{key}' 的值 '{value}' 转换为十进制数");
            
        return result;
    }
    
    /// <summary>
    /// 检查是否包含指定的键（包括普通键和数组键）
    /// </summary>
    /// <param name="key">键名</param>
    /// <returns>如果包含指定键则返回true，否则返回false</returns>
    public bool HasKey(string key)
    {
        return _data.ContainsKey(key) || _arrayData.ContainsKey(key);
    }
    
    /// <summary>
    /// 检查是否包含指定的数组键
    /// </summary>
    /// <param name="key">键名</param>
    /// <returns>如果包含指定数组键则返回true，否则返回false</returns>
    public bool HasArrayKey(string key)
    {
        return _arrayData.ContainsKey(key);
    }
    
    /// <summary>
    /// 获取整数数组
    /// </summary>
    /// <param name="key">键名</param>
    /// <returns>整数数组</returns>
    /// <exception cref="KeyNotFoundException">当键不存在时抛出</exception>
    /// <exception cref="FormatException">当数组中的值无法转换为整数时抛出</exception>
    public int[] GetIntArray(string key)
    {
        if (!_arrayData.TryGetValue(key, out var stringArray))
            throw new KeyNotFoundException($"数组键 '{key}' 不存在,responseString:{_responseString}");
            
        var result = new int[stringArray.Length];
        for (int i = 0; i < stringArray.Length; i++)
        {
            if (!int.TryParse(stringArray[i], NumberStyles.Integer, CultureInfo.InvariantCulture, out result[i]))
                throw new FormatException($"无法将数组键 '{key}' 索引 {i} 的值 '{stringArray[i]}' 转换为整数");
        }
        return result;
    }
    
    /// <summary>
    /// 获取双精度浮点数数组
    /// </summary>
    /// <param name="key">键名</param>
    /// <returns>双精度浮点数数组</returns>
    /// <exception cref="KeyNotFoundException">当键不存在时抛出</exception>
    /// <exception cref="FormatException">当数组中的值无法转换为双精度浮点数时抛出</exception>
    public double[] GetDoubleArray(string key)
    {
        if (!_arrayData.TryGetValue(key, out var stringArray))
            throw new KeyNotFoundException($"数组键 '{key}' 不存在,responseString:{_responseString}");
            
        var result = new double[stringArray.Length];
        for (int i = 0; i < stringArray.Length; i++)
        {
            if (!double.TryParse(stringArray[i], NumberStyles.Float, CultureInfo.InvariantCulture, out result[i]))
                throw new FormatException($"无法将数组键 '{key}' 索引 {i} 的值 '{stringArray[i]}' 转换为双精度浮点数");
        }
        return result;
    }
    
    /// <summary>
    /// 获取单精度浮点数数组
    /// </summary>
    /// <param name="key">键名</param>
    /// <returns>单精度浮点数数组</returns>
    /// <exception cref="KeyNotFoundException">当键不存在时抛出</exception>
    /// <exception cref="FormatException">当数组中的值无法转换为单精度浮点数时抛出</exception>
    public float[] GetFloatArray(string key)
    {
        if (!_arrayData.TryGetValue(key, out var stringArray))
            throw new KeyNotFoundException($"数组键 '{key}' 不存在,responseString:{_responseString}");
            
        var result = new float[stringArray.Length];
        for (int i = 0; i < stringArray.Length; i++)
        {
            if (!float.TryParse(stringArray[i], NumberStyles.Float, CultureInfo.InvariantCulture, out result[i]))
                throw new FormatException($"无法将数组键 '{key}' 索引 {i} 的值 '{stringArray[i]}' 转换为单精度浮点数");
        }
        return result;
    }
    
    /// <summary>
    /// 获取字符串数组
    /// </summary>
    /// <param name="key">键名</param>
    /// <returns>字符串数组</returns>
    /// <exception cref="KeyNotFoundException">当键不存在时抛出</exception>
    public string[] GetStringArray(string key)
    {
        if (!_arrayData.TryGetValue(key, out var stringArray))
            throw new KeyNotFoundException($"数组键 '{key}' 不存在,responseString:{_responseString}");
            
        return (string[])stringArray.Clone(); // 返回副本以避免外部修改
    }
    
    /// <summary>
    /// 获取长整数数组
    /// </summary>
    /// <param name="key">键名</param>
    /// <returns>长整数数组</returns>
    /// <exception cref="KeyNotFoundException">当键不存在时抛出</exception>
    /// <exception cref="FormatException">当数组中的值无法转换为长整数时抛出</exception>
    public long[] GetLongArray(string key)
    {
        if (!_arrayData.TryGetValue(key, out var stringArray))
            throw new KeyNotFoundException($"数组键 '{key}' 不存在,responseString:{_responseString}");
            
        var result = new long[stringArray.Length];
        for (int i = 0; i < stringArray.Length; i++)
        {
            if (!long.TryParse(stringArray[i], NumberStyles.Integer, CultureInfo.InvariantCulture, out result[i]))
                throw new FormatException($"无法将数组键 '{key}' 索引 {i} 的值 '{stringArray[i]}' 转换为长整数");
        }
        return result;
    }
    
    /// <summary>
    /// 获取十进制数数组
    /// </summary>
    /// <param name="key">键名</param>
    /// <returns>十进制数数组</returns>
    /// <exception cref="KeyNotFoundException">当键不存在时抛出</exception>
    /// <exception cref="FormatException">当数组中的值无法转换为十进制数时抛出</exception>
    public decimal[] GetDecimalArray(string key)
    {
        if (!_arrayData.TryGetValue(key, out var stringArray))
            throw new KeyNotFoundException($"数组键 '{key}' 不存在,responseString:{_responseString} ");
            
        var result = new decimal[stringArray.Length];
        for (int i = 0; i < stringArray.Length; i++)
        {
            if (!decimal.TryParse(stringArray[i], NumberStyles.Float, CultureInfo.InvariantCulture, out result[i]))
                throw new FormatException($"无法将数组键 '{key}' 索引 {i} 的值 '{stringArray[i]}' 转换为十进制数");
        }
        return result;
    }
    
    /// <summary>
    /// 尝试获取整数值
    /// </summary>
    /// <param name="key">键名</param>
    /// <param name="value">输出参数，如果成功则包含转换后的值</param>
    /// <returns>如果键存在且转换成功则返回true，否则返回false</returns>
    public bool TryGetInt(string key, out int value)
    {
        value = 0;
        return _data.TryGetValue(key, out var stringValue) && 
               int.TryParse(stringValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
    }
    
    /// <summary>
    /// 尝试获取双精度浮点数值
    /// </summary>
    /// <param name="key">键名</param>
    /// <param name="value">输出参数，如果成功则包含转换后的值</param>
    /// <returns>如果键存在且转换成功则返回true，否则返回false</returns>
    public bool TryGetDouble(string key, out double value)
    {
        value = 0.0;
        return _data.TryGetValue(key, out var stringValue) && 
               double.TryParse(stringValue, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
    }
    
    /// <summary>
    /// 尝试获取单精度浮点数值
    /// </summary>
    /// <param name="key">键名</param>
    /// <param name="value">输出参数，如果成功则包含转换后的值</param>
    /// <returns>如果键存在且转换成功则返回true，否则返回false</returns>
    public bool TryGetFloat(string key, out float value)
    {
        value = 0.0f;
        return _data.TryGetValue(key, out var stringValue) && 
               float.TryParse(stringValue, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
    }
    
    /// <summary>
    /// 尝试获取长整数值
    /// </summary>
    /// <param name="key">键名</param>
    /// <param name="value">输出参数，如果成功则包含转换后的值</param>
    /// <returns>如果键存在且转换成功则返回true，否则返回false</returns>
    public bool TryGetLong(string key, out long value)
    {
        value = 0L;
        return _data.TryGetValue(key, out var stringValue) && 
               long.TryParse(stringValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
    }
    
    /// <summary>
    /// 尝试获取十进制数值
    /// </summary>
    /// <param name="key">键名</param>
    /// <param name="value">输出参数，如果成功则包含转换后的值</param>
    /// <returns>如果键存在且转换成功则返回true，否则返回false</returns>
    public bool TryGetDecimal(string key, out decimal value)
    {
        value = 0m;
        return _data.TryGetValue(key, out var stringValue) && 
               decimal.TryParse(stringValue, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
    }
    
    /// <summary>
    /// 尝试获取布尔值
    /// </summary>
    /// <param name="key">键名</param>
    /// <param name="value">输出参数，如果成功则包含转换后的值</param>
    /// <returns>如果键存在且转换成功则返回true，否则返回false</returns>
    public bool TryGetBool(string key, out bool value)
    {
        value = false;
        if (!_data.TryGetValue(key, out var stringValue))
            return false;
            
        var lowerValue = stringValue.ToLowerInvariant();
        
        if (lowerValue is "true" or "1" or "yes" or "on" or "enabled")
        {
            value = true;
            return true;
        }
        
        if (lowerValue is "false" or "0" or "no" or "off" or "disabled")
        {
            value = false;
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// 尝试获取原始字符串值
    /// </summary>
    /// <param name="key">键名</param>
    /// <param name="value">输出参数，如果键存在则包含对应的值</param>
    /// <returns>如果键存在则返回true，否则返回false</returns>
    public bool TryGetValue(string key, out string value)
    {
        return _data.TryGetValue(key, out value!);
    }
    
    /// <summary>
    /// 尝试获取整数数组
    /// </summary>
    /// <param name="key">键名</param>
    /// <param name="value">输出参数，如果成功则包含转换后的数组</param>
    /// <returns>如果键存在且转换成功则返回true，否则返回false</returns>
    public bool TryGetIntArray(string key, out int[] value)
    {
        value = Array.Empty<int>();
        
        if (!_arrayData.TryGetValue(key, out var stringArray))
            return false;
            
        var result = new int[stringArray.Length];
        for (int i = 0; i < stringArray.Length; i++)
        {
            if (!int.TryParse(stringArray[i], NumberStyles.Integer, CultureInfo.InvariantCulture, out result[i]))
                return false;
        }
        
        value = result;
        return true;
    }
    
    /// <summary>
    /// 尝试获取双精度浮点数数组
    /// </summary>
    /// <param name="key">键名</param>
    /// <param name="value">输出参数，如果成功则包含转换后的数组</param>
    /// <returns>如果键存在且转换成功则返回true，否则返回false</returns>
    public bool TryGetDoubleArray(string key, out double[] value)
    {
        value = Array.Empty<double>();
        
        if (!_arrayData.TryGetValue(key, out var stringArray))
            return false;
            
        var result = new double[stringArray.Length];
        for (int i = 0; i < stringArray.Length; i++)
        {
            if (!double.TryParse(stringArray[i], NumberStyles.Float, CultureInfo.InvariantCulture, out result[i]))
                return false;
        }
        
        value = result;
        return true;
    }
    
    /// <summary>
    /// 尝试获取单精度浮点数数组
    /// </summary>
    /// <param name="key">键名</param>
    /// <param name="value">输出参数，如果成功则包含转换后的数组</param>
    /// <returns>如果键存在且转换成功则返回true，否则返回false</returns>
    public bool TryGetFloatArray(string key, out float[] value)
    {
        value = Array.Empty<float>();
        
        if (!_arrayData.TryGetValue(key, out var stringArray))
            return false;
            
        var result = new float[stringArray.Length];
        for (int i = 0; i < stringArray.Length; i++)
        {
            if (!float.TryParse(stringArray[i], NumberStyles.Float, CultureInfo.InvariantCulture, out result[i]))
                return false;
        }
        
        value = result;
        return true;
    }
    
    /// <summary>
    /// 尝试获取字符串数组
    /// </summary>
    /// <param name="key">键名</param>
    /// <param name="value">输出参数，如果键存在则包含对应的数组</param>
    /// <returns>如果键存在则返回true，否则返回false</returns>
    public bool TryGetStringArray(string key, out string[] value)
    {
        if (_arrayData.TryGetValue(key, out var stringArray))
        {
            value = (string[])stringArray.Clone();
            return true;
        }
        
        value = Array.Empty<string>();
        return false;
    }
    
    /// <summary>
    /// 尝试获取长整数数组
    /// </summary>
    /// <param name="key">键名</param>
    /// <param name="value">输出参数，如果成功则包含转换后的数组</param>
    /// <returns>如果键存在且转换成功则返回true，否则返回false</returns>
    public bool TryGetLongArray(string key, out long[] value)
    {
        value = Array.Empty<long>();
        
        if (!_arrayData.TryGetValue(key, out var stringArray))
            return false;
            
        var result = new long[stringArray.Length];
        for (int i = 0; i < stringArray.Length; i++)
        {
            if (!long.TryParse(stringArray[i], NumberStyles.Integer, CultureInfo.InvariantCulture, out result[i]))
                return false;
        }
        
        value = result;
        return true;
    }
    
    /// <summary>
    /// 尝试获取十进制数数组
    /// </summary>
    /// <param name="key">键名</param>
    /// <param name="value">输出参数，如果成功则包含转换后的数组</param>
    /// <returns>如果键存在且转换成功则返回true，否则返回false</returns>
    public bool TryGetDecimalArray(string key, out decimal[] value)
    {
        value = Array.Empty<decimal>();
        
        if (!_arrayData.TryGetValue(key, out var stringArray))
            return false;
            
        var result = new decimal[stringArray.Length];
        for (int i = 0; i < stringArray.Length; i++)
        {
            if (!decimal.TryParse(stringArray[i], NumberStyles.Float, CultureInfo.InvariantCulture, out result[i]))
                return false;
        }
        
        value = result;
        return true;
    }
    
    /// <summary>
    /// 获取所有键值对
    /// </summary>
    /// <returns>键值对集合</returns>
    public IEnumerable<KeyValuePair<string, string>> GetAllPairs()
    {
        return _data;
    }
    
    /// <summary>
    /// 获取所有数组键值对
    /// </summary>
    /// <returns>数组键值对集合</returns>
    public IEnumerable<KeyValuePair<string, string[]>> GetAllArrayPairs()
    {
        return _arrayData;
    }
    
    /// <summary>
    /// 转换为字典
    /// </summary>
    /// <returns>包含所有键值对的字典</returns>
    public Dictionary<string, string> ToDictionary()
    {
        return new Dictionary<string, string>(_data, StringComparer.OrdinalIgnoreCase);
    }
    
    /// <summary>
    /// 转换为数组字典
    /// </summary>
    /// <returns>包含所有数组键值对的字典</returns>
    public Dictionary<string, string[]> ToArrayDictionary()
    {
        return new Dictionary<string, string[]>(_arrayData, StringComparer.OrdinalIgnoreCase);
    }
    
    /// <summary>
    /// 解析数据字符串
    /// </summary>
    /// <param name="responseString">要解析的字符串</param>
    /// <param name="pairSeparator">键值对之间的分隔符</param>
    /// <param name="keyValueSeparator">键和值之间的分隔符</param>
    private void ParseData(string responseString, string pairSeparator, string keyValueSeparator)
    {
        if (string.IsNullOrWhiteSpace(responseString))
            return;

        // 如果返回结果为error,则报错
        if (responseString.ToLower().Contains("error"))
        {
            throw new Exception("视觉服务返回错误，返回结果为：" + responseString);
        }
            
        // 移除可能的各种结束符和空白字符
        var cleanResponse = responseString.Replace("\r", "").Replace("\n", "").Trim();
        
        // 智能分割，考虑数组中的逗号
        var pairs = SplitRespectingBrackets(cleanResponse, pairSeparator);
        
        foreach (var pair in pairs)
        {
            if (string.IsNullOrWhiteSpace(pair))
                continue;
                
            // 按照键值分隔符分割
            var keyValueParts = pair.Split(keyValueSeparator, 2, StringSplitOptions.None); // 限制为2部分，防止值中包含分隔符
            
            if (keyValueParts.Length == 2)
            {
                var key = keyValueParts[0].Trim();
                var value = keyValueParts[1].Trim();
                
                if (!string.IsNullOrEmpty(key))
                {
                    // 检查是否为数组格式 [value1,value2,value3]
                    if (value.StartsWith('[') && value.EndsWith(']'))
                    {
                        // 解析数组
                        var arrayContent = value.Substring(1, value.Length - 2); // 移除 [ 和 ]
                        
                        if (string.IsNullOrWhiteSpace(arrayContent))
                        {
                            // 空数组
                            _arrayData[key] = Array.Empty<string>();
                        }
                        else
                        {
                            // 按逗号分割数组元素
                            var arrayElements = arrayContent.Split(',', StringSplitOptions.None)
                                                            .Select(element => element.Trim())
                                                            .ToArray();
                            _arrayData[key] = arrayElements;
                        }
                    }
                    else
                    {
                        // 普通键值对
                        _data[key] = value; // 如果键重复，后面的值会覆盖前面的
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// 智能分割字符串，尊重方括号内的分隔符
    /// </summary>
    /// <param name="input">输入字符串</param>
    /// <param name="separator">分隔符</param>
    /// <returns>分割后的字符串数组</returns>
    private static string[] SplitRespectingBrackets(string input, string separator)
    {
        var result = new List<string>();
        var current = new StringBuilder();
        var bracketLevel = 0;
        var i = 0;
        
        while (i < input.Length)
        {
            var ch = input[i];
            
            if (ch == '[')
            {
                bracketLevel++;
                current.Append(ch);
            }
            else if (ch == ']')
            {
                bracketLevel--;
                current.Append(ch);
            }
            else if (bracketLevel == 0 && i + separator.Length <= input.Length && 
                     input.Substring(i, separator.Length) == separator)
            {
                // 在括号外遇到分隔符
                result.Add(current.ToString());
                current.Clear();
                i += separator.Length - 1; // -1 因为循环末尾会 i++
            }
            else
            {
                current.Append(ch);
            }
            
            i++;
        }
        
        // 添加最后一个部分
        if (current.Length > 0)
        {
            result.Add(current.ToString());
        }
        
        return result.ToArray();
    }
    
    /// <summary>
    /// 返回解析结果的字符串表示
    /// </summary>
    /// <returns>包含所有键值对和数组的字符串</returns>
    public override string ToString()
    {
        // 返回原始字符串
        return _responseString;
    }
} 