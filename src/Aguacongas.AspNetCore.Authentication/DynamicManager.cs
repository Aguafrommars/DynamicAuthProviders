﻿// Project: DymamicAuthProviders
// Copyright (c) 2018 @Olivier Lefebvre
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyModel;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Aguacongas.AspNetCore.Authentication
{

    /// <summary>
    /// Dynamic scheme manager
    /// </summary>
    /// <typeparam name="TSchemeDefinition">The type of the scheme definition.</typeparam>
    public class DynamicManager<TSchemeDefinition>
        where TSchemeDefinition: SchemeDefinitionBase, new()
    {
        private readonly IDynamicProviderStore<TSchemeDefinition>_store;
        private readonly IAuthenticationSchemeProvider _schemeProvider;
        private readonly OptionsMonitorCacheWrapperFactory _wrapperFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicManager{TSchemeDefinition}"/> class.
        /// </summary>
        /// <param name="schemeProvider">The scheme provider.</param>
        /// <param name="wrapperFactory">The wrapper factory.</param>
        /// <param name="store">The store.</param>
        /// <exception cref="ArgumentNullException">
        /// schemeProvider
        /// or
        /// wrapperFactory
        /// or
        /// store
        /// </exception>
        public DynamicManager(IAuthenticationSchemeProvider schemeProvider,
            OptionsMonitorCacheWrapperFactory wrapperFactory,
            IDynamicProviderStore<TSchemeDefinition> store)
        {
            _schemeProvider = schemeProvider ?? throw new ArgumentNullException(nameof(schemeProvider));
            _wrapperFactory = wrapperFactory ?? throw new ArgumentNullException(nameof(wrapperFactory));
            _store = store ?? throw new ArgumentNullException(nameof(store));
        }

        /// <summary>
        /// Adds a scheme asynchronously.
        /// </summary>
        /// <param name="definition">The definition.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">definition</exception>
        public async Task AddAsync(TSchemeDefinition definition, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (definition == null)
            {
                throw new ArgumentNullException(nameof(definition));
            }
            var handlerType = definition.HandlerType;
            var optionsType = GetOptionsType(handlerType);
            var optionsMonitorCache = _wrapperFactory.Get(optionsType);

            var scheme = definition.Scheme;
            if (await _schemeProvider.GetSchemeAsync(scheme) != null)
            {
                _schemeProvider.RemoveScheme(scheme);
                optionsMonitorCache.TryRemove(scheme);
            }

            await _store.AddAsync(definition, cancellationToken);
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
        public async Task UpdateAsync(TSchemeDefinition definition, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (definition == null)
            {
                throw new ArgumentNullException(nameof(definition));
            }
            var handlerType = definition.HandlerType;
            var optionsType = GetOptionsType(handlerType);
            var scheme = definition.Scheme;

            if (await _schemeProvider.GetSchemeAsync(scheme) == null)
            {
                throw new InvalidOperationException($"The scheme {scheme} does not exist.");
            }

            await _store.UpdateAsync(definition, cancellationToken);

            var optionsMonitorCache = _wrapperFactory.Get(optionsType);

            _schemeProvider.RemoveScheme(scheme);
            optionsMonitorCache.TryRemove(scheme);

            _schemeProvider.AddScheme(new AuthenticationScheme(scheme, definition.DisplayName, handlerType));
            optionsMonitorCache.TryAdd(scheme, definition.Options);
        }

        /// <summary>
        /// Removes the scheme asynchronous.
        /// </summary>
        /// <param name="scheme">The scheme.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">scheme cannot be null or white space.</exception>
        public async Task RemoveAsync(string scheme, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrWhiteSpace(scheme))
            {
                throw new ArgumentException($"{nameof(scheme)} cannot be null or white space.");
            }
            var definition = await _store.FindBySchemeAsync(scheme, cancellationToken);
            if (definition == null)
            {
                var optionsType = GetOptionsType(definition.HandlerType);
                var optionsMonitorCache = _wrapperFactory.Get(optionsType);

                await _store.RemoveAsync(definition, cancellationToken);
                _schemeProvider.RemoveScheme(scheme);
                optionsMonitorCache.TryRemove(scheme);
            }
        }

        /// <summary>
        /// Finds the definition by scheme asynchronous.
        /// </summary>
        /// <param name="scheme">The scheme.</param>
        /// <returns>The scheme definition or null.</returns>
        /// <exception cref="ArgumentException">scheme cannot be null or white space.</exception>
        public Task<TSchemeDefinition> FindBySchemeAsync(string scheme)
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
        public void Load()
        {
            var platform = Environment.OSVersion.Platform.ToString();
            var runtimeAssemblyNames = DependencyContext.Default.GetRuntimeAssemblyNames(platform);

            foreach (var definition in _store.ProviderDefinitions)
            {
                var scheme = definition.Scheme;
                var handlerType = runtimeAssemblyNames
                    .Select(Assembly.Load)
                    .SelectMany(a => a.ExportedTypes)
                    .First(t => t == definition.HandlerType);


                var optionsType = GetOptionsType(handlerType);
                var optionsMonitorCache = _wrapperFactory.Get(optionsType);

                _schemeProvider.AddScheme(new AuthenticationScheme(scheme, definition.DisplayName, handlerType));
                optionsMonitorCache.TryAdd(scheme, definition.Options);
            }
        }

        private Type GetOptionsType(Type handlerType)
        {
            return handlerType.GetAuthenticationSchemeOptionsType();
        }
    }
}