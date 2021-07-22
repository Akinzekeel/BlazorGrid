using BlazorGrid.Abstractions;
using BlazorGrid.Components;
using BlazorGrid.Config;
using BlazorGrid.Infrastructure;
using BlazorGrid.Interfaces;
using BlazorGrid.Tests.Mock;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web.Virtualization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using static Bunit.ComponentParameterFactory;

namespace BlazorGrid.Tests
{
    /// <summary>
    /// Tests regarding the rendering optimization and
    /// efficiency of the grid.
    /// </summary>
    [TestClass]
    public class RenderTests : Bunit.TestContext
    {
        class MyDto
        {
            public string Name { get; set; }
        }

        [TestInitialize]
        public void Initialize()
        {
            var mockNav = new MockNav();
            Services.AddSingleton(mockNav);
            Services.AddSingleton<NavigationManager>(mockNav);
            Services.AddSingleton<IBlazorGridConfig>(new DefaultConfig());
        }

        [TestMethod]
        public void Does_Initial_Rendering()
        {
            ProviderDelegate<MyDto> provider = (r, _) =>
            {
                return ValueTask.FromResult(new BlazorGridResult<MyDto>
                {
                    TotalCount = 1,
                    Data = new List<MyDto> {
                        new MyDto { Name = "Unit test" }
                    }
                });
            };

            var grid = RenderComponent<BlazorGrid<MyDto>>(
                Parameter(nameof(BlazorGrid<MyDto>.Provider), provider),
                Template<MyDto>(nameof(ChildContent), (context) => (RenderTreeBuilder b) =>
                {
                    Expression<Func<string>> colFor = () => context.Name;

                    b.OpenComponent<GridCol<string>>(0);
                    b.AddAttribute(1, "For", colFor);
                    b.CloseComponent();
                })
            );

            Assert.AreEqual(1, grid.RenderCount);

            var virtualize = grid.FindComponent<Virtualize<RowWrapper<MyDto>>>();
            Assert.AreEqual(1, virtualize.RenderCount);
        }

        [TestMethod]
        public async Task Sorting_Triggers_Rerender()
        {
            ProviderDelegate<MyDto> provider = (r, _) =>
            {
                return ValueTask.FromResult(new BlazorGridResult<MyDto>
                {
                    TotalCount = 1,
                    Data = new List<MyDto> {
                        new MyDto { Name = "Unit test" }
                    }
                });
            };

            var grid = RenderComponent<BlazorGrid<MyDto>>(
                Parameter(nameof(BlazorGrid<MyDto>.Provider), provider),
                Template<MyDto>(nameof(ChildContent), (context) => (RenderTreeBuilder b) =>
                {
                    Expression<Func<string>> colFor = () => context.Name;

                    b.OpenComponent<GridCol<string>>(0);
                    b.AddAttribute(1, "For", colFor);
                    b.CloseComponent();
                })
            );

            // Now let's try changing the sorting
            var headerCell = grid.Find(".grid-header .sortable");
            await grid.InvokeAsync(() => headerCell.Click());

            Assert.AreEqual(2, grid.RenderCount);
        }

        [TestMethod]
        public void Query_Triggers_Rerender()
        {
            ProviderDelegate<MyDto> provider = (r, _) =>
            {
                return ValueTask.FromResult(new BlazorGridResult<MyDto>
                {
                    TotalCount = 1,
                    Data = new List<MyDto> {
                        new MyDto { Name = "Unit test" }
                    }
                });
            };

            var grid = RenderComponent<BlazorGrid<MyDto>>(
                Parameter(nameof(BlazorGrid<MyDto>.Provider), provider),
                Template<MyDto>(nameof(ChildContent), (context) => (RenderTreeBuilder b) =>
                {
                    Expression<Func<string>> colFor = () => context.Name;

                    b.OpenComponent<GridCol<string>>(0);
                    b.AddAttribute(1, "For", colFor);
                    b.CloseComponent();
                })
            );

            // Now let's try changing the sorting
            var col = grid.FindComponent<GridCol<string>>();
            grid.SetParametersAndRender(
                Parameter(nameof(BlazorGrid<MyDto>.QueryUserInput), "Hello world")
            );

            // Since this property uses a debounce, there shouldn't be any render yet
            Assert.AreEqual(1, grid.RenderCount);

            // Wait for it...
            Task.Delay(500).Wait();

            Assert.AreNotEqual(1, grid.RenderCount);
        }

        [TestMethod]
        public void ChildContent_Triggers_Rerender()
        {
            ProviderDelegate<MyDto> provider = (r, _) =>
            {
                return ValueTask.FromResult(new BlazorGridResult<MyDto>
                {
                    TotalCount = 1,
                    Data = new List<MyDto> {
                        new MyDto { Name = "Unit test" }
                    }
                });
            };

            var grid = RenderComponent<BlazorGrid<MyDto>>(
                Parameter(nameof(BlazorGrid<MyDto>.Provider), provider),
                Template<MyDto>(nameof(ChildContent), (context) => (RenderTreeBuilder b) =>
                {
                    Expression<Func<string>> colFor = () => context.Name;

                    b.OpenComponent<GridCol<string>>(0);
                    b.AddAttribute(1, "For", colFor);
                    b.CloseComponent();
                })
            );

            // Now let's try changing the sorting
            grid.SetParametersAndRender(
                Template<MyDto>(nameof(ChildContent), (context) => (RenderTreeBuilder b) =>
                {

                })
            );

            Assert.AreEqual(2, grid.RenderCount);
        }

