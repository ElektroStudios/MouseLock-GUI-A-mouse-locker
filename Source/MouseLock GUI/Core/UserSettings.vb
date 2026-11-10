Option Strict On
Option Explicit On
Option Infer Off

#Region " Imports "

Imports System.Globalization
Imports System.IO
Imports System.Text
Imports System.Windows.Forms

#End Region

Namespace Core

    ''' <summary>
    ''' The user settings persisted in the "MouseLock.ini" file when "Remember settings" is checked.
    ''' </summary>
    ''' <remarks>
    ''' The file format ("Key=Value" lines) and key names are kept compatible with MouseLock v1.0/v1.1.
    ''' Parsing is tolerant: unknown keys are ignored and invalid or out-of-range values fall back
    ''' to their defaults or are clamped, so a damaged file can never crash the application on start-up.
    ''' </remarks>
    Friend NotInheritable Class UserSettings

#Region " Constants "

        ''' <summary>The settings file name.</summary>
        Friend Const FileName As String = "MouseLock.ini"

        ''' <summary>The minimum initial delay, in seconds.</summary>
        Friend Const MinInitialDelaySeconds As Integer = 0

        ''' <summary>
        ''' The maximum initial delay, in seconds. It is limited so the delay in milliseconds
        ''' fits in a <see cref="System.Windows.Forms.Timer.Interval"/> (an <see cref="Integer"/>).
        ''' </summary>
        Friend Const MaxInitialDelaySeconds As Integer = Integer.MaxValue \ 1000

        ''' <summary>
        ''' The minimum lock interval, in milliseconds. A <see cref="System.Windows.Forms.Timer"/> does not accept zero.
        ''' </summary>
        Friend Const MinLockIntervalMilliseconds As Integer = 1

        ''' <summary>The maximum lock interval, in milliseconds.</summary>
        Friend Const MaxLockIntervalMilliseconds As Integer = 9999

        ''' <summary>The default list of application names.</summary>
        Friend Const DefaultApplications As String = "Example1.exe;Example2.exe"

        ''' <summary>The default time of the scheduled lock.</summary>
        Friend Const DefaultTime As String = "00:00"

        ''' <summary>The default modifier of the toggle hot key.</summary>
        Friend Const DefaultModifierKey As String = "NONE"

        ''' <summary>The default key of the toggle hot key.</summary>
        Friend Const DefaultNormalKey As String = "F1"

        ''' <summary>The default lock interval, in milliseconds.</summary>
        Friend Const DefaultLockIntervalMilliseconds As Integer = 10

#End Region

#Region " Properties "

        ''' <summary>Gets or sets the initial delay before locking, in seconds ("Delay" key).</summary>
        Friend Property InitialDelaySeconds As Integer = UserSettings.MinInitialDelaySeconds

        ''' <summary>Gets or sets the interval between cursor re-centerings, in milliseconds ("Interval" key).</summary>
        Friend Property LockIntervalMilliseconds As Integer = UserSettings.DefaultLockIntervalMilliseconds

        ''' <summary>Gets or sets what automatically starts a lock ("LockType" key).</summary>
        Friend Property LockTrigger As LockTrigger = LockTrigger.None

        ''' <summary>Gets or sets the time of the scheduled lock, formatted as "HH:mm" ("Time" key).</summary>
        Friend Property Time As String = UserSettings.DefaultTime

        ''' <summary>Gets or sets the semicolon-separated list of application names ("APPS" key).</summary>
        Friend Property Applications As String = UserSettings.DefaultApplications

        ''' <summary>Gets or sets what to do when the monitored application ends ("IfAPPNotRunning" key).</summary>
        Friend Property ApplicationExitAction As ApplicationExitAction = ApplicationExitAction.UnlockMouse

        ''' <summary>Gets or sets a value indicating whether the toggle hot key is enabled ("UseToggleKey" key).</summary>
        Friend Property UseToggleKey As Boolean = True

        ''' <summary>Gets or sets the modifier name of the toggle hot key ("SpecialKey" key).</summary>
        Friend Property ModifierKey As String = UserSettings.DefaultModifierKey

        ''' <summary>Gets or sets the key name of the toggle hot key ("NormalKey" key).</summary>
        Friend Property NormalKey As String = UserSettings.DefaultNormalKey

        ''' <summary>Gets or sets a value indicating whether the window is minimized to the tray at startup ("SysTray" key).</summary>
        Friend Property MinimizeAtStartup As Boolean

        ''' <summary>Gets or sets a value indicating whether the settings are remembered ("RememberSettings" key).</summary>
        Friend Property RememberSettings As Boolean

