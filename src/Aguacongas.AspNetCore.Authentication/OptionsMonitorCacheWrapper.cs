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
        private readonly Action<AuthenticationSchemeOptions> _configure;

        public OptionsMonitorCacheWrapper(object parent, Action<AuthenticationSchemeOptions> configure)
        {
            _parent = parent;
            _type = parent.GetType();
            _configure = configure;
        }

        public void Clear()
        {
            _type.GetMethod("Clear").Invoke(_parent, null);
        }

        public AuthenticationSchemeOptions GetOrAdd(string name, Func<AuthenticationSchemeOptions> createOptions)
        {
            return (AuthenticationSchemeOptions)_type
                .GetMethod("GetOrAdd")
                .Invoke(_parent, new object[] { name, createOptions });
        }

        public bool TryAdd(string name, AuthenticationSchemeOptions options)
        {
            _configure?.Invoke(options);
            return (bool)_type
                .GetMethod("TryAdd")
                .Invoke(_parent, new object[] { name, options });
        }

        public bool TryRemove(string name)
        {
            return (bool)_type
                .GetMethod("TryRemove")
                .Invoke(_parent, new object[] { name });
        }
    }

    public class OptionsMonitorCacheWrapperFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public OptionsMonitorCacheWrapperFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IOptionsMonitorCache<AuthenticationSchemeOptions> Get(Type optionsType)
        {
            var type = typeof(OptionsMonitorCacheWrapper<>).MakeGenericType(optionsType);
            var wrapper = _serviceProvider.GetRequiredService(type);
            return (IOptionsMonitorCache<AuthenticationSchemeOptions>)wrapper;
        }
    }
}
