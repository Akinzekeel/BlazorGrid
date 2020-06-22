using BlazorGrid.Abstractions;
using BlazorGrid.Components;
using BlazorGrid.Tests.Mock;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace BlazorGrid.Tests
{
    [TestClass]
    public class MarkupTests : ComponentTestFixture
    {
        class MyDto
        {
            public string Name { get; set; }
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
        public void Can_Merge_ClassNames()
        {
            var noData = new List<MyDto>();

            var grid = RenderComponent<BlazorGrid<MyDto>>(
                Parameter("class", "my-custom-class"),
                Parameter(nameof(BlazorGrid<MyDto>.Rows), noData),
                Template<MyDto>(nameof(ChildContent), (dto) => (b) =>
                {

                })
            );

            var outer = grid.Find("*");
            Assert.AreEqual("blazor-grid my-custom-class", outer.ClassName);
        }

        [TestMethod]
        public void Can_Have_Custom_Attributes()
        {
            var noData = new List<MyDto>();

            var grid = RenderComponent<BlazorGrid<MyDto>>(
                Parameter("data-custom", "my-custom-value"),
                Parameter(nameof(BlazorGrid<MyDto>.Rows), noData),
                Template<MyDto>(nameof(ChildContent), (dto) => (b) =>
                {

                })
            );

            var outer = grid.Find("*");
            Assert.IsTrue(outer.HasAttribute("data-custom"));
            Assert.AreEqual("my-custom-value", outer.GetAttribute("data-custom"));
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
                    b.OpenComponent(0, typeof(GridCol<string>));
                    b.AddMarkupContent(1, "<span></span>");
                    b.CloseComponent();

                    b.OpenComponent(2, typeof(GridCol<string>));
                    b.AddAttribute(3, nameof(GridCol<string>.FitToContent), true);
                    b.AddMarkupContent(4, "<span></span>");
                    b.CloseComponent();
                })
            );

            var scroller = grid.Find("*").FirstElementChild;
            var style = scroller.GetAttribute("style");

            Assert.AreEqual("grid-template-columns: auto max-content", style);
        }

        [TestMethod]
        public void Sortable_Column_Header_Has_Class()
        {
            var noData = new List<MyDto>();

            var grid = RenderComponent<BlazorGrid<MyDto>>(
                Parameter(nameof(BlazorGrid<MyDto>.Rows), noData),
                Template<MyDto>(nameof(ChildContent), (dto) => (b) =>
                {
                    b.OpenComponent<GridCol<string>>(0);
                    b.AddAttribute(1, nameof(GridCol<string>.For), (Expression<Func<string>>)(() => dto.Name));
                    b.CloseComponent();
                })
            );

            var th = grid.Find(".grid-header > *");
            Assert.AreEqual("sortable", th.ClassName);
        }

        [TestMethod]
        public void Non_Sortable_Column_Header_Has_Class()
        {
            var noData = new List<MyDto>();

            var grid = RenderComponent<BlazorGrid<MyDto>>(
                Parameter(nameof(BlazorGrid<MyDto>.Rows), noData),
                Template<MyDto>(nameof(ChildContent), (dto) => (b) =>
                {
                    b.OpenComponent<GridCol<string>>(0);
                    b.CloseComponent();
                })
            );

            var th = grid.Find(".grid-header > *");
            Assert.AreEqual("sortable", th.ClassName);
        }
    }
}