using BlazorGrid.Abstractions;
using BlazorGrid.Abstractions.Filters;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace BlazorGrid.Providers
{
    public class DefaultHttpProvider : IGridProvider
    {
        private readonly HttpClient http;

        public DefaultHttpProvider(HttpClient http)
        {
            this.http = http;
        }

        public virtual async Task<BlazorGridResult<T>> GetAsync<T>(
            string baseUrl,
            int offset,
            int length,
            string orderBy,
            bool orderByDescending,
            string searchQuery,
            FilterDescriptor filter,
            CancellationToken cancellationToken
        )
        {
            var url = GetRequestUrl(baseUrl, offset, length, orderBy, orderByDescending, searchQuery, filter);
            var response = await http.GetAsync(url, cancellationToken);
            var result = await DeserializeJsonAsync<BlazorGridResult<T>>(response);

            return result;
        }

        protected static async Task<T> DeserializeJsonAsync<T>(HttpResponseMessage response)
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

        protected virtual string GetRequestUrl(string BaseUrl, int Offset, int Length, string OrderBy, bool OrderByDescending, string SearchQuery, FilterDescriptor Filter)
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
                Query = SearchQuery,
                Filter = Filter
            };

            uri.Query = parameters.ToQueryString();

            return uri.Uri.ToString();
        }
    }
}