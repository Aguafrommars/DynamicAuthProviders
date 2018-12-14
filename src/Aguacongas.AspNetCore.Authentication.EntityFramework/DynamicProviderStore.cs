// Project: aguacongas/DymamicAuthProviders
// Copyright (c) 2018 @Olivier Lefebvre
using Microsoft.Extensions.DependencyModel;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Aguacongas.AspNetCore.Authentication.EntityFramework
{
    /// <summary>
    /// Implement a store for <see cref="NoPersistentDynamicManager{TSchemeDefinition}"/> with EntityFramework.
    /// </summary>
    /// <typeparam name="TSchemeDefinition">The type of the definition.</typeparam>
    /// <seealso cref="Aguacongas.AspNetCore.Authentication.IDynamicProviderStore{TSchemeDefinition}" />
    public class DynamicProviderStore<TSchemeDefinition> : IDynamicProviderStore<TSchemeDefinition>
        where TSchemeDefinition: SchemeDefinition, new()
    {
        private static JsonSerializerSettings _jsonSerializerSettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            Formatting = Formatting.None
        };

        private readonly SchemeDbContext<TSchemeDefinition> _context;
        private readonly IAuthenticationSchemeOptionsSerializer _authenticationSchemeOptionsSerializer;
        private readonly ILogger<DynamicProviderStore<TSchemeDefinition>> _logger;

        /// <summary>
        /// Gets the scheme definitions list.
        /// </summary>
        /// <value>
        /// The scheme definitions list.
        /// </value>
        public virtual IQueryable<TSchemeDefinition> SchemeDefinitions => _context.Providers
            .ToList()
            .Select(definition =>
            {
                Deserialize(definition);
                return definition;
            })
            .AsQueryable();

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicProviderStore{TSchemeDefinition}"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="authenticationSchemeOptionsSerializer">The authentication scheme options serializer.</param>
        /// <param name="logger">The logger.</param>
        /// <exception cref="System.ArgumentNullException">
        /// context
        /// or
        /// authenticationSchemeOptionsSerializer
        /// or
        /// logger
        /// </exception>
        public DynamicProviderStore(SchemeDbContext<TSchemeDefinition> context, IAuthenticationSchemeOptionsSerializer authenticationSchemeOptionsSerializer, ILogger<DynamicProviderStore<TSchemeDefinition>> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _authenticationSchemeOptionsSerializer = authenticationSchemeOptionsSerializer ?? throw new ArgumentNullException(nameof(authenticationSchemeOptionsSerializer));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Adds a defnition asynchronously.
        /// </summary>
        /// <param name="definition">The definition.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">definition</exception>
        public virtual async Task AddAsync(TSchemeDefinition definition, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (definition == null)
            {
                throw new ArgumentNullException(nameof(definition));
            }
            cancellationToken.ThrowIfCancellationRequested();

            Serialize(definition);

            _context.Add(definition);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Scheme {scheme} added for {handlerType} with options: {options}", definition.Scheme, definition.HandlerType, definition.SerializedOptions);
        }

        /// <summary>
        /// Removes a scheme definition asynchronous.
        /// </summary>
        /// <param name="definition">The definition.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">definition</exception>
        public virtual async Task RemoveAsync(TSchemeDefinition definition, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (definition == null)
            {
                throw new ArgumentNullException(nameof(definition));
            }
            cancellationToken.ThrowIfCancellationRequested();
            _context.Remove(definition);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Scheme {scheme} removed", definition.Scheme);
        }

        /// <summary>
        /// Updates a scheme definition asynchronous.
        /// </summary>
        /// <param name="definition">The definition.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">definition</exception>
        public virtual async Task UpdateAsync(TSchemeDefinition definition, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (definition == null)
            {
                throw new ArgumentNullException(nameof(definition));
            }
            cancellationToken.ThrowIfCancellationRequested();
            _context.Attach(definition);

            Serialize(definition);
            definition.ConcurrencyStamp = Guid.NewGuid().ToString();

            _context.Update(definition);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Scheme {scheme} updated for {handlerType} with options: {options}", definition.Scheme, definition.HandlerType, definition.SerializedOptions);
        }

        /// <summary>
        /// Finds scheme definition by scheme asynchronous.
        /// </summary>
        /// <param name="scheme">The scheme.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// An instance of TSchemeDefinition or null.
        /// </returns>
        /// <exception cref="System.ArgumentException">Parameter {nameof(scheme)}</exception>
        public virtual async Task<TSchemeDefinition> FindBySchemeAsync(string scheme, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrWhiteSpace(scheme))
            {
                throw new ArgumentException($"Parameter {nameof(scheme)} cannor be null or empty");
            }

            cancellationToken.ThrowIfCancellationRequested();
            var definition = await _context.FindAsync<TSchemeDefinition>(new[] { scheme }, cancellationToken);

            Deserialize(definition);

            return definition;
        }

        private void Serialize(TSchemeDefinition definition)
        {
            definition.SerializedHandlerType = _authenticationSchemeOptionsSerializer.SerializeType(definition.HandlerType);
            definition.SerializedOptions = _authenticationSchemeOptionsSerializer.SerializeOptions(definition.Options, definition.HandlerType.GetAuthenticationSchemeOptionsType());
        }

        private void Deserialize(TSchemeDefinition definition)
        {
            definition.HandlerType = _authenticationSchemeOptionsSerializer.DeserializeType(definition.SerializedHandlerType);
            definition.Options = _authenticationSchemeOptionsSerializer.DeserializeOptions(definition.SerializedOptions, definition.HandlerType.GetAuthenticationSchemeOptionsType());
        }
    }
}
