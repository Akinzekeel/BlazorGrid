using System.Threading.Tasks;
using BlazorGrid.Abstractions;

namespace BlazorGrid.Abstractions
{
    public interface IGridProvider
    {
        Task<T> GetAsync<T>(string BaseUrl, string RowId);
        Task<DataPageResult<T>> GetAsync<T>(string BaseUrl, int Offset, int Length, string OrderBy, bool OrderByDescending, string SearchQuery);
    }
}