#End Region

#Region " Public Methods "

        ''' <summary>
        ''' Creates a new instance with the default settings.
        ''' </summary>
        ''' <returns>The default settings.</returns>
        Friend Shared Function CreateDefault() As UserSettings
            Return New UserSettings()
        End Function

        ''' <summary>
        ''' Parses the content of a settings file.
        ''' </summary>
        ''' <param name="content">The file content. <see langword="Nothing"/> yields the default settings.</param>
        ''' <returns>The parsed settings.</returns>
        Friend Shared Function Parse(content As String) As UserSettings

            Dim settings As UserSettings = UserSettings.CreateDefault()
            If String.IsNullOrEmpty(content) Then
                Return settings
            End If

            Using reader As New StringReader(content)
                Dim line As String = reader.ReadLine()
                While line IsNot Nothing
                    settings.ApplyLine(line)
                    line = reader.ReadLine()
                End While
            End Using

            Return settings
        End Function

        ''' <summary>
        ''' Loads the settings from the specified file.
        ''' </summary>
        ''' <param name="filePath">The settings file path.</param>
        ''' <returns>The loaded settings.</returns>
        ''' <exception cref="ArgumentException">Thrown when <paramref name="filePath"/> is empty.</exception>
        ''' <exception cref="IOException">Thrown when the file cannot be read.</exception>
        Friend Shared Function Load(filePath As String) As UserSettings

            If String.IsNullOrWhiteSpace(filePath) Then
                Throw New ArgumentException("The file path cannot be empty.", NameOf(filePath))
            End If
            Return UserSettings.Parse(File.ReadAllText(filePath, Encoding.UTF8))
        End Function

        ''' <summary>
        ''' Serializes the settings using the "Key=Value" file format.
        ''' </summary>
        ''' <returns>The serialized settings.</returns>
        Friend Function Serialize() As String

            Dim culture As CultureInfo = CultureInfo.InvariantCulture
            Dim sb As New StringBuilder()
            sb.AppendLine($"Delay={Me.InitialDelaySeconds.ToString(culture)}")
            sb.AppendLine($"Interval={Me.LockIntervalMilliseconds.ToString(culture)}")
            sb.AppendLine($"LockType={CInt(Me.LockTrigger).ToString(culture)}")
            sb.AppendLine($"Time={Me.Time}")
            sb.AppendLine($"APPS={Me.Applications}")
            sb.AppendLine($"IfAPPNotRunning={CInt(Me.ApplicationExitAction).ToString(culture)}")
            sb.AppendLine($"UseToggleKey={Me.UseToggleKey.ToString(culture)}")
            sb.AppendLine($"SpecialKey={Me.ModifierKey}")
            sb.AppendLine($"NormalKey={Me.NormalKey}")
            sb.AppendLine($"SysTray={Me.MinimizeAtStartup.ToString(culture)}")
            sb.Append($"RememberSettings={Me.RememberSettings.ToString(culture)}")
            Return sb.ToString()
        End Function

        ''' <summary>
        ''' Saves the settings to the specified file, replacing its content in a single write.
        ''' </summary>
        ''' <param name="filePath">The settings file path.</param>
        ''' <exception cref="ArgumentException">Thrown when <paramref name="filePath"/> is empty.</exception>
        ''' <exception cref="IOException">Thrown when the file cannot be written.</exception>
        ''' <exception cref="UnauthorizedAccessException">Thrown when the file cannot be written due to permissions.</exception>
        Friend Sub Save(filePath As String)

            If String.IsNullOrWhiteSpace(filePath) Then
                Throw New ArgumentException("The file path cannot be empty.", NameOf(filePath))
            End If
            File.WriteAllText(filePath, Me.Serialize(), New UTF8Encoding(encoderShouldEmitUTF8Identifier:=False))
        End Sub

        ''' <summary>
        ''' Converts the initial delay from seconds to milliseconds.
        ''' </summary>
        ''' <param name="seconds">The delay, in seconds.</param>
        ''' <returns>The delay, in milliseconds.</returns>
        ''' <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="seconds"/> is out of the
        ''' <see cref="MinInitialDelaySeconds"/> - <see cref="MaxInitialDelaySeconds"/> range.</exception>
        Friend Shared Function SecondsToMilliseconds(seconds As Integer) As Integer

            If seconds < UserSettings.MinInitialDelaySeconds OrElse seconds > UserSettings.MaxInitialDelaySeconds Then
                Throw New ArgumentOutOfRangeException(NameOf(seconds), seconds,
                    $"The delay must be in the range {UserSettings.MinInitialDelaySeconds} - {UserSettings.MaxInitialDelaySeconds} seconds.")
            End If
            Return seconds * 1000
        End Function

