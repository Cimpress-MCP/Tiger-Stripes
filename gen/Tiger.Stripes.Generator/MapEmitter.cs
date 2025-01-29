// <copyright file="MapEmitter.cs" company="Cimpress plc">
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

namespace Tiger.Stripes.Generator;

/// <summary>Emits overloads for invocation mapping methods.</summary>
/// <param name="builder">The builder into which to emit the generated code.</param>
[SuppressMessage("StyleCop.Readability", "SA1118", Justification = "Analyzer doesn't understand raw string literals.")]
sealed class MapEmitter(StringBuilder builder)
    : Emitter(builder)
{
    /// <inheritdoc/>
    public override string ClassName { get; } = "InvocationBuilderExtensions";

    /// <inheritdoc/>
    public override string MethodName { get; } = "MapInvoke";

    /// <inheritdoc/>
    public override string Summary { get; } = """Extensions to the functionality of the <see cref="global::Tiger.Stripes.IInvocationBuilder"/> interface.""";

    /// <inheritdoc/>
    protected override StringBuilder GenerateOverload(Overload overload)
    {
        _ = Builder
            .AppendLine("""
    /// <summary>Maps a Lambda Function invocation to the specified parameters.</summary>
""")
            .AppendTypeDocumentation(overload, OrdinalScale)
            .AppendLine("""
    /// <param name="builder">The invocation builder.</param>
    /// <param name="name">The name to which to map the provided handler.</param>
""")
            .AppendHandlerDocumentation()
            .AppendSerializationParametersDocumentation(overload)
            .AppendLine("""
    /// <returns>The invocation builder for further customization.</returns>
""")
            .AppendGeneratedCodeAttribute()
            .Append($"    public static global::Tiger.Stripes.IInvocationBuilder {MethodName}")
            .AppendTypeParameters(overload)
            .AppendLine("""
(
        this global::Tiger.Stripes.IInvocationBuilder builder,
        string name,
""")
            .AppendTrailingParameters(overload)
            .AppendLine(")")
            .AppendTrailingConstraints(overload);

        return overload.GenerateSerializationCleanup(overload.GenerateSerialization(overload.GenerateSerializationSetup(Builder
            .AppendLine("""
    {
        var cts = new global::System.Threading.CancellationTokenSource();
        var notifier = builder.Services.GetRequiredService<global::Tiger.Stripes.NearlyOutOfTimeNotifier>();

        var isColdStart = true;
""")
            .AppendLine()
            .AppendLine("""
        return builder.MapInvoke(name, async req =>
        {
""")
            .AppendLine("""
            var invocationTagList = InvocationTags(req.LambdaContext, name, isColdStart);
            using var invocationActivity = s_lambdaActivitySource.StartActivity(global::System.Diagnostics.ActivityKind.Server, name: req.LambdaContext.FunctionName, tags: invocationTagList);
            using var handlingLogScope = Handling(builder.Logger, req.LambdaContext, name);
"""))

            // note(cosborn) Slight performance optimization for no dependencies case.
            .AppendLineIf(overload.Count is not 0, """
            await using var dependencyResolutionScope = builder.Services.CreateAsyncScope();
""")
            .AppendLine($$"""
            try
            {
                await using var warningRegistration = cts.Token.UnsafeRegister(
                    static o => ((global::Tiger.Stripes.NearlyOutOfTimeNotifier)o).Notify(),
                    notifier);
                cts.CancelAfter(req.LambdaContext.RemainingTime - builder.Environment.CancellationLeadTime);

                var input = await global::System.Text.Json.JsonSerializer.DeserializeAsync(req.InputStream, {{(overload.HasUnifiedContext ? "typeof(TInput), serializerContext" : "inputTypeInfo")}}, cts.Token);
""")
            .AppendDependencyDeclarations(overload)
            .AppendLine()
            .Append("                ")
            .AppendIf(overload.HasResult, "var output = ")
            .Append(overload.PreCall)
            .Append("handler(")
            .AppendIf(overload.HasUnifiedContext, "(TInput)")
            .Append("input!")
            .AppendDependencyArguments(overload.Count)
            .AppendIf(overload.IsCancellable, ", cts.Token")
            .AppendLine(");"))
            .AppendLine($$"""
                return new({{(overload.HasResult ? "s_outputPipe.Reader.AsStream()" : "global::System.IO.Stream.Null")}}, disposeOutputStream: true);
            }
""")
            .AppendLine("""
            catch (global::System.Exception e)
            {
                invocationActivity?.SetStatus(global::System.Diagnostics.ActivityStatusCode.Error, e.Message);
                invocationActivity?.AddException(e);
""")
            .AppendLineIf(overload.HasResult, """
                // note(cosborn) Pre-complete the reader on exception, since the Lambda RIC won't get to.
                await s_outputPipe.Reader.CompleteAsync(e);
""")
            .AppendLine("""
                throw;
            }
""")
            .AppendLine("""
            finally
            {
"""))
            .AppendLine("""
            }
        });
    }
""");
    }
}
