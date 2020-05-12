using System;

namespace BlazorGrid.Abstractions
{
    [Obsolete("This interface was necessary for a so-called smart refresh feature. This feature has been deprecated, therefore this interface is no longer necessary.")]
    public interface IGridRow
    {
        string RowId { get; }
    }
}