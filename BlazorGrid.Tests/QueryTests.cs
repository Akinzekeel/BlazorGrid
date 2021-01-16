using BlazorGrid.Abstractions;
using BlazorGrid.Components;
using BlazorGrid.Config;
using BlazorGrid.Config.Styles;
using BlazorGrid.Interfaces;
using BlazorGrid.Tests.Mock;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static Bunit.ComponentParameterFactory;

namespace BlazorGrid.Tests
{
    [TestClass]
    public class QueryTests : Bunit.TestContext
    {
        class Model
        {
            public string Name { get; set; }
        }

        [TestInitialize]
        public void Initialize()
        {
            Services.AddSingleton<IBlazorGridConfig>(_ => new DefaultConfig { Styles = new SpectreStyles() });
            Services.AddTransient<NavigationManager>(_ => new MockNav());
        }

        [TestMethod]
        public async Task Query_Set_Triggers_Provider_Call()
        {
            int providerCallCount = 0;
            string providerCallQuery = null;

            ProviderDelegate<Model> provider = (BlazorGridRequest r, CancellationToken c) =>
            {
                providerCallCount++;
                providerCallQuery = r.Query;

                return ValueTask.FromResult(new BlazorGridResult<Model>
                {
                    Data = Enumerable.Repeat(new Model(), 3).ToList(),
                    TotalCount = 3
                });
            };

            var grid = RenderComponent<BlazorGrid<Model>>(
                Parameter(nameof(BlazorGrid<Model>.Provider), provider),
                Template<Model>(nameof(ChildContent), (dto) => (b) =>
                {
                    b.OpenComponent(0, typeof(GridCol<string>));
                    b.AddAttribute(1, nameof(GridCol<string>.Caption), nameof(Model.Name));
                    b.CloseComponent();
                })
            );

            // The initial request to the provider must have happened
            Assert.AreEqual(1, providerCallCount);
            Assert.IsNull(providerCallQuery);

            // Set a filter query
            grid.SetParametersAndRender(
                Parameter(nameof(BlazorGrid<Model>.QueryUserInput), "unit-test")
            );

            // The Query property has a debounce, so no
            // request must happen immediately
            Assert.AreEqual(1, providerCallCount);

            // Wait for the debounce
            await Task.Delay(500);

            // Verify the request which must include the filter
            Assert.AreEqual(2, providerCallCount);
            Assert.AreEqual("unit-test", providerCallQuery);
        }

        [TestMethod]
        public async Task Query_Set_Triggers_Render()
        {
            ProviderDelegate<Model> provider = (BlazorGridRequest r, CancellationToken c) =>
            {
                return ValueTask.FromResult(new BlazorGridResult<Model>
                {
                    Data = Enumerable.Repeat(new Model(), 3).ToList(),
                    TotalCount = 3
                });
            };

            var grid = RenderComponent<BlazorGrid<Model>>(
                Parameter(nameof(BlazorGrid<Model>.Provider), provider),
                Template<Model>(nameof(ChildContent), (dto) => (b) =>
                {
                    b.OpenComponent(0, typeof(GridCol<string>));
                    b.AddAttribute(1, nameof(GridCol<string>.Caption), nameof(Model.Name));
                    b.CloseComponent();
                })
            );

            // Set a filter query
            grid.SetParametersAndRender(
                Parameter(nameof(BlazorGrid<Model>.QueryUserInput), "unit-test")
            );

            var rc = grid.RenderCount;

            // Wait for the debounce
            await Task.Delay(500);

            Assert.AreNotEqual(rc, grid.RenderCount);
        }
    }
}