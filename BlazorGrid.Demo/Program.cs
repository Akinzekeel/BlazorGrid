using BlazorGrid.Config.Styles;
using BlazorGrid.Demo.Interfaces;
using BlazorGrid.Demo.Providers;
using BlazorGrid.Demo.Services;
using BlazorGrid.Extensions;
using BlazorGrid.Providers;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace BlazorGrid.Demo
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("app");

            builder.Services.AddTransient(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

            builder.Services.AddBlazorGrid<DefaultHttpProvider>(o =>
            {
                o.Styles = new BootstrapStyles();
            });

            builder.Services.AddScoped(x =>
            {
                var http = x.GetService<HttpClient>();
                return new CustomProvider(http, "/data/employees.json");
            });

            builder.Services.AddSingleton<ITitleService, TitleService>();

            await builder.Build().RunAsync();
        }
    }
}
