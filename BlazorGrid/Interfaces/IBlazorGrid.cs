using System.Threading.Tasks;

namespace BlazorGrid.Interfaces
{
    public interface IBlazorGrid
    {
        string OrderByPropertyName { get; }
        bool OrderByDescending { get; }

        void Add(IGridCol col);
        Task TryApplySorting(string PropertyName);
    }
}