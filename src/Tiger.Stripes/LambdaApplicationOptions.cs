// <copyright file="LambdaApplicationOptions.cs" company="Cimpress plc">
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

namespace Tiger.Stripes;

/// <summary>Application options for configuring the <see cref="LambdaApplication"/> class.</summary>
public sealed record class LambdaApplicationOptions
{
    /// <summary>Gets the command line arguments.</summary>
    [SuppressMessage("Microsoft.Performance", "CA1819", Justification = "Matches the design of `Microsoft.AspNetCore.Builder.WebApplicationOptions`.")]
    public string[]? Args { get; init; }

    /// <summary>Gets the name of the environment in which the application is running.</summary>
    public string? EnvironmentName { get; init; }

    /// <summary>Gets the name of the application.</summary>
    public string? ApplicationName { get; init; }

    /// <summary>Gets the content root path.</summary>
    public string? ContentRootPath { get; init; }

    /// <summary>
    /// Gets the time before freeze of the Lambda execution environment at which
    /// cancellation tokens should request cancellation.
    /// </summary>
    public TimeSpan? CancellationLeadTime { get; init; }
}
