using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace BlazorGrid.Demo.Infrastructure
{
    public abstract class CommonPageBase : ComponentBase
    {
        [CascadingParameter] private App App { get; set; }

        protected abstract string Title { get; }

        protected override async Task OnInitializedAsync()
        {
            await App.SetHtmlTitleAsync(Title);
        }
    }
}
