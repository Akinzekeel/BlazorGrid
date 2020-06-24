using BlazorGrid.Components;
using BlazorGrid.Interfaces;
using Bunit;
using Castle.DynamicProxy.Generators.Emitters.SimpleAST;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
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
            var fakeGrid = new Mock<IBlazorGrid>();

            var unit = RenderComponent<GridCol<string>>(
                CascadingValue(fakeGrid.Object),
                Parameter("class", "my-custom-class"),
                Parameter(nameof(GridCol<string>.AlignRight), true)
            );

            unit.MarkupMatches("<div class=\"text-right sortable my-custom-class\"><span class=\"blazor-grid-sort-icon\">‹›</span></div>");
        }

        [TestMethod]
        public void Can_Set_Custom_Attributes()
        {
            var unit = RenderComponent<GridCol<string>>(
                Parameter("title", "Unit test"),
                ChildContent("Hello world")
            );

            unit.MarkupMatches("<div class=\"sortable\" title=\"Unit test\">Hello world</div>");
        }

        [TestMethod]
        public void Header_Ignores_Custom_Attributes()
        {
            var fakeGrid = new Mock<IBlazorGrid>();

            var unit = RenderComponent<GridCol<string>>(
                CascadingValue(fakeGrid.Object),
                Parameter("title", "Unit test")
            );

            unit.MarkupMatches("<div class=\"sortable\"><span class=\"blazor-grid-sort-icon\">‹›</span></div>");
        }

        [TestMethod]
        public void Can_Set_Custom_Caption()
        {
            var fakeGrid = new Mock<IBlazorGrid>();
            var m = new MyDto();

            var col = RenderComponent<GridCol<string>>(
                CascadingValue(fakeGrid.Object),
                Parameter(nameof(GridCol<string>.Caption), "Unit test"),
                Parameter(nameof(GridCol<string>.For), (Expression<Func<string>>)(() => m.Name))
            );

            col.MarkupMatches("<div class=\"sortable\">Unit test<span class=\"blazor-grid-sort-icon\">‹›</span></div>");
        }

        [TestMethod]
        public void Custom_Caption_Override_Display_Name()
        {
            var fakeGrid = new Mock<IBlazorGrid>();
            var m = new MyDto();

            var col = RenderComponent<GridCol<string>>(
                CascadingValue(fakeGrid.Object),
                Parameter(nameof(GridCol<string>.Caption), "Unit test"),
                Parameter(nameof(GridCol<string>.For), (Expression<Func<string>>)(() => m.NameWithCaption))
            );

            col.MarkupMatches("<div class=\"sortable\">Unit test<span class=\"blazor-grid-sort-icon\">‹›</span></div>");
        }

        [TestMethod]
        public void Uses_Display_Name_As_Caption()
        {
            var fakeGrid = new Mock<IBlazorGrid>();
            var m = new MyDto();

            var col = RenderComponent<GridCol<string>>(
                CascadingValue(fakeGrid.Object),
                Parameter(nameof(GridCol<string>.For), (Expression<Func<string>>)(() => m.NameWithCaption))
            );

            col.MarkupMatches("<div class=\"sortable\">My caption<span class=\"blazor-grid-sort-icon\">‹›</span></div>");

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

            col.MarkupMatches($"<div class=\"sortable\">{expectedCaption}<span class=\"blazor-grid-sort-icon\">‹›</span></div>");
        }
    }
}