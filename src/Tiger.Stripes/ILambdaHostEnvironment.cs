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

/// <summary>Provides information about the hosting environment an application is running in.</summary>
public interface ILambdaHostEnvironment
    : IHostEnvironment
{
    /// <summary>
    /// Gets or sets the lead time before Lambda environment freeze at which cancellation tokens should request cancellation.
    /// </summary>
    TimeSpan CancellationLeadTime { get; set; }
}
