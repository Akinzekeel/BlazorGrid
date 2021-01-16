using BlazorGrid.Abstractions;
using BlazorGrid.Components;
using BlazorGrid.Config;
using BlazorGrid.Config.Styles;
using BlazorGrid.Demo.Interfaces;
using BlazorGrid.Demo.Models;
using BlazorGrid.Demo.Pages.Examples;
using BlazorGrid.Demo.Tests.Mock;
using BlazorGrid.Interfaces;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace BlazorGrid.Tests.Demo
{
    [TestClass]
    public class SearchPageTests : Bunit.TestContext
    {
        [TestInitialize]
        public void Initialize()
        {
            var ts = new Mock<ITitleService>();
            Services.AddSingleton(ts.Object);

            var provider = new Mock<ICustomProvider>();

            provider.Setup(x => x.GetAsync<Employee>(
                It.IsAny<BlazorGridRequest>(),
                It.IsAny<CancellationToken>()
            )).ReturnsAsync(new BlazorGridResult<Employee>
            {
                TotalCount = 50,
                Data = Enumerable.Repeat(new Employee(), 50).ToList()
            });

            Services.AddSingleton(provider);
            Services.AddSingleton(provider.Object);

            var nav = new MockNav();
            Services.AddSingleton<NavigationManager>(nav);

            Services.AddSingleton<IBlazorGridConfig>(_ => new DefaultConfig { Styles = new SpectreStyles() });
        }

        [TestMethod]
        public async Task Search_Input_Triggers_Provider_Call_Delayed()
        {
            var provider = Services.GetRequiredService<Mock<ICustomProvider>>();

            provider.Setup(x => x.GetAsync<Employee>(
                It.IsAny<BlazorGridRequest>(),
                It.IsAny<CancellationToken>()
            )).ReturnsAsync(new BlazorGridResult<Employee>
            {
                TotalCount = 50,
                Data = Enumerable.Repeat(new Employee(), 50).ToList()
            });

            var page = RenderComponent<Search>();
            var input = page.Find("input[type=search]");

            provider.Verify((Expression<Func<ICustomProvider, ValueTask<BlazorGridResult<Employee>>>>)provider.Setups.First().OriginalExpression, Times.Once());
            Assert.AreEqual(1, provider.Invocations.Count);

            await page.InvokeAsync(() => input.Input("test"));

            Assert.AreEqual(1, provider.Invocations.Count);

            var conf = Services.GetRequiredService<IBlazorGridConfig>();
            await Task.Delay(conf.SearchQueryInputDebounceMs + 200);

            Assert.AreEqual(2, provider.Invocations.Count);
        }
    }
}