#End Region

#Region " Private Methods "

        ''' <summary>
        ''' Applies a single "Key=Value" line. Unknown keys and invalid values are ignored.
        ''' </summary>
        ''' <param name="line">The line to apply.</param>
        Private Sub ApplyLine(line As String)

            Dim separatorIndex As Integer = line.IndexOf("="c)
            If separatorIndex <= 0 Then
                Return
            End If

            Dim key As String = line.Substring(0, separatorIndex).Trim()
            Dim value As String = line.Substring(separatorIndex + 1).Trim()

            Select Case key.ToUpperInvariant()
                Case "DELAY"
                    Me.InitialDelaySeconds = UserSettings.ParseInteger(value, Me.InitialDelaySeconds,
                                                                       UserSettings.MinInitialDelaySeconds, UserSettings.MaxInitialDelaySeconds)
                Case "INTERVAL"
                    Me.LockIntervalMilliseconds = UserSettings.ParseInteger(value, Me.LockIntervalMilliseconds,
                                                                            UserSettings.MinLockIntervalMilliseconds, UserSettings.MaxLockIntervalMilliseconds)
                Case "LOCKTYPE"
                    Dim trigger As Integer = UserSettings.ParseInteger(value, CInt(Me.LockTrigger), Integer.MinValue, Integer.MaxValue)
                    If [Enum].IsDefined(GetType(LockTrigger), trigger) Then
                        Me.LockTrigger = CType(trigger, LockTrigger)
                    End If
                Case "TIME"
                    Dim timeOfDay As TimeSpan
                    If ScheduledTimeTrigger.TryParseTime(value, timeOfDay) Then
                        Me.Time = ScheduledTimeTrigger.FormatTime(timeOfDay)
                    End If
                Case "APPS"
                    Me.Applications = value
                Case "IFAPPNOTRUNNING"
                    Dim action As Integer = UserSettings.ParseInteger(value, CInt(Me.ApplicationExitAction), Integer.MinValue, Integer.MaxValue)
                    If [Enum].IsDefined(GetType(ApplicationExitAction), action) Then
                        Me.ApplicationExitAction = CType(action, ApplicationExitAction)
                    End If
                Case "USETOGGLEKEY"
                    Me.UseToggleKey = UserSettings.ParseBoolean(value, Me.UseToggleKey)
                Case "SPECIALKEY"
                    Dim modifiers As Interop.HotkeyModifiers
                    If ToggleKeyParser.TryParseModifier(value, modifiers) Then
                        Me.ModifierKey = value.ToUpperInvariant()
                    End If
                Case "NORMALKEY"
                    Dim parsedKey As Keys
                    If ToggleKeyParser.TryParseKey(value, useNumPadForDigits:=False, key:=parsedKey) Then
                        Me.NormalKey = value.ToUpperInvariant()
                    End If
                Case "SYSTRAY"
                    Me.MinimizeAtStartup = UserSettings.ParseBoolean(value, Me.MinimizeAtStartup)
                Case "REMEMBERSETTINGS"
                    Me.RememberSettings = UserSettings.ParseBoolean(value, Me.RememberSettings)
            End Select
        End Sub

        ''' <summary>
        ''' Parses an invariant-culture integer and clamps it to a range.
        ''' </summary>
        ''' <param name="value">The text to parse.</param>
        ''' <param name="fallback">The value returned when <paramref name="value"/> is not a valid integer.</param>
        ''' <param name="minimum">The minimum allowed value.</param>
        ''' <param name="maximum">The maximum allowed value.</param>
        ''' <returns>The parsed and clamped value, or <paramref name="fallback"/>.</returns>
        Private Shared Function ParseInteger(value As String, fallback As Integer, minimum As Integer, maximum As Integer) As Integer

            Dim result As Long
            If Not Long.TryParse(value, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, result) Then
                Return fallback
            End If
            Return CInt(Math.Max(minimum, Math.Min(maximum, result)))
        End Function

        ''' <summary>
        ''' Parses a boolean ("True"/"False", case-insensitive).
        ''' </summary>
        ''' <param name="value">The text to parse.</param>
        ''' <param name="fallback">The value returned when <paramref name="value"/> is not a valid boolean.</param>
        ''' <returns>The parsed value, or <paramref name="fallback"/>.</returns>
        Private Shared Function ParseBoolean(value As String, fallback As Boolean) As Boolean

            Dim result As Boolean
            Return If(Boolean.TryParse(value, result), result, fallback)
        End Function

#End Region

    End Class

End Namespace
