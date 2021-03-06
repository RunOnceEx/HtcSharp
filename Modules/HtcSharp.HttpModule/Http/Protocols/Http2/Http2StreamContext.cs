// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using HtcSharp.HttpModule.Http.Protocols.Http2.FlowControl;
using HtcSharp.HttpModule.Infrastructure;

namespace HtcSharp.HttpModule.Http.Protocols.Http2 {
    internal sealed class Http2StreamContext : HttpConnectionContext {
        public int StreamId { get; set; }
        public IHttp2StreamLifetimeHandler StreamLifetimeHandler { get; set; }
        public Http2PeerSettings ClientPeerSettings { get; set; }
        public Http2PeerSettings ServerPeerSettings { get; set; }
        public Http2FrameWriter FrameWriter { get; set; }
        public InputFlowControl ConnectionInputFlowControl { get; set; }
        public OutputFlowControl ConnectionOutputFlowControl { get; set; }
    }
}
