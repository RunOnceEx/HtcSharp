// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using HtcSharp.HttpModule.Http.Features.Interfaces;

namespace HtcSharp.HttpModule.Http.Features {
    public class ServiceProvidersFeature : IServiceProvidersFeature {
        public IServiceProvider RequestServices { get; set; }
    }
}