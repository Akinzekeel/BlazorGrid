using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace BlazorGrid.Interfaces
{
    public interface IBlazorGrid
    {
        string OrderByPropertyName { get; }
        bool OrderByDescending { get; }
        void Add(IGridCol col);
        Task TryApplySorting<T>(Expression<Func<T>> Property);
        bool IsFilteredBy<T>(Expression<Func<T>> Property);
        bool IsSortedBy<T>(Expression<Func<T>> Property);
        IEnumerable<IGridCol> Columns { get; }
        string GetPropertyName<T>(Expression<Func<T>> Property);
    }
}