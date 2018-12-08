using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Aguacongas.AspNetCore.Authentication
{
    public class DynamicManager : DynamicManager<ProviderDefinition>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicManager"/> class.
        /// </summary>
        /// <param name="schemeProvider">The scheme provider.</param>
        /// <param name="wrapperFactory">The wrapper factory.</param>
        /// <param name="store">The store.</param>
        /// <param name="logger">The logger.</param>
        public DynamicManager(IAuthenticationSchemeProvider schemeProvider,
            OptionsMonitorCacheWrapperFactory wrapperFactory,
            IDynamicProviderStore<ProviderDefinition> store,
            ILogger<DynamicManager<ProviderDefinition>> logger)
            : base(schemeProvider, wrapperFactory, store, logger)
        {
        }
    }

    public class DynamicManager<TDefinition>
        where TDefinition: ProviderDefinition, new()
    {
        private static readonly JsonSerializerSettings _jsonSerializerSettings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                Formatting = Formatting.None,
                DefaultValueHandling = DefaultValueHandling.Ignore,
                ContractResolver = new ContractResolver()
            };

        private readonly IDynamicProviderStore<TDefinition>_store;
        private readonly ILogger<DynamicManager<TDefinition>> _logger;
        private readonly IAuthenticationSchemeProvider _schemeProvider;
        private readonly OptionsMonitorCacheWrapperFactory _wrapperFactory;

        public Func<AuthenticationSchemeOptions, Type, string> Serialize { get; set; } = SerializeOptions;

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
            _schemeProvider = schemeProvider;
            _wrapperFactory = wrapperFactory;
            _store = store;
            _logger = logger;
        }

        /// <summary>
        /// Adds the scheme asynchronously.
        /// </summary>
        /// <typeparam name="THandler">The type of the handler.</typeparam>
        /// <typeparam name="TOptions">The type of the options.</typeparam>
        /// <param name="scheme">The scheme.</param>
        /// <param name="displayName">The display name.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public Task AddAsync<THandler, TOptions>(string scheme, string displayName, TOptions options, CancellationToken cancellationToken = default(CancellationToken))
            where THandler : AuthenticationHandler<TOptions>
            where TOptions : AuthenticationSchemeOptions, new()
        {
            return AddAsync(scheme, displayName, typeof(THandler), (AuthenticationSchemeOptions)options, cancellationToken);
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
        public async Task AddAsync(string scheme, string displayName, Type handlerType, AuthenticationSchemeOptions options, CancellationToken cancellationToken = default(CancellationToken))
        {
            var genericTypeArguments = GetGenericTypeArguments(handlerType);
            if (handlerType.GetInterface(nameof(IAuthenticationHandler)) == null || genericTypeArguments.Length == 0)
            {
                throw new ArgumentException($"Parameter {nameof(handlerType)} should be a {nameof(AuthenticationHandler<AuthenticationSchemeOptions>)}");
            }

            var optionsType = genericTypeArguments[0];
            var optionsMonitorCache = _wrapperFactory.Get(optionsType);

            if (await _schemeProvider.GetSchemeAsync(scheme) != null)
            {
                _schemeProvider.RemoveScheme(scheme);
                optionsMonitorCache.TryRemove(scheme);
            }

            string serializerOptions = Serialize(options, optionsType);

            var handlerTypeName = handlerType.FullName;

            await _store.AddAsync(new TDefinition
            {
                DisplayName = displayName,
                Id = scheme,
                HandlerTypeName = handlerTypeName,
                SerializedOptions = serializerOptions
            }, cancellationToken);

            _schemeProvider.AddScheme(new AuthenticationScheme(scheme, displayName, handlerType));
            optionsMonitorCache.TryAdd(scheme, options);

            _logger.LogInformation("Scheme {scheme} added with name {displayName} for {handlerType} with options {options}", scheme, displayName, handlerType, serializerOptions);
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
        public async Task UpdateAsync(string scheme, string displayName, Type handlerType, AuthenticationSchemeOptions options, CancellationToken cancellationToken = default(CancellationToken))
        {
            var genericTypeArguments = GetGenericTypeArguments(handlerType);
            if (handlerType.GetInterface(nameof(IAuthenticationHandler)) == null || handlerType.GenericTypeArguments.Length == 0)
            {
                throw new ArgumentException($"Parameter {nameof(handlerType)} should be a {nameof(AuthenticationHandler<AuthenticationSchemeOptions>)}");
            }

            var definition = await _store.FindBySchemeAsync(scheme, cancellationToken);
            if (definition == null)
            {
                throw new InvalidOperationException($"The scheme {scheme} does not exist in the database");
            }

            if (await _schemeProvider.GetSchemeAsync(scheme) == null)
            {
                throw new InvalidOperationException($"The scheme {scheme} does not exist");
            }

            var optionsType = genericTypeArguments[0];
            var serializerOptions = Serialize(options, optionsType);
            var handlerTypeName = handlerType.FullName;

            definition.DisplayName = displayName;
            definition.HandlerTypeName = handlerTypeName;
            definition.SerializedOptions = serializerOptions;
            await _store.UpdateAsync(definition);

            var optionsMonitorCache = _wrapperFactory.Get(optionsType);

            _schemeProvider.RemoveScheme(scheme);
            optionsMonitorCache.TryRemove(scheme);

            _schemeProvider.AddScheme(new AuthenticationScheme(scheme, displayName, handlerType));
            optionsMonitorCache.TryAdd(scheme, options);
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
                var handlerType = Type.GetType(definition.HandlerTypeName);
                var optionsType = handlerType.GenericTypeArguments[0];
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
            foreach(var definition in _store.ProviderDefinitions)
            {
                var scheme = definition.Id;
                var handlerType = Type.GetType(definition.HandlerTypeName);
                var optionsType = handlerType.GenericTypeArguments[0];
                var optionsMonitorCache = _wrapperFactory.Get(optionsType);
                var options = JsonConvert.DeserializeObject(definition.SerializedOptions, optionsType) as AuthenticationSchemeOptions;

                _schemeProvider.AddScheme(new AuthenticationScheme(scheme, definition.DisplayName, handlerType));
                optionsMonitorCache.TryAdd(scheme, options);
            }
        }

        protected static string SerializeOptions(AuthenticationSchemeOptions options, Type optionsType)
        {
            return JsonConvert.SerializeObject(options, optionsType, _jsonSerializerSettings);
        }

        private Type[] GetGenericTypeArguments(Type type)
        {
            if (type.GenericTypeArguments.Length > 0)
            {
                return type.GenericTypeArguments;
            }

            if (type.BaseType == null)
            {
                return new Type[0];
            }

            return GetGenericTypeArguments(type.BaseType);
        }

        class OptionsMonitorCacheWrapper
        {
            public OptionsMonitorCacheWrapper(Type optionsType)
            {

            }
        }
    }
}
