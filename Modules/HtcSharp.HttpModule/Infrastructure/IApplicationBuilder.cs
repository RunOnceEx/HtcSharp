﻿using System;
using System.Collections.Generic;
using HtcSharp.HttpModule.Http.Http.Abstractions;
using HtcSharp.HttpModule.Infrastructure.Features;

namespace HtcSharp.HttpModule.Infrastructure {
    /// <summary>
    /// Defines a class that provides the mechanisms to configure an application's request pipeline.
    /// </summary>
    public interface IApplicationBuilder {
        /// <summary>
        /// Gets or sets the <see cref="IServiceProvider"/> that provides access to the application's service container.
        /// </summary>
        IServiceProvider ApplicationServices { get; set; }

        /// <summary>
        /// Gets the set of HTTP features the application's server provides.
        /// </summary>
        IFeatureCollection ServerFeatures { get; }

        /// <summary>
        /// Gets a key/value collection that can be used to share data between middleware.
        /// </summary>
        IDictionary<string, object> Properties { get; }

        /// <summary>
        /// Adds a middleware delegate to the application's request pipeline.
        /// </summary>
        /// <param name="middleware">The middleware delegate.</param>
        /// <returns>The <see cref="IApplicationBuilder"/>.</returns>
        IApplicationBuilder Use(Func<RequestDelegate, RequestDelegate> middleware);

        /// <summary>
        /// Creates a new <see cref="IApplicationBuilder"/> that shares the <see cref="Properties"/> of this
        /// <see cref="IApplicationBuilder"/>.
        /// </summary>
        /// <returns>The new <see cref="IApplicationBuilder"/>.</returns>
        IApplicationBuilder New();

        /// <summary>
        /// Builds the delegate used by this application to process HTTP requests.
        /// </summary>
        /// <returns>The request handling delegate.</returns>
        RequestDelegate Build();
    }
}