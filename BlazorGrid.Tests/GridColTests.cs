using BlazorGrid.Abstractions;
using BlazorGrid.Abstractions.Filters;
using BlazorGrid.Components;
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
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using static Bunit.ComponentParameterFactory;

namespace BlazorGrid.Tests
{
    [TestClass]
    public class GridColTests : Bunit.TestContext
    {
        [TestInitialize]
        public void Initialize()
        {
        }

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

            fakeGrid.Verify(x => x.Register(It.Is<IGridCol>(col => ReferenceEquals(col, unit.Instance))), Times.Once());
        }

        [TestMethod]
        public void Can_Merge_Css_Classes()
        {
            var row = new MyDto { Name = "Unit test" };
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

            Services.AddSingleton(provider.Object);
            Services.AddSingleton<IBlazorGridConfig>(new Config.DefaultConfig());
            Services.AddSingleton<NavigationManager>(new MockNav());

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
        public void Can_Set_Custom_Caption()
        {
            var row = new MyDto { };
            var parent = new Mock<BlazorGrid<MyDto>>();
            Expression<Func<string>> colFor = () => row.Name;

            var col = RenderComponent<GridCol<string>>(
                Parameter("For", colFor),
                Parameter("Caption", "Unit test"),
                CascadingValue("Parent", parent)
            );

            var caption = col.Instance.GetCaptionOrDefault();

            Assert.AreEqual("Unit test", caption);
        }

        [TestMethod]
        public void Custom_Caption_Override_Display_Name()
        {
            var row = new MyDto { };
            var parent = new Mock<BlazorGrid<MyDto>>();
            Expression<Func<string>> colFor = () => row.NameWithCaption;

            var col = RenderComponent<GridCol<string>>(
                Parameter("For", colFor),
                Parameter("Caption", "Unit test"),
                CascadingValue("Parent", parent)
            );

            var caption = col.Instance.GetCaptionOrDefault();

            Assert.AreEqual("Unit test", caption);
        }

        [TestMethod]
        public void Uses_Display_Name_As_Caption()
        {
            var row = new MyDto { };
            var parent = new Mock<BlazorGrid<MyDto>>();
            Expression<Func<string>> colFor = () => row.NameWithCaption;

            var col = RenderComponent<GridCol<string>>(
                Parameter("For", colFor),
                CascadingValue("Parent", parent)
            );

            var caption = col.Instance.GetCaptionOrDefault();

            Assert.AreEqual("My caption", caption);
        }

        [DataTestMethod]
        [DataRow("de-de", "Meine Überschrift")]
        [DataRow("en-us", "My caption")]
        public void Uses_Display_Name_As_Caption_With_Resources(string locale, string expectedCaption)
        {
            var row = new MyDto { };
            var parent = new Mock<BlazorGrid<MyDto>>();
            Expression<Func<string>> colFor = () => row.NameWithResourceCaption;

            var culture = CultureInfo.GetCultureInfo(locale);
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;

            var col = RenderComponent<GridCol<string>>(
                Parameter("For", colFor),
                CascadingValue("Parent", parent)
            );

            var caption = col.Instance.GetCaptionOrDefault();

            Assert.AreEqual(expectedCaption, caption);
        }
    }
}