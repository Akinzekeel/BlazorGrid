using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace BlazorGrid.Abstractions.Helpers
{
    public static class ExpressionHelper
    {
        public static string GetPropertyName<T>(Expression<Func<T, object>> expression)
            => GetPropertyName(expression.Body);

        public static string GetPropertyName(LambdaExpression expression)
            => GetPropertyName(expression.Body);

        /// <summary>
        /// This method gets information about the property while
        /// being compatible with nullable types. It can also resolve
        /// paths such as (a => a.b.c) into "b.c".
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        private static string GetPropertyName(Expression body)
        {
            while (body.NodeType == ExpressionType.Convert || body.NodeType == ExpressionType.ConvertChecked)
                body = ((UnaryExpression)body).Operand;

            List<string> result = new List<string>();
            MemberExpression me = body as MemberExpression;

            if (me != null && me.Expression.NodeType == ExpressionType.MemberAccess)
            {
                var path = me.Expression as MemberExpression;

                do
                {
                    result.Add(path.Member.Name);
                    path = path.Expression as MemberExpression;
                } while (path != null && path.NodeType == ExpressionType.MemberAccess);
            }

            var propertyName = me?.Member.Name;
            result.Add(propertyName);

            if (result.Count == 0)
                throw new InvalidOperationException("Expression does not refer to a property: " + body.ToString());

            return string.Join(".", result);
        }

        public static LambdaExpression CreatePropertyExpression<T>(string PropertyName)
        {
            var entityType = typeof(T);
            ParameterExpression arg = Expression.Parameter(entityType, "x");

            // The property name might be a nested expression like a.b.c
            var path = PropertyName.Split('.');
            MemberExpression currentPath = null;

            foreach (var p in path)
            {

                if (currentPath == null)
                    currentPath = Expression.Property(arg, p);

                else
                    currentPath = Expression.Property(currentPath, p);

            }

            var selector = Expression.Lambda(currentPath, new ParameterExpression[] { arg });

            return selector;
        }
    }
}