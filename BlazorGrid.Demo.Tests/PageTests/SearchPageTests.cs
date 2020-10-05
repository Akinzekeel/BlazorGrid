using BlazorGrid.Abstractions;
using BlazorGrid.Abstractions.Filters;
using BlazorGrid.Components;
using BlazorGrid.Config;
using BlazorGrid.Config.Styles;
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
using System.Threading.Tasks;

namespace BlazorGrid.Tests.Demo
{
    [TestClass]
    public class SearchPageTests : ComponentTestFixture
    {
        public IRenderedComponent<Search> RenderPage()
        {
            var provider = new Mock<IGridProvider>();
            provider.Setup(x => x.GetAsync<Employee>(
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<string>(),
                It.IsAny<FilterDescriptor>()
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

            return RenderComponent<Search>();
        }

        [TestMethod]
        public void Search_Input_Triggers_Provider_Call_Delayed()
        {
            var page = RenderPage();
            var input = page.Find("input[type=search]");
            var provider = Services.GetRequiredService<Mock<IGridProvider>>();

            provider.Verify((Expression<Func<IGridProvider, Task<BlazorGridResult<Employee>>>>)provider.Setups.First().OriginalExpression, Times.Once());
            Assert.AreEqual(1, provider.Invocations.Count);

            input.Input("test");

            Assert.AreEqual(1, provider.Invocations.Count);

            Task.Delay(BlazorGrid<Employee>.SearchQueryInputDebounceMs).Wait();

            Assert.AreEqual(2, provider.Invocations.Count);
        }
    }
}
