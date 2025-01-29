// <copyright file="TigerStripesInstrumentationResourceProviderBuilderExtensions.cs" company="Cimpress plc">
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

namespace OpenTelemetry.Resources;

/// <summary>Extensions to the functionality of the <see cref="ResourceBuilder"/> class.</summary>
public static class TigerStripesInstrumentationResourceProviderBuilderExtensions
{
    /// <summary>Enables AWS Lambda instrumentation.</summary>
    /// <param name="builder"><see cref="ResourceBuilder"/> being configured.</param>
    /// <returns>The instance of <see cref="ResourceBuilder"/> to chain the calls.</returns>
    public static ResourceBuilder AddAwsLambdaDetector(this ResourceBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder.AddDetector(b => b.GetRequiredService<AwsLambdaResourceDetector>());
    }
}
