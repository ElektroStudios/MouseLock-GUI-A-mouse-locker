Option Strict On
Option Explicit On
Option Infer Off

#Region " Imports "

Imports System.ComponentModel

Imports MouseLock.Theming

#End Region

Namespace Theming

    ''' <summary>
    ''' Unit tests for <see cref="ThemePreference"/>.
    ''' </summary>
    <TestClass>
    Public NotInheritable Class ThemePreferenceTests

        <DataTestMethod>
        <DataRow("Light", 0)>
        <DataRow("Dark", 1)>
        <DataRow("dark", 1)>
        <DataRow(" LIGHT ", 0)>
        Public Sub TryParse_ValidValues(value As String, expected As Integer)
            Dim theme As AppTheme
            Assert.IsTrue(ThemePreference.TryParse(value, theme))
            Assert.AreEqual(CType(expected, AppTheme), theme)
        End Sub

        <DataTestMethod>
        <DataRow(Nothing)>
        <DataRow("")>
        <DataRow("System")>
        <DataRow("1")>
        Public Sub TryParse_InvalidValues_ReturnFalse(value As String)
            Dim theme As AppTheme = AppTheme.Dark
            Assert.IsFalse(ThemePreference.TryParse(value, theme))
            Assert.AreEqual(AppTheme.Light, theme)
        End Sub

        <TestMethod>
        Public Sub Resolve_StoredValue_WinsOverSystem()
            Assert.AreEqual(AppTheme.Light, ThemePreference.Resolve("Light", systemUsesDarkTheme:=True))
            Assert.AreEqual(AppTheme.Dark, ThemePreference.Resolve("Dark", systemUsesDarkTheme:=False))
        End Sub

        <TestMethod>
        Public Sub Resolve_NothingStored_FollowsWindows()
            Assert.AreEqual(AppTheme.Dark, ThemePreference.Resolve("", systemUsesDarkTheme:=True))
            Assert.AreEqual(AppTheme.Light, ThemePreference.Resolve(Nothing, systemUsesDarkTheme:=False))
            Assert.AreEqual(AppTheme.Dark, ThemePreference.Resolve("garbage", systemUsesDarkTheme:=True))
        End Sub

        <TestMethod>
        Public Sub ToStoredValue_RoundTripsThroughTryParse()
            For Each theme As AppTheme In [Enum].GetValues(GetType(AppTheme))
                Dim parsed As AppTheme
                Assert.IsTrue(ThemePreference.TryParse(ThemePreference.ToStoredValue(theme), parsed))
                Assert.AreEqual(theme, parsed)
            Next
        End Sub

        <TestMethod>
        Public Sub ToStoredValue_UndefinedTheme_Throws()
            Assert.ThrowsException(Of InvalidEnumArgumentException)(Function() ThemePreference.ToStoredValue(CType(42, AppTheme)))
        End Sub

        <TestMethod>
        Public Sub IsDarkAppsModeValue_InterpretsTheRegistryDword()
            Assert.IsTrue(ThemePreference.IsDarkAppsModeValue(0))
            Assert.IsFalse(ThemePreference.IsDarkAppsModeValue(1))
            Assert.IsFalse(ThemePreference.IsDarkAppsModeValue(Nothing))
            Assert.IsFalse(ThemePreference.IsDarkAppsModeValue("0"))
        End Sub

        <TestMethod>
        Public Sub ReadSystemUsesDarkTheme_DoesNotThrow()
            ' The result depends on the machine; it must simply be readable.
            ThemePreference.ReadSystemUsesDarkTheme()
        End Sub

    End Class

End Namespace
