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

namespace Tiger.Stripes;

/// <summary>Constants.</summary>
static class Constants
{
    /// <summary>The name of the default handler – that is, the name of the "unnamed" handler.</summary>
    public const string DefaultHandlerName = "";

    /// <summary>The configuration key for the cancellation lead time.</summary>
    public const string CancellationLeadTimeKey = "cancellationLeadTime";

    /// <summary>The name of the environment variable which contains the handler name.</summary>
    public const string HandlerNameEnvironmentVariableName = "_HANDLER";

    /// <summary>The name of the attribute which contains the cloud account ID.</summary>
    public const string CloudAccountId = "cloud.account.id";

    /// <summary>The name of the attribute which contains the cloud resource ID.</summary>
    public const string CloudResourceId = "cloud.resource.id";

    /// <summary>The name of the attribute which contains the FaaS cold start flag.</summary>
    public const string FaasColdStart = "faas.coldstart";

    /// <summary>The name of the attribute which contains the FaaS execution ID.</summary>
    public const string FaasExecution = "faas.execution";

    /// <summary>The name of the attribute which contains the AWS Lambda invoked ARN.</summary>
    public const string AwsLambdaInvokedArn = "aws.lambda.invoked_arn";

    /// <summary>The name of the attribute which contains the number of times an invocation has neared its time limit.</summary>
    public const string TigerStripesNearlyOutOfTime = "tiger.stripes.nearly_out_of_time";

    /// <summary>The description of the attribute which contains the number of times an invocation has neared its time limit.</summary>
    public const string TigerStripesNearlyOutOfTimeDescription = "The number of times an invocation has neared its time limit.";

    /// <summary>The message logged when an invocation is nearly out of time.</summary>
    public const string TigerStripesNearlyOutOfTimeMessage = "Invocation is nearly out of time!";

    /// <summary>The name of the attribute which contains the handler name.</summary>
    public const string TigerStripesHandlerName = "tiger.stripes.handler_name";
}
