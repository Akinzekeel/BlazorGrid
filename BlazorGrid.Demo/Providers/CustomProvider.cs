using BlazorGrid.Abstractions;
using BlazorGrid.Abstractions.Extensions;
using BlazorGrid.Demo.Interfaces;
using BlazorGrid.Demo.Models;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace BlazorGrid.Demo.Providers
{
    public class CustomProvider : ICustomProvider
    {
        private readonly string Url;
        private readonly HttpClient Http;
        internal static int ArtificialDelayMs { get; set; }

        public CustomProvider(HttpClient http, string url)
        {
            Url = url;
            Http = http;
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

        public async ValueTask<BlazorGridResult<T>> GetAsync<T>(
            BlazorGridRequest request,
            CancellationToken cancellationToken
        )
        {
            var url = GetRequestUrl(
                Url,
                request.Offset,
                request.Length,
                request.OrderBy,
                request.OrderByDescending,
                request.Query
            );

            var httpTask = Http.GetAsync(url, cancellationToken);

            Task delay;

            if (ArtificialDelayMs <= 0)
            {
                delay = Task.CompletedTask;
            }
            else
            {
                delay = Task.Delay(ArtificialDelayMs, cancellationToken);
            }

            if (delay != null)
            {
                await Task.WhenAll(delay, httpTask);
            }
            else
            {
                await httpTask;
            }

            var result = await DeserializeJsonAsync<BlazorGridResult<T>>(httpTask.Result);
            var totalCount = result.TotalCount;

            var data = result.Data.AsQueryable();

            if (request.OrderBy != null)
            {
                if (request.OrderByDescending)
                {
                    data = data.OrderByDescending(request.OrderBy);
                }
                else
                {
                    data = data.OrderBy(request.OrderBy);
                }
            }

            if (!string.IsNullOrEmpty(request.Query) && data is IQueryable<Employee> employees)
            {
                data = employees.Where(x =>
                    x.Email.IndexOf(request.Query, StringComparison.CurrentCultureIgnoreCase) > -1
                    || x.FirstName.IndexOf(request.Query, StringComparison.CurrentCultureIgnoreCase) == 0
                    || x.LastName.IndexOf(request.Query, StringComparison.CurrentCultureIgnoreCase) == 0
                    || x.Id.ToString() == request.Query
                ).Cast<T>();

                totalCount = data.Count();
            }

            //if (Filter?.Filters.Any() == true)
            //{
            //    var f = Filters.FilterHelper.Build<Employee>(Filter);
            //    data = (data as IQueryable<Employee>).Where(f).Cast<T>();

            //    totalCount = data.Count();
            //}

            var finalResult = new BlazorGridResult<T>
            {
                TotalCount = totalCount,
                Data = data.Skip(request.Offset).Take(request.Length).ToList()
            };

            return finalResult;
        }

        protected string GetRequestUrl(string BaseUrl, int Offset, int Length, string OrderBy, bool OrderByDescending, string SearchQuery)
        {
            var b = Http.BaseAddress;

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