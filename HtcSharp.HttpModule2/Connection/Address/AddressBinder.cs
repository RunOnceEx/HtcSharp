﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using HtcSharp.Core.Logging;
using HtcSharp.HttpModule2.Connection.Exceptions;
using HtcSharp.HttpModule2.Connection.ListenOptions;
using HtcSharp.HttpModule2.Core;
using HtcSharp.HttpModule2.Core.Infrastructure;
using Microsoft.AspNetCore.Builder;

namespace HtcSharp.HttpModule2.Connection.Address {
    internal class AddressBinder {
        public static async Task BindAsync(IServerAddressesFeature addresses, HtcServerOptions serverOptions, Logger logger, Func<ListenOptions.ListenOptions, Task> createBinding) {
            var listenOptions = serverOptions.ListenOptions;
            var strategy = CreateStrategy(
                listenOptions.ToArray(),
                addresses.Addresses.ToArray(),
                addresses.PreferHostingUrls);

            var context = new AddressBindContext {
                Addresses = addresses.Addresses,
                ListenOptions = listenOptions,
                ServerOptions = serverOptions,
                Logger = logger,
                CreateBinding = createBinding
            };

            // reset options. The actual used options and addresses will be populated
            // by the address binding feature
            listenOptions.Clear();
            addresses.Addresses.Clear();

            await strategy.BindAsync(context).ConfigureAwait(false);
        }

        private static IStrategy CreateStrategy(ListenOptions.ListenOptions[] listenOptions, string[] addresses, bool preferAddresses) {
            var hasListenOptions = listenOptions.Length > 0;
            var hasAddresses = addresses.Length > 0;

            if (preferAddresses && hasAddresses) {
                if (hasListenOptions) {
                    return new OverrideWithAddressesStrategy(addresses);
                }

                return new AddressesStrategy(addresses);
            } else if (hasListenOptions) {
                if (hasAddresses) {
                    return new OverrideWithEndpointsStrategy(listenOptions, addresses);
                }

                return new EndpointsStrategy(listenOptions);
            } else if (hasAddresses) {
                // If no endpoints are configured directly using HtcServerOptions, use those configured via the IServerAddressesFeature.
                return new AddressesStrategy(addresses);
            } else {
                // "localhost" for both IPv4 and IPv6 can't be represented as an IPEndPoint.
                return new DefaultAddressStrategy();
            }
        }

        /// <summary>
        /// Returns an <see cref="IPEndPoint"/> for the given host an port.
        /// If the host parameter isn't "localhost" or an IP address, use IPAddress.Any.
        /// </summary>
        protected internal static bool TryCreateIPEndPoint(BindingAddress address, out IPEndPoint endpoint) {
            if (!IPAddress.TryParse(address.Host, out var ip)) {
                endpoint = null;
                return false;
            }

            endpoint = new IPEndPoint(ip, address.Port);
            return true;
        }

        internal static async Task BindEndpointAsync(ListenOptions.ListenOptions endpoint, AddressBindContext context) {
            try {
                await context.CreateBinding(endpoint).ConfigureAwait(false);
            } catch (AddressInUseException ex) {
                throw new IOException($"Failed to bind to address {endpoint}: address already in use.", ex);
            }
            context.ListenOptions.Add(endpoint);
        }

        internal static ListenOptions.ListenOptions ParseAddress(string address, out bool https) {
            var parsedAddress = BindingAddress.Parse(address);
            https = false;

            if (parsedAddress.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase)) {
                https = true;
            } else if (!parsedAddress.Scheme.Equals("http", StringComparison.OrdinalIgnoreCase)) {
                throw new InvalidOperationException($"Unrecognized scheme in server address '{address}'. Only 'http://' is supported.");
            }

            if (!string.IsNullOrEmpty(parsedAddress.PathBase)) {
                throw new InvalidOperationException($"A path base can only be configured using {nameof(IApplicationBuilder)}.UsePathBase().");
            }

