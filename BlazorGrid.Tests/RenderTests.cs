using BlazorGrid.Abstractions;
using BlazorGrid.Abstractions.Filters;
using BlazorGrid.Components;
using BlazorGrid.Config;
using BlazorGrid.Interfaces;
using BlazorGrid.Tests.Mock;
using Bunit;
using Bunit.TestDoubles;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.Web.Virtualization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
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

            var provider = new Mock<IGridProvider>();
            Services.AddSingleton(provider);
            Services.AddSingleton(provider.Object);
        }

        [TestMethod]
        public void Does_Initial_Rendering()
        {
            var provider = Services.GetRequiredService<Mock<IGridProvider>>();

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
                Data = new List<MyDto> {
                    new MyDto { Name = "Unit test" }
                }
            });

            var grid = RenderComponent<BlazorGrid<MyDto>>(
                Template<MyDto>(nameof(ChildContent), (context) => (RenderTreeBuilder b) =>
                {
                    Expression<Func<string>> colFor = () => context.Name;

                    b.OpenComponent<GridCol<string>>(0);
                    b.AddAttribute(1, "For", colFor);
                    b.CloseComponent();
                })
            );

            Assert.AreEqual(1, grid.RenderCount);

            var virtualize = grid.FindComponent<Virtualize<MyDto>>();
            Assert.AreEqual(1, virtualize.RenderCount);
        }

        [TestMethod]
        public async Task Sorting_Triggers_Rerender()
        {
            var provider = Services.GetRequiredService<Mock<IGridProvider>>();

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
                Data = new List<MyDto> {
                    new MyDto { Name = "Unit test" }
                }
            });

            var grid = RenderComponent<BlazorGrid<MyDto>>(
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
            var provider = Services.GetRequiredService<Mock<IGridProvider>>();

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
                Data = new List<MyDto> {
                    new MyDto { Name = "Unit test" }
                }
            });

            var grid = RenderComponent<BlazorGrid<MyDto>>(
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
            var provider = Services.GetRequiredService<Mock<IGridProvider>>();

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
                Data = new List<MyDto> {
                    new MyDto { Name = "Unit test" }
                }
            });

            var grid = RenderComponent<BlazorGrid<MyDto>>(
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
            var provider = Services.GetRequiredService<Mock<IGridProvider>>();

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
                Data = new List<MyDto> {
                    new MyDto { Name = "Unit test" }
                }
            });

            var clickCount = 0;
            var grid = RenderComponent<BlazorGrid<MyDto>>(
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
            var provider = Services.GetRequiredService<Mock<IGridProvider>>();
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
            )).ReturnsAsync(new BlazorGridResult<MyDto>
            {
                TotalCount = 0,
                Data = new List<MyDto> { }
            }).Verifiable();

            var grid = RenderComponent<BlazorGrid<MyDto>>(
                Template<MyDto>(nameof(ChildContent), (context) => (RenderTreeBuilder b) =>
                {
                    Expression<Func<string>> colFor = () => context.Name;

                    b.OpenComponent<GridCol<string>>(0);
                    b.AddAttribute(1, "For", colFor);
                    b.CloseComponent();
                })
            );

            provider.Verify();

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
    }
}
