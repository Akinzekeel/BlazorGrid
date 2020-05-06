using BlazorGrid.Interfaces;
using BlazorGrid.Providers;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorGrid.Extensions
{
    public static class ProviderExtensions
    {
        public static void UseDefaultHttpProvider(this IServiceCollection services)
        {
            services.AddTransient<IDataProvider, DefaultHttpProvider>();
        }
    }
}