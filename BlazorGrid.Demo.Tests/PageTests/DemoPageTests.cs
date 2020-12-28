using BlazorGrid.Abstractions;
using BlazorGrid.Abstractions.Filters;
using BlazorGrid.Config;
using BlazorGrid.Config.Styles;
using BlazorGrid.Demo.Interfaces;
using BlazorGrid.Demo.Models;
using BlazorGrid.Demo.Pages.Examples;
using BlazorGrid.Demo.Tests.Mock;
using BlazorGrid.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Linq;
using System.Threading;

namespace BlazorGrid.Tests.Demo
{
    [TestClass]
    public class ExamplesPageTests : Bunit.TestContext
    {
        [TestInitialize]
        public void Initialize()
        {
            var ts = new Mock<ITitleService>();
            Services.AddSingleton(ts.Object);
        }

        [TestMethod]
        public void Can_Render_Page()
        {
            var provider = new Mock<IGridProvider>();
            Services.AddSingleton(provider.Object);

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

            var nav = new MockNav();
            Services.AddSingleton<NavigationManager>(nav);

            Services.AddSingleton<IBlazorGridConfig>(_ => new DefaultConfig { Styles = new SpectreStyles() });

            RenderComponent<Basic>();
        }
    }
}
