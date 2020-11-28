using System;
using System.Linq.Expressions;

namespace BlazorGrid.Interfaces
{
    internal interface IColumnRegister
    {
        void Register(IGridCol col);
        string GetPropertyName<T>(Expression<Func<T>> accessor);
    }
}
