// <copyright file="DiscoveryExtensions.cs" company="Cimpress plc">
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

namespace Tiger.Stripes.Experimental;

/// <summary>Extensions for discovering correct invocations and overloads.</summary>
static class DiscoveryExtensions
{
        /// <summary>Indicates whether the given operation is a valid operation for this library.</summary>
    /// <param name="operation">The operation to check.</param>
    /// <param name="invocationOperation">
    /// When this method returns, contains the <see cref="IInvocationOperation"/> that was found if
    /// the operation is valid, or <see langword="null"/> if the operation is not valid.
    /// </param>
    /// <returns><see langword="true"/> if the operation is valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValidOperation(this IOperation? operation, [NotNullWhen(true)] out IInvocationOperation? invocationOperation)
    {
        /* note(cosborn)
         * We're looking for this signature:
         * static IInvocationBuilder MapInvoke<TService>(this IInvocationBuilder, string, JsonSerializerContext)
         */
        if (operation is IInvocationOperation { TargetMethod: { } tm } io &&
            tm.ContainingNamespace is { Name: "Stripes", ContainingNamespace: { Name: "Tiger", ContainingNamespace.IsGlobalNamespace: true } } &&
            tm is { ContainingAssembly.Name: "Tiger.Stripes", ContainingType.Name: "InvocationBuilderExtensions", IsExtensionMethod: true, TypeArguments: [{ } ta] } &&
            !tm.Parameters.Any(p => SymbolEqualityComparer.Default.Equals(p.Type, ta)))
        {
            invocationOperation = io;
            return true;
        }

        invocationOperation = null;
        return false;
    }

    /// <summary>Attempts to get the name of the method invoked by the given node.</summary>
    /// <param name="node">The node to analyze.</param>
    /// <param name="methodName">
    /// When this method returns, contains the name of the method invoked by the node if one was found,
    /// or <see langword="null"/> if no method was found.
    /// </param>
    /// <returns><see langword="true"/> if a method was found; otherwise, <see langword="false"/>.</returns>
    public static bool TryGetMethodName(this SyntaxNode node, out string? methodName)
    {
        if (node is InvocationExpressionSyntax { Expression: MemberAccessExpressionSyntax { Name.Identifier.ValueText: var method } })
        {
            methodName = method;
            return true;
        }

        methodName = null;
        return false;
    }
}
