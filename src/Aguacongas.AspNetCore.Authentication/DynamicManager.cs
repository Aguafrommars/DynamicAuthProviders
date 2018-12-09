using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyModel;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Aguacongas.AspNetCore.Authentication
{
    public class DynamicManager<TDefinition>
        where TDefinition: SchemeDefinitionBase, new()
    {
        private readonly IDynamicProviderStore<TDefinition>_store;
        private readonly ILogger<DynamicManager<TDefinition>> _logger;
        private readonly IAuthenticationSchemeProvider _schemeProvider;
        private readonly OptionsMonitorCacheWrapperFactory _wrapperFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicManager{TDefinition}"/> class.
        /// </summary>
        /// <param name="schemeProvider">The scheme provider.</param>
        /// <param name="wrapperFactory">The wrapper factory.</param>
        /// <param name="store">The store.</param>
        /// <param name="logger">The logger.</param>
        public DynamicManager(IAuthenticationSchemeProvider schemeProvider,
            OptionsMonitorCacheWrapperFactory wrapperFactory,
            IDynamicProviderStore<TDefinition> store, ILogger<DynamicManager<TDefinition>> logger)
        {
            _schemeProvider = schemeProvider ?? throw new ArgumentNullException(nameof(schemeProvider));
            _wrapperFactory = wrapperFactory ?? throw new ArgumentNullException(nameof(wrapperFactory));
            _store = store ?? throw new ArgumentNullException(nameof(store));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Adds the scheme asynchronously.
        /// </summary>
        /// <param name="scheme">The scheme.</param>
        /// <param name="displayName">The display name.</param>
        /// <param name="handlerType">Type of the handler.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">Parameter {nameof(handlerType)} should be a {nameof(AuthenticationHandler<AuthenticationSchemeOptions>)}</exception>
        public async Task AddAsync(TDefinition definition, CancellationToken cancellationToken = default(CancellationToken))
        {
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
        /// Updates the scheme asynchronously.
        /// </summary>
        /// <param name="scheme">The scheme.</param>
        /// <param name="displayName">The display name.</param>
        /// <param name="handlerType">Type of the handler.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">Parameter {nameof(handlerType)} should be a {nameof(AuthenticationHandler<AuthenticationSchemeOptions>)}</exception>
        /// <exception cref="InvalidOperationException">
        /// The scheme {scheme} is not found
        /// </exception>
        public async Task UpdateAsync(TDefinition definition, CancellationToken cancellationToken = default(CancellationToken))
        {
            var handlerType = definition.HandlerType;
            var optionsType = GetOptionsType(handlerType);
            var scheme = definition.Scheme;

            if (await _schemeProvider.GetSchemeAsync(scheme) == null)
            {
                throw new InvalidOperationException($"The scheme {scheme} does not exist");
            }

            await _store.UpdateAsync(definition, cancellationToken);

            var optionsMonitorCache = _wrapperFactory.Get(optionsType);

            _schemeProvider.RemoveScheme(scheme);
            optionsMonitorCache.TryRemove(scheme);

            _schemeProvider.AddScheme(new AuthenticationScheme(scheme, definition.DisplayName, handlerType));
            optionsMonitorCache.TryAdd(scheme, definition.Options);
        }

        /// <summary>
        /// Removes the scheme asynchronously.
        /// </summary>
        /// <param name="scheme">The scheme.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public async Task RemoveAsync(string scheme, CancellationToken cancellationToken = default(CancellationToken))
        {
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
