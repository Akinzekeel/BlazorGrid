using BlazorGrid.Abstractions.Filters;
using BlazorGrid.Filters.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace BlazorGrid.Filters.Tests
{
    [TestClass]
    public class HelperTests
    {
        class Model
        {
            public string UnitTest { get; set; }
        }

        [TestMethod]
        public void Can_Build_Filter_From_Descriptor()
        {
            var descriptor = new FilterDescriptor()
            {
                Connector = default,
                Filters = new ObservableCollection<PropertyFilter>
                {
                    new PropertyFilter
                    {
                        Operator = FilterOperator.DoesNotContain,
                        Property = "UnitTest",
                        Value = "foo"
                    }
                }
            };

            var f = FilterHelper.Build<Model>(descriptor);

            var data = new Model[]
            {
                new Model { UnitTest = "barfoobar" },
                new Model { UnitTest = "foobar" },
                new Model { UnitTest = "unit test" }
            };

            var filtered = data.AsQueryable().Where(f);

            Assert.AreEqual(1, filtered.Count());
            Assert.AreEqual("unit test", filtered.Single().UnitTest);
        }
    }
}
