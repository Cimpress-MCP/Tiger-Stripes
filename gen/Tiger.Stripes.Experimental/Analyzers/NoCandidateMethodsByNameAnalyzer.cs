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

/// <summary>Analyzes incorrect invocations of MapAsync.</summary>
[DiagnosticAnalyzer(CSharp)]
public sealed class NoCandidateMethodsByNameAnalyzer
    : DiagnosticAnalyzer
{
    /// <summary>The unique identifier of the rule associated with this analyzer.</summary>
    public const string Id = "TS0001";

    static readonly DiagnosticDescriptor s_rule = new(
        id: Id,
        title: "No handler method found",
        messageFormat: "No handler method found by name on service type '{0}'",
        category: "Tiger.Stripes.Handler",
        defaultSeverity: Error,
        isEnabledByDefault: true,
        description: "Service types require a method named 'Handle' or 'HandleAsync'.");

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = [s_rule];

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        if (context is null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

        context.RegisterCompilationStartAction(static csac =>
        {
            if (csac.Compilation.GetTypeByMetadataName("Tiger.Stripes.InvocationBuilderExtensions") is not { } ct)
            {
                return;
            }

            csac.RegisterSyntaxNodeAction(snac => AnalyzeSyntaxNode(snac, ct), InvocationExpression);
        });
    }

    static void AnalyzeSyntaxNode(SyntaxNodeAnalysisContext context, INamedTypeSymbol containingType)
    {
        var ies = (InvocationExpressionSyntax)context.Node;
        if (Locate.Candidates(context, containingType, ies) is not var (serviceType, candidateMethods))
        {
            return;
        }

        // note(cosborn) This is the final check – if there are any candidate methods, we have nothing to do.
        if (candidateMethods is not [])
        {
            return;
        }

        var gns = (GenericNameSyntax)((MemberAccessExpressionSyntax)ies.Expression).Name;
        var arg = gns.TypeArgumentList.Arguments[0];
        var diagnostic = Diagnostic.Create(s_rule, arg.GetLocation(), serviceType.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat));
        context.ReportDiagnostic(diagnostic);
    }
}
