// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace HtcSharp.HttpModule2.Http.Protocols.Http
{
    public enum HttpMethod: byte
    {
        Get,
        Put,
        Delete,
        Post,
        Head,
        Trace,
        Patch,
        Connect,
        Options,

        Custom,

        None = byte.MaxValue,
    }
}