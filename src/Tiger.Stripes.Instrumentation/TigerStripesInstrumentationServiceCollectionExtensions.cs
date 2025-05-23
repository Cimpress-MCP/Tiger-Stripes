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

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>Extensions to the functionality of the <see cref="IServiceCollection"/> interface.</summary>
public static class TigerStripesInstrumentationServiceCollectionExtensions
{
    /// <summary>Registers the services required for Tiger Stripes Instrumentation.</summary>
    /// <param name="services">The collection of services to which the required services will be added.</param>
    /// <returns>The collection of services to which the required services have been added.</returns>
    public static IServiceCollection AddAwsLambdaInstrumentation(this IServiceCollection services) => services
        .AddSingleton<AwsLambdaResourceDetector>()
        .AddSingleton<IInvocationLifecycleService, InstrumentationInvocationLifecycle>();
}
