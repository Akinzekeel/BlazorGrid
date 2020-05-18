using BlazorGrid.Providers;
using System.Net.Http;
using System.Linq;
using System.Threading.Tasks;
using BlazorGrid.Abstractions;
using BlazorGrid.Abstractions.Extensions;
using BlazorGrid.Demo.Models;
using System;

namespace BlazorGrid.Demo.Providers
{
    public class CustomProvider : DefaultHttpProvider
    {
        private readonly HttpClient http;

        public CustomProvider(HttpClient http) : base(http)
        {
            this.http = http;
        }

        public override async Task<BlazorGridResult<T>> GetAsync<T>(string BaseUrl, int Offset, int Length, string OrderBy, bool OrderByDescending, string SearchQuery)
        {
            var url = GetRequestUrl(BaseUrl, Offset, Length, OrderBy, OrderByDescending, SearchQuery);
            var response = await http.GetAsync(url);
            var result = await DeserializeJsonAsync<BlazorGridResult<T>>(response);
            var totalCount = result.TotalCount;

            var data = result.Data.AsQueryable();

            if (OrderBy != null)
            {
                if (OrderByDescending)
                {
                    data = data.OrderByDescending(OrderBy);
                }
                else
                {
                    data = data.OrderBy(OrderBy);
                }
            }

            if (!string.IsNullOrEmpty(SearchQuery) && data is IQueryable<Employee> employees)
            {
                data = employees.Where(x =>
                    x.Email.IndexOf(SearchQuery, StringComparison.CurrentCultureIgnoreCase) > -1
                    || x.FirstName.IndexOf(SearchQuery, StringComparison.CurrentCultureIgnoreCase) == 0
                    || x.LastName.IndexOf(SearchQuery, StringComparison.CurrentCultureIgnoreCase) == 0
                    || x.Id.ToString() == SearchQuery
                ).Cast<T>();

                totalCount = data.Count();
            }

            var finalResult = new BlazorGridResult<T>
            {
                TotalCount = totalCount,
                Data = data.Skip(Offset).Take(Length).ToList()
            };

            return finalResult;
        }
    }
}