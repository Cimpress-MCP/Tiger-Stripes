// <copyright file="StringBuilderExtensions.cs" company="Cimpress, Inc.">
//   Copyright 2023 Cimpress, Inc.
//
//   Licensed under the Apache License, Version 2.0 (the "License") –
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.
// </copyright>

namespace Tiger.Stripes.Generator;

/// <summary>Extensions to the functionality of the <see cref="StringBuilder"/> class.</summary>
[SuppressMessage("StyleCop.Readability", "SA1118", Justification = "Analyzer doesn't understand raw string literals.")]
static class StringBuilderExtensions
{
    /// <summary>Appends arguments to this instance.</summary>
    /// <param name="builder">The instance to which to append arguments.</param>
    /// <param name="count">The number of arguments.</param>
    /// <returns>This instance.</returns>
    public static StringBuilder AppendArguments(this StringBuilder builder, int count) =>
        AppendAggregate(builder, count, static (acc, cur) => acc.Append(InvariantCulture, $", dependency{cur}"));

    /// <summary>Appends declarations to this instance.</summary>
    /// <param name="builder">The instance to which to append declarations.</param>
    /// <param name="count">The number of declarations.</param>
    /// <returns>This instance.</returns>
    public static StringBuilder AppendDeclarations(this StringBuilder builder, int count) =>
        AppendAggregate(builder, count, static (acc, cur) => acc.AppendLine(InvariantCulture, $"""
                var dependency{cur} = scope.ServiceProvider.GetOverridableService<TDependency{cur}>(req, name);
"""));

    /// <summary>Appends parameters to this instance.</summary>
    /// <param name="builder">The instance to which to append parameters.</param>
    /// <param name="count">The number of parameters.</param>
    /// <returns>This instance.</returns>
    public static StringBuilder AppendParameters(this StringBuilder builder, int count) =>
        AppendAggregate(builder, count, static (acc, cur) => acc.Append(InvariantCulture, $", TDependency{cur} dependency{cur}"));

    /// <summary>Appends types to this instance.</summary>
    /// <param name="builder">The instance to which to append types.</param>
    /// <param name="count">The number of types.</param>
    /// <returns>This instance.</returns>
    public static StringBuilder AppendTypes(this StringBuilder builder, int count) =>
        AppendAggregate(builder, count, static (acc, cur) => acc.Append(InvariantCulture, $", TDependency{cur}"));

    /// <summary>Appends type constraints to this instance.</summary>
    /// <param name="builder">The instance to which to append type constraints.</param>
    /// <param name="count">The number of type constraints.</param>
    /// <returns>This instance.</returns>
    public static StringBuilder AppendTypeConstraints(this StringBuilder builder, int count) =>
        AppendAggregate(builder, count, static (acc, cur) => acc.AppendLine().Append(InvariantCulture, $"        where TDependency{cur} : notnull"));

    /// <summary>Appends type documentation to this instance.</summary>
    /// <param name="builder">The instance to which to append type documentation.</param>
    /// <param name="count">The number of type constraints.</param>
    /// <param name="ordinal">A mapping of numbers to ordinal numbers.</param>
    /// <returns>This instance.</returns>
    public static StringBuilder AppendTypeDocumentation(this StringBuilder builder, int count, IReadOnlyDictionary<int, string> ordinal) =>
        AppendAggregate(builder, count, (acc, cur) => acc.AppendLine(InvariantCulture, $"""    /// <typeparam name="TDependency{cur}">The type of the {ordinal[cur]} dependency to resolve in order to handle the invocation.</typeparam>"""));

    /// <summary>Appends the value if the provided condition is <see langword="true"/>.</summary>
    /// <param name="builder">The instance to which to append a value.</param>
    /// <param name="condition">The condition.</param>
    /// <param name="value">The value.</param>
    /// <returns>This instance.</returns>
    public static StringBuilder AppendIf(this StringBuilder builder, bool condition, string value) => condition
        ? builder.Append(value) : builder;

    /// <summary>Appends the value followed by a newline if the provided condition is <see langword="true"/>.</summary>
    /// <param name="builder">The instance to which to append a value.</param>
    /// <param name="condition">The condition.</param>
    /// <param name="value">The value.</param>
    /// <returns>This instance.</returns>
    public static StringBuilder AppendLineIf(this StringBuilder builder, bool condition, string value) => condition
        ? builder.AppendLine(value) : builder;

    /// <summary>Appends values depending on the provided overloadable.</summary>
    /// <param name="builder">The instance to which to append a value.</param>
    /// <param name="overload">The value from which to determine which values to append.</param>
    /// <param name="hasUnifiedContext">The value if the overloadable has a unified context.</param>
    /// <param name="hasSplitContext">The value if the overloadable does not have a unified context.</param>
    /// <param name="hasSplitContextAndResult">The value if the overloadable does not have a unified context and it has a result.</param>
    /// <returns>This instance.</returns>
    public static StringBuilder AppendByOverload(
        this StringBuilder builder,
        Overload overload,
        string hasUnifiedContext,
        string hasSplitContext,
        string hasSplitContextAndResult) => (overload.HasResult, overload.HasUnifiedContext) switch
        {
            (_, true) => builder.Append(hasUnifiedContext),
            (false, false) => builder.Append(hasSplitContext),
            (true, false) => builder.Append(hasSplitContext).Append(hasSplitContextAndResult),
        };

    /// <summary>Appends values depending on the provided overloadable followed by a newline.</summary>
    /// <param name="builder">The instance to which to append a value.</param>
    /// <param name="overload">The value from which to determine which values to append.</param>
    /// <param name="hasUnifiedContext">The value if the overloadable has a unified context.</param>
    /// <param name="hasSplitContext">The value if the overloadable does not have a unified context.</param>
    /// <param name="hasSplitContextAndResult">The value if the overloadable does not have a unified context and it has a result.</param>
    /// <returns>This instance.</returns>
    public static StringBuilder AppendLineByOverload(
        this StringBuilder builder,
        Overload overload,
        string hasUnifiedContext,
        string hasSplitContext,
        string hasSplitContextAndResult) => (overload.HasResult, overload.HasUnifiedContext) switch
        {
            (_, true) => builder.AppendLine(hasUnifiedContext),
            (false, false) => builder.AppendLine(hasSplitContext),
            (true, false) => builder.AppendLine(hasSplitContext).AppendLine(hasSplitContextAndResult),
        };

    static StringBuilder AppendAggregate(StringBuilder builder, int count, Func<StringBuilder, int, StringBuilder> aggregator) =>
        Enumerable.Range(1, count).Aggregate(builder, aggregator);
}
