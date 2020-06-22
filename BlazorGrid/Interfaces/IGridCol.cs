using Microsoft.AspNetCore.Components;
using System;
using System.Linq.Expressions;

namespace BlazorGrid.Interfaces
{
    public interface IGridCol<T> : IGridCol
    {
        new Expression<Func<T>> For { get; }
    }

    public interface IGridCol
    {
        string Caption { get; }
        RenderFragment ChildContent { get; }
        string CssClass { get; }
        bool AlignRight { get; }
        bool FitToContent { get; }
        bool IsFilterable { get; }
        Expression For { get; }
    }
}