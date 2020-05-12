using System;
using System.Linq;
using System.Linq.Expressions;
using BlazorGrid.Abstractions.Helpers;

namespace BlazorGrid.Abstractions.Extensions
{
    public static class IQueryableExtensions
    {
        /// <summary>
        /// Order a queryable by the name of a property.
        /// This method is compatible with EF6.
        /// </summary>
        /// <typeparam name="TSource">The type of the source</typeparam>
        /// <param name="query">The source queryable</param>
        /// <param name="propertyName">The name of the property to order by</param>
        /// <returns>A new, ordered queryable</returns>
        public static IOrderedQueryable<TSource> OrderBy<TSource>(this IQueryable<TSource> query, string propertyName)
            => OrderBy(query, propertyName, nameof(Enumerable.OrderBy));

        /// <summary>
        /// Order a queryable by the name of a property.
        /// This method is compatible with EF6.
        /// </summary>
        /// <typeparam name="TSource">The type of the source</typeparam>
        /// <param name="query">The source queryable</param>
        /// <param name="propertyName">The name of the property to order by</param>
        /// <returns>A new, ordered queryable</returns>
        public static IOrderedQueryable<TSource> OrderByDescending<TSource>(this IQueryable<TSource> query, string propertyName)
            => OrderBy(query, propertyName, nameof(Enumerable.OrderByDescending));

        private static IOrderedQueryable<TSource> OrderBy<TSource>(this IQueryable<TSource> source, string orderByProperty, string command)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (string.IsNullOrEmpty(orderByProperty))
                throw new ArgumentNullException(nameof(orderByProperty));

            var orderByExpression = ExpressionHelper.CreatePropertyExpression<TSource>(orderByProperty);

            if (command == nameof(OrderByDescending))
                return OrderByDescending(source, orderByExpression);

            return OrderBy(source, orderByExpression);
        }

        public static IOrderedQueryable<TSource> OrderBy<TSource>(this IQueryable<TSource> source, LambdaExpression expression)
        {
            var type = typeof(TSource);

            var resultExpression = Expression.Call(
                    typeof(Queryable),
                    nameof(OrderBy),
                    new Type[] { type, expression.ReturnType },
                    source.Expression,
                    Expression.Quote(expression));
            return (IOrderedQueryable<TSource>)source.Provider.CreateQuery(resultExpression);
        }

        public static IOrderedQueryable<TSource> OrderByDescending<TSource>(this IQueryable<TSource> source, LambdaExpression expression)
        {
            var type = typeof(TSource);

            var resultExpression = Expression.Call(
                    typeof(Queryable),
                    nameof(OrderByDescending),
                    new Type[] { type, expression.ReturnType },
                    source.Expression,
                    Expression.Quote(expression));
            return (IOrderedQueryable<TSource>)source.Provider.CreateQuery(resultExpression);
        }
    }
}