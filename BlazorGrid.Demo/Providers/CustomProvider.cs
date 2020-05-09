using BlazorGrid.Providers;
using System.Net.Http;
using System.Linq;
using System.Threading.Tasks;
using BlazorGrid.Abstractions;

namespace BlazorGrid.Demo.Providers
{
    public class CustomProvider : DefaultHttpProvider
    {
        public CustomProvider(HttpClient http) : base(http) { }

        public override async Task<T> GetAsync<T>(string BaseUrl, string RowId)
        {
            var url = GetRequestUrl(BaseUrl, RowId);
            var response = await http.GetAsync(url);
            var result = await DeserializeJsonAsync<T>(response);

            return result.Data.FirstOrDefault(x => x.RowId == RowId);
        }

        public override async Task<DataPageResult<T>> GetAsync<T>(string BaseUrl, int Offset, int Length, string OrderBy, bool OrderByDescending, string SearchQuery)
        {
            var url = GetRequestUrl(BaseUrl, Offset, Length, OrderBy, OrderByDescending, SearchQuery);
            var response = await http.GetAsync(url);
            var result = await DeserializeJsonAsync<DataPageResult<T>>(response);

            var finalResult = new DataPageResult<T>
            {
                TotalCount = result.TotalCount,
                Data = result.Data.Skip(Offset).Take(Length)
            };

            return finalResult;
        }
    }
}