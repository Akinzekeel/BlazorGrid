using System.Threading;
using System.Threading.Tasks;

namespace BlazorGrid.Abstractions
{
    public delegate ValueTask<BlazorGridResult<T>> ProviderDelegate<T>(
        BlazorGridRequest request,
        CancellationToken cancellationToken
    );
}
