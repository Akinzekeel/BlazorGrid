using BlazorGrid.Abstractions;
using BlazorGrid.Abstractions.Filters;
using BlazorGrid.Components;
using BlazorGrid.Config;
using BlazorGrid.Config.Styles;
using BlazorGrid.Interfaces;
using BlazorGrid.Tests.Mock;
using Bunit;
using Bunit.TestDoubles;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using static Bunit.ComponentParameterFactory;

namespace BlazorGrid.Tests
{
    [TestClass]
    public class SortableTests : Bunit.TestContext
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
            Services.AddSingleton<IBlazorGridConfig>(_ => new DefaultConfig { Styles = new SpectreStyles() });
            Services.AddTransient<NavigationManager>(_ => new MockNav());
        }

        [TestMethod]
        public async Task Header_Click_Triggers_Sort()
        {
            mockProvider.Setup(x => x.GetAsync<MyDto>(
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<string>(),
                It.IsAny<FilterDescriptor>(),
                It.IsAny<CancellationToken>()
            )).ReturnsAsync(new BlazorGridResult<MyDto>
            {
                TotalCount = 3,
                Data = Enumerable.Repeat(new MyDto(), 3).ToList()
            });

            var grid = RenderComponent<BlazorGrid<MyDto>>(
                Template<MyDto>(nameof(ChildContent), (dto) => (b) =>
                {
                    b.OpenComponent(0, typeof(GridCol<string>));
                    b.AddAttribute(1, nameof(GridCol<string>.For), (Expression<Func<string>>)(() => dto.Name));
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
                It.IsAny<FilterDescriptor>(),
                It.IsAny<CancellationToken>()
            ), Times.Once());

            var th = grid.Find(".grid-header > *");
            Assert.IsNotNull(th, "Failed to find the column header element");

            await grid.InvokeAsync(() => th.Click());

            mockProvider.Verify(x => x.GetAsync<MyDto>(
                It.IsAny<string>(),
                0,
                It.IsAny<int>(),
                nameof(MyDto.Name),
                false,
                It.IsAny<string>(),
                It.IsAny<FilterDescriptor>(),
                It.IsAny<CancellationToken>()
            ), Times.Once());

            mockProvider.VerifyNoOtherCalls();
        }

        [DataTestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void Can_Detect_Sorted_Column(bool desc)
        {
            mockProvider.Setup(x => x.GetAsync<MyDto>(
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<string>(),
                It.IsAny<FilterDescriptor>(),
                It.IsAny<CancellationToken>()
            )).ReturnsAsync(new BlazorGridResult<MyDto>
            {
                TotalCount = 3,
                Data = Enumerable.Repeat(new MyDto(), 3).ToList()
            });

            var grid = RenderComponent<BlazorGrid<MyDto>>(
                Parameter(nameof(BlazorGrid<MyDto>.DefaultOrderBy), (Expression<Func<MyDto, object>>)(x => x.Name)),
                Parameter(nameof(BlazorGrid<MyDto>.DefaultOrderByDescending), desc),
                Template<MyDto>(nameof(ChildContent), (dto) => b =>
                {
                    b.OpenComponent(0, typeof(GridCol<string>));
                    b.AddAttribute(1, nameof(GridCol<string>.For), (Expression<Func<string>>)(() => dto.Name));
                    b.CloseComponent();
                })
            );

            mockProvider.Verify(x => x.GetAsync<MyDto>(
                It.IsAny<string>(),
                0,
                It.IsAny<int>(),
                "Name",
                desc,
                It.IsAny<string>(),
                It.IsAny<FilterDescriptor>(),
                It.IsAny<CancellationToken>()
            ), Times.Once());

            var th = grid.Find(".grid-header > *");
            th.MarkupMatches("<div class=\"sorted sortable\"><span class=\"blazor-grid-sort-icon active " + (desc ? "sorted-desc" : "sorted-asc") + "\"></span></div>");
        }
    }
}