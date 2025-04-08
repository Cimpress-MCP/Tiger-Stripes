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

namespace Tiger.Stripes.Generator;

/// <summary>Extensions to the functionality of the <see cref="StringBuilder"/> class.</summary>
[SuppressMessage("StyleCop.Readability", "SA1118", Justification = "Analyzer doesn't understand raw string literals.")]
static class Extensions
{
    static readonly AssemblyName s_assemblyName = typeof(Extensions).Assembly.GetName();

    /// <summary>Appends dependency arguments to this instance.</summary>
    /// <param name="builder">The string builder to append to.</param>
    /// <param name="count">The number of dependencies.</param>
    /// <returns>This instance.</returns>
    public static StringBuilder AppendDependencyArguments(this StringBuilder builder, int count) => builder
        .AppendAggregate(count, static (acc, cur) => acc.Append($", dependency{cur}"));

    /// <summary>Appends declarations to this instance.</summary>
    /// <param name="builder">The string builder to append to.</param>
    /// <param name="overload">The state of the current overload.</param>
    /// <returns>This instance.</returns>
    public static StringBuilder AppendDependencyDeclarations(this StringBuilder builder, Overload overload) => builder
        .AppendAggregate(
            overload.Count,
            static (acc, cur) => acc.AppendLine($"""
                var dependency{cur} = dependencyResolutionScope.ServiceProvider.GetSpecializedService<TDependency{cur}>(extendedContext);
"""));

    /// <summary>Appends the generated code attribute to this instance.</summary>
    /// <param name="builder">The string builder to append to.</param>
    /// <returns>This instance.</returns>
    public static StringBuilder AppendGeneratedCodeAttribute(this StringBuilder builder) => builder.AppendLine(
        $"""    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("{s_assemblyName.Name}", "{s_assemblyName.Version}")]""");

    /// <summary>Appends handler documentation to this instance.</summary>
    /// <param name="builder">The string builder to append to.</param>
    /// <returns>This instance.</returns>
    public static StringBuilder AppendHandlerDocumentation(this StringBuilder builder) => builder
        .AppendLine("""    /// <param name="handler">The handler for the Lambda Function invocation.</param>""");

    /// <summary>Appends serialization parameters' documentation to this instance.</summary>
    /// <param name="builder">The string builder to append to.</param>
    /// <param name="overload">The value from which to determine which values to append.</param>
    /// <returns>This instance.</returns>
    public static StringBuilder AppendSerializationParametersDocumentation(this StringBuilder builder, Overload overload) => builder
        .AppendLineByOverload(
            overload,
            """    /// <param name="serializerContext">A JSON serializer context.</param>""",
            """    /// <param name="inputTypeInfo">A provider of JSON type information for <typeparamref name="TInput"/>.</param>""",
            """    /// <param name="outputTypeInfo">A provider of JSON type information for <typeparamref name="TOutput"/>.</param>""");

    /// <summary>Appends trailing parameters to this instance.</summary>
    /// <param name="builder">The string builder to append to.</param>
    /// <param name="overload">The value from which to determine which values to append.</param>
    /// <returns>This instance.</returns>
    public static StringBuilder AppendTrailingParameters(this StringBuilder builder, Overload overload) => overload.GenerateOutput(builder
        .Append("        global::System.")
        .Append(overload.HandlerType)
        .Append("<TInput")
        .AppendHandlerTypeParameters(overload.Count)
        .AppendIf(overload.IsCancellable, ", global::System.Threading.CancellationToken"))
        .Append("""
> handler,
        
""")
        .AppendSerializationParameterDeclarations(overload);

    /// <summary>Appends dependency type constraints to this instance.</summary>
    /// <param name="builder">The string builder to append to.</param>
    /// <param name="count">The number of dependency types.</param>
    /// <returns>This instance.</returns>
    public static StringBuilder AppendDependencyTypeConstraints(this StringBuilder builder, int count) => builder
        .AppendAggregate(count, static (acc, cur) => acc.AppendLine($"        where TDependency{cur} : notnull"));

