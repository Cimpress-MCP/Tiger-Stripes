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

/// <summary>Handles lifecycle events by emitting log messages.</summary>
/// <param name="logger">An application logger, specialized for <see cref="LoggingInvocationLifecycle"/>.</param>
sealed partial class LoggingInvocationLifecycle(ILogger<LoggingInvocationLifecycle> logger)
    : IInvocationLifecycleService
{
    static readonly Func<ILogger, string, string, IDisposable?> s_handlingScope =
        LoggerMessage.DefineScope<string, string>("Processing request '{AwsRequestId}' with handler '{HandlerName}'…");

    IDisposable? _handlingScope;

    /// <inheritdoc/>
    ValueTask IInvocationLifecycleService.StartedAsync<TLambdaContext>(
        TLambdaContext lambdaContext,
        bool isColdStart,
        CancellationToken cancellationToken)
    {
        _handlingScope = Handling(logger, lambdaContext);
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc/>
    ValueTask IInvocationLifecycleService.ErrorAsync(Exception exception, CancellationToken cancellationToken) => ValueTask.CompletedTask;

    /// <inheritdoc/>
    void IInvocationLifecycleService.NearlyOutOfTime() => NearlyOutOfTime();

    /// <inheritdoc/>
    ValueTask IInvocationLifecycleService.CompletedAsync<TLambdaContext>(TLambdaContext lambdaContext, CancellationToken cancellationToken)
    {
        _handlingScope?.Dispose();
        return ValueTask.CompletedTask;
    }

    static IDisposable? Handling<TLambdaContext>(ILogger logger, TLambdaContext lambdaContext)
        where TLambdaContext : IExtendedLambdaContext => s_handlingScope(logger, lambdaContext.AwsRequestId, lambdaContext.Name);

    [LoggerMessage(Warning, TigerStripesNearlyOutOfTimeMessage)]
    partial void NearlyOutOfTime();
}
