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
using System.Linq.Expressions;

namespace BlazorGrid.Tests
{
    [TestClass]
    public class SortableTests : ComponentTestFixture
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
        public void Header_Click_Triggers_Sort()
        {
            var grid = RenderComponent<BlazorGrid<MyDto>>(
                Template<MyDto>(nameof(ChildContent), (dto) => (b) =>
                {
                    b.OpenComponent(0, typeof(GridCol<string>));
                    b.AddAttribute(1, nameof(GridCol<string>.Value), nameof(MyDto.Name));
                    b.AddAttribute(2, nameof(GridCol<string>.For), (Expression<Func<string>>)(() => dto.Name));
                    b.CloseComponent();
                })
            );

            mockProvider.Verify(x => x.GetAsync<MyDto>(
                It.IsAny<string>(),
                0,
                It.IsAny<int>(),
                null,
                false,
                It.IsAny<string>(),
                It.IsAny<FilterDescriptor>()
            ), Times.Once());

            var th = grid.Find(".grid-header > *");
            th.Click();

            mockProvider.Verify(x => x.GetAsync<MyDto>(
                It.IsAny<string>(),
                0,
                It.IsAny<int>(),
                nameof(MyDto.Name),
                false,
                It.IsAny<string>(),
                It.IsAny<FilterDescriptor>()
            ), Times.Once());

            mockProvider.VerifyNoOtherCalls();
        }

        [TestMethod]
        public void Non_Sortable_Header_Click_Does_Nothing()
        {
            var grid = RenderComponent<BlazorGrid<MyDto>>(
                Template<MyDto>(nameof(ChildContent), (dto) => (b) =>
                {
                    b.OpenComponent(0, typeof(GridCol<string>));
                    b.CloseComponent();
                })
            );

            mockProvider.Verify(x => x.GetAsync<MyDto>(
                It.IsAny<string>(),
                0,
                It.IsAny<int>(),
                null,
                false,
                It.IsAny<string>(),
                It.IsAny<FilterDescriptor>()
            ), Times.Once());

            var th = grid.Find(".grid-header > *");
            th.Click();

            mockProvider.VerifyNoOtherCalls();
        }
    }
}