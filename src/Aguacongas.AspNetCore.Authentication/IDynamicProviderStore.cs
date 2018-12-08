using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Aguacongas.AspNetCore.Authentication
{
    public interface IDynamicProviderStore
    {
        IQueryable<ProviderDefinition> ProviderDefinitions { get; }

        Task AddAsync(ProviderDefinition definition, CancellationToken cancellationToken = default(CancellationToken));
        Task<ProviderDefinition> FindBySchemeAsync(string scheme, CancellationToken cancellationToken = default(CancellationToken));
        Task RemoveAsync(ProviderDefinition definition, CancellationToken cancellationToken = default(CancellationToken));
        Task UpdateAsync(ProviderDefinition definition, CancellationToken cancellationToken = default(CancellationToken));
    }
}