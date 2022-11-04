Option Explicit On
Option Infer On
Option Strict On

Imports System.Text
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.Text
Imports Microsoft.CodeAnalysis.VisualBasic.Syntax

<Generator(LanguageNames.VisualBasic)>
Public Class MyWpfExtensionGenerator
    Implements ISourceGenerator

    Public Sub Initialize(context As GeneratorInitializationContext) Implements ISourceGenerator.Initialize
        ' Register syntax receiver
        context.RegisterForSyntaxNotifications(
            Function() New AllClassesSyntaxReceiver)
    End Sub

    Private Class AllClassesSyntaxReceiver
        Implements ISyntaxReceiver
        Public ReadOnly Property AllClasses As New List(Of ClassStatementSyntax)

        Public Sub OnVisitSyntaxNode(syntaxNode As SyntaxNode) Implements ISyntaxReceiver.OnVisitSyntaxNode
            Dim node = TryCast(syntaxNode, ClassStatementSyntax)
            If node IsNot Nothing Then
                ' Add all classes
                AllClasses.Add(node)
            End If
        End Sub
    End Class

    Public Sub Execute(context As GeneratorExecutionContext) Implements ISourceGenerator.Execute
        ' Get syntax receiver
        Dim receiver = TryCast(context.SyntaxReceiver, AllClassesSyntaxReceiver)
        If receiver Is Nothing Then Return
        Dim allClasses = receiver.AllClasses

        ' Get all classes and select WPF window classes.
        Dim compilation = context.Compilation
        Dim code = GenerateCode(allClasses, compilation, context.CancellationToken)
        If code Is Nothing Then Return

        ' Commit code
        context.AddSource(SourceHintName,
                          SourceText.From(code.ToString, Encoding.UTF8))
    End Sub

End Class
