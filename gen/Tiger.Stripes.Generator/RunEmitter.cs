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

/// <summary>Emits overloads for default invocation mapping methods.</summary>
/// /// <param name="Builder">The builder into which to emit the generated code.</param>
sealed record class RunEmitter(StringBuilder Builder)
    : Emitter(Builder)
{
    /// <inheritdoc/>
    public override string ClassName { get; } = "LambdaApplicationExtensions";

    /// <inheritdoc/>
    public override string MethodName { get; } = "RunAsync";

    /// <inheritdoc/>
    public override string Summary { get; } = """Extensions to the functionality of the <see cref="global::Tiger.Stripes.LambdaApplication"/> interface.""";

    /// <inheritdoc/>
    public override StringBuilder GenerateOverload(Overload overload) => Builder
        .AppendLine("""
    /// <summary>Runs a Lambda Function with the provided default mapping.</summary>
""")
        .AppendTypeDocumentation(overload, OrdinalScale)
        .AppendLine("""
    /// <param name="application">The Lambda application.</param>
""")
        .AppendHandlerDocumentation()
        .AppendSerializationParametersDocumentation(overload)
        .AppendLine("""
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A value which, when resolved, represents operation completion.</returns>
""")
        .AppendGeneratedCodeAttribute()
        .Append($"    public static global::System.Threading.Tasks.Task {MethodName}")
        .AppendTypeParameters(overload)
        .AppendLine("""
(
        this global::Tiger.Stripes.LambdaApplication application,
""")
        .AppendTrailingParameters(overload)
        .AppendLine("""
,
        global::System.Threading.CancellationToken cancellationToken = default)
""")
        .AppendDependencyTypeConstraints(overload.Count)
        .Append("""
    {
        _ = application.MapInvoke(global::Tiger.Stripes.Constants.DefaultHandlerName, handler, 
""")
        .AppendByOverload(overload, "serializerContext", "inputTypeInfo", ", outputTypeInfo")
        .AppendLine("""
);
        return application.RunAsync(cancellationToken);
    }
""");
}
