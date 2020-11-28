using BlazorGrid.Abstractions.Filters;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace BlazorGrid.Interfaces
{
    public interface IBlazorGrid
    {
        string OrderByPropertyName { get; }
        bool OrderByDescending { get; }
        IEnumerable<IGridCol> Columns { get; }
        FilterDescriptor Filter { get; }
    }
}