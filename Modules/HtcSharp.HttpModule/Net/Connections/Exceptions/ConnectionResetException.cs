﻿using System;
using System.IO;

namespace HtcSharp.HttpModule.Net.Connections.Exceptions {
    public class ConnectionResetException : IOException {
        public ConnectionResetException(string message) : base(message) {
        }

        public ConnectionResetException(string message, Exception inner) : base(message, inner) {
        }
    }
}
