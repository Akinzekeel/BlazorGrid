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

        [TestInitialize]
        public void Initialize()
        {
            Services.AddSingleton<IBlazorGridConfig>(_ => new DefaultConfig { Styles = new SpectreStyles() });
            Services.AddTransient<NavigationManager>(_ => new MockNav());
        }

        [TestMethod]
        public async Task Header_Click_Triggers_Sort()
        {
            int providerCallCount = 0;
            string providerCallOrderBy = null;

            ProviderDelegate<MyDto> provider = (BlazorGridRequest r, CancellationToken c) =>
            {
                providerCallCount++;
                providerCallOrderBy = r.OrderBy;

                return ValueTask.FromResult(new BlazorGridResult<MyDto>
                {
                    Data = Enumerable.Repeat(new MyDto(), 3).ToList(),
                    TotalCount = 3
                });
            };

            var grid = RenderComponent<BlazorGrid<MyDto>>(
                Parameter(nameof(BlazorGrid<MyDto>.Provider), provider),
                Template<MyDto>(nameof(ChildContent), (dto) => (b) =>
                {
                    b.OpenComponent(0, typeof(GridCol<string>));
                    b.AddAttribute(1, nameof(GridCol<string>.For), (Expression<Func<string>>)(() => dto.Name));
                    b.CloseComponent();
                })
            );

            Assert.AreEqual(1, providerCallCount);
            Assert.IsNull(providerCallOrderBy);

            var th = grid.Find(".grid-header > *");
            Assert.IsNotNull(th, "Failed to find the column header element");

            await grid.InvokeAsync(() => th.Click());

            Assert.AreEqual(2, providerCallCount);
            Assert.AreEqual(nameof(MyDto.Name), providerCallOrderBy);
        }

        [DataTestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void Can_Detect_Sorted_Column(bool desc)
        {
            int providerCallCount = 0;
            string providerCallOrderBy = null;
            bool? providerCallOrderByDescending = null;

            ProviderDelegate<MyDto> provider = (BlazorGridRequest r, CancellationToken c) =>
            {
                providerCallCount++;
                providerCallOrderBy = r.OrderBy;
                providerCallOrderByDescending = r.OrderByDescending;

                return ValueTask.FromResult(new BlazorGridResult<MyDto>
                {
                    Data = Enumerable.Repeat(new MyDto(), 3).ToList(),
                    TotalCount = 3
                });
            };

            var grid = RenderComponent<BlazorGrid<MyDto>>(
                Parameter(nameof(BlazorGrid<MyDto>.Provider), provider),
                Parameter(nameof(BlazorGrid<MyDto>.DefaultOrderBy), (Expression<Func<MyDto, object>>)(x => x.Name)),
                Parameter(nameof(BlazorGrid<MyDto>.DefaultOrderByDescending), desc),
                Template<MyDto>(nameof(ChildContent), (dto) => b =>
                {
                    b.OpenComponent(0, typeof(GridCol<string>));
                    b.AddAttribute(1, nameof(GridCol<string>.For), (Expression<Func<string>>)(() => dto.Name));
                    b.CloseComponent();
                })
            );

            Assert.AreEqual(1, providerCallCount);
            Assert.AreEqual(nameof(MyDto.Name), providerCallOrderBy);
            Assert.AreEqual(desc, providerCallOrderByDescending.Value);

            var th = grid.Find(".grid-header > *");
            th.MarkupMatches("<div class=\"sorted sortable\"><span class=\"blazor-grid-sort-icon active " + (desc ? "sorted-desc" : "sorted-asc") + "\"></span></div>");
        }
    }
}