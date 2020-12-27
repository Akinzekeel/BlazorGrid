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
using System.Threading;
using System.Threading.Tasks;

namespace BlazorGrid.Tests.Demo
{
    [TestClass]
    public class DynamicLayoutTests : Bunit.TestContext
    {
        private IRenderedComponent<DynamicLayout> RenderPage()
        {
            var provider = new Mock<IGridProvider>();
            provider.Setup(x => x.GetAsync<Employee>(
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<string>(),
                It.IsAny<FilterDescriptor>(),
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

            return RenderComponent<DynamicLayout>();
        }

        private void VerifyGridColumnCount<T>(IRenderedComponent<BlazorGrid<T>> grid, int expectedColumnCount)
            where T : class
        {
            Assert.AreEqual(expectedColumnCount, grid.Instance.Columns.Count());

            // Verify number of th's
            var header = grid.Find(".grid-row.grid-header");
            Assert.AreEqual(expectedColumnCount, header.Children.Count());

            // Verify number of td's per row
            var row = grid.Find(".grid-row:not(.grid-header)");
            Assert.AreEqual(expectedColumnCount, row.Children.Count());
        }

        [TestMethod]
        public void Can_Render_Page()
        {
            RenderPage();
        }

        [TestMethod]
        public void Does_Toggle_Btn_Work()
        {
            var page = RenderPage();

            Assert.IsTrue(page.Instance.ShowAvatars);

            var btn = page.Find(".col-3 .btn.btn-secondary");
            btn.Click();

            Assert.IsFalse(page.Instance.ShowAvatars);

            btn.Click();

            Assert.IsTrue(page.Instance.ShowAvatars);
        }

        [TestMethod]
        public async Task Is_Grid_Layout_Correct()
        {
            var page = RenderPage();
            var grid = page.FindComponent<BlazorGrid<Employee>>();

            // The grid should initially have 5 columns
            VerifyGridColumnCount(grid, 5);

            var toggleBtn = page.Find(".col-3 .btn.btn-secondary");
            await page.InvokeAsync(() => toggleBtn.Click());

            VerifyGridColumnCount(grid, 4);

            // Restore the original columns
            await page.InvokeAsync(() => toggleBtn.Click());

            VerifyGridColumnCount(grid, 5);
        }
    }
}
