// <copyright file="LambdaApplication.cs" company="Cimpress, Inc.">
// Copyright 2023 Cimpress, Inc.
//
// Licensed under the Apache License, Version 2.0 (the "License") –
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//  http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using static System.StringComparer;
using Env = System.Environment;

namespace Tiger.Stripes;

/// <summary>The Lambda application used to configure Function invocations.</summary>
public sealed class LambdaApplication
    : IInvocationBuilder, IAsyncDisposable
{
    readonly IHost _host;
    readonly Dictionary<string, LambdaBootstrapHandler> _handlers = new(Ordinal);

    /// <summary>Initializes a new instance of the <see cref="LambdaApplication"/> class.</summary>
    /// <param name="host">The host which this application wraps.</param>
    internal LambdaApplication(IHost host)
    {
        _host = host;
        Logger = host.Services.GetRequiredService<ILoggerFactory>().CreateLogger(Environment.ApplicationName ?? nameof(LambdaApplication));
    }

    /// <summary>Gets the application's configured services.</summary>
    public IServiceProvider Services => _host.Services;

    /// <summary>Gets the application's configured <see cref="ConfigurationManager"/>.</summary>
    public IConfiguration Configuration => _host.Services.GetRequiredService<IConfiguration>();

    /// <summary>Gets the application's configured <see cref="IHostEnvironment"/>.</summary>
    public ILambdaHostEnvironment Environment => _host.Services.GetRequiredService<ILambdaHostEnvironment>();

    /// <summary>Gets the default logger for the application.</summary>
    public ILogger Logger { get; }

    /// <summary>Initializes a new instance of the <see cref="LambdaApplicationBuilder"/> class with preconfigured defaults.</summary>
    /// <returns>The <see cref="LambdaApplicationBuilder"/>.</returns>
    public static LambdaApplicationBuilder CreateBuilder() => new(new());

    /// <summary>Initializes a new instance of the <see cref="LambdaApplicationBuilder"/> class with preconfigured defaults.</summary>
    /// <param name="options">The application's configuration options.</param>
    /// <returns>The <see cref="LambdaApplicationBuilder"/>.</returns>
    public static LambdaApplicationBuilder CreateBuilder(LambdaApplicationOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        return new(options);
    }

    /// <summary>Initializes a new instance of the <see cref="LambdaApplicationBuilder"/> class with minimal defaults.</summary>
    /// <returns>The <see cref="LambdaApplicationBuilder"/>.</returns>
    public static LambdaApplicationBuilder CreateSlimBuilder() => new(new(), slim: true);

    /// <summary>Initializes a new instance of the <see cref="LambdaApplicationBuilder"/> class with minimal defaults.</summary>
    /// <param name="options">The application's configuration options.</param>
    /// <returns>The <see cref="LambdaApplicationBuilder"/>.</returns>
    public static LambdaApplicationBuilder CreateSlimBuilder(LambdaApplicationOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        return new(options, slim: true);
    }

    /// <inheritdoc/>
    public IInvocationBuilder MapInvoke(string name, LambdaBootstrapHandler handler)
    {
        _handlers[name] = handler;
        return this;
    }

    /// <summary>Runs the application.</summary>
    /// <param name="cancellationToken">A token to watch for operation cancellation.</param>
    /// <returns>A value which, when resolved, represents the completion of the asynchronous operation.</returns>
    /// <exception cref="InvalidOperationException">The environment's handler was provided or not found.</exception>
    public async ValueTask RunAsync(CancellationToken cancellationToken = default)
    {
        var handlerName = Env.GetEnvironmentVariable("_HANDLER")
            ?? throw new InvalidOperationException("Lambda environment has no handler configured!");

        if (!_handlers.TryGetValue(handlerName, out var handler))
        {
            throw new InvalidOperationException($"""
No handler is registered for name "{handlerName}"! (Known handlers are: {string.Join(", ", _handlers.Keys.Select(k => $"'{k}'"))}.)
""");
        }

        using var bootstrap = new LambdaBootstrap(handler);
        await bootstrap.RunAsync(cancellationToken);
    }

    /// <summary>Disposes the application.</summary>
    /// <returns>A task which, when complete, represents the completed disposal operation.</returns>
    public ValueTask DisposeAsync() => ((IAsyncDisposable)_host).DisposeAsync();
}
