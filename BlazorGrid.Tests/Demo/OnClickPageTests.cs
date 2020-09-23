using BlazorGrid.Abstractions;
using BlazorGrid.Config;
using BlazorGrid.Config.Styles;
using BlazorGrid.Demo.Pages.Demos;
using BlazorGrid.Interfaces;
using BlazorGrid.Tests.Mock;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace BlazorGrid.Tests.Demo
{
    [TestClass]
    public class OnClickPageTests : ComponentTestFixture
    {
        [TestMethod]
        public void Can_Render_Page()
        {
            var provider = new Mock<IGridProvider>();
            Services.AddSingleton(provider.Object);

            var nav = new MockNav();
            Services.AddSingleton<NavigationManager>(nav);

            Services.AddSingleton<IBlazorGridConfig>(_ => new DefaultConfig { Styles = new SpectreStyles() });

            RenderComponent<OnClick>();
        }
    }
}
