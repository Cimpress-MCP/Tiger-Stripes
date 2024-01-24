// <copyright file="IInvocationBuilder.cs" company="Cimpress, Inc.">
// Copyright 2023 Cimpress, Inc.
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

/// <summary>A builder for Lambda Function invocations.</summary>
public interface IInvocationBuilder
{
    /// <summary>Gets the application's configured services.</summary>
    IServiceProvider Services { get; }

    /// <summary>Gets the application's configured <see cref="ConfigurationManager"/>.</summary>
    IConfiguration Configuration { get; }

    /// <summary>Gets the application's configured <see cref="ILambdaHostEnvironment"/>.</summary>
    ILambdaHostEnvironment Environment { get; }

    /// <summary>Gets the default logger for the application.</summary>
    ILogger Logger { get; }

    /// <summary>Maps a Lambda Function invocation to the specified parameters.</summary>
    /// <param name="name">The name of the handler.</param>
    /// <param name="handler">The invocation handler.</param>
    /// <returns>The invocation builder for further mapping.</returns>
    IInvocationBuilder MapInvoke(string name, LambdaBootstrapHandler handler);
}
