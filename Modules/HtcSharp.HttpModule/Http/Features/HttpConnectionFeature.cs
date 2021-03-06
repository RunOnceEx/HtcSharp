// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net;
using HtcSharp.HttpModule.Http.Features.Interfaces;

namespace HtcSharp.HttpModule.Http.Features {
    public class HttpConnectionFeature : IHttpConnectionFeature {
        public string ConnectionId { get; set; }

        public IPAddress LocalIpAddress { get; set; }

        public int LocalPort { get; set; }

        public IPAddress RemoteIpAddress { get; set; }

        public int RemotePort { get; set; }
    }
}