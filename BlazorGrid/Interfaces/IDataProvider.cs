using System.Threading.Tasks;

namespace BlazorGrid.Interfaces
{
    public interface IDataProvider
    {
        Task<T> GetAsync<T>(string requestUrl);
        string GetRequestUrl(string BaseUrl, int Offset, int Length, string OrderBy, bool OrderByDescending, string SearchQuery);
    }
}