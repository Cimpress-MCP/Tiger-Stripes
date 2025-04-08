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

/// <summary>Extensions to the functionality of the <see cref="StringBuilder"/> class.</summary>
static class Extensions
{
    /// <summary>Appends the declarations of the parameters of a method to a string builder.</summary>
    /// <param name="builder">The string builder to append to.</param>
    /// <param name="parameters">The parameters to declare.</param>
    /// <returns>The string builder with the declarations appended.</returns>
    public static StringBuilder AppendParameterDeclarations(this StringBuilder builder, ImmutableArray<IParameterSymbol> parameters) =>
        AppendJoin(builder, parameters, AsParameterDeclaration);

    /// <summary>Appends the arguments of a method to a string builder.</summary>
    /// <param name="builder">The string builder to append to.</param>
    /// <param name="parameters">The parameters to use as arguments.</param>
    /// <returns>The string builder with the arguments appended.</returns>
    public static StringBuilder AppendArguments(this StringBuilder builder, ImmutableArray<IParameterSymbol> parameters) =>
        AppendJoin(builder, parameters, static p => p.Name);

    /// <summary>Indicates whether the given operation is a valid operation for this library.</summary>
    /// <param name="operation">The operation to check.</param>
    /// <param name="invocationOperation">
    /// When this method returns, contains the <see cref="IInvocationOperation"/> that was found if
    /// the operation is valid, or <see langword="null"/> if the operation is not valid.
    /// </param>
    /// <returns><see langword="true"/> if the operation is valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValidOperation(this IOperation operation, [NotNullWhen(true)] out IInvocationOperation? invocationOperation)
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
    /// <param name="node">The node to check.</param>
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

    /// <summary>Groups the values of the source by the result of the given selector.</summary>
    /// <typeparam name="TSource">The type of the source values.</typeparam>
    /// <typeparam name="TElement">The type of the elements in the groups.</typeparam>
    /// <param name="source">The source values to group.</param>
    /// <param name="sourceToElementTransform">A function to transform the source values into elements.</param>
    /// <param name="comparer">The comparer to use to compare the source values.</param>
    /// <returns>The grouped values.</returns>
    public static IncrementalValuesProvider<(TSource Source, int Index, ImmutableArray<TElement> Elements)> GroupWith<TSource, TElement>(
        this IncrementalValuesProvider<TSource> source,
        Func<TSource, TElement> sourceToElementTransform,
        IEqualityComparer<TSource> comparer) => source.Collect().SelectMany((values, _) => values
            .GroupBy(Id, sourceToElementTransform, comparer)
            .Select(static (g, i) => (g.Key, Index: i, g.ToImmutableArray())));

    static string AsParameterDeclaration(IParameterSymbol parameter) =>
        $"{parameter.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)} {parameter.Name}";

    static StringBuilder AppendJoin(
        StringBuilder stringBuilder,
        ImmutableArray<IParameterSymbol> parameters,
        Func<IParameterSymbol, string> transform) => parameters.IsEmpty
            ? stringBuilder
            : parameters[1..].Aggregate(
                stringBuilder.Append(transform(parameters[0])),
                (sb, p) => sb.Append(", ").Append(transform(p)));

    static T Id<T>(T t) => t;
}
