// Project: aguacongas/DymamicAuthProviders
// Copyright (c) 2021 @Olivier Lefebvre
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;
using System.Collections.Generic;
using Xunit;

namespace Aguacongas.AspNetCore.Authentication.Redis.Test
{
    public class TestFixture
    {
        public IDatabase Database { get; }

        public TestFixture()
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    ["redis"] = "localhost,allowAdmin=true"
                })
                .AddUserSecrets<TestFixture>(true)
                .Build();
            var redisConnectionString = configuration.GetValue<string>("redis");
            var multiplexer = ConnectionMultiplexer.Connect(redisConnectionString);
            Database = multiplexer.GetDatabase(10);
            var server = multiplexer.GetServer("localhost:6379");
            server.FlushDatabase(10);
        }
    }

    [CollectionDefinition("Redis")]
    public class CollectionFixture: ICollectionFixture<TestFixture>
    {
    }
}
