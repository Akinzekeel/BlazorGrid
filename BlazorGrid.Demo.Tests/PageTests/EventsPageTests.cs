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
using System.Linq;
using System.Threading.Tasks;

namespace BlazorGrid.Tests.Demo
{
    [TestClass]
    public class EventsPageTests : Bunit.TestContext
    {
        private IRenderedComponent<Events> RenderPage()
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
                TotalCount = 100,
                Data = Enumerable.Repeat(new Employee(), 100).ToList()
            });

            Services.AddSingleton(provider.Object);

            var nav = new MockNav();
            Services.AddSingleton<NavigationManager>(nav);

            Services.AddSingleton<IBlazorGridConfig>(_ => new DefaultConfig { Styles = new SpectreStyles() });

            return RenderComponent<Events>();
        }

        [TestMethod]
        public void Can_Render_Page()
        {
            RenderPage();
        }

        [TestMethod]
        public void Row_Click_Does_Not_Cause_Rerender()
        {
            var page = RenderPage();
            var grid = page.FindComponent<BlazorGrid<Employee>>();

            Assert.AreEqual(2, grid.Instance.RenderCount);

            var row = grid.Find(".grid-header + .grid-row");
            row.Click();

            Task.Delay(100).Wait();

            Assert.AreEqual(2, grid.Instance.RenderCount);
        }
    }
}
