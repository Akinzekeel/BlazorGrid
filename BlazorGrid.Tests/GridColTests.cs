using BlazorGrid.Abstractions;
using BlazorGrid.Abstractions.Filters;
using BlazorGrid.Components;
using BlazorGrid.Interfaces;
using BlazorGrid.Tests.Mock;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;

namespace BlazorGrid.Tests
{
    [TestClass]
    public class GridColTests : ComponentTestFixture
    {
        class MyDto
        {
            public string Name { get; set; }

            [Display(Name = "My caption")]
            public string NameWithCaption { get; set; }

            [Display(Name = nameof(Resources.MyDtoCaption), ResourceType = typeof(Resources))]
            public string NameWithResourceCaption { get; set; }
        }

        private Mock<IGridProvider> Initialize(MyDto row)
        {
            var provider = new Mock<IGridProvider>();
            provider.Setup(x => x.GetAsync<MyDto>(
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<string>(),
                It.IsAny<FilterDescriptor>()
            )).ReturnsAsync(new BlazorGridResult<MyDto>
            {
                TotalCount = 1,
                Data = new List<MyDto> { row }
            }).Verifiable();

            Services.AddSingleton(provider.Object);
            Services.AddSingleton<IBlazorGridConfig>(new Config.DefaultConfig());
            Services.AddSingleton<NavigationManager>(new MockNav());

            return provider;
        }

        [TestMethod]
        public void Does_Register_With_Parent()
        {
            var fakeGrid = new Mock<IBlazorGrid>();

            var unit = RenderComponent<GridCol<string>>(
                CascadingValue(fakeGrid.Object),
                Parameter("Caption", "Name")
            );

            fakeGrid.Verify(x => x.Add(It.Is<IGridCol>(col => ReferenceEquals(col, unit.Instance))), Times.Once());
        }

        [TestMethod]
        public void Can_Merge_Css_Classes()
        {
            var row = new MyDto { Name = "Unit test" };
            var provider = Initialize(row);

            Expression<Func<string>> colFor = () => row.Name;

            var grid = RenderComponent<BlazorGrid<MyDto>>(
                Template<MyDto>(nameof(ChildContent), (context) => (RenderTreeBuilder builder) =>
                {
                    builder.OpenComponent<GridCol<string>>(0);
                    builder.AddAttribute(1, nameof(GridCol<string>.For), colFor);
                    builder.AddAttribute(2, "class", "my-custom-class");
                    builder.AddAttribute(3, nameof(GridCol<string>.AlignRight), true);
                    builder.CloseComponent();
                })
            );

            provider.VerifyAll();

            var rowElement = grid.FindAll(".grid-row").Last();
            rowElement.MarkupMatches("<div class=\"grid-row\"><div class=\"text-right my-custom-class\">Unit test</div></div>");
        }

        [TestMethod]
        public void Can_Set_Custom_Attributes()
        {
            var row = new MyDto { Name = "Unit test" };
            var provider = Initialize(row);

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

            provider.VerifyAll();

            var rowElement = grid.FindAll(".grid-row").Last();
            rowElement.MarkupMatches("<div class=\"grid-row\"><div title=\"Hello world\">Unit test</div></div>");
        }

        [TestMethod]
        public void Header_Ignores_Custom_Attributes()
        {
            var row = new MyDto { Name = "Unit test" };
            var provider = Initialize(row);

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

            provider.VerifyAll();

            var rowElement = grid.FindAll(".grid-row").First();
            rowElement.MarkupMatches("<header class=\"grid-row grid-header\"><div class=\"sortable\"><span class=\"blazor-grid-sort-icon\"></span></div></header>");
        }

        [TestMethod]
        public void Can_Set_Custom_Caption()
        {
            var row = new MyDto { Name = "Unit test" };
            var provider = Initialize(row);

            Expression<Func<string>> colFor = () => row.Name;

            var grid = RenderComponent<BlazorGrid<MyDto>>(
                Template<MyDto>(nameof(ChildContent), (context) => (RenderTreeBuilder builder) =>
                {
                    builder.OpenComponent<GridCol<string>>(0);
                    builder.AddAttribute(1, nameof(GridCol<string>.For), colFor);
                    builder.AddAttribute(2, nameof(GridCol<string>.Caption), "Hello world");
                    builder.CloseComponent();
                })
            );

            provider.VerifyAll();

            var rowElement = grid.FindAll(".grid-row").First();
            rowElement.MarkupMatches("<header class=\"grid-row grid-header\"><div class=\"sortable\">Hello world<span class=\"blazor-grid-sort-icon\"></span></div></header>");
        }

