// Project: aguacongas/DymamicAuthProviders
// Copyright (c) 2021 @Olivier Lefebvre
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Aguacongas.AspNetCore.Authentication.EntityFramework
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
        /// <param name="context">The context.</param>
        /// <param name="authenticationSchemeOptionsSerializer">The authentication scheme options serializer.</param>
        /// <param name="logger">The logger.</param>
        public DynamicProviderStore(SchemeDbContext context, IAuthenticationSchemeOptionsSerializer authenticationSchemeOptionsSerializer, ILogger<DynamicProviderStore> logger) : base(context, authenticationSchemeOptionsSerializer, logger)
        {
        }
    }

    /// <summary>
    /// Implement a store for <see cref="NoPersistentDynamicManager{TSchemeDefinition}"/> with EntityFramework.
    /// </summary>
    /// <typeparam name="TSchemeDefinition">The type of the definition.</typeparam>
    /// <seealso cref="Aguacongas.AspNetCore.Authentication.IDynamicProviderStore{TSchemeDefinition}" />
    public class DynamicProviderStore<TSchemeDefinition> : DynamicProviderStore<TSchemeDefinition, SchemeDbContext<TSchemeDefinition>>
        where TSchemeDefinition : SchemeDefinition, new()
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicProviderStore{TSchemeDefinition}"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="authenticationSchemeOptionsSerializer">The authentication scheme options serializer.</param>
        /// <param name="logger">The logger.</param>
        public DynamicProviderStore(SchemeDbContext<TSchemeDefinition> context, IAuthenticationSchemeOptionsSerializer authenticationSchemeOptionsSerializer, ILogger<DynamicProviderStore<TSchemeDefinition>> logger) : base(context, authenticationSchemeOptionsSerializer, logger)
        {
        }
    }

    /// <summary>
    /// Implement a store for <see cref="NoPersistentDynamicManager{TSchemeDefinition}"/> with EntityFramework.
    /// </summary>
    /// <typeparam name="TSchemeDefinition">The type of the definition.</typeparam>
    /// <typeparam name="TContext">The type of the context.</typeparam>
    /// <seealso cref="Aguacongas.AspNetCore.Authentication.IDynamicProviderStore{TSchemeDefinition}" />
    public class DynamicProviderStore<TSchemeDefinition, TContext> : IDynamicProviderStore<TSchemeDefinition>
        where TContext: DbContext
        where TSchemeDefinition: SchemeDefinition, new()
    {
        private readonly DbContext _context;
        private readonly IAuthenticationSchemeOptionsSerializer _authenticationSchemeOptionsSerializer;
        private readonly ILogger<DynamicProviderStore<TSchemeDefinition, TContext>> _logger;

        /// <summary>
        /// Gets the scheme definitions list.
        /// </summary>
        /// <value>
        /// The scheme definitions list.
        /// </value>
        public virtual IQueryable<TSchemeDefinition> SchemeDefinitions => _context.Set<TSchemeDefinition>()
            .Select(Deserialize)
            .AsQueryable();

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicProviderStore{TSchemeDefinition, TContext}"/> class.
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
        public DynamicProviderStore(TContext context, IAuthenticationSchemeOptionsSerializer authenticationSchemeOptionsSerializer, ILogger<DynamicProviderStore<TSchemeDefinition, TContext>> logger)
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
        public virtual async Task AddAsync(TSchemeDefinition definition, CancellationToken cancellationToken = default)
        {
            definition = definition ?? throw new ArgumentNullException(nameof(definition));
            
            cancellationToken.ThrowIfCancellationRequested();

            Serialize(definition);

            _context.Add(definition);
            await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            _logger.LogInformation("Scheme {scheme} added for {handlerType} with options: {options}", definition.Scheme, definition.HandlerType, definition.SerializedOptions);
        }

        /// <summary>
        /// Removes a scheme definition asynchronous.
        /// </summary>
        /// <param name="definition">The definition.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">definition</exception>
        public virtual async Task RemoveAsync(TSchemeDefinition definition, CancellationToken cancellationToken = default)
        {
            definition = definition ?? throw new ArgumentNullException(nameof(definition));
            
            cancellationToken.ThrowIfCancellationRequested();
            _context.Remove(definition);
            await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            _logger.LogInformation("Scheme {scheme} removed", definition.Scheme);
        }

        /// <summary>
        /// Updates a scheme definition asynchronous.
        /// </summary>
        /// <param name="definition">The definition.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">definition</exception>
        public virtual async Task UpdateAsync(TSchemeDefinition definition, CancellationToken cancellationToken = default)
        {
            definition = definition ?? throw new ArgumentNullException(nameof(definition));
            
            cancellationToken.ThrowIfCancellationRequested();
            _context.Attach(definition);

            Serialize(definition);
            definition.ConcurrencyStamp = Guid.NewGuid().ToString();

            _context.Update(definition);
            await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

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
        public virtual async Task<TSchemeDefinition> FindBySchemeAsync(string scheme, CancellationToken cancellationToken = default)
        {
            CheckScheme(scheme);

            cancellationToken.ThrowIfCancellationRequested();
            var definition = await _context.FindAsync<TSchemeDefinition>(new[] { scheme }, cancellationToken);

            if (definition != null)
            {
                Deserialize(definition);
            }

            return definition;
        }

        private static void CheckScheme(string scheme)
        {
            if (string.IsNullOrWhiteSpace(scheme))
            {
                throw new ArgumentException($"Parameter {nameof(scheme)} cannor be null or empty");
            }
        }

        private void Serialize(TSchemeDefinition definition)
        {
            definition.SerializedHandlerType = _authenticationSchemeOptionsSerializer.SerializeType(definition.HandlerType);
            definition.SerializedOptions = _authenticationSchemeOptionsSerializer.SerializeOptions(definition.Options, definition.HandlerType.GetAuthenticationSchemeOptionsType());
        }

        [SuppressMessage("Minor Code Smell", "S3241:Methods should not return values that are never used", Justification = "Used in linq Select clause")]
        private TSchemeDefinition Deserialize(TSchemeDefinition definition)
        {
            definition.HandlerType = _authenticationSchemeOptionsSerializer.DeserializeType(definition.SerializedHandlerType);
            definition.Options = _authenticationSchemeOptionsSerializer.DeserializeOptions(definition.SerializedOptions, definition.HandlerType.GetAuthenticationSchemeOptionsType());
            return definition;
        }
    }
}
