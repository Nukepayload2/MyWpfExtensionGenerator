Option Explicit On
Option Infer On
Option Strict On

Imports System.Text
Imports Microsoft.CodeAnalysis

Class MyServiceCodeWriter

    Private Shared Sub AppendNamespaceMy(code As StringBuilder)
        code.AppendLine("Namespace My")
    End Sub

    Private Shared Sub AppendModuleMyWpfExtension(code As StringBuilder)
        code.Append("
    <Global.Microsoft.VisualBasic.HideModuleName>
    Module MyWpfExtension")
    End Sub

    Private Shared Sub AppendApplicationProperty(code As StringBuilder)
        code.Append("
        Friend ReadOnly Property Application As Application
            Get
                Return CType(Global.System.Windows.Application.Current, Application)
            End Get
        End Property")
    End Sub

    Private Shared Sub AppendComputerProperty(code As StringBuilder)
        code.Append("
        Private ReadOnly s_Computer As New ThreadSafeObjectProvider(Of Global.Microsoft.VisualBasic.Devices.Computer)
        ''' <summary>
        ''' Provides properties for manipulating computer components such as audio, the clock, the keyboard, the file system, and so on.
        ''' </summary>
        Friend ReadOnly Property Computer As Global.Microsoft.VisualBasic.Devices.Computer
            Get
                Return s_Computer.GetInstance()
            End Get
        End Property")
    End Sub

    Private Shared Sub AppendUserProperty(code As StringBuilder)
        code.Append("
        Private ReadOnly s_User As New ThreadSafeObjectProvider(Of Global.Microsoft.VisualBasic.ApplicationServices.User)
        ''' <summary>
        ''' The My.User object provides access to information about the logged-on user by returning an object that implements the IPrincipal interface.
        ''' Use My.User.InitializeWithWindowsUser() to run this app with the current user.
        ''' </summary>
        Friend ReadOnly Property User As Global.Microsoft.VisualBasic.ApplicationServices.User
            Get
                Return s_User.GetInstance()
            End Get
        End Property")
    End Sub

    Private Shared Sub AppendLogProperty(code As StringBuilder)
        code.Append("
        Private ReadOnly s_Log As New ThreadSafeObjectProvider(Of Global.Microsoft.VisualBasic.Logging.Log)
        ''' <summary>
        ''' Returns application logs.
        ''' </summary>
        Friend ReadOnly Property Log As Global.Microsoft.VisualBasic.Logging.Log
            Get
                Return s_Log.GetInstance()
            End Get
        End Property")
    End Sub

    Private Shared Sub AppendWindowsProperty(code As StringBuilder)
        ' I've modified the original implementation.
        ' See comments of AppendMyWindowsClass for details.
        code.Append("
        Private ReadOnly s_Windows As New MyWindows
        ''' <summary>
        ''' Gets a collection of windows in this project.
        ''' </summary>
        Friend ReadOnly Property Windows As MyWindows
            <Global.System.Diagnostics.DebuggerHidden>
            Get
                Return s_Windows
            End Get
        End Property")
    End Sub

    Private Shared Sub AppendMyWindowsClass(code As StringBuilder,
                                            windowSymbols As IEnumerable(Of INamedTypeSymbol))
        ' I've modified the original implementation. The original design is:
        ' 1. Every threads owns a collection of windows.
        ' 2. Use `Property Get` to get or create instances.
        ' 3. Use `Property Set` to reset references.

        ' The original design has the following problems:
        ' 1. Windows are not disconnected automatically from GC root when they are closed.
        '    This will cause memory leak.
        ' 2. You can't get windows from other threads. This limitation doesn't make sense,
        '    because WPF has Dispatcher properties to handle multi-threading cases.

        ' So, I redesigned the rules of singleton window properties generation.
        ' 1. Windows can be accessed across threads.
        '    The source generator will add locks for thread-safety.
        ' 2. Use `Property Get` to get or create instances.
        ' 3. The references to windows will be removed when the window was closed.

        code.Append("
        Friend NotInheritable Class MyWindows")
        Dim usedWindowNames As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
        For Each wnd In windowSymbols
            Dim windowName = AllocateWindowName(wnd.Name, wnd.ContainingNamespace?.ToDisplayString, usedWindowNames)
            Dim windowNamespace = GetAbsoluteNamespace(wnd.ContainingNamespace)
            Dim windowFullName = If(windowNamespace = Nothing,
                windowName,
                windowNamespace & "." & windowName)

            code.Append($"
            Private _{windowName} As {windowFullName}
            Private ReadOnly _Lock{windowName} As New Object
            
            Public ReadOnly Property {windowName} As {windowFullName}
                Get
                    Dim mainWnd = Global.System.Windows.Application.Current?.MainWindow
                    If mainWnd?.GetType = GetType({windowFullName}) Then Return mainWnd

                    SyncLock _Lock{windowName}
                        If _{windowName} Is Nothing Then
                            _{windowName} = New {windowFullName}
                            AddHandler _{windowName}.Closed, AddressOf {windowName}Closed
                        End If
                    
                        Return _{windowName}
                    End SyncLock
                End Get
            End Property
            
            Private Sub {windowName}Closed(sender As Object, e As EventArgs)
                SyncLock _Lock{windowName}
                    RemoveHandler _{windowName}.Closed, AddressOf {windowName}Closed
                    _{windowName} = Nothing
                End SyncLock
            End Sub")
        Next

        code.Append("
        End Class")
    End Sub

    Private Shared Function GetAbsoluteNamespace(ns As INamespaceSymbol) As String
        If ns Is Nothing Then
            Return "Global"
        End If
        Return "Global." & ns.ToDisplayString
    End Function

    Private Shared Function AllocateWindowName(
        suggestedName As String,
        suggestedNamespace As String,
        usedName As HashSet(Of String)) As String

        Dim name = suggestedName
        If usedName.Contains(name) Then
            name = suggestedNamespace?.Replace(".", "_") & "_" & name
        End If

        If usedName.Contains(name) Then
            Dim fullName = name
            Dim num = 1
            Dim nextName As String
            Do
                num += 1
                nextName = fullName & num
            Loop While usedName.Contains(nextName)
            name = nextName
        End If

        usedName.Add(name)
        Return name
    End Function

    Private Shared Sub AppendEndModuleMyWpfExtension(code As StringBuilder)
        code.Append("
    End Module")
    End Sub

    Private Shared Sub AppendThreadSafeObjectProvider(code As StringBuilder)
        code.Append("
    <Global.Microsoft.VisualBasic.HideModuleName>
    Partial Module MyProject
        Friend Class ThreadSafeObjectProvider(Of T As New)
            <Global.System.ThreadStatic>
            Private Shared t_value As T
            Friend Function GetInstance() As T
                If t_value Is Nothing Then t_value = New T
                Return t_value
            End Function
        End Class
    End Module")
    End Sub

    Private Shared Sub AppendEndNamespaceMy(code As StringBuilder)
        code.Append("
End Namespace")
    End Sub

    Private Shared Sub AppendAppInfo(code As StringBuilder)
        code.Append("
Partial Class Application
    Inherits Global.System.Windows.Application
    Friend ReadOnly Property Info As Global.Microsoft.VisualBasic.ApplicationServices.AssemblyInfo
        <Global.System.Diagnostics.DebuggerHidden>
        Get
            Return New Global.Microsoft.VisualBasic.ApplicationServices.AssemblyInfo(Global.System.Reflection.Assembly.GetExecutingAssembly())
        End Get
    End Property
End Class")
    End Sub

    Private Shared Sub AppendMyWpfExtension(code As StringBuilder,
                                            wpfWindows As IEnumerable(Of INamedTypeSymbol),
                                            hasVbWindowsForms As Boolean)
        AppendModuleMyWpfExtension(code)
        AppendApplicationProperty(code)

        If hasVbWindowsForms Then
            AppendComputerProperty(code)
            AppendUserProperty(code)
            AppendLogProperty(code)
        End If

        AppendWindowsProperty(code)
        AppendMyWindowsClass(code, wpfWindows)
        AppendEndModuleMyWpfExtension(code)
    End Sub

    Public Shared Sub GenerateWpfExtensionCode(wpfWindows As IEnumerable(Of INamedTypeSymbol), hasVbWindowsForms As Boolean, code As StringBuilder)
        AppendNamespaceMy(code)
        AppendMyWpfExtension(code, wpfWindows, hasVbWindowsForms)
        If hasVbWindowsForms Then
            AppendThreadSafeObjectProvider(code)
        End If
        AppendEndNamespaceMy(code)

        If hasVbWindowsForms Then
            AppendAppInfo(code)
        End If
    End Sub

End Class