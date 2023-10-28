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

        // context.RegisterCompilationStartAction(this.CompilationStartAction);

        context.RegisterSyntaxNodeAction(AssignmentSyntaxNodeAction, SyntaxKind.SimpleAssignmentExpression);

        context.RegisterSyntaxNodeAction(FieldDeclarationSyntaxNodeAction, SyntaxKind.FieldDeclaration);
    }

    // private void CompilationStartAction(CompilationStartAnalysisContext analysisContext)
    // {
    //     analysisContext.RegisterSyntaxNodeAction(this.SyntaxNodeAction, SyntaxKind.SimpleAssignmentExpression);
    // }

    private static void ReportDiagnostic(SyntaxNodeAnalysisContext analysisContext, Location location, string name)
    {
        var diagnostic = Diagnostic.Create(
            StaticDisposableAnalyzer.Rule,
            location,
            name);

        analysisContext.ReportDiagnostic(diagnostic);
    }

    private static void AssignmentSyntaxNodeAction(SyntaxNodeAnalysisContext analysisContext)
    {
        var node = (AssignmentExpressionSyntax)analysisContext.Node;
        Console.WriteLine(node);

        ISymbol? left = analysisContext.SemanticModel.GetSymbolInfo(node.Left, analysisContext.CancellationToken).Symbol;

        if (left?.IsStatic == true)
        {
            //ReportDiagnostic(analysisContext, left);
        }
    }

    private static void FieldDeclarationSyntaxNodeAction(SyntaxNodeAnalysisContext analysisContext)
    {
        var node = (FieldDeclarationSyntax)analysisContext.Node;

        Location location = node.GetLocation();
        string name = node.Declaration.Variables.First().Identifier.Text;

        node.Modifiers.Any(SyntaxKind.StaticKeyword).Match(Report);

        // ReSharper disable once SeparateLocalFunctionsWithJumpStatement
        // void CheckReadOnly()
        // {
        //     node.Modifiers.Any(SyntaxKind.ReadOnlyKeyword).Match(Ignore, Report);
        // }

        void Report()
        {
            ReportDiagnostic(analysisContext, location, name);
        }
    }
}