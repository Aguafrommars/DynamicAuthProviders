using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Aguacongas.AspNetCore.Authentication
{
    public interface IDynamicProviderStore
    {
        IQueryable<ProviderDefinition> ProviderDefinitions { get; }

        Task Add(ProviderDefinition definition, CancellationToken cancellationToken = default(CancellationToken));
        Task<ProviderDefinition> FindByScheme(string scheme, CancellationToken cancellationToken = default(CancellationToken));
        Task Remove(ProviderDefinition definition, CancellationToken cancellationToken = default(CancellationToken));
        Task Update(ProviderDefinition definition, CancellationToken cancellationToken = default(CancellationToken));
    }
}