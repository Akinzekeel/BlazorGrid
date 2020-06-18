using BlazorGrid.Abstractions;
using BlazorGrid.Abstractions.Filters;
using BlazorGrid.Components;
using BlazorGrid.Tests.Mock;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Linq;

namespace BlazorGrid.Tests
{
    [TestClass]
    public class FilterTests : ComponentTestFixture
    {
        class Model
        {
            public string String { get; set; }
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
        public void Filter_Descriptor_Property_Change_Triggers_Provider_Call()
        {
            var grid = RenderComponent<BlazorGrid<Model>>(
                Template<Model>(nameof(ChildContent), (dto) => (b) =>
                {
                    b.OpenComponent(0, typeof(GridCol));
                    b.AddAttribute(1, nameof(GridCol.Caption), nameof(Model.String));
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

            grid.Instance.Filter.Connector = ConnectorType.Any;

            mockProvider.Verify(x => x.GetAsync<Model>(
                It.IsAny<string>(),
                0,
                It.IsAny<int>(),
                null,
                false,
                null,
                It.Is<FilterDescriptor>(f => f.Connector == ConnectorType.Any)
            ), Times.Once());

            // Those must be the only 2 requests
            mockProvider.VerifyNoOtherCalls();
        }

        [TestMethod]
        public void Filter_Descriptor_Collection_Change_Triggers_Provider_Call()
        {
            var grid = RenderComponent<BlazorGrid<Model>>(
                Template<Model>(nameof(ChildContent), (dto) => (b) =>
                {
                    b.OpenComponent(0, typeof(GridCol));
                    b.AddAttribute(1, nameof(GridCol.Caption), nameof(Model.String));
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

            // Set a filter 
            grid.Instance.Filter.Filters.Add(new PropertyFilter
            {
                Property = "Foo",
                Operator = FilterOperator.Contains,
                Value = "Bar"
            });

            mockProvider.Verify(x => x.GetAsync<Model>(
                It.IsAny<string>(),
                0,
                It.IsAny<int>(),
                null,
                false,
                null,
                It.Is<FilterDescriptor>(f => f.Filters.Any(p => p.Value == "Bar"))
            ), Times.Once());

            // Those must be the only 2 requests
            mockProvider.VerifyNoOtherCalls();
        }
    }
}
