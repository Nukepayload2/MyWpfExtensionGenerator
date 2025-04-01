Option Explicit On
Option Infer On
Option Strict On

Imports System.Text
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.Text
Imports Microsoft.CodeAnalysis.VisualBasic.Syntax

<Generator(LanguageNames.VisualBasic)>
Public Class MyWpfExtensionGenerator
    Implements IIncrementalGenerator

    Public Sub Initialize(context As IncrementalGeneratorInitializationContext) Implements IIncrementalGenerator.Initialize
        ' Create a provider filtering ClassStatementSyntax nodes.
        Dim classesProvider = context.SyntaxProvider.CreateSyntaxProvider(
            Function(syntaxNode, cancellationToken) As Boolean
                Return TypeOf syntaxNode Is ClassStatementSyntax
            End Function,
            Function(sp, cancellationToken) As INamedTypeSymbol
                Dim classSyntax = DirectCast(sp.Node, ClassStatementSyntax)
                Return TryCast(sp.SemanticModel.GetDeclaredSymbol(classSyntax, cancellationToken), INamedTypeSymbol)
            End Function)

        Dim compilationAndClasses = context.CompilationProvider.Combine(classesProvider.Collect())

        context.RegisterSourceOutput(compilationAndClasses,
            Sub(sourceProductionContext, source)
                Dim compilation = source.Left
                Dim classes = source.Right.Where(Function(s) s IsNot Nothing).ToList()

                Dim windowTypeSymbol = compilation.GetTypeByMetadataName("System.Windows.Window")
                If windowTypeSymbol Is Nothing Then Return

                ' Filter candidate classes to those deriving from System.Windows.Window and having an accessible parameterless constructor.
                Dim wpfWindows = GetWpfWindowClasses(classes, windowTypeSymbol)

                ' Check for Microsoft.VisualBasic.Forms reference.
                Dim hasVbWindowsForms = compilation.ReferencedAssemblyNames.Any(Function(an) an.Name = "Microsoft.VisualBasic.Forms")

                Dim code As New StringBuilder
                MyServiceCodeWriter.GenerateWpfExtensionCode(wpfWindows, hasVbWindowsForms, code)
                sourceProductionContext.AddSource("MyWpfExtension", SourceText.From(code.ToString(), Encoding.UTF8))
            End Sub)
    End Sub

    ''' <summary>
    ''' Finds all WPF window classes.
    ''' </summary>
    ''' <remarks>
    ''' Classification of WPF window classes:<br/>
    ''' 1. Inherits System.Windows.Window directly or indirectly.<br/>
    ''' 2. Has a parameterless Public Sub New or no Sub New was defined.<br/>
    ''' 3. Partial classes are usually used. We should distinct classes by full name.
    ''' </remarks>
    Private Shared Function GetWpfWindowClasses(classes As List(Of INamedTypeSymbol), windowTypeSymbol As INamedTypeSymbol) As IEnumerable(Of INamedTypeSymbol)
        Return From cls In classes Where cls.DerivesFromOrImplementsAnyConstructionOf(windowTypeSymbol)
               Let subNews = cls.InstanceConstructors
               Where subNews.Length = 0 OrElse subNews.Any(
                   Function(subNew) subNew.Parameters.Length = 0 AndAlso subNew.DeclaredAccessibility = Accessibility.Public)
               Let fullName = cls.ContainingNamespace?.ToDisplayString() & "." & cls.Name
               Group cls By fullName Into Group
               Select Group.First()
    End Function

End Class
