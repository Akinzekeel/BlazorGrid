using BlazorGrid.Demo.Pages.Demos;
using Bunit;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