        [TestMethod]
        public async Task OnClick_Does_Not_Trigger_Rerender()
        {
            ProviderDelegate<MyDto> provider = (r, _) =>
            {
                return ValueTask.FromResult(new BlazorGridResult<MyDto>
                {
                    TotalCount = 1,
                    Data = new List<MyDto> {
                        new MyDto { Name = "Unit test" }
                    }
                });
            };

            var clickCount = 0;
            var grid = RenderComponent<BlazorGrid<MyDto>>(
                Parameter(nameof(BlazorGrid<MyDto>.Provider), provider),
                EventCallback<MyDto>(nameof(BlazorGrid<MyDto>.OnClick), _ => clickCount++),
                Template<MyDto>(nameof(ChildContent), (context) => (RenderTreeBuilder b) =>
                {
                    Expression<Func<string>> colFor = () => context.Name;

                    b.OpenComponent<GridCol<string>>(0);
                    b.AddAttribute(1, "For", colFor);
                    b.CloseComponent();
                })
            );

            // Try clicking on a row
            var row = grid.Find(".grid-row:not(.grid-header)");
            await grid.InvokeAsync(() => row.Click());

            Task.Delay(100).Wait();

            Assert.AreEqual(1, grid.RenderCount);
        }

        [TestMethod]
        public void No_Data_Shows_Empty_Message()
        {
            var grid = RenderComponent<BlazorGrid<MyDto>>(
                Template<MyDto>(nameof(ChildContent), (context) => (RenderTreeBuilder b) =>
                {
                    Expression<Func<string>> colFor = () => context.Name;

                    b.OpenComponent<GridCol<string>>(0);
                    b.AddAttribute(1, "For", colFor);
                    b.CloseComponent();
                })
            );

            var conf = Services.GetRequiredService<IBlazorGridConfig>();
            var messageContainer = grid.Find("." + string.Join('.', conf.Styles.PlaceholderWrapperClass.Split(' ')));

            Assert.IsNotNull(messageContainer);
            messageContainer.MarkupMatches(
                $"<div class=\"{conf.Styles.PlaceholderWrapperClass}\">" +
                    $"<h5 class=\"{conf.Styles.NoDataHeadingClass}\">{BlazorGrid.Resources.Empty_Title}</h5>" +
                    $"<p class=\"{conf.Styles.NoDataTextClass}\">{BlazorGrid.Resources.Empty_Text}</p>" +
                "</div>"
            );
        }

        [TestMethod]
        public async Task Retry_After_Error_Clears_Error()
        {
            int providerCallCount = 0;
            var styles = Services.GetRequiredService<IBlazorGridConfig>();

            ProviderDelegate<MyDto> provider = (r, _) =>
            {
                providerCallCount++;
                throw new Exception("unit test");
            };

            var grid = RenderComponent<BlazorGrid<MyDto>>(
                Parameter(nameof(BlazorGrid<MyDto>.Provider), provider),
                Template<MyDto>(nameof(ChildContent), (context) => (RenderTreeBuilder b) =>
                {
                    Expression<Func<string>> colFor = () => context.Name;

                    b.OpenComponent<GridCol<string>>(0);
                    b.AddAttribute(1, "For", colFor);
                    b.CloseComponent();
                })
            );

            Assert.AreEqual(1, providerCallCount);

            provider = (r, _) =>
            {
                providerCallCount++;

                return ValueTask.FromResult(new BlazorGridResult<MyDto>
                {
                    TotalCount = 1,
                    Data = new List<MyDto> { new MyDto { Name = "Mike" } }
                });
            };

            grid.SetParametersAndRender(
                Parameter(nameof(BlazorGrid<MyDto>.Provider), provider)
            );

            // Verify that there is an error overlay
            var errorHeading = grid.Find(".grid-overlay ." + styles.Styles.ErrorHeadingClass.Replace(' ', '.'));
            Assert.IsNotNull(errorHeading);

            // Find the retry button
            var retryBtn = grid.Find(".grid-overlay ." + styles.Styles.ErrorFooterBtnClass.Replace(' ', '.'));
            Assert.IsNotNull(retryBtn);

            await grid.InvokeAsync(() => retryBtn.Click());

            Assert.AreEqual(2, providerCallCount);

            try
            {
                grid.Find(".grid-overlay");
                Assert.Fail();
            }
            catch (ElementNotFoundException) { }
        }

        [TestMethod]
        public async Task OnClick_With_Highlighting_Adds_Row_Class()
        {
            ProviderDelegate<MyDto> provider = (r, _) =>
            {
                return ValueTask.FromResult(new BlazorGridResult<MyDto>
                {
                    TotalCount = 1,
                    Data = new List<MyDto> {
                        new MyDto { Name = "Unit test" }
                    }
                });
            };

            int clickCount = 0;
            var grid = RenderComponent<BlazorGrid<MyDto>>(
                Parameter(nameof(BlazorGrid<MyDto>.Provider), provider),
                Parameter(nameof(BlazorGrid<MyDto>.RowHighlighting), true),
                EventCallback<MyDto>(nameof(BlazorGrid<MyDto>.OnClick), _ => clickCount++),
                Template<MyDto>(nameof(ChildContent), (context) => (RenderTreeBuilder b) =>
                {
                    Expression<Func<string>> colFor = () => context.Name;

                    b.OpenComponent<GridCol<string>>(0);
                    b.AddAttribute(1, "For", colFor);
                    b.CloseComponent();
                })
            );

            // Try clicking on a row
            var row = grid.Find(".grid-row:not(.grid-header)");
            await grid.InvokeAsync(() => row.Click());

            Task.Delay(100).Wait();

            row = grid.Find(".grid-row:not(.grid-header)");
            Assert.IsTrue(row.Matches(".highlighted"), row.ToMarkup());
        }
    }
}