        [TestMethod]
        public void Custom_Caption_Override_Display_Name()
        {
            var row = new MyDto { Name = "Unit test" };
            var provider = Initialize(row);

            Expression<Func<string>> colFor = () => row.NameWithCaption;

            var grid = RenderComponent<BlazorGrid<MyDto>>(
                Template<MyDto>(nameof(ChildContent), (context) => (RenderTreeBuilder builder) =>
                {
                    builder.OpenComponent<GridCol<string>>(0);
                    builder.AddAttribute(1, nameof(GridCol<string>.For), colFor);
                    builder.AddAttribute(2, nameof(GridCol<string>.Caption), "Hello world");
                    builder.CloseComponent();
                })
            );

            provider.VerifyAll();

            var rowElement = grid.FindAll(".grid-row").First();
            rowElement.MarkupMatches("<header class=\"grid-row grid-header\"><div class=\"sortable\">Hello world<span class=\"blazor-grid-sort-icon\"></span></div></header>");
        }

        [TestMethod]
        public void Uses_Display_Name_As_Caption()
        {
            var row = new MyDto { Name = "Unit test" };
            var provider = Initialize(row);

            Expression<Func<string>> colFor = () => row.NameWithCaption;

            var grid = RenderComponent<BlazorGrid<MyDto>>(
                Template<MyDto>(nameof(ChildContent), (context) => (RenderTreeBuilder builder) =>
                {
                    builder.OpenComponent<GridCol<string>>(0);
                    builder.AddAttribute(1, nameof(GridCol<string>.For), colFor);
                    builder.CloseComponent();
                })
            );

            provider.VerifyAll();

            var rowElement = grid.FindAll(".grid-row").First();
            rowElement.MarkupMatches("<header class=\"grid-row grid-header\"><div class=\"sortable\">My caption<span class=\"blazor-grid-sort-icon\"></span></div></header>");
        }

        [DataTestMethod]
        [DataRow("de-de", "Meine Überschrift")]
        [DataRow("en-us", "My caption")]
        public void Uses_Display_Name_As_Caption_With_Resources(string locale, string expectedCaption)
        {
            var fakeGrid = new Mock<IBlazorGrid>();
            var m = new MyDto();

            var culture = CultureInfo.GetCultureInfo(locale);
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;

            var col = RenderComponent<GridCol<string>>(
                CascadingValue(fakeGrid.Object),
                Parameter(nameof(GridCol<string>.For), (Expression<Func<string>>)(() => m.NameWithResourceCaption))
            );

            var caption = col.Instance.GetCaptionOrDefault();
            Assert.AreEqual(expectedCaption, caption);
        }

        [TestMethod]
        public void Can_Hide_Column()
        {
            var row = new MyDto { Name = "Unit test" };
            var provider = Initialize(row);

            Expression<Func<string>> colFor = () => row.NameWithCaption;

            var grid = RenderComponent<BlazorGrid<MyDto>>(
                Template<MyDto>(nameof(ChildContent), (context) => (RenderTreeBuilder builder) =>
                {
                    builder.OpenComponent<GridCol<string>>(0);
                    builder.AddAttribute(1, nameof(GridCol<string>.For), colFor);
                    builder.CloseComponent();
                })
            );

            provider.VerifyAll();

            var rowElement = grid.FindAll(".grid-row").First();
            rowElement.MarkupMatches("<header class=\"grid-row grid-header\"><div class=\"sortable\">My caption<span class=\"blazor-grid-sort-icon\"></span></div></header>");

            grid.SetParametersAndRender(
                Template<MyDto>(nameof(ChildContent), (context) => (RenderTreeBuilder b) =>
                {

                })
            );

            rowElement = grid.FindAll(".grid-row").First();
            rowElement.MarkupMatches("<header class=\"grid-row grid-header\"></header>");

            rowElement = grid.FindAll(".grid-row").Last();
            rowElement.MarkupMatches("<div class=\"grid-row\"></div>");
        }
    }
}