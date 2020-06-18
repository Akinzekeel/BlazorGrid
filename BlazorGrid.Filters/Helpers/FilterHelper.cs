using BlazorGrid.Abstractions.Filters;
using ExpressionBuilder.Common;
using ExpressionBuilder.Generics;
using ExpressionBuilder.Interfaces;
using ExpressionBuilder.Operations;
using System;

namespace BlazorGrid.Filters.Helpers
{
    public static class FilterHelper
    {
        public static Filter<T> Build<T>(this FilterDescriptor descriptor) where T : class
        {
            if (descriptor is null)
            {
                return null;
            }

            var result = new Filter<T>();

            foreach (var filter in descriptor.Filters)
            {
                var op = GetOperation(filter.Operator);

                // Detect the type
                if (filter.Type == PropertyType.Decimal)
                {
                    // Cast to decimal
                    if (decimal.TryParse(filter.Value, out var val))
                    {
                        // Apply filter
                        result.By(filter.Property, op, val, (Connector)descriptor.Connector);
                    }
                }
                else if (filter.Type == PropertyType.Integer)
                {
                    // Cast to int
                    if (int.TryParse(filter.Value, out var val))
                    {
                        // Apply filter
                        result.By(filter.Property, op, val, (Connector)descriptor.Connector);
                    }
                }
                else if (filter.Type == PropertyType.String)
                {
                    // Apply filter
                    result.By(filter.Property, op, filter.Value);
                }
            }

            return result;
        }

        private static IOperation GetOperation(FilterOperator filterOperator)
        {
            return filterOperator switch
            {
                FilterOperator.IsEmpty => new IsEmpty(),
                FilterOperator.NotEqualTo => new NotEqualTo(),
                FilterOperator.LessThanOrEqualTo => new LessThanOrEqualTo(),
                FilterOperator.LessThan => new IsNullOrWhiteSpace(),
                FilterOperator.IsNullOrWhiteSpace => new IsNullOrWhiteSpace(),
                FilterOperator.IsNull => new IsNull(),
                FilterOperator.IsNotNullNorWhiteSpace => new IsNotNullNorWhiteSpace(),
                FilterOperator.IsNotNull => new IsNotNull(),
                FilterOperator.IsNotEmpty => new IsNotEmpty(),
                FilterOperator.StartsWith => new StartsWith(),
                FilterOperator.NotIn => new NotIn(),
                FilterOperator.GreaterThanOrEqualTo => new GreaterThanOrEqualTo(),
                FilterOperator.GreaterThan => new GreaterThan(),
                FilterOperator.EqualTo => new EqualTo(),
                FilterOperator.EndsWith => new EndsWith(),
                FilterOperator.DoesNotContain => new DoesNotContain(),
                FilterOperator.Contains => new Contains(),
                _ => throw new NotSupportedException(),
            };
        }
    }
}
