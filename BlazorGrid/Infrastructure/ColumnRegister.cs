using BlazorGrid.Abstractions.Helpers;
using BlazorGrid.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace BlazorGrid.Infrastructure
{
    internal class ColumnRegister<TRow> : IColumnRegister
    {
        public readonly ICollection<IGridCol> Columns = new List<IGridCol>();

        public void Register(IGridCol col)
        {
            if (!Columns.Any(x => ReferenceEquals(x, col)))
            {
                Columns.Add(col);
            }
        }

        public string GetPropertyName<T>(Expression<Func<T>> property)
        {
            return ExpressionHelper.GetPropertyName<TRow, T>(property);
        }

        public override int GetHashCode()
        {
            var hashes = Columns.Select(x => x.GetHashCode().ToString());
            return string.Join(',', hashes).GetHashCode();
        }
    }
}
