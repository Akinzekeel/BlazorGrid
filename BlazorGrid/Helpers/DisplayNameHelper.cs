using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace BlazorGrid.Helpers
{
    internal static class DisplayNameHelper
    {
        public static string GetDisplayName<T>(Expression<Func<T>> accessor)
        {
            string result = null;
            MemberExpression memberExpr = accessor?.Body as MemberExpression;

            if (memberExpr == null)
            {
                if (accessor is UnaryExpression unaryExpr && unaryExpr.NodeType == ExpressionType.Convert)
                {
                    memberExpr = unaryExpr.Operand as MemberExpression;
                }
            }

            if (memberExpr != null && memberExpr.Member.MemberType == MemberTypes.Property)
            {
                var member = memberExpr.Member;

                var attrs = member.GetCustomAttributes(typeof(DisplayAttribute), false);

                if (attrs.Any())
                {

                    if (((DisplayAttribute)attrs[0]).ResourceType != null)
                    {
                        result = ((DisplayAttribute)attrs[0]).GetName();
                    }
                    else
                    {
                        result = ((DisplayAttribute)attrs[0]).Name;
                    }
                }
            }

            return result;
        }
    }
}
