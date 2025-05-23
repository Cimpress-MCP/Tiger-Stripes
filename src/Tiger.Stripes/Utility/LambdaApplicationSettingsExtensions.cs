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

/// <summary>Extensions to the functionality of the <see cref="LambdaApplicationOptions"/> class.</summary>
static class LambdaApplicationSettingsExtensions
{
    /// <summary>
    /// Converts the provided instance of the <see cref="LambdaApplicationOptions"/> class
    /// to an equivalent instance of the <see cref="HostApplicationBuilderSettings"/> class.
    /// </summary>
    /// <param name="options">The options to convert.</param>
    /// <param name="configuration">The configuration to set on the created host application builder settings.</param>
    /// <returns>An instance of <see cref="HostApplicationBuilderSettings"/> equivalent to the provided instance.</returns>
    public static HostApplicationBuilderSettings ToHostSettings(this LambdaApplicationOptions options, ConfigurationManager configuration) => new()
    {
        Args = options.Args,
        ApplicationName = options.ApplicationName,
        EnvironmentName = options.EnvironmentName,
        Configuration = configuration,
        ContentRootPath = options.ContentRootPath,
    };
}
