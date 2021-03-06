// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Primitives;
using HtcSharp.HttpModule.IO.Buffers;
using HeaderUtilities = HtcSharp.HttpModule.Http.Headers.HeaderUtilities;

namespace HtcSharp.HttpModule.Http.Protocols.Http
{
    internal sealed partial class HttpResponseHeaders : HttpHeaders
    {
        private static ReadOnlySpan<byte> CrLf => new[] { (byte)'\r', (byte)'\n' };
        private static ReadOnlySpan<byte> ColonSpace => new[] { (byte)':', (byte)' ' };

        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        protected override IEnumerator<KeyValuePair<string, StringValues>> GetEnumeratorFast()
        {
            return GetEnumerator();
        }

        internal void CopyTo(ref BufferWriter<PipeWriter> buffer)
        {
            CopyToFast(ref buffer);

            var extraHeaders = MaybeUnknown;
            if (extraHeaders != null && extraHeaders.Count > 0)
            {
                // Only reserve stack space for the enumartors if there are extra headers
                CopyExtraHeaders(ref buffer, extraHeaders);
            }

            static void CopyExtraHeaders(ref BufferWriter<PipeWriter> buffer, Dictionary<string, StringValues> headers)
            {
                foreach (var kv in headers)
                {
                    foreach (var value in kv.Value)
                    {
                        if (value != null)
                        {
                            buffer.Write(CrLf);
                            buffer.WriteAsciiNoValidation(kv.Key);
                            buffer.Write(ColonSpace);
                            buffer.WriteAsciiNoValidation(value);
                        }
                    }
                }
            }
        }

        private static long ParseContentLength(string value)
        {
            if (!HeaderUtilities.TryParseNonNegativeInt64(value, out var parsed))
            {
                ThrowInvalidContentLengthException(value);
            }

            return parsed;
        }

        private static void ThrowInvalidContentLengthException(string value)
        {
            throw new InvalidOperationException($@"Invalid Content-Length: ""{value}"". Value must be a positive integral number.");
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void SetValueUnknown(string key, StringValues value)
        {
            ValidateHeaderNameCharacters(key);
            Unknown[key] = value;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private bool AddValueUnknown(string key, StringValues value)
        {
            ValidateHeaderNameCharacters(key);
            Unknown.Add(key, value);
            // Return true, above will throw and exit for false
            return true;
        }

        public partial struct Enumerator : IEnumerator<KeyValuePair<string, StringValues>>
        {
            private readonly HttpResponseHeaders _collection;
            private readonly long _bits;
            private int _next;
            private KeyValuePair<string, StringValues> _current;
            private readonly bool _hasUnknown;
            private Dictionary<string, StringValues>.Enumerator _unknownEnumerator;

            internal Enumerator(HttpResponseHeaders collection)
            {
                _collection = collection;
                _bits = collection._bits;
                _next = 0;
                _current = default;
                _hasUnknown = collection.MaybeUnknown != null;
                _unknownEnumerator = _hasUnknown
                    ? collection.MaybeUnknown.GetEnumerator()
                    : default;
            }

            public KeyValuePair<string, StringValues> Current => _current;

            object IEnumerator.Current => _current;

            public void Dispose()
            {
            }

            public void Reset()
            {
                _next = 0;
            }
        }

    }
}
