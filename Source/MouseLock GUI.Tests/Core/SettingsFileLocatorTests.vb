Option Strict On
Option Explicit On
Option Infer Off

#Region " Imports "

Imports System.IO

Imports MouseLock.Core

#End Region

Namespace Core

    ''' <summary>
    ''' Unit tests for <see cref="SettingsFileLocator"/>.
    ''' </summary>
    <TestClass>
    Public NotInheritable Class SettingsFileLocatorTests

        Private Const AppDir As String = "C:\Program Files\MouseLock"
        Private Const UserDir As String = "C:\Users\Test\AppData\Roaming\ElektroStudios\MouseLock"

        <TestMethod>
        Public Sub Resolve_ExistingFileInAppDirectory_WinsEvenIfReadOnly()
            Dim result As String = SettingsFileLocator.Resolve(AppDir, UserDir,
                                                               Function(p As String) True,
                                                               Function(d As String) False)
            Assert.AreEqual(Path.Combine(AppDir, UserSettings.FileName), result)
        End Sub

        <TestMethod>
        Public Sub Resolve_WritableAppDirectory_IsUsed()
            Dim result As String = SettingsFileLocator.Resolve(AppDir, UserDir,
                                                               Function(p As String) False,
                                                               Function(d As String) True)
            Assert.AreEqual(Path.Combine(AppDir, UserSettings.FileName), result)
        End Sub

        <TestMethod>
        Public Sub Resolve_ReadOnlyAppDirectory_FallsBackToUserDirectory()
            ' Regression: v1.1 used ".\MouseLock.ini" and silently failed to save under "Program Files".
            Dim result As String = SettingsFileLocator.Resolve(AppDir, UserDir,
                                                               Function(p As String) False,
                                                               Function(d As String) False)
            Assert.AreEqual(Path.Combine(UserDir, UserSettings.FileName), result)
        End Sub

        <TestMethod>
        Public Sub Resolve_ReturnsRootedPath()
            Dim appDirectory As String = Path.GetTempPath()
            Dim result As String = SettingsFileLocator.Resolve(appDirectory, UserDir, Function(p As String) False, Function(d As String) True)
            Assert.IsTrue(Path.IsPathRooted(result))
            Assert.AreEqual(UserSettings.FileName, Path.GetFileName(result))
        End Sub

        <TestMethod>
        Public Sub Resolve_InvalidArguments_Throw()
            Dim exists As Func(Of String, Boolean) = Function(p As String) False
            Dim writable As Func(Of String, Boolean) = Function(d As String) True

            Assert.ThrowsException(Of ArgumentException)(Function() SettingsFileLocator.Resolve("", UserDir, exists, writable))
            Assert.ThrowsException(Of ArgumentException)(Function() SettingsFileLocator.Resolve(AppDir, " ", exists, writable))
            Assert.ThrowsException(Of ArgumentNullException)(Function() SettingsFileLocator.Resolve(AppDir, UserDir, Nothing, writable))
            Assert.ThrowsException(Of ArgumentNullException)(Function() SettingsFileLocator.Resolve(AppDir, UserDir, exists, Nothing))
        End Sub

        <TestMethod>
        Public Sub IsDirectoryWritable_TempDirectory_IsWritable_AndLeavesNoProbeFile()
            Dim directoryPath As String = Path.Combine(Path.GetTempPath(), $"MouseLock.Tests.{Guid.NewGuid():N}")
            Directory.CreateDirectory(directoryPath)
            Try
                Assert.IsTrue(SettingsFileLocator.IsDirectoryWritable(directoryPath))
                Assert.AreEqual(0, Directory.GetFiles(directoryPath).Length)
            Finally
                Directory.Delete(directoryPath, recursive:=True)
            End Try
        End Sub

        <DataTestMethod>
        <DataRow(Nothing)>
        <DataRow("")>
        Public Sub IsDirectoryWritable_EmptyPath_ReturnsFalse(directoryPath As String)
            Assert.IsFalse(SettingsFileLocator.IsDirectoryWritable(directoryPath))
        End Sub

        <TestMethod>
        Public Sub IsDirectoryWritable_MissingDirectory_ReturnsFalse()
            Assert.IsFalse(SettingsFileLocator.IsDirectoryWritable(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"))))
        End Sub

    End Class

End Namespace
