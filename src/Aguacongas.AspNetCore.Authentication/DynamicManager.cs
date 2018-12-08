using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace Aguacongas.AspNetCore.Authentication
{
    public class DynamicManager
    {
        private readonly JsonSerializerSettings _jsonSerializerSettings;
        private readonly IDynamicProviderStore _store;
        private readonly ILogger<DynamicManager> _logger;
        private readonly IAuthenticationSchemeProvider _schemeProvider;
        private readonly OptionsMonitorCacheWrapperFactory _wrapperFactory;

        public DynamicManager(IAuthenticationSchemeProvider schemeProvider,
            OptionsMonitorCacheWrapperFactory wrapperFactory,
            IDynamicProviderStore store, ILogger<DynamicManager> logger)
        {
            var contractResolver = new ContractResolver();
            _jsonSerializerSettings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                Formatting = Formatting.None,
                DefaultValueHandling = DefaultValueHandling.Ignore,
                ContractResolver = contractResolver
            };

            _schemeProvider = schemeProvider;
            _wrapperFactory = wrapperFactory;
            _store = store;
            _logger = logger;
        }

        public Task AddAsync<THandler, TOptions>(string scheme, string displayName, TOptions options, CancellationToken cancellationToken = default(CancellationToken))
            where THandler : AuthenticationHandler<TOptions>
            where TOptions : AuthenticationSchemeOptions, new()
        {
            return AddAsync(scheme, displayName, typeof(THandler), (AuthenticationSchemeOptions)options, cancellationToken);
        }
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

            string serializerOptions = SerializeOptions(options, optionsType);

            var handlerTypeName = handlerType.FullName;

            await _store.AddAsync(new ProviderDefinition
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
            var serializerOptions = SerializeOptions(options, optionsType);
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

        protected virtual string SerializeOptions(AuthenticationSchemeOptions options, Type optionsType)
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
