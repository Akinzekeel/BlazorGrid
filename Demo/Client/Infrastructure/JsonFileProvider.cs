using System.Threading;
using System;
using System.Security.AccessControl;
using System.Data.Common;
using System.Data;
using System.Runtime.CompilerServices;
using System.Net;
using System.Threading.Tasks;
using BlazorGrid.Interfaces;
using BlazorGrid.Abstractions.Models;
using BlazorGrid.Abstractions.Interfaces;
using System.Net.Http;
using Demo.Shared;
using System.Collections.Generic;
using System.Linq;

namespace Demo.Client.Infrastructure
{
    public class JsonFileProvider : IGridProvider
    {
        private readonly HttpClient http;

        public JsonFileProvider(HttpClient http)
        {
            this.http = http;
        }

        public async Task<DataPageResult<T>> GetAsync<T>(string requestUrl)
        {
            var result = await http.GetAsync(requestUrl);
            var content = await result.Content.ReadAsStringAsync();
            var rows = System.Text.Json.JsonSerializer.Deserialize<IEnumerable<T>>(content);
            var data = rows.ToList();

            return new DataPageResult<T> {
                TotalCount = data.Count,
                Data = data
            };
        }

        public string GetRequestUrl(string BaseUrl, string RowId)
        {
            return BaseUrl;
        }

        public string GetRequestUrl(string BaseUrl, int Offset, int Length, string OrderBy, bool OrderByDescending, string SearchQuery)
        {
            return BaseUrl;
        }
    }
}