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

/// <summary>A builder for Lambda applications and services.</summary>
public sealed class LambdaApplicationBuilder
    : IHostApplicationBuilder
{
    static readonly TimeSpan s_cancellationLeadTimeDefaultValue = TimeSpan.FromMilliseconds(500);

    readonly HostApplicationBuilder _hostApplicationBuilder;

    /// <summary>Initializes a new instance of the <see cref="LambdaApplicationBuilder"/> class.</summary>
    /// <param name="options">The application's configuration options.</param>
    internal LambdaApplicationBuilder(LambdaApplicationOptions options)
    {
        var configuration = PreConfigure();

        _hostApplicationBuilder = Host.CreateApplicationBuilder(options.ToHostSettings(configuration));

        Environment = new LambdaHostEnvironment(_hostApplicationBuilder.Environment)
        {
            CancellationLeadTime = CancellationLeadTime(options, Configuration),
        };
        PostConfigure();
    }

    /// <summary>Initializes a new instance of the <see cref="LambdaApplicationBuilder"/> class.</summary>
    /// <param name="options">The application's configuration options.</param>
    /// <param name="slim">A marker token indicating slim building.</param>
    internal LambdaApplicationBuilder(LambdaApplicationOptions options, bool slim)
    {
        D.Assert(slim, "should only be called with slim: true");

        var configuration = PreConfigure();

        /* note(cosborn)
         * Value of `ContentRoot` is set *between* `LAMBDA_` and `DOTNET_` environment variables
         * in order to match the behavior of other implementations of `IHostApplicationBuilder`.
         */
        if (options is { ContentRootPath: null } && configuration[HostDefaults.ContentRootKey] is null)
        {
            /* note(cosborn)
             * Other implementations do more checks here, but Lambda only runs as Linux.
             * `System.Environment.SystemDirectory` is always the empty string on Linux.
             */
            _ = configuration.AddInMemoryCollection([new(HostDefaults.ContentRootKey, Env.CurrentDirectory)]);
        }

        _ = configuration.AddEnvironmentVariables(prefix: "DOTNET_");

        _hostApplicationBuilder = Host.CreateEmptyApplicationBuilder(options.ToHostSettings(configuration));

        _ = Configuration.AddEnvironmentVariables();

        var serviceProviderFactory = _hostApplicationBuilder.Environment.IsDevelopment()
            ? new(new()
            {
                ValidateOnBuild = true,
                ValidateScopes = true,
            }) : new DefaultServiceProviderFactory();
        _hostApplicationBuilder.ConfigureContainer(serviceProviderFactory);

        Environment = new LambdaHostEnvironment(_hostApplicationBuilder.Environment)
        {
            CancellationLeadTime = CancellationLeadTime(options, Configuration),
        };
        PostConfigure();
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

    [MethodImpl(AggressiveInlining)]
    static TimeSpan CancellationLeadTime(LambdaApplicationOptions options, IConfigurationManager configuration) => options.CancellationLeadTime
        ?? configuration.GetValue<TimeSpan?>(CancellationLeadTimeKey)
        ?? s_cancellationLeadTimeDefaultValue;

    [MethodImpl(AggressiveInlining)]
    static ConfigurationManager PreConfigure()
    {
        var configuration = new ConfigurationManager();

        // note(cosborn) I don't want to reimplement HostApplicationBuilder, so we'll wrap the environment here.
        _ = configuration.AddEnvironmentVariables(prefix: "LAMBDA_");
        return configuration;
    }

    [MethodImpl(AggressiveInlining)]
    void PostConfigure() => Services
        .AddSingleton(Environment)
        .AddScoped<IInvocationLifecycleService, LoggingInvocationLifecycle>()
        .AddScoped<InvocationLifecycleServiceManager>()
        .AddSingleton<LambdaBootstrapHandlerRegistry>()
        .AddHostedService<LambdaBackgroundService>();

    sealed class LambdaHostEnvironment(IHostEnvironment env)
        : ILambdaHostEnvironment
    {
        /// <inheritdoc/>
        public TimeSpan CancellationLeadTime { get; set; }

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
