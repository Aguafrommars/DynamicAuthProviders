// Project: DymamicAuthProviders
// Copyright (c) 2018 @Olivier Lefebvre
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;

namespace Aguacongas.AspNetCore.Authentication
{
    /// <summary>
    /// Wrapper for <see cref="IOptionsMonitorCache{TOptions}"/>
    /// </summary>
    /// <remarks>For internal use, you should not use this class</remarks>
    /// <typeparam name="TOptions">The type of the options.</typeparam>
    /// <seealso cref="Microsoft.Extensions.Options.IOptionsMonitorCache{Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions}" />
    public class OptionsMonitorCacheWrapper<TOptions> : IOptionsMonitorCache<AuthenticationSchemeOptions>
    {
        private readonly Type _type;
        private readonly object _parent;
        private readonly Action<string, AuthenticationSchemeOptions> _onAdded;
        private readonly Action<string> _onRemoved;

        /// <summary>
        /// Initializes a new instance of the <see cref="OptionsMonitorCacheWrapper{TOptions}"/> class.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <param name="onAdded">The on added.</param>
        /// <param name="onRemoved">The on removed.</param>
        /// <remarks>For internal user, you should not use this class</remarks>
        public OptionsMonitorCacheWrapper(object parent, Action<string, AuthenticationSchemeOptions> onAdded, Action<string> onRemoved)
        {
            _parent = parent ?? throw new ArgumentNullException(nameof(parent));
            _type = parent.GetType();
            _onAdded = onAdded;
            _onRemoved = onRemoved;
        }

        /// <summary>
        /// Clears all options instances from the cache.
        /// </summary>
        public void Clear()
        {
            _type.GetMethod("Clear").Invoke(_parent, null);
        }

        /// <summary>
        /// Gets a named options instance, or adds a new instance created with createOptions.
        /// </summary>
        /// <param name="name">The name of the options instance.</param>
        /// <param name="createOptions">The func used to create the new instance.</param>
        /// <returns>
        /// The options instance.
        /// </returns>
        /// <exception cref="System.NotImplementedException"></exception>
        /// <remarks>This method is not implemented.</remarks>
        public AuthenticationSchemeOptions GetOrAdd(string name, Func<AuthenticationSchemeOptions> createOptions)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Tries to adds a new option to the cache, will return false if the name already exists.
        /// </summary>
        /// <param name="name">The name of the options instance.</param>
        /// <param name="options">The options instance.</param>
        /// <returns>
        /// Whether anything was added.
        /// </returns>
        public bool TryAdd(string name, AuthenticationSchemeOptions options)
        {
            var result = (bool)_type
                .GetMethod("TryAdd")
                .Invoke(_parent, new object[] { name, options });
            _onAdded?.Invoke(name, options);
            return result;
        }

        /// <summary>
        /// Try to remove an options instance.
        /// </summary>
        /// <param name="name">The name of the options instance.</param>
        /// <returns>
        /// Whether anything was removed.
        /// </returns>
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
    /// Factory to create wrapper for <see cref="IOptionsMonitorCache{TOptions}"/>
    /// </summary>
    /// <remarks>For internal user, you should not use this class</remarks>
    public class OptionsMonitorCacheWrapperFactory
    {
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="OptionsMonitorCacheWrapperFactory"/> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        /// <remarks>For internal user, you should not use this class</remarks>
        public OptionsMonitorCacheWrapperFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        /// <summary>
        /// Gets the <see cref="IOptionsMonitorCache{AuthenticationSchemeOptions}"/> wrapper for the option type
        /// </summary>
        /// <param name="optionsType">Type of the options.</param>
        /// <returns></returns>
        /// <remarks>For internal user, you should not use this class</remarks>
        public IOptionsMonitorCache<AuthenticationSchemeOptions> Get(Type optionsType)
        {
            var type = typeof(OptionsMonitorCacheWrapper<>).MakeGenericType(optionsType);
            var wrapper = _serviceProvider.GetRequiredService(type);
            return (IOptionsMonitorCache<AuthenticationSchemeOptions>)wrapper;
        }
    }
}
