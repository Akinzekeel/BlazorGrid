using BlazorGrid.Abstractions;
using BlazorGrid.Abstractions.Filters;
using BlazorGrid.Components;
using BlazorGrid.Config;
using BlazorGrid.Config.Styles;
using BlazorGrid.Interfaces;
using BlazorGrid.Tests.Mock;
using Bunit;
using static Bunit.ComponentParameterFactory;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Threading.Tasks;

namespace BlazorGrid.Tests
{
    [TestClass]
    public class QueryTests : Bunit.TestContext
    {
        class Model
        {
            public string Name { get; set; }
        }

        private Mock<IGridProvider> mockProvider;

        [TestInitialize]
        public void Initialize()
        {
            mockProvider = new Mock<IGridProvider>();
            Services.AddTransient(_ => mockProvider.Object);
            Services.AddSingleton<IBlazorGridConfig>(_ => new DefaultConfig { Styles = new SpectreStyles() });
            Services.AddTransient<NavigationManager>(_ => new MockNav());
        }

        [TestMethod]
        public async Task Query_Set_Triggers_Provider_Call()
        {
            var grid = RenderComponent<BlazorGrid<Model>>(
                Template<Model>(nameof(ChildContent), (dto) => (b) =>
                {
                    b.OpenComponent(0, typeof(GridCol<string>));
                    b.AddAttribute(1, nameof(GridCol<string>.Caption), nameof(Model.Name));
                    b.CloseComponent();
                })
            );

            // The initial request to the provider must have happened
            mockProvider.Verify(x => x.GetAsync<Model>(
                It.IsAny<string>(),
                0,
                It.IsAny<int>(),
                null,
                false,
                null,
                It.IsAny<FilterDescriptor>()
            ), Times.Once());

            // No other requests must have happened at this point
            mockProvider.VerifyNoOtherCalls();

            // Set a filter query
            grid.SetParametersAndRender(
                Parameter(nameof(BlazorGrid<Model>.QueryUserInput), "unit-test")
            );

            // The Query property has a debounce, so no
            // request must happen immediately
            mockProvider.VerifyNoOtherCalls();

            // Wait for the debounce
            await Task.Delay(500);

            // Verify the request which must include the filter
            mockProvider.Verify(x => x.GetAsync<Model>(
                It.IsAny<string>(),
                0,
                It.IsAny<int>(),
                null,
                false,
                "unit-test",
                It.IsAny<FilterDescriptor>()
            ), Times.Once());

            // Those must be the only 2 requests
            mockProvider.VerifyNoOtherCalls();
        }
    }
}