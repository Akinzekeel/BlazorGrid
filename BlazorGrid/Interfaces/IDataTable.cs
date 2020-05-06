namespace BlazorGrid.Interfaces
{
    public interface IBlazorGrid<TRow>
    {
        void Add(IColumn<TRow> Column);
    }
}