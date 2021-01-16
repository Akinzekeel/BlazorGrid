using BlazorGrid.Abstractions;
using BlazorGrid.Components;
using BlazorGrid.Config;
using BlazorGrid.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
    }
}
