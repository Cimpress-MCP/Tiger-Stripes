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
public sealed class TooManyCandidateMethodsByNameAnalyzer
    : DiagnosticAnalyzer
{
    /// <summary>The unique identifier of the rule associated with this analyzer.</summary>
    public const string Id = "TS0003";

    static readonly DiagnosticDescriptor s_rule = new(
        id: Id,
        title: "Multiple handler methods found",
        messageFormat: "Multiple handler methods found by name on service type '{0}'",
        category: "Tiger.Stripes.Handler",
        defaultSeverity: Error,
        isEnabledByDefault: true,
        description: "Service types require a single method named 'Handle' or 'HandleAsync'.");

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

        // note(cosborn) This is the final check – if there are zero or one candidate methods, we have nothing to do.
        if (candidateMethods is not { Length: > 1 })
        {
            return;
        }

        var gns = (GenericNameSyntax)((MemberAccessExpressionSyntax)ies.Expression).Name;
        var methodDefinitions = candidateMethods
            .SelectMany(
                static cm => cm.DeclaringSyntaxReferences,
                static (_, dsr) => dsr.GetSyntax())
            .Select(static s => s.GetLocation());

        var arg = gns.TypeArgumentList.Arguments[0];
        var diagnostic = Diagnostic.Create(s_rule, arg.GetLocation(), methodDefinitions, serviceType.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat));
        context.ReportDiagnostic(diagnostic);
    }
}
