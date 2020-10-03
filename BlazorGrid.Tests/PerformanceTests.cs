using BlazorGrid.Abstractions;
using BlazorGrid.Abstractions.Filters;
using BlazorGrid.Components;
using BlazorGrid.Config;
using BlazorGrid.Interfaces;
using BlazorGrid.Tests.Mock;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Transactions;

namespace BlazorGrid.Tests
{
    [TestClass]
    public class PerformanceTests : ComponentTestFixture
    {
        class MyDto
        {
            public string Name1 { get; set; }
            public string Name2 { get; set; }
            public string Name3 { get; set; }
            public string Name4 { get; set; }
        }

        [Ignore]
        [TestMethod]
        public void OnClick_Handler_Does_Not_Slow_Down()
        {
            var rows = Enumerable.Repeat(new MyDto
            {
                Name1 = "Un",
                Name2 = "it",
                Name3 = "Te",
                Name4 = "st"
            }, 10000).ToList();

            var provider = new Mock<IGridProvider>();
            provider.Setup(x => x.GetAsync<MyDto>(
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<string>(),
                It.IsAny<FilterDescriptor>()
            )).ReturnsAsync(() => new BlazorGridResult<MyDto>
            {
                TotalCount = rows.Count,
                Data = rows
            });

            Services.AddSingleton(provider.Object);
            Services.AddSingleton<NavigationManager>(new MockNav());
            Services.AddSingleton<IBlazorGridConfig>(new DefaultConfig());

            var clickCount = 0;

            var grid = RenderComponent<BlazorGrid<MyDto>>(
                EventCallback<MyDto>(nameof(BlazorGrid<MyDto>.OnClick), () => clickCount++),
                Template<MyDto>("ChildContent", context => (RenderTreeBuilder b) =>
                {
                    var i = 0;
                    Expression<Func<string>> for1 = () => context.Name1;
                    Expression<Func<string>> for2 = () => context.Name1;
                    Expression<Func<string>> for3 = () => context.Name1;
                    Expression<Func<string>> for4 = () => context.Name1;

                    b.OpenComponent<GridCol<string>>(i++);
                    b.AddAttribute(i++, "For", for1);
                    b.CloseComponent();

                    b.OpenComponent<GridCol<string>>(i++);
                    b.AddAttribute(i++, "For", for2);
                    b.CloseComponent();

                    b.OpenComponent<GridCol<string>>(i++);
                    b.AddAttribute(i++, "For", for3);
                    b.CloseComponent();

                    b.OpenComponent<GridCol<string>>(i++);
                    b.AddAttribute(i++, "For", for4);
                    b.CloseComponent();
                })
            );

            var renderedRows = grid.FindAll(".grid-row");
            var currentRow = renderedRows.ElementAt(1);

            var ticks = new long[rows.Count];
            var watch = Stopwatch.StartNew();

            for (int i = 0; i < 100; i++)
            {
                watch.Restart();
                currentRow.Click();
                watch.Stop();

                ticks[i] = watch.ElapsedTicks;
            }

            var firstRowClickTime = ticks.Average();

            currentRow = renderedRows.Last();

            for (int i = 0; i < 100; i++)
            {
                watch.Restart();
                currentRow.Click();
                watch.Stop();

                ticks[i] = watch.ElapsedTicks;
            }

            var lastRowClickTime = ticks.Average();

            Assert.AreEqual(firstRowClickTime, lastRowClickTime);
        }
    }
}
