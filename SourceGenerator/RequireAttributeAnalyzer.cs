namespace SourceGenerator;

using System.Collections.Immutable;

using JetBrains.Annotations;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
[UsedImplicitly]
public class RequireAttributeAnalyzer : DiagnosticAnalyzer
{
    private const string AttributeName = "MyAttribute";
    private const string Description = "Require attribute.";
    private const string InterfaceName = "IMyInterface";
    private const string MessageFormat = "{0} is missing {2}. Methods in types implementing {1} should be decorated with {2}.";
    private const string ProblemCategory = "Conventions";
    private const string ProblemId = "DEMO001";
    private const string Title = "My Demo Analyzer";

    private static readonly DiagnosticDescriptor Rule = new(
        RequireAttributeAnalyzer.ProblemId,
        RequireAttributeAnalyzer.Title,
        RequireAttributeAnalyzer.MessageFormat,
        RequireAttributeAnalyzer.ProblemCategory,
        DiagnosticSeverity.Warning,
        true,
        RequireAttributeAnalyzer.Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(RequireAttributeAnalyzer.Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.ReportDiagnostics);
        context.EnableConcurrentExecution();

        context.RegisterCompilationStartAction(CompilationStartAction);
    }

    private static void CompilationStartAction(CompilationStartAnalysisContext analysisContext)
    {
        analysisContext.RegisterSymbolAction(SymbolAction, SymbolKind.NamedType);
    }

    private static void SymbolAction(SymbolAnalysisContext analysisContext)
    {
        var typeSymbol = analysisContext.Symbol as INamedTypeSymbol;

        bool shouldBeAnalyzed = typeSymbol?.Interfaces.Any(symbol => symbol.Name.EndsWith(RequireAttributeAnalyzer.InterfaceName)) == true;

        shouldBeAnalyzed.Match(() => { }, () => CheckAttribute(analysisContext, typeSymbol));
    }

    private static void CheckAttribute(SymbolAnalysisContext analysisContext, INamedTypeSymbol typeSymbol)
    {
        bool hasAttribute = typeSymbol.GetAttributes().Any(data => data.AttributeClass?.Name == RequireAttributeAnalyzer.AttributeName) == true;

        hasAttribute.Match(() => { }, () => ReportDiagnostic(analysisContext, typeSymbol));
    }

    private static void ReportDiagnostic(SymbolAnalysisContext analysisContext, INamedTypeSymbol typeSymbol)
    {
        Location location = typeSymbol!.Locations.First();

        var diagnostic = Diagnostic.Create(RequireAttributeAnalyzer.Rule,
            location,
            typeSymbol.Name,
            RequireAttributeAnalyzer.InterfaceName,
            RequireAttributeAnalyzer.AttributeName);

        analysisContext.ReportDiagnostic(diagnostic);
    }
}

internal static class FunctionalHelpers
{
    internal static void Match(this bool match, Action onTrue, Action onFalse)
    {
        Action act = match ? onTrue : onFalse;
        act();
    }
}