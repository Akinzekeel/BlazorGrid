using Microsoft.AspNetCore.Components;

namespace BlazorGrid.Interfaces
{
    public interface IGridCol
    {
        string Caption { get; }
        RenderFragment ChildContent { get; }
        string CssClass { get; }
        bool AlignRight { get; }
        string SortablePropertyName { get; }
        bool FitToContent { get; }
    }
}