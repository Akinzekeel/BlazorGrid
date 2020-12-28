using System.Threading.Tasks;

namespace BlazorGrid.Demo.Interfaces
{
    internal interface ITitleService
    {
        ValueTask SetHtmlTitleAsync(string newTitle);
    }
}
