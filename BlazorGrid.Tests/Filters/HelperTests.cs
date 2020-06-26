using BlazorGrid.Abstractions.Filters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.ObjectModel;
using System.Linq;

namespace BlazorGrid.Filters.Tests
{
    [TestClass]
    public class HelperTests
    {
        class Model
        {
            public int IntVal { get; set; }
            public byte ByteVal { get; set; }
            public string StringVal { get; set; }
        }

        [TestMethod]
        public void Can_Build_Filter_From_Descriptor_String_DoesNotContain()
        {
            var descriptor = new FilterDescriptor()
            {
                Connector = default,
                Filters = new ObservableCollection<PropertyFilter>
                {
                    new PropertyFilter
                    {
                        Operator = FilterOperator.DoesNotContain,
                        Property = nameof(Model.StringVal),
                        Value = "foo"
                    }
                }
            };

            var f = FilterHelper.Build<Model>(descriptor);

            var data = new Model[]
            {
                new Model { StringVal = "barfoobar" },
                new Model { StringVal = "foobar" },
                new Model { StringVal = "unit test" }
            };

            var filtered = data.AsQueryable().Where(f);

            Assert.AreEqual(1, filtered.Count());
            Assert.AreEqual("unit test", filtered.Single().StringVal);
        }

        [TestMethod]
        public void Can_Build_Filter_From_Descriptor_Int_LessThan()
        {
            var descriptor = new FilterDescriptor()
            {
                Connector = default,
                Filters = new ObservableCollection<PropertyFilter>
                {
                    new PropertyFilter
                    {
                        Operator = FilterOperator.LessThan,
                        Property = nameof(Model.IntVal),
                        Value = "20"
                    }
                }
            };

            var f = FilterHelper.Build<Model>(descriptor);

            var data = new Model[]
            {
                new Model { IntVal = 40 },
                new Model { IntVal = 20 },
                new Model { IntVal = 19 }
            };

            var filtered = data.AsQueryable().Where(f);

            Assert.AreEqual(1, filtered.Count());
            Assert.AreEqual(19, filtered.Single().IntVal);
        }

        [TestMethod]
        public void Can_Build_Filter_From_Descriptor_Int_LessThanOrEqualTo()
        {
            var descriptor = new FilterDescriptor()
            {
                Connector = default,
                Filters = new ObservableCollection<PropertyFilter>
                {
                    new PropertyFilter
                    {
                        Operator = FilterOperator.LessThanOrEqualTo,
                        Property = nameof(Model.IntVal),
                        Value = "20"
                    }
                }
            };

            var f = FilterHelper.Build<Model>(descriptor);

            var data = new Model[]
            {
                new Model { IntVal = 40 },
                new Model { IntVal = 20 },
                new Model { IntVal = 19 }
            };

            var filtered = data.AsQueryable().Where(f);

            Assert.AreEqual(2, filtered.Count());
            Assert.AreEqual(20, filtered.First().IntVal);
            Assert.AreEqual(19, filtered.Last().IntVal);
        }

        [TestMethod]
        public void Can_Build_Filter_From_Descriptor_Int_EqualTo()
        {
            var descriptor = new FilterDescriptor()
            {
                Filters = new ObservableCollection<PropertyFilter>
                {
                    new PropertyFilter
                    {
                        Operator = FilterOperator.EqualTo,
                        Property = nameof(Model.IntVal),
                        Value = "20"
                    }
                }
            };

            var f = FilterHelper.Build<Model>(descriptor);

            var data = new Model[]
            {
                new Model { IntVal = 40 },
                new Model { IntVal = 20 },
                new Model { IntVal = 19 }
            };

            var filtered = data.AsQueryable().Where(f);

            Assert.AreEqual(1, filtered.Count());
            Assert.AreEqual(20, filtered.Single().IntVal);
        }

        [TestMethod]
        public void Can_Build_Filter_From_Descriptor_Byte_EqualTo()
        {
            var descriptor = new FilterDescriptor()
            {
                Filters = new ObservableCollection<PropertyFilter>
                {
                    new PropertyFilter
                    {
                        Operator = FilterOperator.EqualTo,
                        Property = nameof(Model.ByteVal),
                        Value = "20"
                    }
                }
            };

            var f = FilterHelper.Build<Model>(descriptor);

            var data = new Model[]
            {
                new Model { ByteVal = 40 },
                new Model { ByteVal = 20 },
                new Model { ByteVal = 19 }
            };

            var filtered = data.AsQueryable().Where(f);

            Assert.AreEqual(1, filtered.Count());
            Assert.AreEqual(20, filtered.Single().ByteVal);
        }
    }
}
