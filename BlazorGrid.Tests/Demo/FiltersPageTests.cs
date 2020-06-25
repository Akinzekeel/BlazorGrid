using BlazorGrid.Abstractions;
using BlazorGrid.Tests.Mock;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace BlazorGrid.Tests.Demo
{
    [TestClass]
    public class FiltersPageTests : ComponentTestFixture
    {
        [TestMethod]
        public void Can_Select_Filter_Property()
        {
            var provider = new Mock<IGridProvider>();
            Services.AddSingleton(provider.Object);

            var nav = new MockNav();
            Services.AddSingleton<NavigationManager>(nav);

            var unit = RenderComponent<BlazorGrid.Demo.Pages.Demos.Filters>();

            // Switch to "add filter mode"
            unit.Find("#addFilterBtn").Click();

            // Validate options
            var options = unit.FindAll("#filterPropertySelect option");

            Assert.AreEqual(4, options.Count);

            foreach (var o in options)
            {
                Assert.IsTrue(o.HasAttribute("value"), "option element is missing the value attribute");
                Assert.IsFalse(string.IsNullOrEmpty(o.GetAttribute("value")), "option element had an empty value attribute");
            }
        }
    }
}
