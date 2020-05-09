namespace BlazorGrid.Interfaces
{
    public interface IBlazorGrid<TRow>
    {
        void Add(IGridCol<TRow> Column);
    }
}