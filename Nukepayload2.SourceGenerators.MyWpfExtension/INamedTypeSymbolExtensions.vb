Imports System.Runtime.CompilerServices
Imports Microsoft.CodeAnalysis

Module INamedTypeSymbolExtensions
    ''' <summary>
    ''' Returns a value indicating whether <paramref name="type"/> derives from, or implements
    ''' any generic construction of, the type defined by <paramref name="parentType"/>.
    ''' </summary>
    ''' <remarks>
    ''' This method only works when <paramref name="parentType"/> is a definition,
    ''' not a constructed type.
    ''' </remarks>
    ''' <example>
    ''' <para>
    ''' If <paramref name="parentType"/> is the class <see cref="Stack(Of T)"/>, then this
    ''' method will return <code>true</code> when called on <see cref="Stack(Of Integer)"/>
    ''' or any type derived it, because <see cref="Stack(Of Integer)"/> is constructed from
    ''' <see cref="Stack(Of T)"/>.
    ''' </para>
    ''' <para>
    ''' Similarly, if <paramref name="parentType"/> is the interface <see cref="IList(Of T)"/>, 
    ''' then this method will return <code>true</code> for <see cref="List(Of Integer)"/>
    ''' or any other class that implements <see cref="IList(Of T)"/> or an class that implements it,
    ''' because <see cref="IList(Of Integer)"/> is constructed from <see cref="IList(Of T)"/>.
    ''' </para>
    ''' </example>
    <Extension>
    Function DerivesFromOrImplementsAnyConstructionOf(
            type As INamedTypeSymbol, parentType As INamedTypeSymbol) As Boolean
        If Not parentType.IsDefinition Then
            Throw New ArgumentException($"The type {NameOf(parentType)} is not a definition; it is a constructed type", NameOf(parentType))
        End If

        Dim baseType = type.OriginalDefinition
        Do While baseType IsNot Nothing
            If baseType.Equals(parentType, SymbolEqualityComparer.Default) Then
                Return True
            End If
            baseType = baseType.BaseType?.OriginalDefinition
        Loop

        Dim implementedInterface =
            Aggregate baseInterface In type.OriginalDefinition.AllInterfaces
            Where baseInterface.OriginalDefinition.Equals(
                parentType, SymbolEqualityComparer.Default)
            Into Any

        Return implementedInterface
    End Function
End Module
