// Project: DymamicAuthProviders
// Copyright (c) 2018 @Olivier Lefebvre
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;

namespace Aguacongas.AspNetCore.Authentication
{
    public class OptionsMonitorCacheWrapper<TOptions> : IOptionsMonitorCache<AuthenticationSchemeOptions>
    {
        private readonly Type _type;
        private readonly object _parent;
        private readonly Action<string, AuthenticationSchemeOptions> _onAdded;
        private readonly Action<string> _onRemoved;

        public OptionsMonitorCacheWrapper(object parent, Action<string, AuthenticationSchemeOptions> onAdded, Action<string> onRemoved)
        {
            _parent = parent;
            _type = parent.GetType();
            _onAdded = onAdded;
            _onRemoved = onRemoved;
        }

        public void Clear()
        {
            _type.GetMethod("Clear").Invoke(_parent, null);
        }

        public AuthenticationSchemeOptions GetOrAdd(string name, Func<AuthenticationSchemeOptions> createOptions)
        {
            throw new NotImplementedException();
        }

        public bool TryAdd(string name, AuthenticationSchemeOptions options)
        {
            var result = (bool)_type
                .GetMethod("TryAdd")
                .Invoke(_parent, new object[] { name, options });
            _onAdded?.Invoke(name, options);
            return result;
        }

        public bool TryRemove(string name)
        {
            var result = (bool)_type
                .GetMethod("TryRemove")
                .Invoke(_parent, new object[] { name });
            _onRemoved?.Invoke(name);
            return result;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class OptionsMonitorCacheWrapperFactory
    {
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="OptionsMonitorCacheWrapperFactory"/> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        public OptionsMonitorCacheWrapperFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Gets the <see cref="IOptionsMonitorCache{AuthenticationSchemeOptions}"/>. for the option type
        /// </summary>
        /// <param name="optionsType">Type of the options.</param>
        /// <returns></returns>
        public IOptionsMonitorCache<AuthenticationSchemeOptions> Get(Type optionsType)
        {
            var type = typeof(OptionsMonitorCacheWrapper<>).MakeGenericType(optionsType);
            var wrapper = _serviceProvider.GetRequiredService(type);
            return (IOptionsMonitorCache<AuthenticationSchemeOptions>)wrapper;
        }
    }
}
