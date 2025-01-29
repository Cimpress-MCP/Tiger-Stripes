// <copyright file="InvocationBuilderExtensions.cs" company="Cimpress plc">
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
// </copyright>

namespace Tiger.Stripes;

/// <summary>Extensions to the functionality of the <see cref="IInvocationBuilder"/> interface.</summary>
public static partial class InvocationBuilderExtensions
{
    static readonly ActivitySource s_lambdaActivitySource = new(TelemetrySourceName);

    static readonly Func<ILogger, string, string, IDisposable?> s_handlingScope =
        LoggerMessage.DefineScope<string, string>("Processing request '{AwsRequestId}' with handler '{HandlerName}'…");

    static readonly Pipe s_outputPipe = new(new(useSynchronizationContext: false));
    static readonly Utf8JsonWriter s_jsonWriter = new(s_outputPipe.Writer);

    [MethodImpl(AggressiveInlining)]
    static TDependency GetSpecializedService<TDependency>(this IServiceProvider sp, InvocationRequest req, string name)
        where TDependency : notnull
    {
        /* note(cosborn)
         * Whether or not the ordering here is actually important, it feels like it is,
         * so let's go over the reasoning in detail. First are the pseudo-dependencies:
         * these are things that are not actually in the DI container, but are provided
         * by the Lambda runtime. These change with each request (even the logger; it's
         * associated with the request ID for the log messages), so they can be neither
         * resolved nor specialized. Then comes a check to see if the dependency can be
         * resolved at all. If someone has already registered in the DI container, say,
         * a bare `ILogger`, then we should respect that. Then come the specializations
         * for which we can do something helpful or smart. Finally, we fall back to the
         * original `GetRequiredService` method so that expected exceptions get thrown.
         */

        if (typeof(TDependency) == typeof(ILambdaContext))
        {
            return (TDependency)req.LambdaContext;
        }

        if (typeof(TDependency) == typeof(ILambdaLogger))
        {
            return (TDependency)req.LambdaContext.Logger;
        }

        if (sp.GetService<TDependency>() is { } dep)
        {
            return dep;
        }

        /* note(cosborn)
         * A bare `ILoggger` doesn't get added to the DI container – only `ILogger<T>`.
         * We predict that handlers will frequently be static methods defined in static
         * classes, so we allow it to be resolved and given a reasonable category name.
         */
        if (typeof(TDependency) == typeof(ILogger))
        {
            var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
            return name is DefaultHandlerName
                ? (TDependency)loggerFactory.CreateApplicationLogger(sp.GetRequiredService<ILambdaHostEnvironment>())
                : (TDependency)loggerFactory.CreateLogger(name);
        }

        // note(cosborn) Retain existing exception behavior if dependency is not found.
        return sp.GetRequiredService<TDependency>();
    }

    static IDisposable? Handling(ILogger logger, ILambdaContext context, string name) =>
        s_handlingScope(logger, context.AwsRequestId, name);

    static TagList InvocationTags(ILambdaContext context, string name, bool isColdStart)
    {
        const char ArnSeparator = ':';
        const string Arn = "arn";
        const string Lambda = "lambda";
        const string Function = "function";

        var arnParts = context.InvokedFunctionArn.Split(ArnSeparator);
        var tagList = new TagList(
            new(AwsLambdaInvokedArn, context.InvokedFunctionArn),
            new(FaasColdStart, isColdStart),
            new(FaasExecution, context.AwsRequestId),
            new(TigerStripesHandlerName, name));

        if (AccountId(arnParts) is { } accountId)
        {
            tagList.Add(new(CloudAccountId, accountId));
        }

        if (VersionedArn(arnParts, context.FunctionVersion) is { } versionedArn)
        {
            tagList.Add(new(CloudResourceId, versionedArn));
        }

        return tagList;

        static string? AccountId(ReadOnlySpan<string> arnParts) =>
            arnParts is [Arn, _, Lambda, _, _, { } accountId, .. _] ? accountId : null;

        // arn:aws:lambda:<region>:<account-id>:function:<function-name>
        static string? VersionedArn(ReadOnlySpan<string> arnParts, string version) =>
            arnParts is [Arn, { } partition, Lambda, { } region, { } accountId, Function, { } resourceName, .. _]
                ? string.Join(ArnSeparator, Arn, partition, Lambda, region, accountId, Function, resourceName, version)
                : null;
    }
}
