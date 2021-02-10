// Project: aguacongas/DymamicAuthProviders
// Copyright (c) 2021 @Olivier Lefebvre
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Aguacongas.AspNetCore.Authentication
{
    /// <summary>
    /// Interface for store use by <see cref="NoPersistentDynamicManager{TSchemeDefinition}"/>
    /// </summary>
    /// <typeparam name="TSchemeDefinition">The type of the scheme definition.</typeparam>
    public interface IDynamicProviderStore<TSchemeDefinition> 
        where TSchemeDefinition: SchemeDefinitionBase, new()
    {
        /// <summary>
        /// Gets the scheme definitions list.
        /// </summary>
        /// <value>
        /// The scheme definitions list.
        /// </value>
        IQueryable<TSchemeDefinition> SchemeDefinitions { get; }

        /// <summary>
        /// Adds a defnition asynchronously.
        /// </summary>
        /// <param name="definition">The definition.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task AddAsync(TSchemeDefinition definition, CancellationToken cancellationToken = default(CancellationToken));
        /// <summary>
        /// Finds scheme definition by scheme asynchronous.
        /// </summary>
        /// <param name="scheme">The scheme.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>An instance of TSchemeDefinition or null.</returns>
        Task<TSchemeDefinition> FindBySchemeAsync(string scheme, CancellationToken cancellationToken = default(CancellationToken));
        /// <summary>
        /// Removes a scheme definition asynchronous.
        /// </summary>
        /// <param name="definition">The definition.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task RemoveAsync(TSchemeDefinition definition, CancellationToken cancellationToken = default(CancellationToken));
        /// <summary>
        /// Updates a scheme definition asynchronous.
        /// </summary>
        /// <param name="definition">The definition.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task UpdateAsync(TSchemeDefinition definition, CancellationToken cancellationToken = default(CancellationToken));
    }
}