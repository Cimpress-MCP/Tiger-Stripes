// <copyright file="LoggerFactoryExtensions.cs" company="Cimpress plc">
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

/// <summary>Extensions to the functionality of the <see cref="ILoggerFactory"/> interface.</summary>
static class LoggerFactoryExtensions
{
    /// <summary>Creates a logger specialized for the application.</summary>
    /// <param name="factory">The factory with which to create the logger.</param>
    /// <param name="hostEnvironment">The application's host environment.</param>
    /// <returns>A logger specialized for the application.</returns>
    [MethodImpl(AggressiveInlining)]
    public static ILogger CreateApplicationLogger(this ILoggerFactory factory, IHostEnvironment hostEnvironment) =>
        factory.CreateLogger(hostEnvironment.ApplicationName ?? nameof(LambdaApplication));
}
