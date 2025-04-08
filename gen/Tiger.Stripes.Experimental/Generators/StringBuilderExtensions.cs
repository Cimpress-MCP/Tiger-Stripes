// <copyright file="StringBuilderExtensions.cs" company="Cimpress plc">
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

/// <summary>Extensions to the functionality of the <see cref="StringBuilder"/> class.</summary>
static class StringBuilderExtensions
{
    /// <summary>Appends the declarations of the parameters of a method to a string builder.</summary>
    /// <param name="stringBuilder">The string builder to which to append the declarations.</param>
    /// <param name="parameters">The parameters to declare.</param>
    /// <returns>The string builder with the declarations appended.</returns>
    public static StringBuilder AppendParameterDeclarations(
        this StringBuilder stringBuilder,
        ImmutableArray<IParameterSymbol> parameters) => AppendJoin(stringBuilder, parameters, AsParameterDeclaration);

    /// <summary>Appends the arguments of a method to a string builder.</summary>
    /// <param name="stringBuilder">The string builder to which to append the arguments.</param>
    /// <param name="parameters">The parameters to use as arguments.</param>
    /// <returns>The string builder with the arguments appended.</returns>
    public static StringBuilder AppendArguments(
        this StringBuilder stringBuilder,
        ImmutableArray<IParameterSymbol> parameters) => AppendJoin(stringBuilder, parameters, static p => p.Name);

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
}
