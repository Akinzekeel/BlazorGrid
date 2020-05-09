using System.Collections.Generic;

namespace BlazorGrid.Abstractions
{
    public class DataPageResult<T>
    {
        public int TotalCount { get; set; }
        public IReadOnlyList<T> Data { get; set; }
    }
}