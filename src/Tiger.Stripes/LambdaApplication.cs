// Copyright 2024 Cimpress plc
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

namespace Tiger.Stripes;

/// <summary>The Lambda application used to configure Function invocations.</summary>
public sealed partial class LambdaApplication
    : IInvocationBuilder, IHost
{
    readonly IHost _host;
    readonly LambdaBootstrapHandlerRegistry _handlerRegistry;

    /// <summary>Initializes a new instance of the <see cref="LambdaApplication"/> class.</summary>
    /// <param name="host">The host which this application wraps.</param>
    internal LambdaApplication(IHost host)
    {
        _host = host;
        _handlerRegistry = _host.Services.GetRequiredService<LambdaBootstrapHandlerRegistry>();
        Logger = CreateApplicationLogger(_host, Environment);

        static ILogger CreateApplicationLogger(IHost host, ILambdaHostEnvironment env) =>
            host.Services.GetRequiredService<ILoggerFactory>().CreateLogger(env.ApplicationName ?? nameof(LambdaApplication));
    }

    /// <summary>Gets the application's configured services.</summary>
    public IServiceProvider Services => _host.Services;

    /// <summary>Gets the application's configured <see cref="ConfigurationManager"/>.</summary>
    public IConfiguration Configuration => _host.Services.GetRequiredService<IConfiguration>();

    /// <summary>Gets the application's configured <see cref="ILambdaHostEnvironment"/>.</summary>
    public ILambdaHostEnvironment Environment => _host.Services.GetRequiredService<ILambdaHostEnvironment>();

    /// <summary>Gets the default logger for the application.</summary>
    public ILogger Logger { get; }

    /// <inheritdoc/>
    public IInvocationBuilder MapInvoke(string name, LambdaBootstrapHandler handler)
    {
        // note(cosborn) I wonder sometimes whether it's better to overwrite or to throw a runtime exception.
        // todo(cosborn) Try to see what ASP.NET Core does if you map to the same path twice.
        _handlerRegistry.Add(name, handler);
        return this;
    }

    /// <inheritdoc/>
    public void Dispose() => _host.Dispose();

    /// <inheritdoc/>
    Task IHost.StartAsync(CancellationToken cancellationToken) => _host.StartAsync(cancellationToken);

    /// <inheritdoc/>
    Task IHost.StopAsync(CancellationToken cancellationToken) => _host.StopAsync(cancellationToken);
}
