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

namespace Tiger.Stripes.Instrumentation;

/// <summary>Detects resources in a Lambda Function environment.</summary>
/// <param name="config">The application's configuration values.</param>
sealed class AwsLambdaResourceDetector(IConfiguration config)
    : IResourceDetector
{
    const long MiB = 1024 * 1024;
    const string Aws = "aws";
    const string AwsLambda = "aws_lambda";

    /// <inheritdoc/>
    Resource IResourceDetector.Detect()
    {
        var attrs = new Attributes
        {
            [CloudProvider] = Aws,
            [CloudPlatform] = AwsLambda,
        };

        if (config.GetValue<string>(RegionEnvironmentVariableName) is { Length: not 0 } region)
        {
            attrs[CloudRegion] = region;
        }

        if (config.GetValue<string>(FunctionNameEnvironmentVariableName) is { Length: not 0 } name)
        {
            attrs[FaasName] = name;
        }

        if (config.GetValue<string>(FunctionVersionEnvironmentVariableName) is { Length: not 0 } version)
        {
            attrs[FaasVersion] = version;
        }

        // note(cosborn) Yup, this identifies the instance. This is what the spec says.
        if (config.GetValue<string>(LogStreamNameEnvironmentVariableName) is { Length: not 0 } instance)
        {
            attrs[FaasInstance] = instance;
        }

        if (config.GetValue<string>(MemorySizeEnvironmentVariableName) is { Length: not 0 } maxMemory)
        {
            attrs[FaasMaxMemory] = int.Parse(maxMemory, InvariantCulture) * MiB;
        }

        if (config.GetValue<string>(LogGroupNameEnvironmentVariableName) is { Length: not 0 } logGroupName)
        {
            attrs[AwsLogGroupNames] = new[] { logGroupName };
        }

        if (config.GetValue<string>(LogStreamNameEnvironmentVariableName) is { Length: not 0 } logStreamName)
        {
            attrs[AwsLogStreamNames] = new[] { logStreamName };
        }

        return new(attrs);
    }
}
