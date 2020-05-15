using System;
using System.Threading.Tasks;
using BlazorGrid.Abstractions;

namespace BlazorGrid.Abstractions
{
    public interface IGridProvider
    {
        Task<BlazorGridResult<T>> GetAsync<T>(string BaseUrl, int Offset, int Length, string OrderBy, bool OrderByDescending, string SearchQuery);
    }
}