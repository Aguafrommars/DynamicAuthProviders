// Project: aguacongas/DymamicAuthProviders
// Copyright (c) 2018 @Olivier Lefebvre
using Microsoft.AspNetCore.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Aguacongas.AspNetCore.Authentication
{
    /// <summary>
    /// Dynamic scheme manager which persist the changes.
    /// </summary>
    /// <typeparam name="TSchemeDefinition">The type of the scheme definition.</typeparam>
    /// <seealso cref="Aguacongas.AspNetCore.Authentication.NoPersistentDynamicManager{TSchemeDefinition}" />
    public class PersistentDynamicManager<TSchemeDefinition> : NoPersistentDynamicManager<TSchemeDefinition>
        where TSchemeDefinition : SchemeDefinitionBase, new()
    {
        private readonly IDynamicProviderStore<TSchemeDefinition> _store;

        /// <summary>
        /// Gets the scheme definitions list.
        /// </summary>
        /// <value>
        /// The scheme definitions list.
        /// </value>
        public virtual IEnumerable<TSchemeDefinition> SchemeDefinitions => _store.SchemeDefinitions;

        /// <summary>
        /// Initializes a new instance of the <see cref="PersistentDynamicManager{TSchemeDefinition}"/> class.
        /// </summary>
        /// <param name="schemeProvider">The scheme provider.</param>
        /// <param name="wrapperFactory">The wrapper factory.</param>
        /// <param name="store">The store.</param>
        /// <param name="managedTypes">The managed types.</param>
        /// <exception cref="ArgumentNullException">store</exception>
        public PersistentDynamicManager(IAuthenticationSchemeProvider schemeProvider, OptionsMonitorCacheWrapperFactory wrapperFactory, IDynamicProviderStore<TSchemeDefinition> store, IEnumerable<Type> managedTypes)
            : base(schemeProvider, wrapperFactory, managedTypes)
        {
            _store = store ?? throw new ArgumentNullException(nameof(store));
        }

        /// <summary>
        /// Adds a scheme asynchronously.
        /// </summary>
        /// <param name="definition">The definition.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public override async Task AddAsync(TSchemeDefinition definition, CancellationToken cancellationToken = default)
        {
            await base.AddAsync(definition, cancellationToken);
            await _store.AddAsync(definition, cancellationToken);
        }

        /// <summary>
        /// Removes the scheme asynchronous.
        /// </summary>
        /// <param name="name">The scheme.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public override async Task RemoveAsync(string name, CancellationToken cancellationToken = default)
        {
            await base.RemoveAsync(name, cancellationToken);
            var definition = await _store.FindBySchemeAsync(name);
            if (definition != null)
            {
                await _store.RemoveAsync(definition, cancellationToken);
            }
        }

        /// <summary>
        /// Updates the scheme asynchronous.
        /// </summary>
        /// <param name="definition">The definition.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public override async Task UpdateAsync(TSchemeDefinition definition, CancellationToken cancellationToken = default)
        {
            await base.UpdateAsync(definition, cancellationToken);
            await _store.UpdateAsync(definition, cancellationToken);
        }
        /// <summary>
        /// Finds the definition by scheme asynchronous.
        /// </summary>
        /// <param name="scheme">The scheme.</param>
        /// <returns>The scheme definition or null.</returns>
        /// <exception cref="ArgumentException">scheme cannot be null or white space.</exception>
        public virtual Task<TSchemeDefinition> FindBySchemeAsync(string scheme)
        {
            if (string.IsNullOrWhiteSpace(scheme))
            {
                throw new ArgumentException($"{nameof(scheme)} cannot be null or white space.");
            }
            return _store.FindBySchemeAsync(scheme);
        }


        /// <summary>
        /// Loads the configuration.
        /// </summary>
        public virtual void Load()
        {
            foreach (var definition in _store.SchemeDefinitions)
            {
                if (ManagedHandlerType.Contains(definition.HandlerType))
                {
                    base.AddAsync(definition).GetAwaiter().GetResult();
                }                
            }
        }
    }

    /// <summary>
    /// Dynamic scheme manager which not persist the changes.
    /// </summary>
    /// <typeparam name="TSchemeDefinition">The type of the scheme definition.</typeparam>
    public class NoPersistentDynamicManager<TSchemeDefinition>
        where TSchemeDefinition: SchemeDefinitionBase, new()
    {
        private readonly IAuthenticationSchemeProvider _schemeProvider;
        private readonly OptionsMonitorCacheWrapperFactory _wrapperFactory;

        /// <summary>
        /// Gets the type of the managed handler.
        /// </summary>
        /// <value>
        /// The type of the managed handler.
        /// </value>
        public virtual IEnumerable<Type> ManagedHandlerType { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="NoPersistentDynamicManager{TSchemeDefinition}"/> class.
        /// </summary>
        /// <param name="schemeProvider">The scheme provider.</param>
        /// <param name="wrapperFactory">The wrapper factory.</param>
        /// <param name="managedTypes">The list of managed handlers types.</param>
        /// <exception cref="ArgumentNullException">
        /// schemeProvider
        /// or
        /// wrapperFactory
        /// or
        /// store
        /// </exception>
        public NoPersistentDynamicManager(IAuthenticationSchemeProvider schemeProvider,
            OptionsMonitorCacheWrapperFactory wrapperFactory,
            IEnumerable<Type> managedTypes)
        {
            _schemeProvider = schemeProvider ?? throw new ArgumentNullException(nameof(schemeProvider));
            _wrapperFactory = wrapperFactory ?? throw new ArgumentNullException(nameof(wrapperFactory));
            ManagedHandlerType = managedTypes ?? throw new ArgumentNullException(nameof(managedTypes));
        }

        /// <summary>
        /// Adds a scheme asynchronously.
        /// </summary>
        /// <param name="definition">The definition.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">definition</exception>
        public virtual async Task AddAsync(TSchemeDefinition definition, CancellationToken cancellationToken = default)
        {
            definition = definition ?? throw new ArgumentNullException(nameof(definition));

            var handlerType = definition.HandlerType;
            var optionsType = GetOptionsType(handlerType);
            var optionsMonitorCache = _wrapperFactory.Get(optionsType);

            var scheme = definition.Scheme;
            if (await _schemeProvider.GetSchemeAsync(scheme) != null)
            {
                _schemeProvider.RemoveScheme(scheme);
                optionsMonitorCache.TryRemove(scheme);
            }

            _schemeProvider.AddScheme(new AuthenticationScheme(scheme, definition.DisplayName, handlerType));
            optionsMonitorCache.TryAdd(scheme, definition.Options);


        }

        /// <summary>
        /// Updates the scheme asynchronous.
        /// </summary>
        /// <param name="definition">The definition.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">definition</exception>
        /// <exception cref="InvalidOperationException">The scheme does not exist.</exception>
        public virtual async Task UpdateAsync(TSchemeDefinition definition, CancellationToken cancellationToken = default)
        {
            definition = definition ?? throw new ArgumentNullException(nameof(definition));

            var handlerType = definition.HandlerType;
            var optionsType = GetOptionsType(handlerType);
            var scheme = definition.Scheme;

            if (await _schemeProvider.GetSchemeAsync(scheme) == null)
            {
                throw new InvalidOperationException($"The scheme {scheme} does not exist.");
            }

            var optionsMonitorCache = _wrapperFactory.Get(optionsType);

            _schemeProvider.RemoveScheme(scheme);
            optionsMonitorCache.TryRemove(scheme);

            _schemeProvider.AddScheme(new AuthenticationScheme(scheme, definition.DisplayName, handlerType));
            optionsMonitorCache.TryAdd(scheme, definition.Options);
        }

        /// <summary>
        /// Removes the scheme asynchronous.
        /// </summary>
        /// <param name="name">The scheme.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">scheme cannot be null or white space.</exception>
        public virtual async Task RemoveAsync(string name, CancellationToken cancellationToken = default)
        {
            CheckName(name);

            var scheme = await _schemeProvider.GetSchemeAsync(name);
            if (scheme != null)
            {
                var optionsType = GetOptionsType(scheme.HandlerType);
                var optionsMonitorCache = _wrapperFactory.Get(optionsType);

                _schemeProvider.RemoveScheme(name);
                optionsMonitorCache.TryRemove(name);
            }
        }

        private static void CheckName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException($"{nameof(name)} cannot be null or white space.");
            }
        }

        private Type GetOptionsType(Type handlerType)
        {
            return handlerType.GetAuthenticationSchemeOptionsType();
        }
    }
}
