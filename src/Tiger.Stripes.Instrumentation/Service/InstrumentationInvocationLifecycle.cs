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

namespace Tiger.Stripes.Instrumentation;

/// <summary>Handles lifecycle events by emitting telemetry.</summary>
sealed partial class InstrumentationInvocationLifecycle
    : IInvocationLifecycleService
{
    static readonly ActivitySource s_lambdaActivitySource = new(TelemetrySourceName);

    readonly NearlyOutOfTimeCounter _counter;

    Activity? _activity;

    /// <summary>Initializes a new instance of the <see cref="InstrumentationInvocationLifecycle"/> class.</summary>
    /// <param name="meterFactory">A factory for creating meters.</param>
    [SuppressMessage("Microsoft.Reliability", "CA2000", Justification = "The lifetime of Meter instances is managed by the factory.")]
    public InstrumentationInvocationLifecycle(IMeterFactory meterFactory)
    {
        var meter = meterFactory.Create(TelemetrySourceName);
        _counter = Instruments.CreateNearlyOutOfTimeCounter(meter);
    }

    /// <inheritdoc/>
    ValueTask IInvocationLifecycleService.StartedAsync<TLambdaContext>(TLambdaContext lambdaContext, bool isColdStart, CancellationToken cancellationToken)
    {
        var invocationTagList = CreateInvocationTags(lambdaContext, isColdStart);
        _activity = s_lambdaActivitySource.StartActivity(ActivityKind.Server, tags: invocationTagList, name: lambdaContext.FunctionName);
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc/>
    ValueTask IInvocationLifecycleService.ErrorAsync(Exception exception, CancellationToken cancellationToken)
    {
        _ = _activity?.SetStatus(Error)?.AddException(exception);
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc/>
    void IInvocationLifecycleService.NearlyOutOfTime() => _counter.Add(1);

    /// <inheritdoc/>
    ValueTask IInvocationLifecycleService.CompletedAsync<TLambdaContext>(TLambdaContext lambdaContext, CancellationToken cancellationToken)
    {
        _activity?.Dispose();
        return ValueTask.CompletedTask;
    }

    static TagList CreateInvocationTags<TLambdaContext>(TLambdaContext context, bool isColdStart)
        where TLambdaContext : IExtendedLambdaContext
    {
        var arn = VersionedArn.Create(context);
        return [
            new(AwsLambdaInvokedArn, context.InvokedFunctionArn),
            new(FaasColdStart, isColdStart),
            new(FaasExecution, context.AwsRequestId),
            new(TigerStripesHandlerName, context.Name),
            new(CloudAccountId, arn.AccountId),
            new(CloudResourceId, arn.ToString())];
    }

    static partial class Instruments
    {
        [Counter(Name = TigerStripesNearlyOutOfTime)]
        public static partial NearlyOutOfTimeCounter CreateNearlyOutOfTimeCounter(Meter meter);
    }
}
