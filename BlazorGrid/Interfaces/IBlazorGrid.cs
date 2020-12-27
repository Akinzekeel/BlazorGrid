using BlazorGrid.Abstractions.Filters;

namespace BlazorGrid.Interfaces
{
    public interface IBlazorGrid
    {
        FilterDescriptor Filter { get; }
    }
}