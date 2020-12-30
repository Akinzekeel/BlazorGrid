using BlazorGrid.Abstractions.Filters;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BlazorGrid.Abstractions
{
    [Obsolete]
    public interface IGridProvider
    {
        Task<BlazorGridResult<T>> GetAsync<T>(
            string baseUrl,
            int offset,
            int length,
            string orderBy,
            bool orderByDescending,
            string searchQuery,
            FilterDescriptor filter,
            CancellationToken cancellationToken
        );
    }
}