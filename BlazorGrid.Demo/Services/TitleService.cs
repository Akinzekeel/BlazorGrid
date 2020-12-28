using BlazorGrid.Demo.Interfaces;
using Microsoft.JSInterop;
using System.Threading.Tasks;

namespace BlazorGrid.Demo.Services
{
    public class TitleService : ITitleService
    {
        private readonly IJSRuntime Js;

        public TitleService(IJSRuntime js)
        {
            Js = js;
        }

        ValueTask ITitleService.SetHtmlTitleAsync(string newTitle)
        {
            var t = "BlazorGrid";

            if (!string.IsNullOrEmpty(newTitle))
            {
                t += " – " + newTitle;
            }

            return Js.InvokeVoidAsync("setDocumentTitle", t);
        }
    }
}
