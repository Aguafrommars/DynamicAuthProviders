// Project: DymamicAuthProviders
// Copyright (c) 2018 @Olivier Lefebvre
using Moq;
using System;
using Xunit;

namespace Aguacongas.AspNetCore.Authentication.Test
{
    public class NotificationContextTest
    {
        [Fact]
        public void Assertions()
        {
            Assert.Throws<ArgumentNullException>(() => new NotificationContext(null, null, SchemeAction.Added));
            Assert.Throws<ArgumentNullException>(() => new NotificationContext(new Mock<IServiceProvider>().Object, null, SchemeAction.Added));
        }
    }
}