    /// <summary>Appends type documentation to this instance.</summary>
    /// <param name="builder">The string builder to append to.</param>
    /// <param name="overload">The value from which to determine which values to append.</param>
    /// <param name="ordinal">A mapping of numbers to ordinal numbers.</param>
    /// <returns>This instance.</returns>
    public static StringBuilder AppendTypeDocumentation(this StringBuilder builder, Overload overload, IReadOnlyDictionary<int, string> ordinal) => builder
        .AppendLine("""    /// <typeparam name="TInput">The type of the input to the Lambda Function handler.</typeparam>""")
        .AppendAggregate(
            overload.Count,
            (acc, cur) => acc.AppendLine($"""
    /// <typeparam name="TDependency{cur}">The type of the {ordinal[cur]} dependency to resolve in order to handle the invocation.</typeparam>
"""))
        .AppendLineIf(
            overload.HasResult,
            """    /// <typeparam name="TOutput">The type of the output to the Lambda Function handler.</typeparam>""");

    /// <summary>Appends type parameters to this instance.</summary>
    /// <param name="builder">The string builder to append to.</param>
    /// <param name="overload">The value from which to determine which values to append.</param>
    /// <returns>This instance.</returns>
    public static StringBuilder AppendTypeParameters(this StringBuilder builder, Overload overload) => builder
        .Append("<TInput")
        .AppendHandlerTypeParameters(overload.Count)
        .AppendIf(overload.HasResult, ", TOutput")
        .Append('>');

    /// <summary>Appends the value if the provided condition is <see langword="true"/>.</summary>
    /// <param name="builder">The string builder to append to.</param>
    /// <param name="condition">The condition.</param>
    /// <param name="value">The value.</param>
    /// <returns>This instance.</returns>
    public static StringBuilder AppendIf(this StringBuilder builder, bool condition, string value) => condition
        ? builder.Append(value) : builder;

    /// <summary>Appends the value followed by a newline if the provided condition is <see langword="true"/>.</summary>
    /// <param name="builder">The string builder to append to.</param>
    /// <param name="condition">The condition.</param>
    /// <param name="value">The value.</param>
    /// <returns>This instance.</returns>
    public static StringBuilder AppendLineIf(this StringBuilder builder, bool condition, string value) => condition
        ? builder.AppendLine(value) : builder;

    /// <summary>Appends values depending on the provided overloadable.</summary>
    /// <param name="builder">The string builder to append to.</param>
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
        string hasSplitContextAndResult) => overload switch
        {
            { HasUnifiedContext: true } => builder.Append(hasUnifiedContext),
            { HasResult: false } => builder.Append(hasSplitContext),
            _ => builder.Append(hasSplitContext).Append(hasSplitContextAndResult),
        };

    static StringBuilder AppendAggregate(this StringBuilder builder, int count, Func<StringBuilder, int, StringBuilder> aggregator) =>
        Enumerable.Range(1, count).Aggregate(builder, aggregator);

    static StringBuilder AppendHandlerTypeParameters(this StringBuilder builder, int count) => builder
        .AppendAggregate(count, static (acc, cur) => acc.Append($", TDependency{cur}"));

    static StringBuilder AppendSerializationParameterDeclarations(this StringBuilder builder, Overload overload) => builder
        .AppendByOverload(
            overload,
            "global::System.Text.Json.Serialization.JsonSerializerContext serializerContext",
            "global::System.Text.Json.Serialization.Metadata.JsonTypeInfo<TInput> inputTypeInfo",
            """
,
        global::System.Text.Json.Serialization.Metadata.JsonTypeInfo<TOutput> outputTypeInfo
""");

    static StringBuilder AppendLineByOverload(
        this StringBuilder builder,
        Overload overload,
        string hasUnifiedContext,
        string hasSplitContext,
        string hasSplitContextAndResult) => overload switch
        {
            { HasUnifiedContext: true } => builder.AppendLine(hasUnifiedContext),
            { HasResult: false } => builder.AppendLine(hasSplitContext),
            _ => builder.AppendLine(hasSplitContext).AppendLine(hasSplitContextAndResult),
        };
}
