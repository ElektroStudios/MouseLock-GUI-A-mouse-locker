Option Strict On
Option Explicit On
Option Infer Off

#Region " Imports "

Imports System.IO

#End Region

Namespace Core

    ''' <summary>
    ''' Resolves the full path of the settings file.
    ''' </summary>
    ''' <remarks>
    ''' Previous versions used the relative path ".\MouseLock.ini", which depends on the current
    ''' working directory (e.g. "C:\Windows\System32" when started from the Run key) and fails silently
    ''' when the application is installed in a read-only folder such as "Program Files".
    ''' </remarks>
    Friend NotInheritable Class SettingsFileLocator

#Region " Constructors "

        ''' <summary>
        ''' Prevents a default instance of the <see cref="SettingsFileLocator"/> class from being created.
        ''' </summary>
        Private Sub New()
        End Sub

#End Region

#Region " Public Methods "

        ''' <summary>
        ''' Resolves the settings file path with the following precedence:
        ''' <list type="number">
        '''   <item><description>The application directory, if a settings file already exists there (portable mode and backward compatibility).</description></item>
        '''   <item><description>The application directory, if it is writable.</description></item>
        '''   <item><description>The per-user application data directory otherwise.</description></item>
        ''' </list>
        ''' </summary>
        ''' <param name="applicationDirectory">The directory of the executable.</param>
        ''' <param name="userDataDirectory">The per-user data directory (e.g. "%APPDATA%\ElektroStudios\MouseLock").</param>
        ''' <param name="fileExists">Function that determines whether a file exists.</param>
        ''' <param name="isDirectoryWritable">Function that determines whether a directory is writable.</param>
        ''' <returns>The full path of the settings file.</returns>
        ''' <exception cref="ArgumentException">Thrown when a directory is empty.</exception>
        ''' <exception cref="ArgumentNullException">Thrown when a function is <see langword="Nothing"/>.</exception>
        Friend Shared Function Resolve(applicationDirectory As String, userDataDirectory As String,
                                       fileExists As Func(Of String, Boolean),
                                       isDirectoryWritable As Func(Of String, Boolean)) As String

            If String.IsNullOrWhiteSpace(applicationDirectory) Then
                Throw New ArgumentException("The application directory cannot be empty.", NameOf(applicationDirectory))
            End If
            If String.IsNullOrWhiteSpace(userDataDirectory) Then
                Throw New ArgumentException("The user data directory cannot be empty.", NameOf(userDataDirectory))
            End If
            If fileExists Is Nothing Then
                Throw New ArgumentNullException(NameOf(fileExists))
            End If
            If isDirectoryWritable Is Nothing Then
                Throw New ArgumentNullException(NameOf(isDirectoryWritable))
            End If

            Dim localPath As String = Path.Combine(applicationDirectory, UserSettings.FileName)
            If fileExists(localPath) OrElse isDirectoryWritable(applicationDirectory) Then
                Return localPath
            End If

            Return Path.Combine(userDataDirectory, UserSettings.FileName)
        End Function

        ''' <summary>
        ''' Resolves the settings file path for the running application.
        ''' </summary>
        ''' <returns>The full path of the settings file.</returns>
        Friend Shared Function ResolveForCurrentApplication() As String

            Dim userDataDirectory As String = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ElektroStudios", "MouseLock")

            Return SettingsFileLocator.Resolve(Global.System.Windows.Forms.Application.StartupPath, userDataDirectory,
                                               AddressOf File.Exists, AddressOf SettingsFileLocator.IsDirectoryWritable)
        End Function

        ''' <summary>
        ''' Determines whether files can be created in the specified directory, by creating and deleting a probe file.
        ''' </summary>
        ''' <param name="directoryPath">The directory to test.</param>
        ''' <returns><see langword="True"/> if the directory is writable; otherwise, <see langword="False"/>.</returns>
        Friend Shared Function IsDirectoryWritable(directoryPath As String) As Boolean

            If String.IsNullOrWhiteSpace(directoryPath) OrElse Not Directory.Exists(directoryPath) Then
                Return False
            End If

            Dim probePath As String = Path.Combine(directoryPath, $"{Guid.NewGuid():N}.tmp")
            Try
                Using stream As New FileStream(probePath, FileMode.CreateNew, FileAccess.Write, FileShare.None, 1, FileOptions.DeleteOnClose)
                End Using
                Return True
            Catch ex As UnauthorizedAccessException
                Return False
            Catch ex As IOException
                Return False
            End Try
        End Function

#End Region

    End Class

End Namespace
