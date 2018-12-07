using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Aguacongas.AspNetCore.Authentication
{
    public class DynamicManager
    {
        private readonly JsonSerializerSettings _jsonSerializerSettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };
        private readonly IDynamicProviderStore _store;
        private readonly IAuthenticationSchemeProvider _schemeProvider;
        private readonly IServiceProvider _serviceProvider;

        public DynamicManager(IAuthenticationSchemeProvider schemeProvider, IServiceProvider serviceProvider, IDynamicProviderStore store)
        {
            _schemeProvider = schemeProvider;
            _serviceProvider = serviceProvider;
            _store = store;
        }

        public Task AddOrUpdateAsync<THandlerType, TOptions>(string scheme, string displayName, THandlerType handlerType, TOptions options, CancellationToken cancellationToken = default(CancellationToken))
            where THandlerType: AuthenticationHandler<TOptions>
            where TOptions : AuthenticationSchemeOptions, new()
        {
            return AddOrUpdateAsync(scheme, displayName, typeof(THandlerType), (AuthenticationSchemeOptions)options, cancellationToken);
        }
        public async Task AddOrUpdateAsync(string scheme, string displayName, Type handlerType, AuthenticationSchemeOptions options, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (handlerType.GetInterface(nameof(IAuthenticationHandler)) == null || handlerType.GenericTypeArguments.Length == 0)
            {
                throw new ArgumentException($"Parameter {nameof(handlerType)} should be a {nameof(AuthenticationHandler<AuthenticationSchemeOptions>)}");
            }
            var optionsType = handlerType.GenericTypeArguments[0];
            var serializerOptions = JsonConvert.SerializeObject(options, optionsType, _jsonSerializerSettings);
            var definition = await _store.FindByScheme(scheme);
            var handlerTypeName = handlerType.FullName;
            if (definition == null)
            {
                await _store.Add(new ProviderDefinition
                {
                    DisplayName = displayName,
                    Scheme = scheme,
                    HandlerTypeName = handlerTypeName,
                    SerializedOptions = serializerOptions
                }, cancellationToken);
            }
            else
            {
                definition.DisplayName = displayName;
                definition.HandlerTypeName = handlerTypeName;
                definition.SerializedOptions = serializerOptions;
                await _store.Update(definition);
            }

            var optionsCache = (IOptionsMonitorCache<AuthenticationSchemeOptions>)_serviceProvider.GetService(optionsType);
            if (await _schemeProvider.GetSchemeAsync(scheme) == null)
            {
                _schemeProvider.AddScheme(new AuthenticationScheme(scheme, displayName, handlerType));
            }
            else
            {
                optionsCache.TryRemove(scheme);
            }
            optionsCache.TryAdd(scheme, options);
        }
    }
}
