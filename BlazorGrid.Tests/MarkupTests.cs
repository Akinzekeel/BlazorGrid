using AngleSharp.Css.Dom;
using AngleSharp.Dom;
using BlazorGrid.Abstractions;
using BlazorGrid.Abstractions.Filters;
using BlazorGrid.Components;
using BlazorGrid.Config;
using BlazorGrid.Config.Styles;
using BlazorGrid.Interfaces;
using BlazorGrid.Tests.Mock;
using Bunit;
using Bunit.TestDoubles;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using static Bunit.ComponentParameterFactory;

namespace BlazorGrid.Tests
{
    [TestClass]
    public class MarkupTests : Bunit.TestContext
    {
        class MyDto
        {
            public string Name { get; set; }
        }

        [TestInitialize]
        public void Initialize()
        {
            var row = new MyDto { Name = "Unit Test" };

            var provider = new Mock<IGridProvider>();

            provider.Setup(x => x.GetAsync<MyDto>(
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<string>(),
                It.IsAny<FilterDescriptor>(),
                It.IsAny<CancellationToken>()
            )).ReturnsAsync(new BlazorGridResult<MyDto>
            {
                TotalCount = 1,
                Data = new List<MyDto> { row }
            }).Verifiable();

            Services.AddSingleton(provider);
            Services.AddSingleton(provider.Object);
            Services.AddSingleton<IBlazorGridConfig>(new DefaultConfig { Styles = new SpectreStyles() });
            Services.AddSingleton<NavigationManager>(new MockNav());

            Services.AddMockJSRuntime();
        }

        private static void VerifyColumnCount(IRenderedComponent<BlazorGrid<MyDto>> grid, int expectedColumnCount)
        {
            Assert.AreEqual(expectedColumnCount, grid.Instance.Columns.Count());

            // Verify number of column headers
            var header = grid.Find(".grid-row.grid-header");
            Assert.AreEqual(expectedColumnCount, header.Children.Count());

            // Verify number of cells per row
            var row = grid.Find(".grid-header + .grid-row");
            Assert.AreEqual(expectedColumnCount, row.Children.Count());

            // Verify colspan of the footer
            var colspan = grid.Find(".grid-header + .grid-row + div");
            var columnStyle = colspan.GetStyle().First(x => x.Name == "grid-column-start");
            Assert.AreEqual("span " + expectedColumnCount, columnStyle.Value);

            // Verify the number of column widths
            var scroller = grid.Find(".grid-scrollview");
            var scrollerStyle = scroller.GetStyle().First(x => x.Name == "grid-template-columns");
            var colSizes = scrollerStyle.Value.Trim().Split(' ');
            Assert.AreEqual(expectedColumnCount, colSizes.Count());
        }

        [TestMethod]
        public void Has_Outer_Classes()
        {
            var grid = RenderComponent<BlazorGrid<MyDto>>(
                Template<MyDto>(nameof(ChildContent), (dto) => (b) =>
                {
                    Expression<Func<string>> f = () => dto.Name;

                    b.OpenComponent<GridCol<string>>(0);
                    b.AddAttribute(1, nameof(GridCol<string>.For), f);
                    b.CloseComponent();
                })
            );

            var outer = grid.Find("*");
            Assert.AreEqual("blazor-grid", outer.ClassName);
        }

        [TestMethod]
        public void Can_Merge_ClassNames()
        {
            var grid = RenderComponent<BlazorGrid<MyDto>>(
                Parameter("class", "my-custom-class"),
                Template<MyDto>(nameof(ChildContent), (dto) => (b) =>
                {

                    Expression<Func<string>> f = () => dto.Name;

                    b.OpenComponent<GridCol<string>>(0);
                    b.AddAttribute(1, nameof(GridCol<string>.For), f);
                    b.CloseComponent();
                })
            );

            var outer = grid.Find("*");
            Assert.AreEqual("blazor-grid my-custom-class", outer.ClassName);
        }

