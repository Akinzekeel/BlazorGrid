using BlazorGrid.Abstractions;
using BlazorGrid.Config;
using BlazorGrid.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace BlazorGrid.Extensions
{
    public static class ConfigExtensions
    {
        public static void AddBlazorGrid<TProvider>(this IServiceCollection services)
            where TProvider : class, IGridProvider
        {
            AddBlazorGrid<TProvider>(services, null);
        }

        public static void AddBlazorGrid<TProvider>(this IServiceCollection services, Action<IBlazorGridConfig> configuration)
            where TProvider : class, IGridProvider
        {
            services.AddTransient<IGridProvider, TProvider>();

            services.AddSingleton<IBlazorGridConfig>(_ =>
            {
                var config = new DefaultConfig();

                configuration?.Invoke(config);

                return config;
            });
        }
    }
}