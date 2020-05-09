using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using BlazorGrid.Abstractions.Models;
using BlazorGrid.Abstractions.Interfaces;

namespace BlazorGrid.Providers
{
    public class DefaultHttpProvider : IGridProvider
    {
        private readonly HttpClient http;

        public DefaultHttpProvider(HttpClient http)
        {
            this.http = http;
        }

        public async Task<DataPageResult<T>> GetAsync<T>(string requestUrl)
        {
            var result = await http.GetAsync(requestUrl);

            if (!result.IsSuccessStatusCode)
            {
                if (result.StatusCode == HttpStatusCode.NotFound)
                {
                    return default;
                }
                else
                {
                    result.EnsureSuccessStatusCode();
                }
            }

            var content = await result.Content.ReadAsStringAsync();
            return System.Text.Json.JsonSerializer.Deserialize<DataPageResult<T>>(content);
        }

        public string GetRequestUrl(string BaseUrl, string RowId)
        {
            return BaseUrl.TrimEnd('/') + '/' + RowId + "?More=false";
        }

        public string GetRequestUrl(string BaseUrl, int Offset, int Length, string OrderBy, bool OrderByDescending, string SearchQuery)
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