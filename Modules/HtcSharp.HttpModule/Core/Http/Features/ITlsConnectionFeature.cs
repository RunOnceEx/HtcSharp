﻿using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace HtcSharp.HttpModule.Core.Http.Features {
    public interface ITlsConnectionFeature {
        /// <summary>
        /// Synchronously retrieves the client certificate, if any.
        /// </summary>
        X509Certificate2 ClientCertificate { get; set; }

        /// <summary>
        /// Asynchronously retrieves the client certificate, if any.
        /// </summary>
        /// <returns></returns>
        Task<X509Certificate2> GetClientCertificateAsync(CancellationToken cancellationToken);
    }
}
