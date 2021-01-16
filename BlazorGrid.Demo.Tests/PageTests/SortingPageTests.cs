using BlazorGrid.Abstractions;
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
    public class SortingPageTests : Bunit.TestContext
    {
        [TestInitialize]
        public void Initialize()
        {
            var provider = new Mock<ICustomProvider>();
            Services.AddSingleton(provider);
            Services.AddSingleton(provider.Object);

            var nav = new MockNav();
            Services.AddSingleton<NavigationManager>(nav);

            Services.AddSingleton<IBlazorGridConfig>(_ => new DefaultConfig { Styles = new SpectreStyles() });

            var ts = new Mock<ITitleService>();
            Services.AddSingleton(ts.Object);
        }

        [TestMethod]
        public void Initial_Sorting_Triggers_Single_Provider_Call()
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

            var page = RenderComponent<Sorting>();

            provider.Verify(x => x.GetAsync<Employee>(
                It.IsAny<BlazorGridRequest>(),
                It.IsAny<CancellationToken>()
            ));

            provider.VerifyNoOtherCalls();
        }
    }
}