        [TestMethod]
        public void Can_Have_Custom_Attributes()
        {
            var grid = RenderComponent<BlazorGrid<MyDto>>(
                Parameter("data-custom", "my-custom-value"),
                Template<MyDto>(nameof(ChildContent), (dto) => (b) =>
                {
                    Expression<Func<string>> f = () => dto.Name;

                    b.OpenComponent<GridCol<string>>(0);
                    b.AddAttribute(1, nameof(GridCol<string>.For), f);
                    b.CloseComponent();
                })
            );

            var outer = grid.Find("*");
            Assert.IsTrue(outer.HasAttribute("data-custom"));
            Assert.AreEqual("my-custom-value", outer.GetAttribute("data-custom"));
        }

        [TestMethod]
        public void Has_Inner_Classes()
        {
            var grid = RenderComponent<BlazorGrid<MyDto>>(
                Template<MyDto>(nameof(ChildContent), (dto) => (b) =>
                {
                    Expression<Func<string>> f = () => dto.Name;

                    b.OpenComponent<GridCol<string>>(0);
                    b.AddAttribute(1, nameof(GridCol<string>.For), f);
                    b.CloseComponent();
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

        [TestMethod]
        public void Can_Modify_Columns()
        {
            var row = new MyDto();
            Expression<Func<string>> colFor = () => row.Name;

            var grid = RenderComponent<BlazorGrid<MyDto>>(
                Template<MyDto>(nameof(ChildContent), (context) => (RenderTreeBuilder builder) =>
                {
                    builder.OpenComponent<GridCol<string>>(0);
                    builder.AddAttribute(1, nameof(GridCol<string>.For), colFor);
                    builder.AddAttribute(2, "Caption", "Name");
                    builder.CloseComponent();

                    builder.OpenComponent<GridCol<string>>(3);
                    builder.AddAttribute(4, nameof(GridCol<string>.For), colFor);
                    builder.AddAttribute(5, "Caption", "Also name");
                    builder.CloseComponent();
                })
            );

            // Two renders should have happened at this point
            Assert.AreEqual(2, grid.RenderCount);

            // Verify that two columns are rendered
            var rowElement = grid.FindAll(".grid-row").First();
            rowElement.MarkupMatches("<header class=\"grid-row grid-header\">" +
                "<div class=\"sortable\">Name<span class=\"blazor-grid-sort-icon\"></span></div>" +
                "<div class=\"sortable\">Also name<span class=\"blazor-grid-sort-icon\"></span></div>" +
                "</header>"
            );

            VerifyColumnCount(grid, 2);

            // Change the ChildContent to only contain one column
            grid.SetParametersAndRender(
                Template<MyDto>(nameof(ChildContent), (context) => (RenderTreeBuilder builder) =>
                {
                    builder.OpenComponent<GridCol<string>>(0);
                    builder.AddAttribute(1, nameof(GridCol<string>.For), colFor);
                    builder.AddAttribute(2, "Caption", "Name");
                    builder.CloseComponent();
                })
            );

            // Another render should have happened
            Assert.AreEqual(4, grid.RenderCount);

            // Verify that only one column is rendered
            rowElement = grid.FindAll(".grid-row").First();
            rowElement.MarkupMatches("<header class=\"grid-row grid-header\">" +
                "<div class=\"sortable\">Name<span class=\"blazor-grid-sort-icon\"></span></div>" +
                "</header>"
            );

            VerifyColumnCount(grid, 1);

            // Change it back to two columns
            grid.SetParametersAndRender(
                Template<MyDto>(nameof(ChildContent), (context) => (RenderTreeBuilder builder) =>
                {
                    builder.OpenComponent<GridCol<string>>(0);
                    builder.AddAttribute(1, nameof(GridCol<string>.For), colFor);
                    builder.AddAttribute(2, "Caption", "Name");
                    builder.CloseComponent();

                    builder.OpenComponent<GridCol<string>>(3);
                    builder.AddAttribute(4, nameof(GridCol<string>.For), colFor);
                    builder.AddAttribute(5, "Caption", "Also name");
                    builder.CloseComponent();
                })
            );

            // Another render should have happened
            Assert.AreEqual(6, grid.RenderCount);

            // Verify that two columns are rendered
            rowElement = grid.FindAll(".grid-row").First();
            rowElement.MarkupMatches("<header class=\"grid-row grid-header\">" +
                "<div class=\"sortable\">Name<span class=\"blazor-grid-sort-icon\"></span></div>" +
                "<div class=\"sortable\">Also name<span class=\"blazor-grid-sort-icon\"></span></div>" +
                "</header>"
            );

            VerifyColumnCount(grid, 2);
        }

        [TestMethod]
        public void Can_Set_Custom_Attributes()
        {
            var row = new MyDto { Name = "Unit test" };
            Expression<Func<string>> colFor = () => row.Name;

            var grid = RenderComponent<BlazorGrid<MyDto>>(
                Template<MyDto>(nameof(ChildContent), (context) => (RenderTreeBuilder builder) =>
                {
                    builder.OpenComponent<GridCol<string>>(0);
                    builder.AddAttribute(1, nameof(GridCol<string>.For), colFor);
                    builder.AddAttribute(2, "title", "Hello world");
                    builder.CloseComponent();
                })
            );

            var rowElement = grid.FindAll(".grid-row").Last();
            rowElement.MarkupMatches("<div class=\"grid-row\"><div title=\"Hello world\">Unit test</div></div>");
        }

        [TestMethod]
        public void Header_Ignores_Custom_Attributes()
        {
            var row = new MyDto { Name = "Unit test" };
            Expression<Func<string>> colFor = () => row.Name;

            var grid = RenderComponent<BlazorGrid<MyDto>>(
                Template<MyDto>(nameof(ChildContent), (context) => (RenderTreeBuilder builder) =>
                {
                    builder.OpenComponent<GridCol<string>>(0);
                    builder.AddAttribute(1, nameof(GridCol<string>.For), colFor);
                    builder.AddAttribute(2, "title", "Hello world");
                    builder.CloseComponent();
                })
            );

            var rowElement = grid.FindAll(".grid-row").First();
            rowElement.MarkupMatches("<header class=\"grid-row grid-header\"><div class=\"sortable\"><span class=\"blazor-grid-sort-icon\"></span></div></header>");
        }

        [TestMethod]
        public void No_Columns_Does_Not_Render_Anything()
        {
            var grid = RenderComponent<BlazorGrid<MyDto>>(
                Template<MyDto>(nameof(ChildContent), (context) => (RenderTreeBuilder b) =>
                {

                })
            );

            grid.MarkupMatches("");
        }

        [TestMethod]
        public void Sorting_Shows_Loading_State()
        {
            var provider = Services.GetRequiredService<Mock<IGridProvider>>();

            var grid = RenderComponent<BlazorGrid<MyDto>>(
                Template<MyDto>("ChildContent", context => (RenderTreeBuilder b) =>
                {
                    Expression<Func<string>> colFor = () => context.Name;
                    b.OpenComponent<GridCol<string>>(0);
                    b.AddAttribute(1, "For", colFor);
                    b.CloseComponent();
                })
            );

            var promise = new TaskCompletionSource<BlazorGridResult<MyDto>>();

            provider.Reset();
            provider.Setup(x => x.GetAsync<MyDto>(
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<string>(),
                It.IsAny<FilterDescriptor>(),
                It.IsAny<CancellationToken>()
            )).Returns(promise.Task);

            // Trigger sorting
            var th = grid.Find(".grid-row.grid-header > .sortable");
            th.Click();

            var gridStyle = Services.GetRequiredService<IBlazorGridConfig>();
            Assert.Fail();
            //var spinner = grid.Find("." + gridStyle.Styles.LoadingSpinnerOuterClass.Replace(' ', '.'));

            //Assert.IsNotNull(spinner);

            //promise.SetResult(new BlazorGridResult<MyDto>
            //{
            //    TotalCount = 1,
            //    Data = new List<MyDto> { new MyDto() }
            //});

            //Task.Delay(100).Wait();

            //var spinners = grid.FindAll("." + gridStyle.Styles.LoadingSpinnerOuterClass.Replace(' ', '.'));
            //Assert.AreEqual(0, spinners.Count);
        }
    }
}