using Microsoft.Extensions.DependencyInjection;
using VMHelper.Interfaces;

namespace VMHelper;

public static class ServiceExtension
{
    /// <summary>
    /// 扩展方法：将服务添加到服务集合中
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddVMHelperServices(this IServiceCollection services)
    {
        // 添加TCP通讯服务
        services.AddSingleton<ITcpService, TcpService>();
        
        return services;
    }
}
