// <copyright file="InvocationBuilderExtensions.cs" company="Cimpress, Inc.">
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

namespace Tiger.Stripes;

/// <summary>Extensions to the functionality of the <see cref="IInvocationBuilder"/> interface.</summary>
public static partial class InvocationBuilderExtensions
{
    static readonly Func<ILogger, string, IDisposable?> s_handlingScope = LoggerMessage.DefineScope<string>("Processing request {AwsRequestId}…");

    [MethodImpl(AggressiveInlining)]
    static TDependency GetOverridableService<TDependency>(this IServiceProvider serviceProvider, InvocationRequest req, string name)
        where TDependency : notnull
    {
        if (typeof(TDependency) == typeof(ILambdaContext))
        {
            return (TDependency)req.LambdaContext;
        }

        if (typeof(TDependency) == typeof(ILogger))
        {
            var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
            return (TDependency)loggerFactory.CreateLogger(name);
        }

        return serviceProvider.GetRequiredService<TDependency>();
    }

    static IDisposable? Handling(ILogger logger, ILambdaContext context) => s_handlingScope(logger, context.AwsRequestId);

    [LoggerMessage(Warning, "Invocation is nearly out of time!")]
    static partial void NearlyOutOfTime(ILogger logger);
}
