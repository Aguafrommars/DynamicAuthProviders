// Project: aguacongas/DymamicAuthProviders
// Copyright (c) 2021 @Olivier Lefebvre
using System.Threading;
using System.Threading.Tasks;

namespace Aguacongas.AspNetCore.Authentication.Redis
{
    public interface IDynamicProviderMutationStore<TSchemeDefinition> where TSchemeDefinition : SchemeDefinition, new()
    {
        Task AddAsync(TSchemeDefinition definition, CancellationToken cancellationToken = default);
        Task RemoveAsync(TSchemeDefinition definition, CancellationToken cancellationToken = default);
        Task UpdateAsync(TSchemeDefinition definition, CancellationToken cancellationToken = default);
    }
}