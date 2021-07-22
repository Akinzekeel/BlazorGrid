namespace BlazorGrid.Infrastructure
{
    internal class RowWrapper<TRow>
    {
        public RowWrapper(TRow row, int index)
        {
            Row = row;
            Index = index;
        }

        public readonly TRow Row;
        public readonly int Index;
    }
}
