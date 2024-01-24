// <copyright file="OverloadsGenerator.cs" company="Cimpress, Inc.">
// Copyright 2023 Cimpress, Inc.
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

/// <summary>Generates overloads for invocation mapping methods.</summary>
[Generator]
public sealed partial class OverloadsGenerator
    : IIncrementalGenerator
{
    /* note(cosborn)
     * Because this is for internal use on well-known targets, it's not
     * gonna be _particularly robust_ in the face of unexpected inputs.
     */

    static readonly Encoding s_encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

    /// <inheritdoc/>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var overloadableMethodDeclarations = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                Parser.GenerateOverloadsAttribute,
                (n, _) => n is CompilationUnitSyntax,
                (gasc, _) => (CompilationUnitSyntax)gasc.TargetNode);
        var compilationAndMethods = context.CompilationProvider.Combine(overloadableMethodDeclarations.Collect());
        context.RegisterSourceOutput(compilationAndMethods, static (spc, s) => Execute(s.Right, spc));
    }

    static void Execute(ImmutableArray<CompilationUnitSyntax> assemblies, SourceProductionContext context)
    {
        if (assemblies.IsDefaultOrEmpty)
        {
            // note(cosborn) Nothing to generate. We're done here.
            return;
        }

        var e = new Emitter();
        var result = e.Emit(context.CancellationToken);
        context.AddSource("InvocationBuilderExtensions.g.cs", SourceText.From(result, s_encoding));
    }
}
