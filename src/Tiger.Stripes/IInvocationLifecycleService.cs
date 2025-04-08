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

/// <summary>Defines methods which are called during the lifecycle of a Lambda Function invocation.</summary>
public interface IInvocationLifecycleService
{
    /// <summary>Called when the Lambda Function invocation is started.</summary>
    /// <typeparam name="TLambdaContext">The type of the Lambda context.</typeparam>
    /// <param name="lambdaContext">The context of the current Lambda invocation.</param>
    /// <param name="isColdStart">A value indicating whether this is the first invocation of the Lambda Function.</param>
    /// <param name="cancellationToken">A token to watch for operation cancellation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    ValueTask StartedAsync<TLambdaContext>(TLambdaContext lambdaContext, bool isColdStart, CancellationToken cancellationToken)
        where TLambdaContext : IExtendedLambdaContext;

    /// <summary>Called when an error occurs during the Lambda Function invocation.</summary>
    /// <param name="exception">The exception that occurred.</param>
    /// <param name="cancellationToken">A token to watch for operation cancellation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    ValueTask ErrorAsync(Exception exception, CancellationToken cancellationToken);

    /// <summary>Called when the Lambda Function invocation is nearly out of time.</summary>
    void NearlyOutOfTime();

    /// <summary>Called when the Lambda Function invocation is completed.</summary>
    /// <typeparam name="TLambdaContext">The type of the Lambda context.</typeparam>
    /// <param name="lambdaContext">The context of the current Lambda invocation.</param>
    /// <param name="cancellationToken">A token to watch for operation cancellation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    ValueTask CompletedAsync<TLambdaContext>(TLambdaContext lambdaContext, CancellationToken cancellationToken)
        where TLambdaContext : IExtendedLambdaContext;
}
