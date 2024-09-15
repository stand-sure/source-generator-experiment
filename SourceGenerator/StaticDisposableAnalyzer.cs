namespace SourceGenerator;

using System.Collections.Immutable;

using JetBrains.Annotations;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
[UsedImplicitly]
public class StaticDisposableAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor Rule = new(
        "DEMO002",
        "IDisposable assigned to static member",
        "{0}, which implements IDisposable, is assigned to a static member",
        "Reliability",
        DiagnosticSeverity.Warning,
        true,
        "IDisposable references are not garbage-collected when assign to static members."
    );

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(StaticDisposableAnalyzer.Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.ReportDiagnostics);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(FieldDeclarationSyntaxNodeAction, SyntaxKind.FieldDeclaration);

        context.RegisterSyntaxNodeAction(PropertyDeclarationSyntaxNodeAction, SyntaxKind.PropertyDeclaration);
    }

    private static void FieldDeclarationSyntaxNodeAction(SyntaxNodeAnalysisContext analysisContext)
    {
        var node = (FieldDeclarationSyntax)analysisContext.Node;

        if (!node.Modifiers.Any(SyntaxKind.StaticKeyword))
        {
            return;
        }

        var typeSymbol = analysisContext.SemanticModel.GetSymbolInfo(node.Declaration.Type).Symbol as ITypeSymbol;

        IsDisposable(typeSymbol).Match(Report);

        // ReSharper disable once SeparateLocalFunctionsWithJumpStatement
        void Report()
        {
            Location location = node.GetLocation();
            string name = node.Declaration.Variables.First().Identifier.Text;
            ReportDiagnostic(analysisContext, location, name);
        }
    }

    private static bool IsDisposable(ITypeSymbol? typeSymbol)
    {
        return typeSymbol?.Interfaces.Any(symbol => symbol.Name == nameof(IDisposable)) == true;
    }

    private static void PropertyDeclarationSyntaxNodeAction(SyntaxNodeAnalysisContext analysisContext)
    {
        var node = (PropertyDeclarationSyntax)analysisContext.Node;

        if (!node.Modifiers.Any(SyntaxKind.StaticKeyword))
        {
            return;
        }

        var typeSymbol = analysisContext.SemanticModel.GetSymbolInfo(node.Type).Symbol as ITypeSymbol;

        IsDisposable(typeSymbol).Match(Report);

        // ReSharper disable once SeparateLocalFunctionsWithJumpStatement
        void Report()
        {
            Location location = node.GetLocation();
            string name = node.Identifier.Text;
            ReportDiagnostic(analysisContext, location, name);
        }
    }

    private static void ReportDiagnostic(SyntaxNodeAnalysisContext analysisContext, Location location, string name)
    {
        var diagnostic = Diagnostic.Create(
            StaticDisposableAnalyzer.Rule,
            location,
            name);

        analysisContext.ReportDiagnostic(diagnostic);
    }
}