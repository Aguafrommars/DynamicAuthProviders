// Project: DymamicAuthProviders
// Copyright (c) 2018 @Olivier Lefebvre
using System;

namespace Aguacongas.AspNetCore.Authentication
{
    /// <summary>
    /// Notification context
    /// </summary>
    public class NotificationContext
    {
        /// <summary>
        /// Gets the services.
        /// </summary>
        /// <value>
        /// The services.
        /// </value>
        public IServiceProvider Services { get; }
        /// <summary>
        /// Gets the scheme.
        /// </summary>
        /// <value>
        /// The scheme.
        /// </value>
        public string Scheme { get; }
        /// <summary>
        /// Gets the action.
        /// </summary>
        /// <value>
        /// The action.
        /// </value>
        public SchemeAction Action { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationContext"/> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        /// <param name="scheme">The scheme.</param>
        /// <param name="action">The action.</param>
        /// <exception cref="System.ArgumentNullException">
        /// serviceProvider
        /// or
        /// scheme
        /// </exception>
        public NotificationContext(IServiceProvider serviceProvider, string scheme, SchemeAction action)
        {
            Services = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            Scheme = scheme ?? throw new ArgumentNullException(nameof(scheme));
            Action = action;
        }
    }
}
