using BlazorGrid.Abstractions;

namespace BlazorGrid.Demo.Models
{
    public class ExchangeRate : IGridRow
    {
        public string Id { get; set; }
        public decimal Rate { get; set; }
        public string RowId => Id;
    }
}