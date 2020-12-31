using BlazorGrid.Demo.Interfaces;
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace BlazorGrid.Demo.Infrastructure
{
    public abstract class CommonPageBase : ComponentBase
    {
        [Inject] private ITitleService TitleService { get; set; }
        [Inject] protected Providers.CustomProvider Provider { get; set; }

        protected abstract string Title { get; }

        protected override async Task OnInitializedAsync()
        {
            await TitleService.SetHtmlTitleAsync(Title);
        }
    }
}
