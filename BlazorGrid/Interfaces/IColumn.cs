using Microsoft.AspNetCore.Components;

namespace BlazorGrid.Interfaces
{
    public interface IColumn<TRow>
    {
        string Caption { get; }
        RenderFragment<TRow> ChildContent { get; }
        string CssClass { get; }
        bool AlignRight { get; }
        string SortablePropertyName { get; }
        bool FitToContent { get; }
    }
}