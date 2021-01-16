using BlazorGrid.Abstractions;
using BlazorGrid.Abstractions.Filters;
using BlazorGrid.Components;
using BlazorGrid.Config;
using BlazorGrid.Config.Styles;
using BlazorGrid.Interfaces;
using BlazorGrid.Tests.Mock;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using static Bunit.ComponentParameterFactory;

namespace BlazorGrid.Tests
{
    [TestClass]
    public class FilterTests : Bunit.TestContext
    {
        class MyDto
        {
            public string Name { get; set; }
        }

        [TestInitialize]
        public void Initialize()
        {
            Services.AddSingleton<IBlazorGridConfig>(_ => new DefaultConfig() { Styles = new SpectreStyles() });
            Services.AddTransient<NavigationManager>(_ => new MockNav());
        }

        [Ignore]
        [TestMethod]
        public void Filter_Descriptor_Property_Change_Triggers_Provider_Call()
        {
            int providerCallCount = 0;
            FilterDescriptor callFilterDescriptor = null;

            ProviderDelegate<MyDto> provider = (r, c) =>
            {
                providerCallCount++;
                callFilterDescriptor = r.Filter;

                return ValueTask.FromResult(new BlazorGridResult<MyDto>
                {
                    Data = new List<MyDto>(),
                    TotalCount = 0
                });
            };

            var grid = RenderComponent<BlazorGrid<MyDto>>(
                Parameter(nameof(BlazorGrid<MyDto>.Provider), provider),
                Template<MyDto>(nameof(ChildContent), (dto) => (b) =>
                {
                    Expression<Func<string>> colFor = () => dto.Name;
                    b.OpenComponent(0, typeof(GridCol<string>));
                    b.AddAttribute(1, nameof(GridCol<string>.Caption), nameof(MyDto.Name));
                    b.AddAttribute(2, nameof(GridCol<string>.For), colFor);
                    b.CloseComponent();
                })
            );

            // The initial request to the provider must have happened
            Assert.AreEqual(1, providerCallCount);

            grid.Instance.Filter.Connector = ConnectorType.Any;

            Assert.AreEqual(2, providerCallCount);
            Assert.IsNotNull(callFilterDescriptor);
            Assert.AreEqual(ConnectorType.Any, callFilterDescriptor.Connector);
        }

        [Ignore]
        [TestMethod]
        public void Filter_Descriptor_Collection_Change_Triggers_Provider_Call()
        {
            int providerCallCount = 0;
            FilterDescriptor callFilterDescriptor = null;

            ProviderDelegate<MyDto> provider = (r, c) =>
            {
                providerCallCount++;
                callFilterDescriptor = r.Filter;

                return ValueTask.FromResult(new BlazorGridResult<MyDto>
                {
                    Data = new List<MyDto>(),
                    TotalCount = 0
                });
            };
            var grid = RenderComponent<BlazorGrid<MyDto>>(
                Template<MyDto>(nameof(ChildContent), (dto) => (b) =>
                {
                    Expression<Func<string>> colFor = () => dto.Name;
                    b.OpenComponent(0, typeof(GridCol<string>));
                    b.AddAttribute(1, nameof(GridCol<string>.Caption), nameof(dto.Name));
                    b.AddAttribute(2, nameof(GridCol<string>.For), colFor);
                    b.CloseComponent();
                })
            );

            Assert.AreEqual(1, providerCallCount);

            // Set a filter 
            grid.Instance.Filter.Filters.Add(new PropertyFilter
            {
                Property = "Foo",
                Operator = FilterOperator.Contains,
                Value = "Bar"
            });

            Assert.AreEqual(2, providerCallCount);
            Assert.IsNotNull(callFilterDescriptor);
            Assert.IsTrue(callFilterDescriptor.Filters.Any(x => x.Value == "Bar"));
        }
    }
}
