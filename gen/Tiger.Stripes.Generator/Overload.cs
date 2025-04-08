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

/// <summary>Represents the kinds of overload which can be generated.</summary>
/// <param name="HasResult">Whether the overload produces a result.</param>
/// <param name="HasUnifiedContext">Whether the overload uses a single, unified serialization context.</param>
/// <param name="Count">The number of dependencies to the overload.</param>
[SuppressMessage("StyleCop.Readability", "SA1118", Justification = "Analyzer doesn't understand raw string literals.")]
abstract record class Overload(bool HasResult, bool HasUnifiedContext, int Count)
{
    /// <summary>
    /// Gets a value indicating whether the overload can be cancelled
    /// by means of an instance of the <see cref="CancellationToken"/> struct.
    /// </summary>
    public virtual bool IsCancellable { get; } = true;

    /// <summary>Gets the type of the handler method before applying type parameters.</summary>
    /// <remarks>This is almost always "Func". Only synchronous without result is "Action".</remarks>
    public virtual string HandlerType { get; } = "Func";

    /// <summary>Gets an optional prefix to the handler call.</summary>
    /// <remarks>Like how a Task-asynchronous call is prefixed with "await".</remarks>
    public virtual string? PreCall { get; }

    /// <summary>Gets a value indicating whether the overload is <see langword="void"/>.</summary>
    /// <remarks>Asynchronous methods return a value. Only synchronous without result is truly <see langword="void"/>.</remarks>
    public virtual bool IsVoid { get; }

    /// <summary>Generates serialization setup code.</summary>
    /// <param name="builder">The string builder into which to generate serialization setup code.</param>
    /// <returns>The string builder.</returns>
    public virtual StringBuilder GenerateSerializationSetup(StringBuilder builder) => builder
        .AppendLine("""
            if (!isColdStart)
            {
                s_outputPipe.Reset();
                s_jsonWriter.Reset(s_outputPipe.Writer);
            }
""")
        .AppendLine();

    /// <summary>Generates serialization code.</summary>
    /// <param name="builder">The string builder into which to generate serialization code.</param>
    /// <returns>The string builder.</returns>
    public virtual StringBuilder GenerateSerialization(StringBuilder builder) => builder
        .AppendLineIf(HasResult, $"""
                global::System.Text.Json.JsonSerializer.Serialize(s_jsonWriter, output, {(HasUnifiedContext ? "typeof(TOutput), serializerContext" : "outputTypeInfo")});
""");

    /// <summary>Generates serialization cleanup code.</summary>
    /// <param name="builder">The string builder into which to generate serialization cleanup code.</param>
    /// <returns>The string builder.</returns>
    public StringBuilder GenerateSerializationCleanup(StringBuilder builder) => builder
        .AppendLineIf(HasResult, """
                await s_outputPipe.Writer.CompleteAsync();
""")
        .AppendLine("""
                isColdStart = false;
                if (!cts.TryReset())
                {
                    cts.Dispose();
                    cts = new();
                }
""");

    /// <summary>Generates the output type for an overload, including trivia.</summary>
    /// <param name="builder">The string builder into which to generate an output type.</param>
    /// <returns>The string builder.</returns>
    /// <remarks>Mrgrgr, I hate this, but I don't know how to do without it.</remarks>
    public StringBuilder GenerateOutput(StringBuilder builder) => GenerateOutputCore(builder.AppendIf(!IsVoid, ", "));

    /// <summary>Generates the output type for an overload.</summary>
    /// <param name="builder">The string builder into which to generate an output type.</param>
    /// <returns>The string builder.</returns>
    public abstract StringBuilder GenerateOutputCore(StringBuilder builder);

    /// <summary>Represents a sync overload.</summary>
    /// <param name="HasResult">Whether the overload produces a result.</param>
    /// <param name="HasUnifiedContext">Whether the overload uses a single, unified serialization context.</param>
    /// <param name="Count">The number of dependencies to the overload.</param>
    public sealed record class Sync(bool HasResult, bool HasUnifiedContext, int Count)
        : Overload(HasResult, HasUnifiedContext, Count)
    {
        /// <inheritdoc/>
        public override bool IsCancellable { get; }

        /// <inheritdoc/>
        public override string HandlerType => HasResult ? "Func" : "Action";

        /// <inheritdoc/>
        public override bool IsVoid => !HasResult;

        /// <inheritdoc/>
        public override StringBuilder GenerateOutputCore(StringBuilder builder) => builder.AppendIf(HasResult, "TOutput");
    }

    /// <summary>Represents an async overload.</summary>
    /// <param name="HasResult">Whether the overload produces a result.</param>
    /// <param name="HasUnifiedContext">Whether the overload uses a single, unified serialization context.</param>
    /// <param name="Count">The number of dependencies to the overload.</param>
    public sealed record class ValueAsync(bool HasResult, bool HasUnifiedContext, int Count)
        : Overload(HasResult, HasUnifiedContext, Count)
    {
        /// <inheritdoc/>
        public override string PreCall => "await ";

        /// <inheritdoc/>
        public override StringBuilder GenerateOutputCore(StringBuilder builder) => builder
            .Append("global::System.Threading.Tasks.ValueTask")
            .AppendIf(HasResult, "<TOutput>");
    }

    /// <summary>Represents an async overload.</summary>
    /// <param name="HasResult">Whether the overload produces a result.</param>
    /// <param name="HasUnifiedContext">Whether the overload uses a single, unified serialization context.</param>
    /// <param name="Count">The number of dependencies to the overload.</param>
    public sealed record class Async(bool HasResult, bool HasUnifiedContext, int Count)
        : Overload(HasResult, HasUnifiedContext, Count)
    {
        /// <inheritdoc/>
        public override string PreCall => "await ";

        /// <inheritdoc/>
        public override StringBuilder GenerateOutputCore(StringBuilder builder) => builder
            .Append("global::System.Threading.Tasks.Task")
            .AppendIf(HasResult, "<TOutput>");
    }

    /// <summary>Represents an async enumerable overload.</summary>
    /// <param name="HasResult">Whether the overload produces a result.</param>
    /// <param name="HasUnifiedContext">Whether the overload uses a single, unified serialization context.</param>
    /// <param name="Count">The number of dependencies to the overload.</param>
    public sealed record class AsyncEnumerable(bool HasResult, bool HasUnifiedContext, int Count)
        : Overload(HasResult, HasUnifiedContext, Count)
    {
        /// <summary>Generates serialization setup code.</summary>
        /// <param name="builder">The string builder into which to generate serialization setup code.</param>
        /// <returns>The string builder.</returns>
        public override StringBuilder GenerateSerializationSetup(StringBuilder builder) => builder
            .AppendLine("""
            if (!isColdStart)
            {
                s_outputPipe.Reset();
            }
""")
            .AppendLine();

        /// <inheritdoc/>
        public override StringBuilder GenerateSerialization(StringBuilder builder) => builder
            .AppendLine($"""
                await global::System.Text.Json.JsonSerializer.SerializeAsync(
                    s_outputPipe.Writer.AsStream(),
                    output,
                    {(HasUnifiedContext
                    ? """
typeof(global::System.Collections.Generic.IAsyncEnumerable<TOutput>),
                    serializerContext
"""
                    : "outputTypeInfo")},
                    cts.Token);
""");

        /// <inheritdoc/>
        public override StringBuilder GenerateOutputCore(StringBuilder builder) => builder
            .Append("global::System.Collections.Generic.IAsyncEnumerable<TOutput>");
    }
}
