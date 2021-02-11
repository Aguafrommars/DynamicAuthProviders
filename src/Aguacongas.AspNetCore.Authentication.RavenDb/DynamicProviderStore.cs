// Project: aguacongas/DymamicAuthProviders
// Copyright (c) 2021 @Olivier Lefebvre
using Microsoft.Extensions.Logging;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Aguacongas.AspNetCore.Authentication.RavenDb
{
    /// <summary>
    /// Implement a store for <see cref="NoPersistentDynamicManager{TSchemeDefinition}"/> with EntityFramework.
    /// </summary>
    /// <seealso cref="IDynamicProviderStore{TSchemeDefinition}" />
    public class DynamicProviderStore : DynamicProviderStore<SchemeDefinition>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicProviderStore"/> class.
        /// </summary>
        /// <param name="session">The document session.</param>
        /// <param name="authenticationSchemeOptionsSerializer">The authentication scheme options serializer.</param>
        /// <param name="logger">The logger.</param>
        public DynamicProviderStore(IAsyncDocumentSession session, IAuthenticationSchemeOptionsSerializer authenticationSchemeOptionsSerializer, ILogger<DynamicProviderStore> logger) : base(session, authenticationSchemeOptionsSerializer, logger)
        {
        }
    }

    /// <summary>
    /// Implement a store for <see cref="NoPersistentDynamicManager{TSchemeDefinition}"/> with EntityFramework.
    /// </summary>
    /// <typeparam name="TSchemeDefinition">The type of the definition.</typeparam>
    public class DynamicProviderStore<TSchemeDefinition> : IDynamicProviderStore<TSchemeDefinition>
        where TSchemeDefinition : SchemeDefinition, new()
    {
        private readonly IAsyncDocumentSession _session;
        private readonly IAuthenticationSchemeOptionsSerializer _authenticationSchemeOptionsSerializer;
        private readonly ILogger<DynamicProviderStore<TSchemeDefinition>> _logger;

        /// <summary>
        /// Gets the scheme definitions list.
        /// </summary>
        /// <value>
        /// The scheme definitions list.
        /// </value>
        public virtual IQueryable<TSchemeDefinition> SchemeDefinitions => _session.Query<SerializedData>()
            .ToListAsync().ConfigureAwait(false).GetAwaiter().GetResult()
            .Select(Deserialize)
            .AsQueryable();

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicProviderStore{TSchemeDefinition}"/> class.
        /// </summary>
        /// <param name="session">The document session.</param>
        /// <param name="authenticationSchemeOptionsSerializer">The authentication scheme options serializer.</param>
        /// <param name="logger">The logger.</param>
        /// <exception cref="ArgumentNullException">
        /// context
        /// or
        /// authenticationSchemeOptionsSerializer
        /// or
        /// logger
        /// </exception>
        public DynamicProviderStore(IAsyncDocumentSession session, IAuthenticationSchemeOptionsSerializer authenticationSchemeOptionsSerializer, ILogger<DynamicProviderStore<TSchemeDefinition>> logger)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
            _authenticationSchemeOptionsSerializer = authenticationSchemeOptionsSerializer ?? throw new ArgumentNullException(nameof(authenticationSchemeOptionsSerializer));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Adds a defnition asynchronously.
        /// </summary>
        /// <param name="definition">The definition.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">definition</exception>
        public virtual async Task AddAsync(TSchemeDefinition definition, CancellationToken cancellationToken = default)
        {
            definition = definition ?? throw new ArgumentNullException(nameof(definition));
            
            cancellationToken.ThrowIfCancellationRequested();

            var data = Serialize(definition);
            await _session.StoreAsync(data, cancellationToken).ConfigureAwait(false);
            await _session.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            _logger.LogInformation("Scheme {scheme} added for {handlerType} with options: {options}", definition.Scheme, definition.HandlerType, data.SerializedOptions);
        }

        /// <summary>
        /// Removes a scheme definition asynchronous.
        /// </summary>
        /// <param name="definition">The definition.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">definition</exception>
        public virtual async Task RemoveAsync(TSchemeDefinition definition, CancellationToken cancellationToken = default)
        {
            definition = definition ?? throw new ArgumentNullException(nameof(definition));
            
            cancellationToken.ThrowIfCancellationRequested();

            var data = await _session.LoadAsync<SerializedData>(definition.Scheme, cancellationToken).ConfigureAwait(false);
            _session.Delete(data);
            await _session.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            _logger.LogInformation("Scheme {scheme} removed", definition.Scheme);
        }

        /// <summary>
        /// Updates a scheme definition asynchronous.
        /// </summary>
        /// <param name="definition">The definition.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">definition</exception>
        public virtual async Task UpdateAsync(TSchemeDefinition definition, CancellationToken cancellationToken = default)
        {
            definition = definition ?? throw new ArgumentNullException(nameof(definition));
            
            cancellationToken.ThrowIfCancellationRequested();

            var serialized = Serialize(definition);
            
            var data = await _session.LoadAsync<SerializedData>(definition.Scheme, cancellationToken).ConfigureAwait(false);

            data.SerializedOptions = serialized.SerializedOptions;
            data.SerializedHandlerType = serialized.SerializedHandlerType;

            await _session.StoreAsync(data).ConfigureAwait(false);
            await _session.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            _logger.LogInformation("Scheme {scheme} updated for {handlerType} with options: {options}", definition.Scheme, definition.HandlerType, data.SerializedOptions);
        }

        /// <summary>
        /// Finds scheme definition by scheme asynchronous.
        /// </summary>
        /// <param name="scheme">The scheme.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// An instance of TSchemeDefinition or null.
        /// </returns>
        /// <exception cref="ArgumentException">Parameter {nameof(scheme)}</exception>
        public virtual async Task<TSchemeDefinition> FindBySchemeAsync(string scheme, CancellationToken cancellationToken = default)
        {
            CheckScheme(scheme);

            cancellationToken.ThrowIfCancellationRequested();
            var data = await _session.LoadAsync<SerializedData>(scheme, cancellationToken).ConfigureAwait(false);

            if (data != null)
            {
                return Deserialize(data);
            }

            return null;
        }

        private static void CheckScheme(string scheme)
        {
            if (string.IsNullOrWhiteSpace(scheme))
            {
                throw new ArgumentException($"Parameter {nameof(scheme)} cannor be null or empty");
            }
        }

        private SerializedData Serialize(TSchemeDefinition definition)
        {
            return new SerializedData
            {
                Id = definition.Scheme,
                SerializedHandlerType = _authenticationSchemeOptionsSerializer.SerializeType(definition.HandlerType),
                SerializedOptions = _authenticationSchemeOptionsSerializer.SerializeOptions(definition.Options, definition.HandlerType.GetAuthenticationSchemeOptionsType())
            };
        }

        private TSchemeDefinition Deserialize(SerializedData data)
        {            
            var handlerType = _authenticationSchemeOptionsSerializer.DeserializeType(data.SerializedHandlerType);
            return new TSchemeDefinition
            {
                Scheme = data.Id,
                HandlerType = handlerType,
                Options = _authenticationSchemeOptionsSerializer.DeserializeOptions(data.SerializedOptions, handlerType.GetAuthenticationSchemeOptionsType())
            };
        }

        class SerializedData
        {
            public string Id { get; set; }

            public string SerializedOptions { get; set; }

            public string SerializedHandlerType { get; set; }
            
        }
    }
}