            ListenOptions.ListenOptions options = null;
            if (parsedAddress.IsUnixPipe) {
                options = new ListenOptions.ListenOptions(parsedAddress.UnixPipePath);
            } else if (string.Equals(parsedAddress.Host, "localhost", StringComparison.OrdinalIgnoreCase)) {
                // "localhost" for both IPv4 and IPv6 can't be represented as an IPEndPoint.
                options = new LocalhostListenOptions(parsedAddress.Port);
            } else if (TryCreateIPEndPoint(parsedAddress, out var endpoint)) {
                options = new ListenOptions.ListenOptions(endpoint);
            } else {
                // when address is 'http://hostname:port', 'http://*:port', or 'http://+:port'
                options = new AnyIPListenOptions(parsedAddress.Port);
            }

            return options;
        }

        private interface IStrategy {
            Task BindAsync(AddressBindContext context);
        }

        private class DefaultAddressStrategy : IStrategy {
            public async Task BindAsync(AddressBindContext context) {
                var httpDefault = ParseAddress(Constants.DefaultServerAddress, out var https);
                context.ServerOptions.ApplyEndpointDefaults(httpDefault);
                await httpDefault.BindAsync(context).ConfigureAwait(false);

                // Conditional https default, only if a cert is available
                var httpsDefault = ParseAddress(Constants.DefaultServerHttpsAddress, out https);
                context.ServerOptions.ApplyEndpointDefaults(httpsDefault);

                if (httpsDefault.IsTls || httpsDefault.TryUseHttps()) {
                    await httpsDefault.BindAsync(context).ConfigureAwait(false);
                    context.Logger.Debug($"No listening endpoints were configured. Binding to {Constants.DefaultServerAddress} and {Constants.DefaultServerHttpsAddress} by default.");
                } else {
                    // No default cert is available, do not bind to the https endpoint.
                    context.Logger.Debug($"No listening endpoints were configured. Binding to {Constants.DefaultServerAddress} by default.");
                }
            }
        }

        private class OverrideWithAddressesStrategy : AddressesStrategy {
            public OverrideWithAddressesStrategy(IReadOnlyCollection<string> addresses)
                : base(addresses) {
            }

            public override Task BindAsync(AddressBindContext context) {
                var joined = string.Join(", ", _addresses);
                context.Logger.Info($"Overriding endpoints defined in UseKestrel() because {nameof(IServerAddressesFeature.PreferHostingUrls)} is set to true. Binding to address(es) '{joined}' instead.");

                return base.BindAsync(context);
            }
        }

        private class OverrideWithEndpointsStrategy : EndpointsStrategy {
            private readonly string[] _originalAddresses;

            public OverrideWithEndpointsStrategy(IReadOnlyCollection<ListenOptions.ListenOptions> endpoints, string[] originalAddresses)
                : base(endpoints) {
                _originalAddresses = originalAddresses;
            }

            public override Task BindAsync(AddressBindContext context) {
                var joined = string.Join(", ", _originalAddresses);
                context.Logger.Warn($"Overriding address(es) '{joined}'. Binding to endpoints defined in UseKestrel() instead.");

                return base.BindAsync(context);
            }
        }

        private class EndpointsStrategy : IStrategy {
            private readonly IReadOnlyCollection<ListenOptions.ListenOptions> _endpoints;

            public EndpointsStrategy(IReadOnlyCollection<ListenOptions.ListenOptions> endpoints) {
                _endpoints = endpoints;
            }

            public virtual async Task BindAsync(AddressBindContext context) {
                foreach (var endpoint in _endpoints) {
                    await endpoint.BindAsync(context).ConfigureAwait(false);
                }
            }
        }

        private class AddressesStrategy : IStrategy {
            protected readonly IReadOnlyCollection<string> _addresses;

            public AddressesStrategy(IReadOnlyCollection<string> addresses) {
                _addresses = addresses;
            }

            public virtual async Task BindAsync(AddressBindContext context) {
                foreach (var address in _addresses) {
                    var options = ParseAddress(address, out var https);
                    context.ServerOptions.ApplyEndpointDefaults(options);

                    if (https && !options.IsTls) {
                        options.UseHttps();
                    }

                    await options.BindAsync(context).ConfigureAwait(false);
                }
            }
        }
    }
}