using BlazorGrid.Abstractions;
using BlazorGrid.Abstractions.Filters;
using BlazorGrid.Components;
using BlazorGrid.Config;
using BlazorGrid.Config.Styles;
using BlazorGrid.Interfaces;
using BlazorGrid.Tests.Mock;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace BlazorGrid.Tests
{
    [TestClass]
    public class FilterTests : ComponentTestFixture
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
            Services.AddSingleton(provider);

            return promise;
        }

        [TestMethod]
        public void Filter_Descriptor_Property_Change_Triggers_Provider_Call()
        {
            var promise = SetupMockProvider();

            var grid = RenderComponent<BlazorGrid<MyDto>>(
                Template<MyDto>(nameof(ChildContent), (dto) => (b) =>
                {
                    Expression<Func<string>> colFor = () => dto.Name;
                    b.OpenComponent(0, typeof(GridCol<string>));
                    b.AddAttribute(1, nameof(GridCol<string>.Caption), nameof(MyDto.Name));
                    b.AddAttribute(2, nameof(GridCol<string>.For), colFor);
                    b.CloseComponent();
                })
            );

            var mockProvider = Services.GetRequiredService<Mock<IGridProvider>>();

            // The initial request to the provider must have happened
            mockProvider.Verify(x => x.GetAsync<MyDto>(
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

            mockProvider.Verify(x => x.GetAsync<MyDto>(
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
            var promise = SetupMockProvider();

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

            var mockProvider = Services.GetRequiredService<Mock<IGridProvider>>();

            // The initial request to the provider must have happened
            mockProvider.Verify(x => x.GetAsync<MyDto>(
                It.IsAny<string>(),
                0,
                It.IsAny<int>(),
                null,
                false,
                null,
                It.IsAny<FilterDescriptor>()
            ), Times.Once(), "The provider was not called once before setting the filter");

            // No other requests must have happened at this point
            mockProvider.VerifyNoOtherCalls();

            // Set a filter 
            grid.Instance.Filter.Filters.Add(new PropertyFilter
            {
                Property = "Foo",
                Operator = FilterOperator.Contains,
                Value = "Bar"
            });

            mockProvider.Verify(x => x.GetAsync<MyDto>(
                It.IsAny<string>(),
                0,
                It.IsAny<int>(),
                null,
                false,
                null,
                It.Is<FilterDescriptor>(f => f.Filters.Any(p => p.Value == "Bar"))
            ), Times.Once(), "The provider was not called once after setting the filter");

            // Those must be the only 2 requests
            mockProvider.VerifyNoOtherCalls();
        }
    }
}
