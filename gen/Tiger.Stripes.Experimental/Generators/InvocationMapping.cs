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

/// <summary>Represents a mapping between an invocation operation and the method that it invokes.</summary>
sealed class InvocationMapping
{
    readonly IInvocationOperation _operation;
    readonly ITypeSymbol? _serviceType;
    readonly IMethodSymbol? _handlerMethod;
    readonly ImmutableArray<IParameterSymbol> _parameters = [];

    /// <summary>Initializes a new instance of the <see cref="InvocationMapping"/> class.</summary>
    /// <param name="operation">The invocation operation to map.</param>
    /// <param name="semanticModel">The semantic model in which the operation occurs.</param>
    public InvocationMapping(IInvocationOperation operation, SemanticModel semanticModel)
    {
        _operation = operation;
        InterceptableLocation = semanticModel.GetInterceptableLocation((InvocationExpressionSyntax)operation.Syntax)!;
        if (operation is not { TargetMethod.TypeArguments: [{ } serviceType] })
        {
            IsValid = false;
            return;
        }

        _serviceType = serviceType;
        var candidateMethod = _serviceType.GetMembers().OfType<IMethodSymbol>().SingleOrDefault(IsAcceptable);
        if (candidateMethod is not { Parameters: { Length: not 0 } parameters } handlerMethod)
        {
            IsValid = false;
            return;
        }

        _handlerMethod = handlerMethod;
        _parameters = parameters;

        static bool IsAcceptable(IMethodSymbol method) => AcceptableMethodNames.Contains(method.Name)
            && method is { TypeParameters: [], IsStatic: false, IsExtensionMethod: false };
    }

    /// <summary>Gets a value indicating whether this mapping is valid.</summary>
    [MemberNotNullWhen(true, nameof(_serviceType))]
    [MemberNotNullWhen(true, nameof(_handlerMethod))]
    public bool IsValid { get; } = true;

    /// <summary>Gets the location at which the invocation was intercepted.</summary>
    public InterceptableLocation InterceptableLocation { get; }

    /// <summary>Compares the signatures of two invocation mappings.</summary>
    /// <param name="a">The first invocation mapping to compare.</param>
    /// <param name="b">The second invocation mapping to compare.</param>
    /// <returns><see langword="true"/> if the signatures are equal; otherwise, <see langword="false"/>.</returns>
    public static bool SignatureEquals(InvocationMapping a, InvocationMapping b) => a._operation.TargetMethod.TypeArguments.Length == b._operation.TargetMethod.TypeArguments.Length
        && a._operation.TargetMethod.TypeArguments
            .Zip(b._operation.TargetMethod.TypeArguments, static (l, r) => (Left: l, Right: r))
            .All(static pair => SymbolEqualityComparer.Default.Equals(pair.Left, pair.Right));

    /// <summary>Gets a hash code for the signature of the given invocation mapping.</summary>
    /// <param name="invocationMapping">The invocation mapping for which to get a hash code.</param>
    /// <returns>A hash code for the signature of the given invocation mapping.</returns>
    public static int GetSignatureHashCode(InvocationMapping invocationMapping)
    {
        unchecked
        {
            return invocationMapping._operation.TargetMethod.TypeArguments
                .Aggregate(17, static (acc, cur) => (acc * 23) + SymbolEqualityComparer.Default.GetHashCode(cur));
        }
    }

    /// <summary>Generates a C# expression that invokes the mapped method.</summary>
    /// <returns>A C# expression that invokes the mapped method.</returns>
    public string GenerateInvocation()
    {
        if (!IsValid)
        {
            throw new InvalidOperationException("Invocation mapping must be valid.");
        }

        var sb = new StringBuilder();
        return sb
            .Append("static (")
            .Append(_serviceType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat))
            .Append(" __service, ")
            .AppendParameterDeclarations(_parameters)
            .Append(") => __service.")
            .Append(_handlerMethod.Name)
            .Append('(')
            .AppendArguments(_parameters)
            .Append(')')
            .ToString();
    }

    /// <inheritdoc/>
    public override bool Equals(object o) => o is InvocationMapping other &&
        InterceptableLocation.Equals(other.InterceptableLocation) && SignatureEquals(this, other);

    /// <inheritdoc/>
    public override int GetHashCode() => GetSignatureHashCode(this);
}
