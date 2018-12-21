// Project: aguacongas/DymamicAuthProviders
// Copyright (c) 2018 @Olivier Lefebvre

using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Aguacongas.AspNetCore.Authentication.Redis
{
    /// <summary>
    /// Implement a store for <see cref="NoPersistentDynamicManager{TSchemeDefinition}"/> with EntityFramework.
    /// </summary>
    /// <seealso cref="Aguacongas.AspNetCore.Authentication.IDynamicProviderStore{TSchemeDefinition}" />
    public class DynamicProviderStore : DynamicProviderStore<SchemeDefinition>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicProviderStore"/> class.
        /// </summary>
        /// <param name="db">The Redis db.</param>
        /// <param name="authenticationSchemeOptionsSerializer">The authentication scheme options serializer.</param>
        /// <param name="logger">The logger.</param>
        public DynamicProviderStore(IDatabase db, IRedisAuthenticationSchemeOptionsSerializer<SchemeDefinition> authenticationSchemeOptionsSerializer, ILogger<DynamicProviderStore> logger) : base(db, authenticationSchemeOptionsSerializer, logger)
        {
        }
    }

    /// <summary>
    /// Implement a store for <see cref="NoPersistentDynamicManager{TSchemeDefinition}"/> with EntityFramework.
    /// </summary>
    /// <typeparam name="TSchemeDefinition">The type of the definition.</typeparam>
    /// <seealso cref="Aguacongas.AspNetCore.Authentication.IDynamicProviderStore{TSchemeDefinition}" />
    public class DynamicProviderStore<TSchemeDefinition> : IDynamicProviderStore<TSchemeDefinition>
        where TSchemeDefinition : SchemeDefinition, new()
    {
        public const string StoreKey = "schemes";
        public const string ConcurencyKey = "schemes-concurency";

        private readonly IDatabase _db;
        private readonly IRedisAuthenticationSchemeOptionsSerializer<TSchemeDefinition> _authenticationSchemeOptionsSerializer;
        private readonly ILogger<DynamicProviderStore<TSchemeDefinition>> _logger;


        public IQueryable<TSchemeDefinition> SchemeDefinitions => _db.HashGetAll(StoreKey)
            .Select(entry => _authenticationSchemeOptionsSerializer.Deserialize(entry.Value))
            .AsQueryable();

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicProviderStore{TSchemeDefinition, TContext}"/> class.
        /// </summary>
        /// <param name="db">The Redis db.</param>
        /// <param name="authenticationSchemeOptionsSerializer">The authentication scheme options serializer.</param>
        /// <param name="logger">The logger.</param>
        /// <exception cref="System.ArgumentNullException">
        /// db
        /// or
        /// authenticationSchemeOptionsSerializer
        /// or
        /// logger
        /// </exception>
        public DynamicProviderStore(IDatabase db, IRedisAuthenticationSchemeOptionsSerializer<TSchemeDefinition> authenticationSchemeOptionsSerializer, ILogger<DynamicProviderStore<TSchemeDefinition>> logger)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
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

            var tran = _db.CreateTransaction();
            var notExistsCondition = tran.AddCondition(Condition.HashNotExists(StoreKey, definition.Scheme));
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            tran.HashSetAsync(StoreKey, 
                definition.Scheme, 
                _authenticationSchemeOptionsSerializer.Serialize(definition));
            tran.HashSetAsync(ConcurencyKey, definition.Scheme, 0);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

            var result = await tran.ExecuteAsync();
            if (!result)
            {
                throw new InvalidOperationException($"The scheme {definition.Scheme} already exists");
            }

            _logger.LogInformation("Scheme {scheme} added for {handlerType} with options: {options}", definition.Scheme, definition.HandlerType, definition.SerializedOptions);
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

            var value = await _db.HashGetAsync(StoreKey, scheme).ConfigureAwait(false);
            if (value.HasValue)
            {
                var definition = _authenticationSchemeOptionsSerializer.Deserialize(value);
                definition.ConcurrencyStamp = (long)await _db.HashGetAsync(ConcurencyKey, scheme);
                return definition;
            }

            return default(TSchemeDefinition);
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

            var tran = _db.CreateTransaction();
            var concurrencyCondition = tran.AddCondition(Condition.HashEqual(ConcurencyKey, definition.Scheme, definition.ConcurrencyStamp));
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            tran.HashDeleteAsync(StoreKey, definition.Scheme);
            tran.HashDeleteAsync(ConcurencyKey, definition.Scheme);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

            var result = await tran.ExecuteAsync();
            if (!result)
            {
                throw new InvalidOperationException($"ConcurrencyStamp not match for scheme {definition.Scheme}");
            }

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

            definition.ConcurrencyStamp = 0;

            var tran = _db.CreateTransaction();
            var concurrencyCondition = tran.AddCondition(Condition.HashEqual(ConcurencyKey, definition.Scheme, definition.ConcurrencyStamp));
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            tran.HashSetAsync(StoreKey, definition.Scheme, _authenticationSchemeOptionsSerializer.Serialize(definition));
            var concurency = tran.HashIncrementAsync(ConcurencyKey, definition.Scheme);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

            var result = await tran.ExecuteAsync();
            if (!result)
            {
                throw new InvalidOperationException($"ConcurrencyStamp not match for scheme {definition.Scheme}");
            }

            definition.ConcurrencyStamp = concurency.Result;

            _logger.LogInformation("Scheme {scheme} updated for {handlerType} with options: {options}", definition.Scheme, definition.HandlerType, definition.SerializedOptions);
        }
    }
}
