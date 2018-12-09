using System;
using Xunit;

namespace Aguacongas.AspNetCore.Authentication.Test
{
    public class OptionsMonitorCacheWrapperTest
    {
        [Fact]
        public void Assertions()
        {
            Assert.Throws<ArgumentNullException>(() => new OptionsMonitorCacheWrapper<string>(null, null, null));
            Assert.Throws<ArgumentNullException>(() => new OptionsMonitorCacheWrapperFactory(null));
        }
    }
}
