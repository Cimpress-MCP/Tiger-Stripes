// <copyright file="LambdaApplication.Create.cs" company="Cimpress plc">
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

/// <summary>The Lambda application used to configure Function invocations.</summary>
public sealed partial class LambdaApplication
    : IInvocationBuilder, IHost
{
    /// <summary>Initializes a new instance of the <see cref="LambdaApplication"/> class with preconfigured defaults.</summary>
    /// <param name="args">The command line arguments.</param>
    /// <returns>The <see cref="LambdaApplication"/>.</returns>
    public static LambdaApplication Create(string[]? args = null) => new LambdaApplicationBuilder(new()
    {
        Args = args,
    }).Build();

    /// <summary>Initializes a new instance of the <see cref="LambdaApplicationBuilder"/> class with preconfigured defaults.</summary>
    /// <returns>The <see cref="LambdaApplicationBuilder"/>.</returns>
    public static LambdaApplicationBuilder CreateBuilder() => new(new());

    /// <summary>Initializes a new instance of the <see cref="LambdaApplicationBuilder"/> class with preconfigured defaults.</summary>
    /// <param name="args">The command line arguments.</param>
    /// <returns>The <see cref="LambdaApplicationBuilder"/>.</returns>
    public static LambdaApplicationBuilder CreateBuilder(string[] args) => new(new()
    {
        Args = args,
    });

    /// <summary>Initializes a new instance of the <see cref="LambdaApplicationBuilder"/> class with preconfigured defaults.</summary>
    /// <param name="options">The application's configuration options.</param>
    /// <returns>The <see cref="LambdaApplicationBuilder"/>.</returns>
    public static LambdaApplicationBuilder CreateBuilder(LambdaApplicationOptions options) => new(options);

    /// <summary>Initializes a new instance of the <see cref="LambdaApplicationBuilder"/> class with minimal defaults.</summary>
    /// <returns>The <see cref="LambdaApplicationBuilder"/>.</returns>
    public static LambdaApplicationBuilder CreateSlimBuilder() => new(new(), slim: true);

    /// <summary>Initializes a new instance of the <see cref="LambdaApplicationBuilder"/> class with preconfigured defaults.</summary>
    /// <param name="args">The command line arguments.</param>
    /// <returns>The <see cref="LambdaApplicationBuilder"/>.</returns>
    public static LambdaApplicationBuilder CreateSlimBuilder(string[] args) => new(
        new()
        {
            Args = args,
        },
        slim: true);

    /// <summary>Initializes a new instance of the <see cref="LambdaApplicationBuilder"/> class with minimal defaults.</summary>
    /// <param name="options">The application's configuration options.</param>
    /// <returns>The <see cref="LambdaApplicationBuilder"/>.</returns>
    public static LambdaApplicationBuilder CreateSlimBuilder(LambdaApplicationOptions options) => new(options, slim: true);
}
