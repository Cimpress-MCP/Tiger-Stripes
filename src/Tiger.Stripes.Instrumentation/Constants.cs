// <copyright file="Constants.cs" company="Cimpress plc">
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

namespace Tiger.Stripes.Instrumentation;

/// <summary>Constants.</summary>
static class Constants
{
    /// <summary>The name of the environment variable which contains the AWS region in which the Lambda Function is running.</summary>
    public const string RegionEnvironmentVariableName = "AWS_REGION";

    /// <summary>The name of the environment variable which contains the name of the Lambda Function.</summary>
    public const string FunctionNameEnvironmentVariableName = "AWS_LAMBDA_FUNCTION_NAME";

    /// <summary>The name of the environment variable which contains the version of the Lambda Function.</summary>
    public const string FunctionVersionEnvironmentVariableName = "AWS_LAMBDA_FUNCTION_VERSION";

    /// <summary>The name of the environment variable which contains the name of the log stream to which the Lambda Function is writing.</summary>
    public const string LogStreamNameEnvironmentVariableName = "AWS_LAMBDA_LOG_STREAM_NAME";

    /// <summary>The name of the environment variable which contains the amount of memory allocated to the Lambda Function.</summary>
    public const string MemorySizeEnvironmentVariableName = "AWS_LAMBDA_FUNCTION_MEMORY_SIZE";

    /// <summary>The name of the environment variable which contains the name of the log group to which the Lambda Function is writing.</summary>
    public const string LogGroupNameEnvironmentVariableName = "AWS_LAMBDA_LOG_GROUP_NAME";

    /// <summary>The name of the attribute which contains the cloud provider.</summary>
    /// <remarks>For us, this is always "aws".</remarks>
    public const string CloudProvider = "cloud.provider";

    /// <summary>The name of the attribute which contains the cloud platform.</summary>
    /// <remarks>For us, this is always "aws_lambda".</remarks>
    public const string CloudPlatform = "cloud.platform";

    /// <summary>The name of the attribute which contains the cloud region.</summary>
    /// <remarks>This is extracted from the executed Function's ARN.</remarks>
    public const string CloudRegion = "cloud.region";

    /// <summary>The name of the attribute which contains the FaaS name.</summary>
    public const string FaasName = "faas.name";

    /// <summary>The name of the attribute which contains the FaaS version.</summary>
    public const string FaasVersion = "faas.version";

    /// <summary>The name of the attribute which contains the FaaS instance.</summary>
    /// <remarks>This is the same value as the name of the log stream, interestingly.</remarks>
    public const string FaasInstance = "faas.instance";

    /// <summary>The name of the attribute which contains the FaaS maximum memory.</summary>
    /// <remarks>This must be multiplied from mebibytes to bytes.</remarks>
    public const string FaasMaxMemory = "faas.max_memory";

    /// <summary>The name of the attribute which contains the AWS log group names.</summary>
    /// <remarks>This is an array value.</remarks>
    public const string AwsLogGroupNames = "aws.log.group.names";

    /// <summary>The name of the attribute which contains the AWS log stream names.</summary>
    /// <remarks>This is an array value.</remarks>
    public const string AwsLogStreamNames = "aws.log.stream.names";
}
