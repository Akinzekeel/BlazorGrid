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

            Services.AddMockJSRuntime();
        }

        private TaskCompletionSource<BlazorGridResult<MyDto>> SetupMockProvider()
        {
            var promise = new TaskCompletionSource<BlazorGridResult<MyDto>>();

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
            )).Returns(promise.Task);

            Services.AddSingleton(provider.Object);

            return promise;
        }

        [TestMethod]
        public void Does_Initial_Rendering()
        {
            var promise = SetupMockProvider();

            var grid = RenderComponent<BlazorGrid<MyDto>>(
                Template<MyDto>(nameof(ChildContent), (context) => (RenderTreeBuilder b) =>
                {
                    Expression<Func<string>> colFor = () => context.Name;

                    b.OpenComponent<GridCol<string>>(0);
                    b.AddAttribute(1, "For", colFor);
                    b.CloseComponent();
                })
            );

            // There should have been one initial render 
            // to process the columns and then a second 
            // one to actually render the grid itself
            Assert.AreEqual(2, grid.RenderCount);

            promise.SetResult(new BlazorGridResult<MyDto>
            {
                TotalCount = 1,
                Data = new List<MyDto> {
                    new MyDto { Name = "Unit test" }
                }
            });

            // After receiving a response from the provider,
            // the result must be rendered. This may take a
            // moment
            Task.Delay(150).Wait();
            Assert.AreEqual(3, grid.RenderCount);
        }

        [TestMethod]
        public async Task Sorting_Triggers_Rerender()
        {
            var promise = SetupMockProvider();

            promise.SetResult(new BlazorGridResult<MyDto>
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

            // There should have been one initial render 
            // to process the columns and then a second 
            // one to actually render the grid itself
            Assert.AreEqual(2, grid.RenderCount);

            // Now let's try changing the sorting
            var col = grid.FindComponent<GridCol<string>>();
            await col.InvokeAsync(() => grid.Instance.TryApplySortingAsync(col.Instance));

            Assert.AreEqual(4, grid.RenderCount);
        }

        [TestMethod]
        public void Query_Triggers_Rerender()
        {
            var promise = SetupMockProvider();

            promise.SetResult(new BlazorGridResult<MyDto>
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

            // There should have been one initial render 
            // to process the columns and then a second 
            // one to actually render the grid itself
            Assert.AreEqual(2, grid.RenderCount);

            // Now let's try changing the sorting
            var col = grid.FindComponent<GridCol<string>>();
            grid.SetParametersAndRender(
                Parameter(nameof(BlazorGrid<MyDto>.QueryUserInput), "Hello world")
            );

            // Since this property uses a debounce, there shouldn't be any render yet
            Assert.AreEqual(2, grid.RenderCount);

            // Wait for it...
            Task.Delay(500).Wait();

            Assert.AreNotEqual(2, grid.RenderCount);
        }

        [TestMethod]
        public void ChildContent_Triggers_Rerender()
        {
            var promise = SetupMockProvider();

            promise.SetResult(new BlazorGridResult<MyDto>
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

            // There should have been one initial render 
            // to process the columns and then a second 
            // one to actually render the grid itself
            Assert.AreEqual(2, grid.RenderCount);

            // Now let's try changing the sorting
            grid.SetParametersAndRender(
                Template<MyDto>(nameof(ChildContent), (context) => (RenderTreeBuilder b) =>
                {

                })
            );

            Assert.AreEqual(3, grid.RenderCount);
        }

        [TestMethod]
        public void OnClick_Does_Not_Trigger_Rerender()
        {
            var promise = SetupMockProvider();

            promise.SetResult(new BlazorGridResult<MyDto>
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

            // There should have been one initial render 
            // to process the columns and then a second 
            // one to actually render the grid itself
            Assert.AreEqual(2, grid.RenderCount);

            // Try clicking on a row
            var row = grid.Find(".grid-header + .grid-row");
            row.Click();

            Task.Delay(100).Wait();

            Assert.AreEqual(2, grid.RenderCount);
        }

        [TestMethod]
        public void Href_Does_Not_Trigger_Rerender()
        {
            var promise = SetupMockProvider();

            promise.SetResult(new BlazorGridResult<MyDto>
            {
                TotalCount = 1,
                Data = new List<MyDto> {
                    new MyDto { Name = "Unit test" }
                }
            });

            Func<MyDto, string> href = (MyDto _) => "/go-to/here";

            var grid = RenderComponent<BlazorGrid<MyDto>>(
                Parameter(nameof(BlazorGrid<MyDto>.Href), href),
                Template<MyDto>(nameof(ChildContent), (context) => (RenderTreeBuilder b) =>
                {
                    Expression<Func<string>> colFor = () => context.Name;

                    b.OpenComponent<GridCol<string>>(0);
                    b.AddAttribute(1, "For", colFor);
                    b.CloseComponent();
                })
            );

            var nav = Services.GetRequiredService<MockNav>();
            nav.LocationChanged += (object sender, LocationChangedEventArgs args)
                => grid.SetParametersAndRender(
                    Parameter(nameof(BlazorGrid<string>.QueryUserInput), grid.Instance.QueryUserInput)
                );

            // There should have been one initial render 
            // to process the columns and then a second 
            // one to actually render the grid itself
            Assert.AreEqual(2, grid.RenderCount);

            // Try clicking on a row
            var row = grid.Find(".grid-header + .grid-row");
            row.Click();

            Task.Delay(100).Wait();

            Assert.AreEqual(2, grid.RenderCount);
        }
    }
}
