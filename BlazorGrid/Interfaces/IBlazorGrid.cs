using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace BlazorGrid.Interfaces
{
    public interface IBlazorGrid
    {
        string OrderByPropertyName { get; }
        bool OrderByDescending { get; }
        void Add(IGridCol col);
        IEnumerable<IGridCol> Columns { get; }
        string GetPropertyName<T>(Expression<Func<T>> Property);
        bool IsSortedBy(IGridCol column);
        bool IsFilteredBy(IGridCol column);
    }
}