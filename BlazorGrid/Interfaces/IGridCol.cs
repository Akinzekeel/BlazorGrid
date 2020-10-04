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
        bool IsRegistered { get; }
        string Caption { get; }
        RenderFragment ChildContent { get; }
        string CssClass { get; }
        bool AlignRight { get; }
        bool FitToContent { get; }
        bool IsFilterable { get; }
        Expression For { get; }
        string PropertyName { get; }
        string SortIconCssClass();

        /// <summary>
        /// Returns the Caption property if it is not null,
        /// otherwise will try to get the Display(Name) 
        /// of the For Expression (if any)
        /// </summary>
        /// <returns>A caption or null</returns>
        string GetCaptionOrDefault();
        void Unlink();
    }
}