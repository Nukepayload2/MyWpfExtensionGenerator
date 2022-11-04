Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.VisualBasic.Syntax

<Generator(LanguageNames.VisualBasic)>
Public Class MyWpfExtensionGenerator
    Implements IIncrementalGenerator

    Public Sub Initialize(context As IncrementalGeneratorInitializationContext) Implements IIncrementalGenerator.Initialize
        ' �ο����ϣ�https://blog.lindexi.com/post/%E5%B0%9D%E8%AF%95-IIncrementalGenerator-%E8%BF%9B%E8%A1%8C%E5%A2%9E%E9%87%8F-Source-Generator-%E7%94%9F%E6%88%90%E4%BB%A3%E7%A0%81.html

        '  1. ���߿�ܲ���Ҫ��ע��Щ�ļ��ı��
        '  ���ж�Ӧ���ļ��ı������£��Żᴥ���������衣��˾��������������ɵĹؼ�
        Dim compilationProvider = context.CompilationProvider

        '  2. ���߿�ܲ�ӱ�����ļ��������Ȥʲô���ݣ�������Ԥ�Ƚ��д���
        '  Ԥ�ȴ�������У��ǻ᲻�Ͻ��ж��������
        '  ���е�һ���͵ڶ������Ժ���һ��
        Dim allClasses = compilationProvider.Select(
        Function(compilation, token)
            ' ������ȡ���Ĺ��ܣ��������һ�ξͰ�ֵ���������Ȼ�ᵽ SourceOutput ���津����� Select �����ȡ����
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

        '  3. ʹ�ø��������ݽ��д���Դ���������߼�
        '  ��һ�����߼�����ͨ�� Source Generator ����ͬ�ģ�ֻ������Ĳ�����ͬ
        context.RegisterSourceOutput(allClasses,
        Sub(sourceProductionContext, sourceInfo)
            Dim compilation = sourceInfo.compilation

            Dim code = GenerateCode(sourceInfo.classes, compilation, sourceProductionContext.CancellationToken)
            If code Is Nothing Then Return

            sourceProductionContext.AddSource(SourceHintName, code.ToString)
        End Sub)
    End Sub
End Class
