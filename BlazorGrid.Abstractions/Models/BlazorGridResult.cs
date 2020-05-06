using System.Collections.Generic;

namespace BlazorGrid.Abstractions.Models
{
    public class DataPageResult<T>
    {
        public int TotalCount { get; set; }
        public IReadOnlyList<T> Data { get; set; }
    }
}