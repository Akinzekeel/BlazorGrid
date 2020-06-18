using BlazorGrid.Abstractions.Filters;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BlazorGrid.Interfaces
{
    public interface IBlazorGrid
    {
        string OrderByPropertyName { get; }
        bool OrderByDescending { get; }

        void Add(IGridCol col);
        Task TryApplySorting(string PropertyName);

        /// <summary>
        /// Provide the appropiate type for the given
        /// property of the grid model. This is used
        /// to determine which kind of filter editor
        /// is being presented.
        /// </summary>
        /// <param name="PropertyName">The name of the property on the model</param>
        /// <returns>The filter-compatible type of the property</returns>
        PropertyType GetTypeFor(string PropertyName);
        bool IsFilteredBy(string PropertyName);
        IEnumerable<IGridCol> Columns { get; }
    }
}