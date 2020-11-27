using BlazorGrid.Abstractions;
using BlazorGrid.Abstractions.Extensions;
using BlazorGrid.Abstractions.Filters;
using BlazorGrid.Demo.Models;
using BlazorGrid.Providers;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace BlazorGrid.Demo.Providers
{
    public class CustomProvider : DefaultHttpProvider
    {
        private readonly HttpClient http;
        internal static int ArtificialDelayMs { get; set; }

        public CustomProvider(HttpClient http) : base(http)
        {
            this.http = http;
        }

        public override async Task<BlazorGridResult<T>> GetAsync<T>(
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

            var httpCancellationToken = new CancellationTokenSource();
            var httpTask = http.GetAsync(url, httpCancellationToken.Token);

            Task delay;

            if (ArtificialDelayMs <= 0)
            {
                delay = Task.CompletedTask;
            }
            else
            {
                delay = Task.Delay(ArtificialDelayMs);
            }

            while (!httpTask.IsCompleted)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    httpCancellationToken.Cancel();
                    return null;
                }

                await Task.Delay(150);
            }

            await delay;

            var result = await DeserializeJsonAsync<BlazorGridResult<T>>(httpTask.Result);
            var totalCount = result.TotalCount;

            var data = result.Data.AsQueryable();

            if (orderBy != null)
            {
                if (orderByDescending)
                {
                    data = data.OrderByDescending(orderBy);
                }
                else
                {
                    data = data.OrderBy(orderBy);
                }
            }

            if (!string.IsNullOrEmpty(searchQuery) && data is IQueryable<Employee> employees)
            {
                data = employees.Where(x =>
                    x.Email.IndexOf(searchQuery, StringComparison.CurrentCultureIgnoreCase) > -1
                    || x.FirstName.IndexOf(searchQuery, StringComparison.CurrentCultureIgnoreCase) == 0
                    || x.LastName.IndexOf(searchQuery, StringComparison.CurrentCultureIgnoreCase) == 0
                    || x.Id.ToString() == searchQuery
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
                Data = data.Skip(offset).Take(length).ToList()
            };

            return finalResult;
        }
    }
}