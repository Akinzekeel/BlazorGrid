using BlazorGrid.Abstractions;
using BlazorGrid.Abstractions.Filters;
using BlazorGrid.Components;
using BlazorGrid.Config;
using BlazorGrid.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
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

        [Obsolete]
        [TestMethod]
        public async Task Can_Handle_Null_ReturnValue()
        {
            var provider = new Mock<IGridProvider>();
            Services.AddSingleton(provider.Object);

            // Verify that the mock does return null
            var emptyResponse = await provider.Object.GetAsync<object>(
                "",
                0,
                1,
                null,
                default,
                null,
                null,
                default
            );

            Assert.IsNull(emptyResponse);
            provider.Invocations.Clear();

            // Render a grid with a column to make sure it has some markup
            var grid = RenderComponent<BlazorGrid<object>>(
                Template<object>(nameof(ChildContent), context => b =>
                {
                    b.OpenComponent<StaticGridCol>(0);
                    b.AddAttribute(1, nameof(StaticGridCol.Caption), "test");
                    b.CloseComponent();
                })
            );

            provider.Verify(x => x.GetAsync<object>(
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<string>(),
                It.IsAny<FilterDescriptor>(),
                It.IsAny<CancellationToken>()
            ), Times.Once());

            Assert.AreNotEqual("", grid.Markup);
        }

        [TestMethod]
        public void Does_Invoke_Parameter_Provider()
        {
            var legacyProvider = new Mock<IGridProvider>();
            Services.AddSingleton(legacyProvider.Object);

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

            legacyProvider.VerifyNoOtherCalls();
        }
    }
}
