// Project: aguacongas/DymamicAuthProviders
// Copyright (c) 2018 @Olivier Lefebvre
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Text;

namespace Aguacongas.AspNetCore.Authentication.Redis
{
    /// <summary>
    /// Redis Logger
    /// </summary>
    public class RedisLogger : TextWriter
    {
        private readonly ILogger<RedisLogger> _logger;

        /// <summary>
        /// When overridden in a derived class, returns the character encoding in which the output is written.
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        public override Encoding Encoding => throw new NotImplementedException();

        /// <summary>
        /// Constructs a new instance of <see cref="RedisLogger"/>.
        /// </summary>
        /// <param name="logger">A logger</param>
        public RedisLogger(ILogger<RedisLogger> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public override void WriteLine(string format, object arg0)
        {
            _logger.LogTrace(format, arg0);
        }

        /// <inheritdoc />
        public override void WriteLine(string format, object arg0, object arg1)
        {
            _logger.LogTrace(format, arg0, arg1);
        }

        /// <inheritdoc />
        public override void WriteLine(string format, object arg0, object arg1, object arg2)
        {
            _logger.LogTrace(format, arg0, arg1, arg2);
        }

        /// <inheritdoc />
        public override void WriteLine(string format, params object[] arg)
        {
            _logger.LogTrace(format, arg);
        }

        /// <inheritdoc />
        public override void WriteLine(string value)
        {
            _logger.LogTrace(value);
        }
    }
}
