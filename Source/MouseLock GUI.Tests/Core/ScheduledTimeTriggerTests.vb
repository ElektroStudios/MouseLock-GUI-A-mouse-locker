Option Strict On
Option Explicit On
Option Infer Off

#Region " Imports "

Imports System.Globalization
Imports System.Threading

Imports MouseLock.Core

#End Region

Namespace Core

    ''' <summary>
    ''' Unit tests for <see cref="ScheduledTimeTrigger"/>.
    ''' </summary>
    <TestClass>
    Public NotInheritable Class ScheduledTimeTriggerTests

        <TestMethod>
        Public Sub ShouldFire_OnlyDuringTheConfiguredMinute()
            Dim trigger As New ScheduledTimeTrigger(New TimeSpan(9, 5, 0))

            Assert.IsFalse(trigger.ShouldFire(New Date(2026, 11, 10, 9, 4, 59)))
            Assert.IsTrue(trigger.ShouldFire(New Date(2026, 11, 10, 9, 5, 30)))
            Assert.IsFalse(trigger.ShouldFire(New Date(2026, 11, 10, 9, 6, 0)))
        End Sub

        <TestMethod>
        Public Sub ShouldFire_FiresOncePerDay_EvenWhenPolledRepeatedly()
            ' Regression: in v1.1 several accumulated Tick handlers could fire within the same minute,
            ' locking and immediately unlocking the mouse.
            Dim trigger As New ScheduledTimeTrigger(New TimeSpan(9, 5, 0))

            Assert.IsTrue(trigger.ShouldFire(New Date(2026, 11, 10, 9, 5, 0)))
            Assert.IsFalse(trigger.ShouldFire(New Date(2026, 11, 10, 9, 5, 1)))
            Assert.IsFalse(trigger.ShouldFire(New Date(2026, 11, 10, 9, 5, 59)))
        End Sub

        <TestMethod>
        Public Sub ShouldFire_FiresAgainTheNextDay()
            Dim trigger As New ScheduledTimeTrigger(New TimeSpan(23, 59, 0))

            Assert.IsTrue(trigger.ShouldFire(New Date(2026, 12, 31, 23, 59, 10)))
            Assert.IsTrue(trigger.ShouldFire(New Date(2027, 1, 1, 23, 59, 10)))
        End Sub

        <TestMethod>
        Public Sub ShouldFire_Midnight()
            Dim trigger As New ScheduledTimeTrigger(TimeSpan.Zero)
            Assert.IsTrue(trigger.ShouldFire(New Date(2026, 11, 11, 0, 0, 0)))
        End Sub

        <TestMethod>
        Public Sub ShouldFire_DoesNotDependOnCurrentCulture()
            ' Regression: v1.1 compared Date.Now.ToShortTimeString() with "HH:mm", which never matched
            ' in cultures without a leading zero (es-ES: "9:05") or with AM/PM designators (en-US: "9:05 AM").
            Dim previous As CultureInfo = Thread.CurrentThread.CurrentCulture
            Try
                For Each name As String In {"es-ES", "en-US", "ja-JP", "ar-SA"}
                    Thread.CurrentThread.CurrentCulture = New CultureInfo(name)
                    Dim timeOfDay As TimeSpan
                    Assert.IsTrue(ScheduledTimeTrigger.TryParseTime("09:05", timeOfDay), name)
                    Dim trigger As New ScheduledTimeTrigger(timeOfDay)
                    Assert.IsTrue(trigger.ShouldFire(New Date(2026, 11, 10, 9, 5, 0)), name)
                Next
            Finally
                Thread.CurrentThread.CurrentCulture = previous
            End Try
        End Sub

        <TestMethod>
        Public Sub Constructor_IgnoresSeconds()
            Dim trigger As New ScheduledTimeTrigger(New TimeSpan(0, 10, 15, 42, 500))
            Assert.AreEqual(New TimeSpan(10, 15, 0), trigger.TimeOfDay)
        End Sub

        <TestMethod>
        Public Sub Constructor_OutOfRange_Throws()
            Assert.ThrowsException(Of ArgumentOutOfRangeException)(Function() New ScheduledTimeTrigger(TimeSpan.FromHours(24)))
            Assert.ThrowsException(Of ArgumentOutOfRangeException)(Function() New ScheduledTimeTrigger(TimeSpan.FromMinutes(-1)))
        End Sub

        <DataTestMethod>
        <DataRow("00:00", 0, 0)>
        <DataRow("23:59", 23, 59)>
        <DataRow("9:05", 9, 5)>
        <DataRow(" 09:05 ", 9, 5)>
        <DataRow("09:05:33", 9, 5)>
        <DataRow("9:05 PM", 21, 5)>
        <DataRow("12:00 AM", 0, 0)>
        <DataRow("09:05PM", 21, 5)>
        Public Sub TryParseTime_ValidValues(text As String, hours As Integer, minutes As Integer)
            Dim result As TimeSpan
            Assert.IsTrue(ScheduledTimeTrigger.TryParseTime(text, result), text)
            Assert.AreEqual(New TimeSpan(hours, minutes, 0), result)
        End Sub

        <DataTestMethod>
        <DataRow(Nothing)>
        <DataRow("")>
        <DataRow("24:00")>
        <DataRow("12:60")>
        <DataRow("noon")>
        <DataRow("12")>
        Public Sub TryParseTime_InvalidValues_ReturnFalse(text As String)
            Dim result As TimeSpan
            Assert.IsFalse(ScheduledTimeTrigger.TryParseTime(text, result))
        End Sub

        <TestMethod>
        Public Sub FormatTime_UsesTwoDigits()
            Assert.AreEqual("09:05", ScheduledTimeTrigger.FormatTime(New TimeSpan(9, 5, 0)))
            Assert.AreEqual("00:00", ScheduledTimeTrigger.FormatTime(TimeSpan.Zero))
        End Sub

    End Class

End Namespace
