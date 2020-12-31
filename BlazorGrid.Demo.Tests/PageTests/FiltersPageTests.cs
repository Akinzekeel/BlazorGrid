using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BlazorGrid.Tests.Demo
{
    [TestClass]
    public class FiltersPageTests : Bunit.TestContext
    {
        //[TestMethod]
        //[Ignore]
        //public void Can_Select_Filter_Property()
        //{
        //    var provider = new Mock<ICustomProvider>();
        //    Services.AddSingleton(provider.Object);

        //    var nav = new MockNav();
        //    Services.AddSingleton<NavigationManager>(nav);

        //    Services.AddSingleton<IBlazorGridConfig>(_ => new DefaultConfig { Styles = new SpectreStyles() });

        //    var unit = RenderComponent<BlazorGrid.Demo.Pages.Examples.Filters>();

        //    // Switch to "add filter mode"
        //    unit.Find("#addFilterBtn").Click();

        //    // Validate options
        //    var options = unit.FindAll("#filterPropertySelect option");

        //    Assert.AreEqual(4, options.Count);

        //    foreach (var o in options)
        //    {
        //        Assert.IsTrue(o.HasAttribute("value"), "option element is missing the value attribute");
        //        Assert.IsFalse(string.IsNullOrEmpty(o.GetAttribute("value")), "option element had an empty value attribute");
        //    }
        //}
    }
}
