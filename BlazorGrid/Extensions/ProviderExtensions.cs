using BlazorGrid.Abstractions;
using BlazorGrid.Providers;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace BlazorGrid.Extensions
{
    public static class ProviderExtensions
    {
        [Obsolete("The grid provider interface will be removed in a future release")]
        public static void UseDefaultHttpProvider(this IServiceCollection services)
        {
            services.AddTransient<IGridProvider, DefaultHttpProvider>();
        }
    }
}