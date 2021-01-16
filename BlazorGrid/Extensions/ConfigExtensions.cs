using BlazorGrid.Config;
using BlazorGrid.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace BlazorGrid.Extensions
{
    public static class ConfigExtensions
    {
        public static void AddBlazorGrid(this IServiceCollection services)
        {
            AddBlazorGrid(services, null);
        }

        public static void AddBlazorGrid(this IServiceCollection services, Action<IBlazorGridConfig> configuration)
        {
            services.AddSingleton<IBlazorGridConfig>(_ =>
            {
                var config = new DefaultConfig();

                configuration?.Invoke(config);

                return config;
            });
        }
    }
}