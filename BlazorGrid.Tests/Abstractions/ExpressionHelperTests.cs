using BlazorGrid.Abstractions.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq.Expressions;

namespace BlazorGrid.Tests.Abstractions
{
    [TestClass]
    public class ExpressionHelperTests
    {
        class Model
        {
            public string Name { get; set; }
            public SubModel Sub { get; set; }
        }

        class SubModel
        {
            public string Name { get; set; }
        }

        [TestMethod]
        public void Can_Get_Expression_Property_Name()
        {
            // Arrange
            Expression<Func<Model, string>> nameProperty = x => x.Name;

            // Act
            var result = ExpressionHelper.GetPropertyName(nameProperty);

            // Assert
            Assert.AreEqual("Name", result);
        }

        [TestMethod]
        public void Can_Get_Expression_Property_Path_Name()
        {
            // Arrange
            Expression<Func<Model, string>> nameProperty = x => x.Sub.Name;

            // Act
            var result = ExpressionHelper.GetPropertyName(nameProperty);

            // Assert
            Assert.AreEqual("Sub.Name", result);
        }
    }
}
