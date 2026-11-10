Option Strict On
Option Explicit On
Option Infer Off

#Region " Imports "

Imports System.IO
Imports System.Text

Imports MouseLock.Core

#End Region

Namespace Core

    ''' <summary>
    ''' Unit tests for <see cref="UserSettings"/>.
    ''' </summary>
    <TestClass>
    Public NotInheritable Class UserSettingsTests

        ''' <summary>
        ''' A settings file exactly as written by MouseLock v1.1.
        ''' </summary>
        Private Const LegacyFile As String =
            "Delay=5" & vbCrLf &
            "Interval=25" & vbCrLf &
            "LockType=3" & vbCrLf &
            "Time=18:30" & vbCrLf &
            "APPS=Example1.exe;MyGame.exe" & vbCrLf &
            "IfAPPNotRunning=2" & vbCrLf &
            "UseToggleKey=True" & vbCrLf &
            "SpecialKey=CTRL" & vbCrLf &
            "NormalKey=F5" & vbCrLf &
            "SysTray=False" & vbCrLf &
            "RememberSettings=True"

        <TestMethod>
        Public Sub CreateDefault_MatchesTheOriginalDefaults()
            Dim settings As UserSettings = UserSettings.CreateDefault()

            Assert.AreEqual(0, settings.InitialDelaySeconds)
            Assert.AreEqual(10, settings.LockIntervalMilliseconds)
            Assert.AreEqual(LockTrigger.None, settings.LockTrigger)
            Assert.AreEqual("00:00", settings.Time)
            Assert.AreEqual("Example1.exe;Example2.exe", settings.Applications)
            Assert.AreEqual(ApplicationExitAction.UnlockMouse, settings.ApplicationExitAction)
            Assert.IsTrue(settings.UseToggleKey)
            Assert.AreEqual("NONE", settings.ModifierKey)
            Assert.AreEqual("F1", settings.NormalKey)
            Assert.IsFalse(settings.MinimizeAtStartup)
            Assert.IsFalse(settings.RememberSettings)
        End Sub

        <TestMethod>
        Public Sub Parse_LegacyFile_ReadsEveryValue()
            Dim settings As UserSettings = UserSettings.Parse(UserSettingsTests.LegacyFile)

            Assert.AreEqual(5, settings.InitialDelaySeconds)
            Assert.AreEqual(25, settings.LockIntervalMilliseconds)
            Assert.AreEqual(LockTrigger.ApplicationRunning, settings.LockTrigger)
            Assert.AreEqual("18:30", settings.Time)
            Assert.AreEqual(ApplicationExitAction.ExitMouseLock, settings.ApplicationExitAction)
            Assert.IsTrue(settings.UseToggleKey)
            Assert.AreEqual("CTRL", settings.ModifierKey)
            Assert.AreEqual("F5", settings.NormalKey)
            Assert.IsFalse(settings.MinimizeAtStartup)
            Assert.IsTrue(settings.RememberSettings)
        End Sub

        <TestMethod>
        Public Sub Parse_PreservesTheCasingOfApplicationNames()
            ' Regression: v1.1 lowercased every line, including the application names.
            Dim settings As UserSettings = UserSettings.Parse(UserSettingsTests.LegacyFile)
            Assert.AreEqual("Example1.exe;MyGame.exe", settings.Applications)
        End Sub

        <TestMethod>
        Public Sub Parse_KeysAreCaseInsensitive_AndWhitespaceTolerant()
            Dim settings As UserSettings = UserSettings.Parse("  delay = 7 " & vbLf & "SPECIALKEY=alt" & vbLf & "normalkey= z ")

            Assert.AreEqual(7, settings.InitialDelaySeconds)
            Assert.AreEqual("ALT", settings.ModifierKey)
            Assert.AreEqual("Z", settings.NormalKey)
        End Sub

        <TestMethod>
        Public Sub Parse_ValueContainingEqualsSign_IsKeptWhole()
            Dim settings As UserSettings = UserSettings.Parse("APPS=a=b.exe;c.exe")
            Assert.AreEqual("a=b.exe;c.exe", settings.Applications)
        End Sub

        <TestMethod>
        Public Sub Parse_InvalidValues_FallBackToDefaults()
            ' Regression: in v1.1 any invalid value threw an unhandled InvalidCastException on start-up.
            Dim content As String = String.Join(vbCrLf,
                "Delay=abc", "Interval=", "LockType=9", "Time=25:99", "IfAPPNotRunning=0",
                "UseToggleKey=maybe", "SpecialKey=WIN", "NormalKey=Escape", "SysTray=1", "RememberSettings=yes")

            Dim actual As UserSettings = UserSettings.Parse(content)
            Dim expected As UserSettings = UserSettings.CreateDefault()

            Assert.AreEqual(expected.Serialize(), actual.Serialize())
        End Sub

        <TestMethod>
        Public Sub Parse_OutOfRangeNumbers_AreClamped()
            ' Regression: in v1.1 an out-of-range value threw ArgumentOutOfRangeException when assigned to the NumericUpDown,
            ' and a zero interval made the lock timer throw.
            Dim settings As UserSettings = UserSettings.Parse("Delay=99999999999" & vbLf & "Interval=0")

            Assert.AreEqual(UserSettings.MaxInitialDelaySeconds, settings.InitialDelaySeconds)
            Assert.AreEqual(UserSettings.MinLockIntervalMilliseconds, settings.LockIntervalMilliseconds)

            settings = UserSettings.Parse("Delay=-5" & vbLf & "Interval=123456")
            Assert.AreEqual(UserSettings.MinInitialDelaySeconds, settings.InitialDelaySeconds)
            Assert.AreEqual(UserSettings.MaxLockIntervalMilliseconds, settings.LockIntervalMilliseconds)
        End Sub

        <TestMethod>
        Public Sub Parse_TimeIsNormalized()
            Assert.AreEqual("09:05", UserSettings.Parse("Time=9:05").Time)
            Assert.AreEqual("21:05", UserSettings.Parse("Time=9:05 PM").Time)
        End Sub

        <TestMethod>
        Public Sub Parse_UnknownKeysCommentsAndBlankLines_AreIgnored()
            Dim settings As UserSettings = UserSettings.Parse("; comment" & vbLf & vbLf & "Foo=Bar" & vbLf & "=orphan" & vbLf & "Interval=50")
            Assert.AreEqual(50, settings.LockIntervalMilliseconds)
        End Sub

        <DataTestMethod>
        <DataRow(Nothing)>
        <DataRow("")>
        Public Sub Parse_EmptyContent_ReturnsDefaults(content As String)
            Assert.AreEqual(UserSettings.CreateDefault().Serialize(), UserSettings.Parse(content).Serialize())
        End Sub

        <TestMethod>
        Public Sub Serialize_UsesTheLegacyFormat()
            Dim settings As UserSettings = UserSettings.Parse(UserSettingsTests.LegacyFile)
            Assert.AreEqual(UserSettingsTests.LegacyFile, settings.Serialize().Replace(Environment.NewLine, vbCrLf))
        End Sub

        <TestMethod>
        Public Sub Serialize_ThenParse_RoundTrips()
            Dim original As New UserSettings() With {
                .InitialDelaySeconds = 120,
                .LockIntervalMilliseconds = 1,
                .LockTrigger = LockTrigger.ScheduledTime,
                .Time = "07:45",
                .Applications = "A.exe;b.EXE",
                .ApplicationExitAction = ApplicationExitAction.ExitMouseLock,
                .UseToggleKey = False,
                .ModifierKey = "SHIFT",
                .NormalKey = "9",
                .MinimizeAtStartup = True,
                .RememberSettings = True
            }

            Dim copy As UserSettings = UserSettings.Parse(original.Serialize())

            Assert.AreEqual(original.Serialize(), copy.Serialize())
            Assert.AreEqual(LockTrigger.ScheduledTime, copy.LockTrigger)
            Assert.AreEqual("9", copy.NormalKey)
        End Sub

        <TestMethod>
        Public Sub Serialize_IsCultureInvariant()
            Dim previous As Globalization.CultureInfo = Threading.Thread.CurrentThread.CurrentCulture
            Try
                Threading.Thread.CurrentThread.CurrentCulture = New Globalization.CultureInfo("ar-SA")
                Dim text As String = New UserSettings() With {.InitialDelaySeconds = 1234567}.Serialize()
                StringAssert.Contains(text, "Delay=1234567")
            Finally
                Threading.Thread.CurrentThread.CurrentCulture = previous
            End Try
        End Sub

        <TestMethod>
        Public Sub SaveAndLoad_RoundTripThroughAFile()
            Dim filePath As String = Path.Combine(Path.GetTempPath(), $"MouseLock.Tests.{Guid.NewGuid():N}.ini")
            Try
                Dim original As UserSettings = UserSettings.Parse(UserSettingsTests.LegacyFile)
                original.Save(filePath)

                Dim bytes As Byte() = File.ReadAllBytes(filePath)
                Assert.IsFalse(bytes.Length >= 3 AndAlso bytes(0) = &HEF AndAlso bytes(1) = &HBB AndAlso bytes(2) = &HBF, "The file must be written without BOM.")
                Assert.AreEqual(original.Serialize(), UserSettings.Load(filePath).Serialize())
            Finally
                File.Delete(filePath)
            End Try
        End Sub

        <TestMethod>
        Public Sub Save_OverwritesThePreviousContent()
            Dim filePath As String = Path.Combine(Path.GetTempPath(), $"MouseLock.Tests.{Guid.NewGuid():N}.ini")
            Try
                File.WriteAllText(filePath, New String("x"c, 5000), Encoding.UTF8)
                UserSettings.CreateDefault().Save(filePath)

                Assert.AreEqual(UserSettings.CreateDefault().Serialize(), File.ReadAllText(filePath))
            Finally
                File.Delete(filePath)
            End Try
        End Sub

        <TestMethod>
        Public Sub Load_And_Save_RequireAPath()
            Assert.ThrowsException(Of ArgumentException)(Function() UserSettings.Load(" "))
            Assert.ThrowsException(Of ArgumentException)(Sub() UserSettings.CreateDefault().Save(""))
        End Sub

        <TestMethod>
        Public Sub Load_MissingFile_Throws()
            Dim filePath As String = Path.Combine(Path.GetTempPath(), $"MouseLock.Tests.{Guid.NewGuid():N}.ini")
            Assert.ThrowsException(Of FileNotFoundException)(Function() UserSettings.Load(filePath))
        End Sub

        <DataTestMethod>
        <DataRow(0, 0)>
        <DataRow(1, 1000)>
        <DataRow(2147483, 2147483000)>
        Public Sub SecondsToMilliseconds_ValidRange(seconds As Integer, expected As Integer)
            Assert.AreEqual(expected, UserSettings.SecondsToMilliseconds(seconds))
        End Sub

        <DataTestMethod>
        <DataRow(-1)>
        <DataRow(2147484)>
        <DataRow(9999999)>
        Public Sub SecondsToMilliseconds_OutOfRange_Throws(seconds As Integer)
            ' Regression: the v1.1 delay box allowed 9999999 seconds, which overflowed Integer milliseconds.
            Assert.ThrowsException(Of ArgumentOutOfRangeException)(Function() UserSettings.SecondsToMilliseconds(seconds))
        End Sub

    End Class

End Namespace
