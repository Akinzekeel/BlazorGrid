using BlazorGrid.Abstractions;
using BlazorGrid.Abstractions.Filters;
using BlazorGrid.Components;
using BlazorGrid.Tests.Mock;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorGrid.Tests
{
    [TestClass]
    public class FilterTests : ComponentTestFixture
    {
        class Model
        {
            public int Int { get; set; }
            public int? NInt { get; set; }
            public decimal Dec { get; set; }
            public decimal? NDec { get; set; }
            public string String { get; set; }
        }

        [TestMethod]
        public void Can_Detect_PropertyType_Integer()
        {
            var result = Components.BlazorGrid<Model>.GetTypeFor(nameof(Model.Int));
            Assert.AreEqual(PropertyType.Integer, result);
        }

        [TestMethod]
        public void Can_Detect_PropertyType_Integer_Nullable()
        {
            var result = Components.BlazorGrid<Model>.GetTypeFor(nameof(Model.NInt));
            Assert.AreEqual(PropertyType.Integer, result);
        }

        [TestMethod]
        public void Can_Detect_PropertyType_Decimal()
        {
            var result = Components.BlazorGrid<Model>.GetTypeFor(nameof(Model.Dec));
            Assert.AreEqual(PropertyType.Decimal, result);
        }

        [TestMethod]
        public void Can_Detect_PropertyType_Decimal_Nullable()
        {
            var result = Components.BlazorGrid<Model>.GetTypeFor(nameof(Model.NDec));
            Assert.AreEqual(PropertyType.Decimal, result);
        }

        [TestMethod]
        public void Can_Detect_PropertyType_String()
        {
            var result = Components.BlazorGrid<Model>.GetTypeFor(nameof(Model.String));
            Assert.AreEqual(PropertyType.String, result);
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
                Value = "Bar",
                Type = PropertyType.String
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
