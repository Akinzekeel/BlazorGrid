using BlazorGrid.Abstractions;
using BlazorGrid.Demo.Pages.Demos;
using BlazorGrid.Tests.Mock;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace BlazorGrid.Tests.Demo
{
    [TestClass]
    public class IndexPageTests : ComponentTestFixture
    {
        [TestMethod]
        public void Can_Render_Page()
        {
            RenderComponent<Index>();
        }
    }
}
