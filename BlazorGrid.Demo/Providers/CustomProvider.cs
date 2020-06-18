using BlazorGrid.Abstractions;
using BlazorGrid.Abstractions.Extensions;
using BlazorGrid.Abstractions.Filters;
using BlazorGrid.Demo.Models;
using BlazorGrid.Providers;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace BlazorGrid.Demo.Providers
{
    public class CustomProvider : DefaultHttpProvider
    {
        private readonly HttpClient http;

        public CustomProvider(HttpClient http) : base(http)
        {
            this.http = http;
        }

        public override async Task<BlazorGridResult<T>> GetAsync<T>(string BaseUrl, int Offset, int Length, string OrderBy, bool OrderByDescending, string SearchQuery, FilterDescriptor Filter)
        {
            var url = GetRequestUrl(BaseUrl, Offset, Length, OrderBy, OrderByDescending, SearchQuery, Filter);
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

            if (Filter?.Filters.Any() == true)
            {
                var f = Filters.Helpers.FilterHelper.Build<Employee>(Filter);
                data = (data as IQueryable<Employee>).Where(f).Cast<T>();

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