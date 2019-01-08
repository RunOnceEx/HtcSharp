﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HTCSharp.Core.Helpers.Http {
    public static class HttpMethodExtensions {
        private static readonly ConcurrentDictionary<string, int> ValueCache;

        static HttpMethodExtensions() {
            ValueCache = new ConcurrentDictionary<string, int>();

            foreach (var ctype in Enum.GetValues(typeof(HttpMethod)).Cast<HttpMethod>()) {
                var key = ctype.ToValue();
                var ext = ctype.ToString();
                if (!ValueCache.ContainsKey(key)) ValueCache[key] = (int)ctype;
            }
        }

        private static HttpMethodMetadata GetMetadata(HttpMethod ct) {
            var info = ct.GetType().GetMember(ct.ToString());
            var attributes = info[0].GetCustomAttributes(typeof(HttpMethodMetadata), false);
            return attributes.Length > 0 ? attributes[0] as HttpMethodMetadata : new HttpMethodMetadata();
        }

        public static string ToValue(this HttpMethod ct) {
            return GetMetadata(ct).Value;
        }

        public static HttpMethod FromString(this HttpMethod ct, string contentType) {
            if (string.IsNullOrWhiteSpace(contentType)) return HttpMethod.GET;
            var contenttype = contentType.Trim();
            if (ValueCache.ContainsKey(contenttype)) return (HttpMethod)ValueCache[contenttype.ToUpper()];
            return HttpMethod.GET;
        }
    }

    [AttributeUsage(AttributeTargets.Field)]
    internal class HttpMethodMetadata : Attribute {
        public string Value { get; set; }
        public HttpMethodMetadata() {
            Value = "Get";
        }
    }

    public enum HttpMethod {
        [HttpMethodMetadata(Value = "GET")]
        GET,
        [HttpMethodMetadata(Value = "HEAD")]
        HEAD,
        [HttpMethodMetadata(Value = "POST")]
        POST,
        [HttpMethodMetadata(Value = "PUT")]
        PUT,
        [HttpMethodMetadata(Value = "DELETE")]
        DELETE,
        [HttpMethodMetadata(Value = "CONNECT")]
        CONNECT,
        [HttpMethodMetadata(Value = "OPTIONS")]
        OPTIONS,
        [HttpMethodMetadata(Value = "TRACE")]
        TRACE,
        [HttpMethodMetadata(Value = "PATCH")]
        PATCH
    }
}
