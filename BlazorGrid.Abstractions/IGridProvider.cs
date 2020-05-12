using System.Threading.Tasks;
using BlazorGrid.Abstractions;

namespace BlazorGrid.Abstractions
{
    public interface IGridProvider
    {
        [Obsolete("This method was required for a feature called smart-refresh. However the feature has been cancelled and this method may be removed in the future.")]
        Task<T> ReloadAsync<T>(string BaseUrl, T Row);
        Task<BlazorGridResult<T>> GetAsync<T>(string BaseUrl, int Offset, int Length, string OrderBy, bool OrderByDescending, string SearchQuery);
    }
}