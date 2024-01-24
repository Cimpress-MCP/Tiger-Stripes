// <copyright file="LambdaApplicationBuilder.cs" company="Cimpress, Inc.">
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
using static System.Diagnostics.Debug;
using Env = System.Environment;

namespace Tiger.Stripes;

/// <summary>A builder for Lambda applications and services.</summary>
public sealed class LambdaApplicationBuilder
    : IHostApplicationBuilder
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

        Environment = new LambdaHostEnvironment(_hostApplicationBuilder.Environment)
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

        _ = _hostApplicationBuilder.Configuration.AddEnvironmentVariables();

        DefaultServiceProviderFactory serviceProviderFactory = _hostApplicationBuilder.Environment.IsDevelopment()
            ? new(new()
            {
                ValidateOnBuild = true,
                ValidateScopes = true,
            }) : new();
        _hostApplicationBuilder.ConfigureContainer(serviceProviderFactory);

        Environment = new LambdaHostEnvironment(_hostApplicationBuilder.Environment)
        {
            CancellationTimeout = CancellationTimeout(options, Configuration),
        };
        _ = Services.AddSingleton(Environment);
    }

    /// <inheritdoc cref="IHostApplicationBuilder.Environment"/>
    public ILambdaHostEnvironment Environment { get; }

    /// <inheritdoc/>
    public IServiceCollection Services => _hostApplicationBuilder.Services;

    /// <inheritdoc/>
    public IConfigurationManager Configuration => _hostApplicationBuilder.Configuration;

    /// <inheritdoc/>
    public ILoggingBuilder Logging => _hostApplicationBuilder.Logging;

    /// <inheritdoc/>
    public IMetricsBuilder Metrics => _hostApplicationBuilder.Metrics;

    /// <inheritdoc/>
    IDictionary<object, object> IHostApplicationBuilder.Properties => ((IHostApplicationBuilder)_hostApplicationBuilder).Properties;

    /// <inheritdoc/>
    IHostEnvironment IHostApplicationBuilder.Environment => Environment;

    /// <summary>Builds the <see cref="LambdaApplication"/>.</summary>
    /// <returns>A configured <see cref="LambdaApplication"/>.</returns>
    public LambdaApplication Build() => new(_hostApplicationBuilder.Build());

    /// <inheritdoc/>
    public void ConfigureContainer<TContainerBuilder>(IServiceProviderFactory<TContainerBuilder> factory, Action<TContainerBuilder>? configure = null)
        where TContainerBuilder : notnull => _hostApplicationBuilder.ConfigureContainer(factory, configure);

    static TimeSpan CancellationTimeout(LambdaApplicationOptions options, IConfigurationManager configuration) => options.CancellationTimeout
        ?? configuration.GetValue<TimeSpan?>(CancellationTimeoutKey)
        ?? s_cancellationTimeoutDefaultValue;

    /// <summary>The environment in which the application is hosted.</summary>
    sealed class LambdaHostEnvironment(IHostEnvironment env)
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
