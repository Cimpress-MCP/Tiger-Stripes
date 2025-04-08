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

namespace Tiger.Stripes.Experimental;

/// <summary>Locates syntaxes.</summary>
static class Locate
{
    /// <summary>Locates the service type and candidate methods for a given invocation of MapInvoke.</summary>
    /// <param name="context">The context in which the invocation occurs.</param>
    /// <param name="containingType">The type that contains the invocation.</param>
    /// <param name="invocationExpressionSyntax">The invocation expression syntax.</param>
    /// <returns>The service type and candidate methods, if any.</returns>
    public static (ITypeSymbol ServiceType, ImmutableArray<IMethodSymbol> CandidateMethods)? Candidates(
        SyntaxNodeAnalysisContext context,
        INamedTypeSymbol containingType,
        InvocationExpressionSyntax invocationExpressionSyntax)
    {
        if (context.SemanticModel.GetSymbolInfo(invocationExpressionSyntax, context.CancellationToken) is not { Symbol: IMethodSymbol { Kind: Method, Name: MapInvokeMethodName } methodSymbol })
        {
            return null;
        }

        if (!SymbolEqualityComparer.Default.Equals(methodSymbol.ContainingType.OriginalDefinition, containingType))
        {
            return null;
        }

        if (methodSymbol is not { TypeArguments.Length: 1 } && !methodSymbol.Parameters.All(p => SymbolEqualityComparer.Default.Equals(p.Type, methodSymbol.TypeArguments[0])))
        {
            return null;
        }

        var serviceType = methodSymbol.TypeArguments[0];
        var candidateMethods = serviceType
            .GetMembers()
            .OfType<IMethodSymbol>()
            .Where(static m => AcceptableMethodNames.Contains(m.Name) && !m.IsStatic)
            .ToImmutableArray();

        return (ServiceType: serviceType, CandidateMethods: candidateMethods);
    }
}
