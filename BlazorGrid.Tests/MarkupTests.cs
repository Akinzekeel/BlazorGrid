using System.Collections.Generic;
using System.Linq;
using BlazorGrid.Abstractions;
using BlazorGrid.Components;
using BlazorGrid.Providers;
using BlazorGrid.Tests.Mock;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace BlazorGrid.Tests
{
    [TestClass]
    public class MarkupTests : ComponentTestFixture
    {
        class MyDto
        {

        }

        [TestInitialize]
        public void Initialize()
        {
            var provider = new Mock<IGridProvider>();
            Services.AddTransient(_ => provider.Object);
            Services.AddTransient<NavigationManager>(_ => new MockNav());
        }

        [TestMethod]
        public void Has_Outer_Classes()
        {
            var noData = new List<MyDto>();

            var grid = RenderComponent<BlazorGrid<MyDto>>(
                Parameter(nameof(BlazorGrid<MyDto>.Rows), noData),
                Template<MyDto>(nameof(ChildContent), (dto) => (b) =>
                {

                })
            );

            var outer = grid.Find("*");
            Assert.AreEqual("blazor-grid", outer.ClassName);
        }

        [TestMethod]
        public void Has_Inner_Classes()
        {
            var noData = new List<MyDto>();

            var grid = RenderComponent<BlazorGrid<MyDto>>(
                Parameter(nameof(BlazorGrid<MyDto>.Rows), noData),
                Template<MyDto>(nameof(ChildContent), (dto) => (b) =>
                {

                })
            );

            var scroller = grid.Find("*").FirstElementChild;
            Assert.AreEqual("grid-scrollview", scroller.ClassName);
        }

        [TestMethod]
        public void Has_Grid_Column_Style()
        {
            var noData = new List<MyDto>();

            var grid = RenderComponent<BlazorGrid<MyDto>>(
                Parameter(nameof(BlazorGrid<MyDto>.Rows), noData),
                Template<MyDto>(nameof(ChildContent), (dto) => (b) =>
                {
                    b.OpenComponent(0, typeof(GridCol));
                    b.AddMarkupContent(1, "<span></span>");
                    b.CloseComponent();

                    b.OpenComponent(2, typeof(GridCol));
                    b.AddAttribute(3, nameof(GridCol.FitToContent), true);
                    b.AddMarkupContent(4, "<span></span>");
                    b.CloseComponent();
                })
            );

            var scroller = grid.Find("*").FirstElementChild;
            var style = scroller.GetAttribute("style");

            Assert.AreEqual("grid-template-columns: auto max-content", style);
        }
    }
}