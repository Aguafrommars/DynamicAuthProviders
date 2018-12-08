using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;

namespace Aguacongas.AspNetCore.Authentication
{
    public class OptionsMonitorCacheWrapperFactory
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ConcurrentDictionary<string, OptionsMonitorCacheWrapper> _cache = new ConcurrentDictionary<string, OptionsMonitorCacheWrapper>();

        public OptionsMonitorCacheWrapperFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IOptionsMonitorCache<AuthenticationSchemeOptions> GetOrCreate(Type optionsType)
        {
            return _cache.GetOrAdd(optionsType.FullName, key =>
            {
                var type = typeof(IOptionsMonitorCache<>).MakeGenericType(optionsType);
                return new OptionsMonitorCacheWrapper(type, _serviceProvider.GetRequiredService(type));
            });
        }
    }

    public class OptionsMonitorCacheWrapper : IOptionsMonitorCache<AuthenticationSchemeOptions>
    {
        private readonly Type _type;
        private readonly object _parent;

        public OptionsMonitorCacheWrapper(Type type, object parent)
        {
            _type = type;
            _parent = parent;;
        }

        public void Clear()
        {
            _type.GetMethod("Clear").Invoke(_parent, null);
        }

        public AuthenticationSchemeOptions GetOrAdd(string name, Func<AuthenticationSchemeOptions> createOptions)
        {
            return (AuthenticationSchemeOptions)_type.GetMethod("GetOrAdd").Invoke(_parent, new object[] { name, createOptions });
        }

        public bool TryAdd(string name, AuthenticationSchemeOptions options)
        {
            return (bool)_type.GetMethod("TryAdd").Invoke(_parent, new object[] { name, options });
        }

        public bool TryRemove(string name)
        {
            return (bool)_type.GetMethod("TryRemove").Invoke(_parent, new object[] { name });
        }
    }
}
