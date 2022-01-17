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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BlazorGrid.Tests.Demo
{
    [TestClass]
    public class DynamicLayoutTests : Bunit.TestContext
    {
        [TestInitialize]
        public void Initialize()
        {
            var ts = new Mock<ITitleService>();
            Services.AddSingleton(ts.Object);
        }

        private IRenderedComponent<DynamicLayout> RenderPage()
        {
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

            return RenderComponent<DynamicLayout>();
        }

        private static void VerifyGridColumnCount<T>(IRenderedComponent<BlazorGrid<T>> grid, int expectedColumnCount)
            where T : class
        {
            grid.Instance.Columns.Should().HaveCount(expectedColumnCount);

            // Verify number of th's
            var headerCells = grid.FindAll(".grid-cell.grid-header-cell");
            headerCells.Should().HaveCount(expectedColumnCount);

            // Verify number of td's per row
            // (it must be a multiple of the expected column count)
            var cells = grid.FindAll(".grid-cell:not(.grid-header-cell):not(.grid-cell-row-anchor)");
            cells.Should().NotBeEmpty();

            var rem = cells.Count % expectedColumnCount;
            rem.Should().Be(0);
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
