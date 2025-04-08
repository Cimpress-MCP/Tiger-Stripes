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

/* todo(cosborn)
 * I wonder sometimes whether it would be better to `Task.WhenAll` all of these methods.
 * (Yes, even taking into consideration the overhead of calling `AsTask` on each value.)
 */

/// <summary>Manages the invocation lifecycle services.</summary>
/// <param name="invocationLifecycles">The invocation lifecycle services to manage.</param>
sealed class InvocationLifecycleServiceManager(IEnumerable<IInvocationLifecycleService> invocationLifecycles)
    : IAsyncDisposable
{
    readonly List<CancellationTokenRegistration> _registrations = [];

    /// <summary>
    /// Invokes the <see cref="IInvocationLifecycleService.StartedAsync{TLambdaContext}(TLambdaContext, bool, CancellationToken)"/> method on each invocation lifecycle service.
    /// </summary>
    /// <typeparam name="TLambdaContext">The type of the Lambda context.</typeparam>
    /// <param name="lambdaContext">The context of the current Lambda invocation.</param>
    /// <param name="isColdStart">A value indicating whether this is the first invocation of the Lambda Function.</param>
    /// <param name="cancellationToken">A token to watch for operation cancellation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async ValueTask StartAsync<TLambdaContext>(TLambdaContext lambdaContext, bool isColdStart, CancellationToken cancellationToken)
        where TLambdaContext : IExtendedLambdaContext
    {
        foreach (var lifecycle in invocationLifecycles)
        {
            await lifecycle.StartedAsync(lambdaContext, isColdStart, cancellationToken);
            var registration = cancellationToken.UnsafeRegister(static l => ((IInvocationLifecycleService)l!).NearlyOutOfTime(), lifecycle);
            _registrations.Add(registration);
        }
    }

    /// <summary>
    /// Invokes the <see cref="IInvocationLifecycleService.ErrorAsync(Exception, CancellationToken)"/> method on each invocation lifecycle service.
    /// </summary>
    /// <param name="exception">The exception that occurred.</param>
    /// <param name="cancellationToken">A token to watch for operation cancellation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async ValueTask ErrorAsync(Exception exception, CancellationToken cancellationToken)
    {
        foreach (var lifecycle in invocationLifecycles)
        {
            await lifecycle.ErrorAsync(exception, cancellationToken);
        }
    }

    /// <summary>
    /// Invokes the <see cref="IInvocationLifecycleService.CompletedAsync{TLambdaContext}(TLambdaContext, CancellationToken)"/> method on each invocation lifecycle service.
    /// </summary>
    /// <typeparam name="TLambdaContext">The type of the Lambda context.</typeparam>
    /// <param name="lambdaContext">The context of the current Lambda invocation.</param>
    /// <param name="cancellationToken">A token to watch for operation cancellation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async ValueTask CompleteAsync<TLambdaContext>(TLambdaContext lambdaContext, CancellationToken cancellationToken)
        where TLambdaContext : IExtendedLambdaContext
    {
        foreach (var lifecycle in invocationLifecycles)
        {
            await lifecycle.CompletedAsync(lambdaContext, cancellationToken);
        }
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        foreach (var registration in _registrations)
        {
            await registration.DisposeAsync();
        }
    }
}
