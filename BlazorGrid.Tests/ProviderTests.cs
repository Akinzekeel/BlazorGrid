using BlazorGrid.Abstractions;
using BlazorGrid.Abstractions.Filters;
using BlazorGrid.Components;
using BlazorGrid.Config;
using BlazorGrid.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    }
}
