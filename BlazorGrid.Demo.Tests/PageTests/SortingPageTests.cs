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
using Bunit.TestDoubles.JSInterop;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Threading;

namespace BlazorGrid.Tests.Demo
{
    [TestClass]
    public class SortingPageTests : Bunit.TestContext
    {
        [TestInitialize]
        public void Initialize()
        {
            Services.AddMockJSRuntime();
        }

        public IRenderedComponent<Sorting> RenderPage()
        {
            var provider = new Mock<IGridProvider>();
            Services.AddSingleton(provider);
            Services.AddSingleton(provider.Object);

            var nav = new MockNav();
            Services.AddSingleton<NavigationManager>(nav);

            Services.AddSingleton<IBlazorGridConfig>(_ => new DefaultConfig { Styles = new SpectreStyles() });

            return RenderComponent<Sorting>();
        }

        [Ignore]
        [TestMethod]
        public void Initial_Sorting_Triggers_Single_Provider_Call()
        {
            var page = RenderPage();
            var provider = Services.GetRequiredService<Mock<IGridProvider>>();

            provider.Verify(x => x.GetAsync<Employee>(
                It.IsAny<string>(),
                0,
                BlazorGrid<Employee>.DefaultPageSize,
                It.Is<string>(s => !string.IsNullOrEmpty(s)),
                false,
                null,
                It.IsAny<FilterDescriptor>(),
                It.IsAny<CancellationToken>()
            ));

            provider.VerifyNoOtherCalls();
        }
    }
}
