using BlazorGrid.Abstractions;
using BlazorGrid.Components;
using BlazorGrid.Config;
using BlazorGrid.Interfaces;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static Bunit.ComponentParameterFactory;

namespace BlazorGrid.Tests
{
    [TestClass]
    public class ProviderTests : Bunit.TestContext
    {
        [TestInitialize]
        public void Initialize()
        {
            Services.AddSingleton<IBlazorGridConfig>(new DefaultConfig());
        }

        [TestMethod]
        public void Can_Handle_Null_ReturnValue()
        {
            int providerCallCount = 0;

            ProviderDelegate<object> provider = (BlazorGridRequest r, CancellationToken c) =>
            {
                providerCallCount++;
                return ValueTask.FromResult<BlazorGridResult<object>>(null);
            };

            // Render a grid with a column to make sure it has some markup
            var grid = RenderComponent<BlazorGrid<object>>(
                Parameter(nameof(BlazorGrid<object>.Provider), provider),
                Template<object>(nameof(ChildContent), context => b =>
                {
                    b.OpenComponent<StaticGridCol>(0);
                    b.AddAttribute(1, nameof(StaticGridCol.Caption), "test");
                    b.CloseComponent();
                })
            );

            Assert.AreEqual(1, providerCallCount);
            Assert.AreNotEqual("", grid.Markup);
        }

        [TestMethod]
        public void Does_Invoke_Parameter_Provider()
        {
            int delegateInvocationCount = 0;

            ProviderDelegate<object> provider = (req, ct) =>
            {
                delegateInvocationCount++;

                return ValueTask.FromResult(new BlazorGridResult<object>
                {

                });
            };

            // Render a grid with a column to make sure it has some markup
            var grid = RenderComponent<BlazorGrid<object>>(
                Parameter(nameof(BlazorGrid<object>.Provider), provider),
                Template<object>(nameof(ChildContent), context => b =>
                {
                    b.OpenComponent<StaticGridCol>(0);
                    b.AddAttribute(1, nameof(StaticGridCol.Caption), "test");
                    b.CloseComponent();
                })
            );

            Assert.AreNotEqual("", grid.Markup);
            Assert.AreEqual(1, delegateInvocationCount);
        }

        [TestMethod]
        public async Task TaskCanceledException_Is_Handled()
        {
            ProviderDelegate<object> provider = (r, _) =>
            {
                return ValueTask.FromResult(new BlazorGridResult<object>
                {
                    Data = new List<object> { },
                    TotalCount = 0
                });
            };

            var grid = RenderComponent<BlazorGrid<object>>(
                Parameter(nameof(BlazorGrid<object>.Provider), provider),
                Template<object>(nameof(ChildContent), context => b =>
                {
                    b.OpenComponent<StaticGridCol>(0);
                    b.AddAttribute(1, nameof(StaticGridCol.Caption), "test");
                    b.CloseComponent();
                })
            );

            // Change the provider to throw an exception
            provider = (r, _) =>
            {
                throw new TaskCanceledException();
            };

            grid.SetParametersAndRender(
                Parameter(nameof(BlazorGrid<object>.Provider), provider)
            );

            await grid.InvokeAsync(() => grid.Instance.ReloadAsync());

            // Assert that there is no error overlay
            var overlays = grid.FindAll(".grid-overlay");
            Assert.AreEqual(0, overlays.Count);
        }

        [TestMethod]
        public async Task OperationCanceledException_Is_Handled()
        {
            ProviderDelegate<object> provider = (r, _) =>
            {
                return ValueTask.FromResult(new BlazorGridResult<object>
                {
                    Data = new List<object> { },
                    TotalCount = 0
                });
            };

            var grid = RenderComponent<BlazorGrid<object>>(
                Parameter(nameof(BlazorGrid<object>.Provider), provider),
                Template<object>(nameof(ChildContent), context => b =>
                {
                    b.OpenComponent<StaticGridCol>(0);
                    b.AddAttribute(1, nameof(StaticGridCol.Caption), "test");
                    b.CloseComponent();
                })
            );

            // Change the provider to throw an exception
            provider = (r, _) =>
            {
                throw new OperationCanceledException();
            };

            grid.SetParametersAndRender(
                Parameter(nameof(BlazorGrid<object>.Provider), provider)
            );

            await grid.InvokeAsync(() => grid.Instance.ReloadAsync());

            // Assert that there is no error overlay
            var overlays = grid.FindAll(".grid-overlay");
            Assert.AreEqual(0, overlays.Count);
        }

        [TestMethod]
        public async Task CancellationTokenSourceDisposedException_Is_Handled()
        {
            ProviderDelegate<object> provider = (r, _) =>
            {
                return ValueTask.FromResult(new BlazorGridResult<object>
                {
                    Data = new List<object> { },
                    TotalCount = 0
                });
            };

            var grid = RenderComponent<BlazorGrid<object>>(
                Parameter(nameof(BlazorGrid<object>.Provider), provider),
                Template<object>(nameof(ChildContent), context => b =>
                {
                    b.OpenComponent<StaticGridCol>(0);
                    b.AddAttribute(1, nameof(StaticGridCol.Caption), "test");
                    b.CloseComponent();
                })
            );

            // Change the provider to throw an exception
            provider = (r, _) =>
            {
                var ct = new CancellationTokenSource();
                var token = ct.Token;
                ct.Dispose();
                ct.Cancel();

                return ValueTask.FromResult(new BlazorGridResult<object>
                {
                    Data = new List<object> { },
                    TotalCount = 0
                });
            };

            grid.SetParametersAndRender(
                Parameter(nameof(BlazorGrid<object>.Provider), provider)
            );

            await grid.InvokeAsync(() => grid.Instance.ReloadAsync());

            // Assert that there is no error overlay
            var overlays = grid.FindAll(".grid-overlay");
            Assert.AreEqual(0, overlays.Count, overlays.Select(x => x.InnerHtml).FirstOrDefault());
        }
    }
}
