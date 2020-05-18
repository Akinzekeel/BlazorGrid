using System;
using System.Threading.Tasks;
using BlazorGrid.Abstractions;
using BlazorGrid.Components;
using BlazorGrid.Tests.Mock;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace BlazorGrid.Tests
{
    [TestClass]
    public class FilterableTests : ComponentTestFixture
    {
        class MyDto
        {
            public string Name { get; set; }
        }

        private Mock<IGridProvider> mockProvider;

        [TestInitialize]
        public void Initialize()
        {
            mockProvider = new Mock<IGridProvider>();
            Services.AddTransient(_ => mockProvider.Object);
            Services.AddTransient<NavigationManager>(_ => new MockNav());
        }

        [TestMethod]
        public async Task Query_Set_Triggers_Provider_Call()
        {
            var grid = RenderComponent<BlazorGrid<MyDto>>(
                Template<MyDto>(nameof(ChildContent), (dto) => (b) =>
                {
                    b.OpenComponent(0, typeof(GridCol));
                    b.AddAttribute(1, nameof(GridCol.Caption), nameof(MyDto.Name));
                    b.CloseComponent();
                })
            );

            // The initial request to the provider must have happened
            mockProvider.Verify(x => x.GetAsync<MyDto>(
                It.IsAny<string>(),
                0,
                It.IsAny<int>(),
                null,
                false,
                null)
            , Times.Once());

            // No other requests must have happened at this point
            mockProvider.VerifyNoOtherCalls();

            // Set a filter query
            grid.SetParametersAndRender(
                Parameter("Query", "unit-test")
            );

            // The Query property has a debounce, so no
            // request must happen immediately
            mockProvider.VerifyNoOtherCalls();

            // Wait for the debounce
            await Task.Delay(500);

            // Verify the request which must include the filter
            mockProvider.Verify(x => x.GetAsync<MyDto>(
                It.IsAny<string>(),
                0,
                It.IsAny<int>(),
                null,
                false,
                "unit-test")
            , Times.Once());

            // Those must be the only 2 requests
            mockProvider.VerifyNoOtherCalls();
        }
    }
}