using BlazorGrid.Abstractions;
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
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
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
            var fakeRegister = new Mock<IColumnRegister>();

            var unit = RenderComponent<GridCol<string>>(
                CascadingValue(fakeRegister.Object),
                Parameter("Caption", "Name")
            );

            fakeRegister.Verify(x => x.Register(It.Is<IGridCol>(col => ReferenceEquals(col, unit.Instance))), Times.Once());
        }

        [TestMethod]
        public void Can_Merge_Css_Classes()
        {
            int providerCallCount = 0;

            ProviderDelegate<MyDto> provider = (BlazorGridRequest r, CancellationToken c) =>
            {
                providerCallCount++;

                return ValueTask.FromResult(new BlazorGridResult<MyDto>
                {
                    Data = Enumerable.Repeat(new MyDto(), 3).ToList(),
                    TotalCount = 3
                });
            };

            Services.AddSingleton<IBlazorGridConfig>(new Config.DefaultConfig());
            Services.AddSingleton<NavigationManager>(new MockNav());

            var row = new MyDto { Name = "Unit test" };
            Expression<Func<string>> colFor = () => row.Name;

            var grid = RenderComponent<BlazorGrid<MyDto>>(
                Parameter(nameof(BlazorGrid<MyDto>.Provider), provider),
                Template<MyDto>(nameof(ChildContent), (context) => (RenderTreeBuilder builder) =>
                {
                    builder.OpenComponent<GridCol<string>>(0);
                    builder.AddAttribute(1, nameof(GridCol<string>.For), colFor);
                    builder.AddAttribute(2, "class", "my-custom-class");
                    builder.AddAttribute(3, nameof(GridCol<string>.AlignRight), true);
                    builder.CloseComponent();
                })
            );

            providerCallCount.Should().Be(1);

            var rowElement = grid.FindAll(".grid-cell")
                .Where(x => !x.ClassList.Contains("grid-cell-row-anchor"))
                .Last();

            rowElement.MarkupMatches("<div class=\"grid-cell text-right my-custom-class\">Unit test</div>");
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