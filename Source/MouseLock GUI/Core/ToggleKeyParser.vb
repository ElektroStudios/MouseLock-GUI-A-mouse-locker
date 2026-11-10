Option Strict On
Option Explicit On
Option Infer Off

#Region " Imports "

Imports System.Collections.ObjectModel
Imports System.Globalization
Imports System.Windows.Forms

Imports MouseLock.Interop

#End Region

Namespace Core

    ''' <summary>
    ''' Converts the texts shown in the toggle hot key combo boxes
    ''' (e.g. "CTRL" + "F1") into a modifier/key pair that can be registered as a global hot key.
    ''' </summary>
    Friend NotInheritable Class ToggleKeyParser

#Region " Properties "

        ''' <summary>
        ''' Gets the modifier names supported by the parser, in display order.
        ''' </summary>
        Friend Shared ReadOnly Property ModifierNames As ReadOnlyCollection(Of String) =
            Array.AsReadOnly({"NONE", "ALT", "CTRL", "SHIFT"})

        ''' <summary>
        ''' Gets the key names supported by the parser, in display order: A-Z, 0-9 and F1-F12.
        ''' </summary>
        Friend Shared ReadOnly Property KeyNames As ReadOnlyCollection(Of String) = ToggleKeyParser.BuildKeyNames()

#End Region

#Region " Constructors "

        ''' <summary>
        ''' Prevents a default instance of the <see cref="ToggleKeyParser"/> class from being created.
        ''' </summary>
        Private Sub New()
        End Sub

#End Region

#Region " Public Methods "

        ''' <summary>
        ''' Tries to convert a modifier name and a key name into a hot key.
        ''' </summary>
        ''' <remarks>
        ''' Digits are mapped to the numeric keypad keys (NumPad0 - NumPad9) when no modifier is used,
        ''' so the bare hot key does not hijack the digit keys of the main keyboard; with a modifier they
        ''' are mapped to the main keyboard digits (D0 - D9).
        ''' </remarks>
        ''' <param name="modifierName">The modifier name: NONE, ALT, CTRL or SHIFT (case-insensitive).</param>
        ''' <param name="keyName">The key name: A-Z, 0-9 or F1-F12 (case-insensitive).</param>
        ''' <param name="modifiers">When this method returns <see langword="True"/>, contains the modifier keys.</param>
        ''' <param name="key">When this method returns <see langword="True"/>, contains the key.</param>
        ''' <returns><see langword="True"/> if both names are supported; otherwise, <see langword="False"/>.</returns>
        Friend Shared Function TryParse(modifierName As String, keyName As String,
                                        ByRef modifiers As HotkeyModifiers, ByRef key As Keys) As Boolean

            modifiers = HotkeyModifiers.None
            key = Keys.None

            Dim parsedModifiers As HotkeyModifiers
            If Not ToggleKeyParser.TryParseModifier(modifierName, parsedModifiers) Then
                Return False
            End If

            Dim parsedKey As Keys
            If Not ToggleKeyParser.TryParseKey(keyName, parsedModifiers = HotkeyModifiers.None, parsedKey) Then
                Return False
            End If

            modifiers = parsedModifiers
            key = parsedKey
            Return True
        End Function

        ''' <summary>
        ''' Tries to convert a modifier name into <see cref="HotkeyModifiers"/>.
        ''' </summary>
        ''' <param name="modifierName">The modifier name: NONE, ALT, CTRL or SHIFT (case-insensitive).</param>
        ''' <param name="modifiers">When this method returns <see langword="True"/>, contains the modifier keys.</param>
        ''' <returns><see langword="True"/> if the name is supported; otherwise, <see langword="False"/>.</returns>
        Friend Shared Function TryParseModifier(modifierName As String, ByRef modifiers As HotkeyModifiers) As Boolean

            modifiers = HotkeyModifiers.None
            If String.IsNullOrWhiteSpace(modifierName) Then
                Return False
            End If

            Select Case modifierName.Trim().ToUpperInvariant()
                Case "NONE"
                    modifiers = HotkeyModifiers.None
                Case "ALT"
                    modifiers = HotkeyModifiers.Alt
                Case "CTRL"
                    modifiers = HotkeyModifiers.Control
                Case "SHIFT"
                    modifiers = HotkeyModifiers.Shift
                Case Else
                    Return False
            End Select
            Return True
        End Function

        ''' <summary>
        ''' Tries to convert a key name into a <see cref="Keys"/> value.
        ''' </summary>
        ''' <param name="keyName">The key name: A-Z, 0-9 or F1-F12 (case-insensitive).</param>
        ''' <param name="useNumPadForDigits"><see langword="True"/> to map digits to NumPad0 - NumPad9;
        ''' <see langword="False"/> to map them to D0 - D9.</param>
        ''' <param name="key">When this method returns <see langword="True"/>, contains the key.</param>
        ''' <returns><see langword="True"/> if the name is supported; otherwise, <see langword="False"/>.</returns>
        Friend Shared Function TryParseKey(keyName As String, useNumPadForDigits As Boolean, ByRef key As Keys) As Boolean

            key = Keys.None
            If String.IsNullOrWhiteSpace(keyName) Then
                Return False
            End If

            Dim name As String = keyName.Trim().ToUpperInvariant()

            ' Letters: A - Z
            If name.Length = 1 AndAlso name(0) >= "A"c AndAlso name(0) <= "Z"c Then
                key = CType(Keys.A + (AscW(name(0)) - AscW("A"c)), Keys)
                Return True
            End If

            ' Digits: 0 - 9
            If name.Length = 1 AndAlso Char.IsDigit(name(0)) AndAlso name(0) <= "9"c Then
                Dim digit As Integer = AscW(name(0)) - AscW("0"c)
                key = CType(If(useNumPadForDigits, Keys.NumPad0, Keys.D0) + digit, Keys)
                Return True
            End If

            ' Function keys: F1 - F12
            Dim number As Integer
            If name.Length >= 2 AndAlso name(0) = "F"c AndAlso
               Integer.TryParse(name.Substring(1), NumberStyles.None, CultureInfo.InvariantCulture, number) AndAlso
               number >= 1 AndAlso number <= 12 AndAlso
               name.Substring(1) = number.ToString(CultureInfo.InvariantCulture) Then

                key = CType(Keys.F1 + (number - 1), Keys)
                Return True
            End If

            Return False
        End Function

#End Region

#Region " Private Methods "

        ''' <summary>
        ''' Builds the list of supported key names.
        ''' </summary>
        ''' <returns>The key names.</returns>
        Private Shared Function BuildKeyNames() As ReadOnlyCollection(Of String)

            Dim names As New List(Of String)
            For c As Integer = AscW("A"c) To AscW("Z"c)
                names.Add(ChrW(c).ToString())
            Next
            For d As Integer = 0 To 9
                names.Add(d.ToString(CultureInfo.InvariantCulture))
            Next
            For f As Integer = 1 To 12
                names.Add($"F{f.ToString(CultureInfo.InvariantCulture)}")
            Next
            Return names.AsReadOnly()
        End Function

#End Region

    End Class

End Namespace
