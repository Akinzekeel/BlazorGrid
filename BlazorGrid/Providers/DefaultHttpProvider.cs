using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using BlazorGrid.Abstractions;

namespace BlazorGrid.Providers
{
    public class DefaultHttpProvider : IGridProvider
    {
        private readonly HttpClient http;

        public DefaultHttpProvider(HttpClient http)
        {
            this.http = http;
        }

        public virtual async Task<BlazorGridResult<T>> GetAsync<T>(string BaseUrl, int Offset, int Length, string OrderBy, bool OrderByDescending, string SearchQuery)
        {
            var url = GetRequestUrl(BaseUrl, Offset, Length, OrderBy, OrderByDescending, SearchQuery);
            var response = await http.GetAsync(url);
            var result = await DeserializeJsonAsync<BlazorGridResult<T>>(response);
            return result;
        }

        protected async Task<T> DeserializeJsonAsync<T>(HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    return default;
                }
                else
                {
                    response.EnsureSuccessStatusCode();
                }
            }

            var content = await response.Content.ReadAsStringAsync();
            return System.Text.Json.JsonSerializer.Deserialize<T>(content);
        }

        protected virtual string GetRequestUrl(string BaseUrl, string RowId)
        {
            return BaseUrl.TrimEnd('/') + '/' + RowId + "?More=false";
        }

        protected virtual string GetRequestUrl(string BaseUrl, int Offset, int Length, string OrderBy, bool OrderByDescending, string SearchQuery)
        {
            var b = http.BaseAddress;

            var uri = new UriBuilder(
                b.Scheme,
                b.Host,
                b.Port,
                Path.Combine(b.LocalPath, BaseUrl)
            );

            var parameters = new BlazorGridRequest
            {
                Offset = Offset,
                Length = Length,
                OrderBy = OrderBy,
                OrderByDescending = OrderByDescending,
                Query = SearchQuery
            };

            uri.Query = parameters.ToQueryString();

            return uri.Uri.ToString();
        }
    }
}