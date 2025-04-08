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

namespace OpenTelemetry.Metrics;

/// <summary>Extensions to the functionality of the <see cref="MeterProviderBuilder"/> class.</summary>
public static class TigerStripesInstrumentationMeterProviderBuilderExtensions
{
    /// <summary>Enables AWS Lambda instrumentation.</summary>
    /// <param name="builder">The <see cref="MeterProviderBuilder"/> to add the instrumentation to.</param>
    /// <returns>The instance of <see cref="MeterProviderBuilder"/> to chain the calls.</returns>
    public static MeterProviderBuilder AddAwsLambdaInstrumentation(this MeterProviderBuilder builder) =>
        builder.AddMeter(TelemetrySourceName);
}
