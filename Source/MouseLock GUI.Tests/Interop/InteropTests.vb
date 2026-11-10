Option Strict On
Option Explicit On
Option Infer Off

#Region " Imports "

Imports MouseLock.Interop

#End Region

Namespace Interop

    ''' <summary>
    ''' Unit tests for the classes of the <c>MouseLock.Interop</c> namespace.
    ''' </summary>
    <TestClass>
    Public NotInheritable Class InteropTests

        <TestMethod>
        Public Sub HotkeyModifiers_HaveTheWin32Values()
            Assert.AreEqual(&H1, CInt(HotkeyModifiers.Alt))
            Assert.AreEqual(&H2, CInt(HotkeyModifiers.Control))
            Assert.AreEqual(&H4, CInt(HotkeyModifiers.Shift))
            Assert.AreEqual(&H8, CInt(HotkeyModifiers.Win))
            Assert.AreEqual(&H4000, CInt(HotkeyModifiers.NoRepeat))
        End Sub

        <TestMethod>
        Public Sub AnimateWindowFlags_HaveTheWin32Values()
            Assert.AreEqual(&H80000, CInt(AnimateWindowFlags.Blend))
            Assert.AreEqual(&H10000, CInt(AnimateWindowFlags.Hide))
            ' The values used by v1.1 for the fade in/out effect.
            Assert.AreEqual(524288, CInt(AnimateWindowFlags.Blend))
            Assert.AreEqual(524288 Or 65536, CInt(AnimateWindowFlags.Blend Or AnimateWindowFlags.Hide))
        End Sub

        <TestMethod>
        Public Sub SetWindowPosFlags_HaveTheWin32Values()
            Assert.AreEqual(&H1UI, CUInt(SetWindowPosFlags.NoSize))
            Assert.AreEqual(&H2UI, CUInt(SetWindowPosFlags.NoMove))
            Assert.AreEqual(&H4UI, CUInt(SetWindowPosFlags.NoZOrder))
            Assert.AreEqual(&H10UI, CUInt(SetWindowPosFlags.NoActivate))
            Assert.AreEqual(&H20UI, CUInt(SetWindowPosFlags.FrameChanged))
            Assert.AreEqual(&H200UI, CUInt(SetWindowPosFlags.NoOwnerZOrder))
        End Sub

        <TestMethod>
        Public Sub DwmAttributes_HaveTheDocumentedValues()
            Assert.AreEqual(20, DwmTitleBar.DwmaUseImmersiveDarkMode)
            Assert.AreEqual(19, DwmTitleBar.DwmaUseImmersiveDarkModeBefore20H1)
        End Sub

        <TestMethod>
        Public Sub FormatHotkey_ListsModifiersInWindowsOrder()
            Assert.AreEqual("F1", GlobalHotkey.FormatHotkey(HotkeyModifiers.None, Keys.F1))
            Assert.AreEqual("CTRL + ALT + SHIFT + A", GlobalHotkey.FormatHotkey(HotkeyModifiers.Shift Or HotkeyModifiers.Alt Or HotkeyModifiers.Control, Keys.A))
        End Sub

        <TestMethod>
        Public Sub GlobalHotkey_InvalidKey_Throws()
            Assert.ThrowsException(Of ArgumentException)(Function() New GlobalHotkey(HotkeyModifiers.None, Keys.None))
            Assert.ThrowsException(Of ArgumentException)(Function() New GlobalHotkey(HotkeyModifiers.None, Keys.Control Or Keys.A))
        End Sub

        <TestMethod>
        <TestCategory("Integration")>
        Public Sub GlobalHotkey_SameCombinationTwice_SecondRegistrationThrows()
            ' An unusual combination that is very unlikely to be in use on the test machine.
            Dim modifiers As HotkeyModifiers = HotkeyModifiers.Control Or HotkeyModifiers.Alt Or HotkeyModifiers.Shift
            Using first As GlobalHotkey = InteropTests.RegisterOrInconclusive(modifiers, Keys.F24)
                Assert.AreEqual(modifiers, first.Modifiers)
                Assert.AreEqual(Keys.F24, first.Key)
                Dim ex As HotkeyRegistrationException =
                    Assert.ThrowsException(Of HotkeyRegistrationException)(Function() New GlobalHotkey(modifiers, Keys.F24))
                StringAssert.Contains(ex.Message, "CTRL + ALT + SHIFT + F24")
            End Using

            ' After disposing, the combination is available again.
            Using again As New GlobalHotkey(modifiers, Keys.F24)
            End Using
        End Sub

        <TestMethod>
        <TestCategory("Integration")>
        Public Sub GlobalHotkey_Dispose_IsIdempotent()
            Dim hotkey As GlobalHotkey = InteropTests.RegisterOrInconclusive(HotkeyModifiers.Control Or HotkeyModifiers.Alt Or HotkeyModifiers.Shift, Keys.F23)
            hotkey.Dispose()
            hotkey.Dispose()
        End Sub

        ''' <summary>
        ''' Registers a hot key, or marks the test as inconclusive when the environment does not allow
        ''' registering hot keys at all (e.g. a CI agent without an interactive window station).
        ''' </summary>
        Private Shared Function RegisterOrInconclusive(modifiers As HotkeyModifiers, key As Keys) As GlobalHotkey
            Try
                Return New GlobalHotkey(modifiers, key)
            Catch ex As HotkeyRegistrationException
                Assert.Inconclusive($"Global hot keys cannot be registered in this environment: {ex.Message}")
                Throw
            End Try
        End Function

        <TestMethod>
        Public Sub DwmTitleBar_SetDarkMode_InvalidArguments_Throw()
            Assert.ThrowsException(Of ArgumentNullException)(Function() DwmTitleBar.SetDarkMode(Nothing, True))
            Using form As New Form()
                Assert.ThrowsException(Of InvalidOperationException)(Function() DwmTitleBar.SetDarkMode(form, True))
            End Using
        End Sub

        <TestMethod>
        Public Sub DwmTitleBar_SetDarkMode_OnACreatedHandle_DoesNotThrow()
            Using form As New Form()
                Dim handle As IntPtr = form.Handle ' Forces the creation of the handle.
                Assert.AreNotEqual(IntPtr.Zero, handle)
                DwmTitleBar.SetDarkMode(form, True)
                DwmTitleBar.SetDarkMode(form, False)
            End Using
        End Sub

        <TestMethod>
        Public Sub WindowTheme_Apply_ZeroHandle_Throws()
            Assert.ThrowsException(Of ArgumentException)(Sub() WindowTheme.Apply(IntPtr.Zero, WindowTheme.DarkExplorer))
        End Sub

    End Class

End Namespace
