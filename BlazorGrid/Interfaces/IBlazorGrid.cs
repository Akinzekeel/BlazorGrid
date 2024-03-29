using System.Threading.Tasks;

namespace BlazorGrid.Interfaces
{
    public interface IBlazorGrid
    {
        Task ReloadAsync();
        void SetHighlight(int index);
        void ClearHighlight();
    }
}