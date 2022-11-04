Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.VisualBasic.Syntax

<Generator(LanguageNames.VisualBasic)>
Public Class MyWpfExtensionGenerator
    Implements IIncrementalGenerator

    Public Sub Initialize(context As IncrementalGeneratorInitializationContext) Implements IIncrementalGenerator.Initialize
        ' 参考资料：https://blog.lindexi.com/post/%E5%B0%9D%E8%AF%95-IIncrementalGenerator-%E8%BF%9B%E8%A1%8C%E5%A2%9E%E9%87%8F-Source-Generator-%E7%94%9F%E6%88%90%E4%BB%A3%E7%A0%81.html

        '  1. 告诉框架层需要关注哪些文件的变更
        '  在有对应的文件的变更情况下，才会触发后续步骤。如此就是增量代码生成的关键
        Dim compilationProvider = context.CompilationProvider

        '  2. 告诉框架层从变更的文件里面感兴趣什么数据，对数据预先进行处理
        '  预先处理过程中，是会不断进行丢掉处理的
        '  其中第一步和第二步可以合在一起
        Dim allClasses = compilationProvider.Select(
        Function(compilation, token)
            ' 这里有取消的功能，所以最好一次就把值算出来。不然会到 SourceOutput 里面触发这个 Select 步骤的取消。
            Return New With {
                .classes =
                Iterator Function()
                    For Each tree In compilation.SyntaxTrees
                        token.ThrowIfCancellationRequested()

                        Dim rootNode = tree.GetRoot(token)
                        Dim nodesStepInWhenNamespace = rootNode.DescendantNodesAndSelf(
                            Function(node) TypeOf node Is NamespaceBlockSyntax)
                        For Each node In nodesStepInWhenNamespace.OfType(Of ClassBlockSyntax)
                            Yield node.ClassStatement
                        Next
                    Next
                End Function().ToArray,
                compilation
            }
        End Function)

        '  3. 使用给出的数据进行处理源代码生成逻辑
        '  这一步的逻辑和普通的 Source Generator 是相同的，只是输入的参数不同
        context.RegisterSourceOutput(allClasses,
        Sub(sourceProductionContext, sourceInfo)
            Dim compilation = sourceInfo.compilation

            Dim code = GenerateCode(sourceInfo.classes, compilation, sourceProductionContext.CancellationToken)
            If code Is Nothing Then Return

            sourceProductionContext.AddSource(SourceHintName, code.ToString)
        End Sub)
    End Sub
End Class
