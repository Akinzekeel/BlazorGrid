using BlazorGrid.Abstractions.Filters;
using System.Threading.Tasks;

namespace BlazorGrid.Abstractions
{
    public interface IGridProvider
    {
        Task<BlazorGridResult<T>> GetAsync<T>(string BaseUrl, int Offset, int Length, string OrderBy, bool OrderByDescending, string SearchQuery, FilterDescriptor Filter);
    }
}