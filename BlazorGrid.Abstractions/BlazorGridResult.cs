using System.Collections.Generic;

namespace BlazorGrid.Abstractions
{
    public class BlazorGridResult<T>
    {
        public int TotalCount { get; set; }
        public IReadOnlyList<T> Data { get; set; }
    }
}