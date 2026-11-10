Option Strict On
Option Explicit On
Option Infer Off

#Region " Imports "

Imports System.Collections.ObjectModel

#End Region

Namespace Core

    ''' <summary>
    ''' Provides token-based operations over the semicolon-separated list of executable
    ''' names typed in the "Application names" text box (e.g. "notepad.exe;game.exe").
    ''' </summary>
    ''' <remarks>
    ''' All comparisons are whole-token and case-insensitive, so "pad.exe" never matches "notepad.exe".
    ''' </remarks>
    Friend NotInheritable Class ProcessNameList

#Region " Constants "

        ''' <summary>
        ''' The separator between list entries.
        ''' </summary>
        Friend Const Separator As Char = ";"c

        ''' <summary>
        ''' The executable file extension that is stripped to obtain a process name.
        ''' </summary>
        Friend Const ExecutableExtension As String = ".exe"

#End Region

#Region " Constructors "

        ''' <summary>
        ''' Prevents a default instance of the <see cref="ProcessNameList"/> class from being created.
        ''' </summary>
        Private Sub New()
        End Sub

#End Region

#Region " Public Methods "

        ''' <summary>
        ''' Splits the list into its entries: trimmed, without empty entries, and without
        ''' case-insensitive duplicates (the first occurrence wins).
        ''' </summary>
        ''' <param name="text">The list text. <see langword="Nothing"/> is treated as an empty list.</param>
        ''' <returns>The entries, in their original order and casing.</returns>
        Friend Shared Function Parse(text As String) As ReadOnlyCollection(Of String)

            Dim entries As New List(Of String)
            If String.IsNullOrEmpty(text) Then
                Return entries.AsReadOnly()
            End If

            Dim seen As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
            For Each token As String In text.Split(ProcessNameList.Separator)
                Dim entry As String = token.Trim()
                If entry.Length <> 0 AndAlso seen.Add(entry) Then
                    entries.Add(entry)
                End If
            Next
            Return entries.AsReadOnly()
        End Function

        ''' <summary>
        ''' Gets the process names (as expected by <see cref="Process.GetProcessesByName(String)"/>)
        ''' of the entries in the list: the ".exe" extension is removed when present.
        ''' </summary>
        ''' <param name="text">The list text.</param>
        ''' <returns>The distinct process names, in their original order.</returns>
        Friend Shared Function GetProcessNames(text As String) As ReadOnlyCollection(Of String)

            Dim names As New List(Of String)
            Dim seen As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
            For Each entry As String In ProcessNameList.Parse(text)
                Dim name As String = ProcessNameList.ToProcessName(entry)
                If name.Length <> 0 AndAlso seen.Add(name) Then
                    names.Add(name)
                End If
            Next
            Return names.AsReadOnly()
        End Function

        ''' <summary>
        ''' Converts an executable name into a process name by removing the ".exe" extension, if present.
        ''' </summary>
        ''' <param name="executableName">The executable name, e.g. "notepad.exe".</param>
        ''' <returns>The process name, e.g. "notepad".</returns>
        ''' <exception cref="ArgumentNullException">Thrown when <paramref name="executableName"/> is <see langword="Nothing"/>.</exception>
        Friend Shared Function ToProcessName(executableName As String) As String

            If executableName Is Nothing Then
                Throw New ArgumentNullException(NameOf(executableName))
            End If

            Dim name As String = executableName.Trim()
            If name.EndsWith(ProcessNameList.ExecutableExtension, StringComparison.OrdinalIgnoreCase) Then
                name = name.Substring(0, name.Length - ProcessNameList.ExecutableExtension.Length).TrimEnd()
            End If
            Return name
        End Function

        ''' <summary>
        ''' Determines whether the list contains the specified entry (whole entry, case-insensitive).
        ''' </summary>
        ''' <param name="text">The list text.</param>
        ''' <param name="entry">The entry to look for, e.g. "notepad.exe".</param>
        ''' <returns><see langword="True"/> if the entry is in the list; otherwise, <see langword="False"/>.</returns>
        ''' <exception cref="ArgumentException">Thrown when <paramref name="entry"/> is empty.</exception>
        Friend Shared Function Contains(text As String, entry As String) As Boolean

            Dim normalizedEntry As String = ProcessNameList.ValidateEntry(entry)
            For Each item As String In ProcessNameList.Parse(text)
                If String.Equals(item, normalizedEntry, StringComparison.OrdinalIgnoreCase) Then
                    Return True
                End If
            Next
            Return False
        End Function

        ''' <summary>
        ''' Adds an entry to the list, unless it is already present.
        ''' </summary>
        ''' <param name="text">The list text.</param>
        ''' <param name="entry">The entry to add, e.g. "notepad.exe".</param>
        ''' <returns>The normalized list text that contains the entry.</returns>
        ''' <exception cref="ArgumentException">Thrown when <paramref name="entry"/> is empty or contains the separator.</exception>
        Friend Shared Function Add(text As String, entry As String) As String

            Dim normalizedEntry As String = ProcessNameList.ValidateEntry(entry)
            Dim entries As New List(Of String)(ProcessNameList.Parse(text))
            If Not entries.Contains(normalizedEntry, StringComparer.OrdinalIgnoreCase) Then
                entries.Add(normalizedEntry)
            End If
            Return String.Join(ProcessNameList.Separator.ToString(), entries)
        End Function

        ''' <summary>
        ''' Removes an entry from the list (whole entry, case-insensitive). The casing of the other entries is preserved.
        ''' </summary>
        ''' <param name="text">The list text.</param>
        ''' <param name="entry">The entry to remove, e.g. "notepad.exe".</param>
        ''' <returns>The normalized list text without the entry.</returns>
        ''' <exception cref="ArgumentException">Thrown when <paramref name="entry"/> is empty or contains the separator.</exception>
        Friend Shared Function Remove(text As String, entry As String) As String

            Dim normalizedEntry As String = ProcessNameList.ValidateEntry(entry)
            Dim entries As New List(Of String)
            For Each item As String In ProcessNameList.Parse(text)
                If Not String.Equals(item, normalizedEntry, StringComparison.OrdinalIgnoreCase) Then
                    entries.Add(item)
                End If
            Next
            Return String.Join(ProcessNameList.Separator.ToString(), entries)
        End Function

#End Region

#Region " Private Methods "

        ''' <summary>
        ''' Validates and trims a single list entry.
        ''' </summary>
        ''' <param name="entry">The entry.</param>
        ''' <returns>The trimmed entry.</returns>
        ''' <exception cref="ArgumentException">Thrown when <paramref name="entry"/> is empty or contains the separator.</exception>
        Private Shared Function ValidateEntry(entry As String) As String

            If String.IsNullOrWhiteSpace(entry) Then
                Throw New ArgumentException("The entry cannot be empty.", NameOf(entry))
            End If
            If entry.IndexOf(ProcessNameList.Separator) >= 0 Then
                Throw New ArgumentException($"The entry cannot contain the '{ProcessNameList.Separator}' separator.", NameOf(entry))
            End If
            Return entry.Trim()
        End Function

#End Region

    End Class

End Namespace
