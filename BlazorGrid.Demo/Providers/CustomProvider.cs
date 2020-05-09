using BlazorGrid.Providers;
using System.Net.Http;

namespace BlazorGrid.Demo.Providers
{
    public class CustomProvider : DefaultHttpProvider
    {
        public CustomProvider(HttpClient http) : base(http) { }

        protected override string GetRequestUrl(string BaseUrl, int Offset, int Length, string OrderBy, bool OrderByDescending, string SearchQuery)
        {
            return BaseUrl;
        }
    }
}