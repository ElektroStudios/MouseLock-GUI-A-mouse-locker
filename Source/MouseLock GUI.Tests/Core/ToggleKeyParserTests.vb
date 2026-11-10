Option Strict On
Option Explicit On
Option Infer Off

#Region " Imports "

Imports MouseLock.Core
Imports MouseLock.Interop

#End Region

Namespace Core

    ''' <summary>
    ''' Unit tests for <see cref="ToggleKeyParser"/>.
    ''' </summary>
    <TestClass>
    Public NotInheritable Class ToggleKeyParserTests

        <TestMethod>
        Public Sub ModifierNames_MatchTheOriginalComboBoxItems()
            CollectionAssert.AreEqual({"NONE", "ALT", "CTRL", "SHIFT"}, ToggleKeyParser.ModifierNames.ToArray())
        End Sub

        <TestMethod>
        Public Sub KeyNames_ContainLettersDigitsAndFunctionKeys_InOrder()
            Dim names As String() = ToggleKeyParser.KeyNames.ToArray()

            Assert.AreEqual(26 + 10 + 12, names.Length)
            Assert.AreEqual("A", names(0))
            Assert.AreEqual("Z", names(25))
            Assert.AreEqual("0", names(26))
            Assert.AreEqual("9", names(35))
            Assert.AreEqual("F1", names(36), "Index 36 (F1) is the historical default key.")
            Assert.AreEqual("F12", names(47))
        End Sub

        <TestMethod>
        Public Sub EveryKeyName_IsParsable_WithEveryModifier()
            For Each modifierName As String In ToggleKeyParser.ModifierNames
                For Each keyName As String In ToggleKeyParser.KeyNames
                    Dim modifiers As HotkeyModifiers
                    Dim key As Keys
                    Assert.IsTrue(ToggleKeyParser.TryParse(modifierName, keyName, modifiers, key), $"{modifierName} + {keyName}")
                    Assert.AreNotEqual(Keys.None, key)
                Next
            Next
        End Sub

        <DataTestMethod>
        <DataRow("NONE", 0)>
        <DataRow("ALT", 1)>
        <DataRow("CTRL", 2)>
        <DataRow("SHIFT", 4)>
        <DataRow("ctrl", 2)>
        <DataRow("  Alt  ", 1)>
        Public Sub TryParseModifier_ValidNames(name As String, expected As Integer)
            Dim modifiers As HotkeyModifiers
            Assert.IsTrue(ToggleKeyParser.TryParseModifier(name, modifiers))
            Assert.AreEqual(CType(expected, HotkeyModifiers), modifiers)
        End Sub

        <DataTestMethod>
        <DataRow(Nothing)>
        <DataRow("")>
        <DataRow("WIN")>
        <DataRow("CONTROL")>
        Public Sub TryParseModifier_InvalidNames_ReturnFalse(name As String)
            Dim modifiers As HotkeyModifiers
            Assert.IsFalse(ToggleKeyParser.TryParseModifier(name, modifiers))
        End Sub

        <DataTestMethod>
        <DataRow("A", Keys.A)>
        <DataRow("z", Keys.Z)>
        <DataRow("F1", Keys.F1)>
        <DataRow("f12", Keys.F12)>
        Public Sub TryParse_LettersAndFunctionKeys(keyName As String, expected As Keys)
            Dim modifiers As HotkeyModifiers
            Dim key As Keys
            Assert.IsTrue(ToggleKeyParser.TryParse("CTRL", keyName, modifiers, key))
            Assert.AreEqual(HotkeyModifiers.Control, modifiers)
            Assert.AreEqual(expected, key)
        End Sub

        <TestMethod>
        Public Sub TryParse_DigitWithoutModifier_UsesNumericKeypad()
            ' Preserves the v1.x behavior: a bare digit hot key uses the numeric keypad.
            Dim modifiers As HotkeyModifiers
            Dim key As Keys
            Assert.IsTrue(ToggleKeyParser.TryParse("NONE", "5", modifiers, key))
            Assert.AreEqual(HotkeyModifiers.None, modifiers)
            Assert.AreEqual(Keys.NumPad5, key)
        End Sub

        <DataTestMethod>
        <DataRow("ALT")>
        <DataRow("CTRL")>
        <DataRow("SHIFT")>
        Public Sub TryParse_DigitWithModifier_UsesMainKeyboardDigit(modifierName As String)
            ' Regression: v1.1 parsed "5" as the enum value 5 (Keys.XButton1, a mouse button) when a modifier was selected.
            Dim modifiers As HotkeyModifiers
            Dim key As Keys
            Assert.IsTrue(ToggleKeyParser.TryParse(modifierName, "5", modifiers, key))
            Assert.AreEqual(Keys.D5, key)

            Assert.IsTrue(ToggleKeyParser.TryParse(modifierName, "0", modifiers, key))
            Assert.AreEqual(Keys.D0, key)
        End Sub

        <DataTestMethod>
        <DataRow(Nothing)>
        <DataRow("")>
        <DataRow("F0")>
        <DataRow("F13")>
        <DataRow("F01")>
        <DataRow("AB")>
        <DataRow("10")>
        <DataRow("LButton")>
        <DataRow("Escape")>
        <DataRow("Ñ")>
        Public Sub TryParseKey_UnsupportedNames_ReturnFalse(keyName As String)
            Dim key As Keys
            Assert.IsFalse(ToggleKeyParser.TryParseKey(keyName, useNumPadForDigits:=False, key:=key))
            Assert.AreEqual(Keys.None, key)
        End Sub

        <TestMethod>
        Public Sub TryParse_InvalidModifier_ReturnsFalse_AndResetsOutputs()
            Dim modifiers As HotkeyModifiers = HotkeyModifiers.Shift
            Dim key As Keys = Keys.A
            Assert.IsFalse(ToggleKeyParser.TryParse("META", "A", modifiers, key))
            Assert.AreEqual(HotkeyModifiers.None, modifiers)
            Assert.AreEqual(Keys.None, key)
        End Sub

        <TestMethod>
        Public Sub TryParse_EmptyKey_ReturnsFalse()
            ' Happens while the combo boxes are being initialized.
            Dim modifiers As HotkeyModifiers
            Dim key As Keys
            Assert.IsFalse(ToggleKeyParser.TryParse("ALT", "", modifiers, key))
        End Sub

    End Class

End Namespace
