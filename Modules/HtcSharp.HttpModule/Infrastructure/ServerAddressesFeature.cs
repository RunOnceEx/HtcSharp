﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using HtcSharp.HttpModule.Features;

namespace HtcSharp.HttpModule.Infrastructure {
    internal class ServerAddressesFeature : IServerAddressesFeature {
        public ICollection<string> Addresses { get; } = new List<string>();
        public bool PreferHostingUrls { get; set; }
    }
}
