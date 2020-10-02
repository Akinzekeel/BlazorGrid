using BlazorGrid.Abstractions;
using BlazorGrid.Abstractions.Filters;
using BlazorGrid.Components;
using BlazorGrid.Config;
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
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace BlazorGrid.Tests
{
    /// <summary>
    /// Tests regarding the rendering optimization and
    /// efficiency of the grid.
    /// </summary>
    [TestClass]
    public class RenderTests : ComponentTestFixture
    {
        class MyDto
        {
            public string Name { get; set; }
        }

        [TestInitialize]
        public void Initialize()
        {
            Services.AddSingleton<NavigationManager>(new MockNav());
            Services.AddSingleton<IBlazorGridConfig>(new DefaultConfig());
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
                It.IsAny<FilterDescriptor>()
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
            Assert.AreEqual(2, grid.Instance.RenderCount);

            Task.Delay(150).Wait();

            // If loading takes more than 100ms, the grid
            // should render a loading state
            Assert.AreEqual(3, grid.Instance.RenderCount);

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
            Assert.AreEqual(4, grid.Instance.RenderCount);
        }

        [TestMethod]
        public void Sorting_Triggers_Rerender()
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
            Assert.AreEqual(2, grid.Instance.RenderCount);

            // Now let's try changing the sorting
            var col = grid.FindComponent<GridCol<string>>();
            grid.Instance.TryApplySorting(col.Instance);

            // Wait a moment to ensure rendering has happened
            Task.Delay(150).Wait();
            Assert.AreEqual(4, grid.Instance.RenderCount);
        }

        [TestMethod]
        public void Query_Triggers_Rerender()
        {

        }

        [TestMethod]
        public void ChildContent_Triggers_Rerender()
        {

        }
    }
}
