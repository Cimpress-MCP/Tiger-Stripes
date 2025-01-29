// <copyright file="Emitter.cs" company="Cimpress plc">
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

/// <summary>Emits overloads for methods.</summary>
abstract class Emitter(StringBuilder builder)
{
    static readonly FrozenSet<bool> s_options = new[] { true, false }.ToFrozenSet();

    /// <summary>Gets the name of the class to emit.</summary>
    public abstract string ClassName { get; }

    /// <summary>Gets the name of the method to emit.</summary>
    public abstract string MethodName { get; }

    /// <summary>Gets the summary of the class to emit.</summary>
    public abstract string Summary { get; }

    /// <summary>Gets a mapping of counts to their ordinal names.</summary>
    protected static FrozenDictionary<int, string> OrdinalScale { get; } = new Dictionary<int, string>
    {
        [0] = "nilth", // note(cosborn) Unused.
        [1] = "first",
        [2] = "second",
        [3] = "third",
        [4] = "fourth",
        [5] = "fifth",
        [6] = "sixth",
        [7] = "seventh",
        [8] = "eighth",
    }.ToFrozenDictionary();

    /// <summary>Gets the builder into which to emit the generated code.</summary>
    protected StringBuilder Builder { get; } = builder;

    /// <summary>Emits the generated code.</summary>
    /// <param name="cancellationToken">A token to monitor for operation cancellation.</param>
    /// <returns>The generated code.</returns>
    public string Emit(CancellationToken cancellationToken)
    {
        _ = Builder.AppendLine($$"""
// <auto-generated/>

namespace Tiger.Stripes;

/// <summary>{{Summary}}</summary>
public static partial class {{ClassName}}
{
""");
        var overloadables =
            from hasResult in s_options
            from hasUnifiedContext in s_options
            from count in OrdinalScale.Keys
            select (HasResult: hasResult, HasUnifiedContext: hasUnifiedContext, Count: count);

        foreach (var (hasResult, hasUnifiedContext, count) in overloadables)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _ = GenerateOverload(new Overload.Async(hasResult, hasUnifiedContext, count));
            _ = GenerateOverload(new Overload.Sync(hasResult, hasUnifiedContext, count));

            // note(cosborn) There is only `IAsyncEnumerable<T>`, not `IAsyncEnumerable`.
            if (hasResult)
            {
                _ = GenerateOverload(new Overload.AsyncEnumerable(hasResult, hasUnifiedContext, count));
            }
        }

        return Builder.AppendLine("}").ToString();
    }

    /// <summary>Generates an overload based on the provided configuration.</summary>
    /// <param name="overload">The configuration for the overload to generate.</param>
    /// <returns>The builder into which the overload was emitted.</returns>
    protected abstract StringBuilder GenerateOverload(Overload overload);
}
