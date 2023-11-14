// <copyright file="LambdaApplicationBuilder.cs" company="Cimpress, Inc.">
//   Copyright 2023 Cimpress, Inc.
//
//   Licensed under the Apache License, Version 2.0 (the "License") –
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.
// </copyright>

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using static System.Diagnostics.Debug;
using Env = System.Environment;

namespace Tiger.Stripes;

/// <summary>A builder for Lambda applications and services.</summary>
public sealed class LambdaApplicationBuilder
{
    const string CancellationTimeoutKey = "cancellationTimeout";
    static readonly TimeSpan s_cancellationTimeoutDefaultValue = TimeSpan.FromMilliseconds(500);

    readonly HostApplicationBuilder _hostApplicationBuilder;

    /// <summary>Initializes a new instance of the <see cref="LambdaApplicationBuilder"/> class.</summary>
    /// <param name="options">The application's configuration options.</param>
    internal LambdaApplicationBuilder(LambdaApplicationOptions options)
    {
        _hostApplicationBuilder = Host.CreateApplicationBuilder(options.ToSettings(new()));

        // note(cosborn) I don't want to reimplement HostApplicationBuilder, so we'll wrap the environment here.
        _ = _hostApplicationBuilder.Configuration.AddEnvironmentVariables(prefix: "LAMBDA_");

        Environment = new LambdaHostingEnvironment(_hostApplicationBuilder.Environment)
        {
            CancellationTimeout = CancellationTimeout(options, Configuration),
        };
        _ = Services.AddSingleton(Environment);
    }

    /// <summary>Initializes a new instance of the <see cref="LambdaApplicationBuilder"/> class.</summary>
    /// <param name="options">The application's configuration options.</param>
    /// <param name="slim">A marker token indicating slim building.</param>
    internal LambdaApplicationBuilder(LambdaApplicationOptions options, bool slim)
    {
        Assert(slim, "should only be called with slim: true");

        var configuration = new ConfigurationManager();
        if (options is { ContentRootPath: null })
        {
            configuration[HostDefaults.ContentRootKey] = Env.CurrentDirectory;
        }

        _ = configuration.AddEnvironmentVariables(prefix: "DOTNET_");
        _ = configuration.AddEnvironmentVariables(prefix: "LAMBDA_");

        _hostApplicationBuilder = Host.CreateEmptyApplicationBuilder(options.ToSettings(configuration));

        _ = configuration.AddEnvironmentVariables();

        DefaultServiceProviderFactory serviceProviderFactory = _hostApplicationBuilder.Environment.IsDevelopment()
            ? new(new()
            {
                ValidateOnBuild = true,
                ValidateScopes = true,
            }) : new();
        _hostApplicationBuilder.ConfigureContainer(serviceProviderFactory);

        Environment = new LambdaHostingEnvironment(_hostApplicationBuilder.Environment)
        {
            CancellationTimeout = CancellationTimeout(options, Configuration),
        };
        _ = Services.AddSingleton(Environment);
    }

    /// <summary>Gets information about the web hosting environment an application is running.</summary>
    public ILambdaHostEnvironment Environment { get; }

    /// <summary>Gets a collection of services for the application to compose. This is useful for adding user provided or framework provided services.</summary>
    public IServiceCollection Services => _hostApplicationBuilder.Services;

    /// <summary>Gets a collection of configuration providers for the application to compose. This is useful for adding new configuration sources and providers.</summary>
    public ConfigurationManager Configuration => _hostApplicationBuilder.Configuration;

    /// <summary>Gets a collection of logging providers for the application to compose. This is useful for adding new logging providers.</summary>
    public ILoggingBuilder Logging => _hostApplicationBuilder.Logging;

    /// <summary>Builds the <see cref="LambdaApplication"/>.</summary>
    /// <returns>A configured <see cref="LambdaApplication"/>.</returns>
    public LambdaApplication Build() => new(_hostApplicationBuilder.Build());

    static TimeSpan CancellationTimeout(LambdaApplicationOptions options, ConfigurationManager configuration) => options.CancellationTimeout
        ?? configuration.GetSection(CancellationTimeoutKey).Get<TimeSpan?>()
        ?? s_cancellationTimeoutDefaultValue;

    /// <summary>The environment in which the application is hosted.</summary>
    sealed class LambdaHostingEnvironment(IHostEnvironment env)
        : ILambdaHostEnvironment
    {
        /// <inheritdoc/>
        public TimeSpan CancellationTimeout { get; set; }

        /// <inheritdoc/>
        public string EnvironmentName
        {
            get => env.EnvironmentName;
            set => env.EnvironmentName = value;
        }

        /// <inheritdoc/>
        public string ApplicationName
        {
            get => env.ApplicationName;
            set => env.ApplicationName = value;
        }

        /// <inheritdoc/>
        public string ContentRootPath
        {
            get => env.ContentRootPath;
            set => env.ContentRootPath = value;
        }

        /// <inheritdoc/>
        public IFileProvider ContentRootFileProvider
        {
            get => env.ContentRootFileProvider;
            set => env.ContentRootFileProvider = value;
        }
    }
}
