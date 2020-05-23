using System.Linq;
using BlazorGrid.Components;
using BlazorGrid.Interfaces;
using Bunit;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace BlazorGrid.Tests
{
    [TestClass]
    public class GridColTests : ComponentTestFixture
    {
        class MyDto
        {
            public string Name { get; set; }
        }

        [TestMethod]
        public void Does_Register_With_Parent()
        {
            var fakeGrid = new Mock<IBlazorGrid>();

            var unit = RenderComponent<GridCol>(
                CascadingValue(fakeGrid.Object),
                Parameter("Caption", "Name")
            );

            fakeGrid.Verify(x => x.Add(It.Is<IGridCol>(col => ReferenceEquals(col, unit.Instance))), Times.Once());
        }

        [TestMethod]
        public void Can_Merge_Css_Classes()
        {
            var fakeGrid = new Mock<IBlazorGrid>();

            var unit = RenderComponent<GridCol>(
                CascadingValue(fakeGrid.Object),
                Parameter("class", "my-custom-class"),
                Parameter(nameof(GridCol.AlignRight), true)
            );

            unit.MarkupMatches("<div class=\"text-right my-custom-class\"></div>");
        }

        [TestMethod]
        public void Can_Set_Custom_Attributes()
        {
            var unit = RenderComponent<GridCol>(
                Parameter("title", "Unit test"),
                ChildContent("Hello world")
            );

            unit.MarkupMatches("<div class=\"\" title=\"Unit test\">Hello world</div>");
        }

        [TestMethod]
        public void Header_Ignores_Custom_Attributes()
        {
            var fakeGrid = new Mock<IBlazorGrid>();

            var unit = RenderComponent<GridCol>(
                CascadingValue(fakeGrid.Object),
                Parameter("title", "Unit test")
            );

            unit.MarkupMatches("<div class=\"\"></div>");
        }
    }
}