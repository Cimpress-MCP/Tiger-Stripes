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

/// <summary>Wraps an <see cref="ILambdaContext"/> to provide additional information about the current Lambda invocation.</summary>
sealed class WrappingExtendedLambdaContext(ILambdaContext innnerContext, ILogger logger, string name)
    : IExtendedLambdaContext
{
    /// <inheritdoc/>
    public ILogger Logger { get; } = logger;

    /// <inheritdoc/>
    public string Name { get; } = name;

    /// <inheritdoc/>
    string ILambdaContext.AwsRequestId => innnerContext.AwsRequestId;

    /// <inheritdoc/>
    IClientContext ILambdaContext.ClientContext => innnerContext.ClientContext;

    /// <inheritdoc/>
    string ILambdaContext.FunctionName => innnerContext.FunctionName;

    /// <inheritdoc/>
    string ILambdaContext.FunctionVersion => innnerContext.FunctionVersion;

    /// <inheritdoc/>
    ICognitoIdentity ILambdaContext.Identity => innnerContext.Identity;

    /// <inheritdoc/>
    string ILambdaContext.InvokedFunctionArn => innnerContext.InvokedFunctionArn;

    /// <inheritdoc/>
    ILambdaLogger ILambdaContext.Logger => innnerContext.Logger;

    /// <inheritdoc/>
    string ILambdaContext.LogGroupName => innnerContext.LogGroupName;

    /// <inheritdoc/>
    string ILambdaContext.LogStreamName => innnerContext.LogStreamName;

    /// <inheritdoc/>
    int ILambdaContext.MemoryLimitInMB => innnerContext.MemoryLimitInMB;

    /// <inheritdoc/>
    TimeSpan ILambdaContext.RemainingTime => innnerContext.RemainingTime;
}
