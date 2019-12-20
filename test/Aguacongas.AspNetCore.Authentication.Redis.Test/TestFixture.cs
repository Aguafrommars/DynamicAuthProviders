// Project: aguacongas/DymamicAuthProviders
// Copyright (c) 2020 @Olivier Lefebvre
using StackExchange.Redis;
using Xunit;

namespace Aguacongas.AspNetCore.Authentication.Redis.Test
{
    public class TestFixture
    {
        public IDatabase Database { get; }

        public TestFixture()
        {
            var options = new ConfigurationOptions();
            options.EndPoints.Add("localhost:6379");
            options.AllowAdmin = true;

            var multiplexer = ConnectionMultiplexer.Connect(options);
            Database = multiplexer.GetDatabase();
            var server = multiplexer.GetServer("localhost:6379");
            server.FlushDatabase();
        }
    }

    [CollectionDefinition("Redis")]
    public class CollectionFixture: ICollectionFixture<TestFixture>
    {
    }
}
