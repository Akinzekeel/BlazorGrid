using BlazorGrid.Abstractions;
using System.Threading;
using System.Threading.Tasks;

namespace BlazorGrid.Demo.Interfaces
{
    public interface ICustomProvider
    {
        ValueTask<BlazorGridResult<T>> GetAsync<T>(BlazorGridRequest request, CancellationToken cancellationToken);
    }
}
