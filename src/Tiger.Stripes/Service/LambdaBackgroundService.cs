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

/// <summary>A background service that runs a Lambda Function.</summary>
sealed class LambdaBackgroundService(LambdaBootstrapHandlerRegistry handlerRegistry, IConfiguration config)
    : BackgroundService
{
    /// <inheritdoc/>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var handlerName = config.GetValue(HandlerNameEnvironmentVariableName, DefaultHandlerName);
        var handler = handlerRegistry[handlerName];
        using var bootstrap = new LambdaBootstrap(handler);
        await bootstrap.RunAsync(stoppingToken);
    }
}
