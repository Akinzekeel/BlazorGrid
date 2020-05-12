using System.Threading.Tasks;
using BlazorGrid.Abstractions;

namespace BlazorGrid.Abstractions
{
    public interface IGridProvider
    {
        Task<T> ReloadAsync<T>(string BaseUrl, T Row);
        Task<BlazorGridResult<T>> GetAsync<T>(string BaseUrl, int Offset, int Length, string OrderBy, bool OrderByDescending, string SearchQuery);
    }
}