Option Strict On
Option Explicit On
Option Infer Off

#Region " Imports "

Imports System.ComponentModel
Imports System.Globalization
Imports System.IO

Imports MouseLock.Core
Imports MouseLock.Interop
Imports MouseLock.Theming

#End Region

''' <summary>
''' The main window of MouseLock.
''' </summary>
Public Class Main

#Region " Constants "

    ''' <summary>
    ''' The title of the tray icon balloon tips.
    ''' </summary>
    Private Const BalloonTitle As String = "MouseLock"

    ''' <summary>
    ''' The time-out of the tray icon balloon tips, in milliseconds.
    ''' </summary>
    Private Const BalloonTimeout As Integer = 10

    ''' <summary>
    ''' The polling interval of the scheduled time and the monitored applications, in milliseconds.
    ''' </summary>
    Private Const MonitorInterval As Integer = 1000

#End Region

#Region " Fields "

    ''' <summary>The full path of the settings file.</summary>
    Private ReadOnly settingsFilePath As String

    ''' <summary>The state machine of the mouse lock.</summary>
    Private ReadOnly lockSession As New LockSession()

    ''' <summary>Timer that elapses when the initial delay ends.</summary>
    Private WithEvents DelayTimer As New Timer()

    ''' <summary>Timer that keeps the mouse pointer at the center of the screen.</summary>
    Private WithEvents LockTimer As New Timer()

    ''' <summary>Timer that polls the scheduled lock time.</summary>
    Private WithEvents ScheduleTimer As New Timer() With {.Interval = Main.MonitorInterval}

    ''' <summary>Timer that polls the monitored applications.</summary>
    Private WithEvents ProcessMonitorTimer As New Timer() With {.Interval = Main.MonitorInterval}

    ''' <summary>The trigger of the daily scheduled lock, or <see langword="Nothing"/> when the time is not valid.</summary>
    Private scheduledTrigger As ScheduledTimeTrigger

    ''' <summary>The name of the monitored process that is currently running, if any.</summary>
    Private runningMonitoredProcess As String

    ''' <summary>The registered global hot key that pauses and resumes the lock, if any.</summary>
    Private WithEvents ToggleHotkey As GlobalHotkey

    ''' <summary>The floating window that lists the running applications, if it is open.</summary>
    Private runningApplicationsWindow As Running_Applications

    ''' <summary>The current lock trigger selected with the radio buttons.</summary>
    Private selectedLockTrigger As LockTrigger = LockTrigger.None

    ''' <summary>The logo recolored for the current theme.</summary>
    Private themedLogo As Bitmap

    ''' <summary>Flag that suppresses UI event handlers while the settings are being applied.</summary>
    Private isApplyingSettings As Boolean

    ''' <summary>Flag that suppresses the theme switch handler while the theme is being applied.</summary>
    Private isApplyingTheme As Boolean

    ''' <summary>Flag that indicates that this window is being moved by the floating window, to avoid feedback loops.</summary>
    Private isMovingFromSecondaryForm As Boolean

    ''' <summary>Flag that indicates that the window must be hidden in the tray as soon as it is shown ("Minimize at startup").</summary>
    Private hideOnShown As Boolean

#End Region

#Region " Properties "

    ''' <summary>
    ''' Gets or sets the semicolon-separated list of application names to monitor.
    ''' </summary>
    Friend Property ApplicationListText As String
        Get
            Return Me.TextBox_Application_Name.Text
        End Get
        Set(value As String)
            Me.TextBox_Application_Name.Text = value
        End Set
    End Property

#End Region

#Region " Constructors "

    ''' <summary>
    ''' Initializes a new instance of the <see cref="Main"/> class.
    ''' </summary>
    Public Sub New()

        Me.InitializeComponent()

        Me.Numeric_Delay.Minimum = UserSettings.MinInitialDelaySeconds
        Me.Numeric_Delay.Maximum = UserSettings.MaxInitialDelaySeconds
        Me.Numeric_Interval.Minimum = UserSettings.MinLockIntervalMilliseconds
        Me.Numeric_Interval.Maximum = UserSettings.MaxLockIntervalMilliseconds

        ' The supported hot key names come from the parser, so the UI and the parser can never disagree.
        Main.SetItems(Me.ComboBox_ToggleKey_SpecialKey, ToggleKeyParser.ModifierNames)
        Main.SetItems(Me.ComboBox_ToggleKey_NormalKey, ToggleKeyParser.KeyNames)

        Me.settingsFilePath = SettingsFileLocator.ResolveForCurrentApplication()

        AddHandler Me.lockSession.StateChanged, AddressOf Me.LockSession_StateChanged
        AddHandler AppThemeService.ThemeChanged, AddressOf Me.AppThemeService_ThemeChanged
    End Sub

#End Region

#Region " Form Events "

    ''' <summary>
    ''' Applies the dark title bar as soon as the native window exists. This must be done here and not in
    ''' <see cref="Form.Load"/>, where it has no effect; it also runs again whenever the handle is recreated
    ''' (e.g. when <see cref="Form.ShowInTaskbar"/> changes).
    ''' </summary>
    ''' <param name="e">The event data.</param>
    Protected Overrides Sub OnHandleCreated(e As EventArgs)
        MyBase.OnHandleCreated(e)
        DwmTitleBar.SetDarkMode(Me, AppThemeService.Current = AppTheme.Dark)
    End Sub

    ''' <summary>
    ''' Handles the <see cref="Form.Load"/> event of the form.
    ''' </summary>
    Private Sub Main_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        Me.Text = $"{My.Application.Info.Title} v{My.Application.Info.Version.ToString(3)}"

        Me.ApplyTheme()

        Dim settings As UserSettings = Me.LoadSettingsOrDefault()
        Me.ApplySettings(settings)
        Me.RegisterToggleHotkey(showErrors:=True)
        Me.UpdateLockUi()

        If settings.MinimizeAtStartup Then
            ' Fully transparent until it is hidden in Shown; hiding it from Load does not work,
            ' because WinForms shows the window right after the Load event.
            Me.hideOnShown = True
            Me.Opacity = 0
        End If

        If Me.selectedLockTrigger = LockTrigger.Startup Then
            Me.StartLock()
        End If
    End Sub

    ''' <summary>
    ''' Handles the <see cref="Form.Shown"/> event of the form.
    ''' </summary>
    Private Sub Main_Shown(sender As Object, e As EventArgs) Handles MyBase.Shown

        If Me.hideOnShown Then
            Me.hideOnShown = False
            Me.Hide()
            Me.Opacity = 1
            Me.UpdateTrayMenu()
        End If
    End Sub

    ''' <summary>
    ''' Handles the <see cref="Form.FormClosing"/> event of the form.
    ''' </summary>
    Private Sub Main_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        Me.SaveOrDeleteSettings()
    End Sub

    ''' <summary>
    ''' Handles the <see cref="Form.FormClosed"/> event of the form.
    ''' </summary>
    Private Sub Main_FormClosed(sender As Object, e As FormClosedEventArgs) Handles MyBase.FormClosed

        RemoveHandler AppThemeService.ThemeChanged, AddressOf Me.AppThemeService_ThemeChanged

        Me.DelayTimer.Stop()
        Me.LockTimer.Stop()
        Me.ScheduleTimer.Stop()
        Me.ProcessMonitorTimer.Stop()
        Me.DelayTimer.Dispose()
        Me.LockTimer.Dispose()
        Me.ScheduleTimer.Dispose()
        Me.ProcessMonitorTimer.Dispose()
        Me.UnregisterToggleHotkey()
        Me.themedLogo?.Dispose()

        Me.NotifyIcon_SysTray.Visible = False
        Me.NotifyIcon_SysTray.Dispose()
    End Sub

    ''' <summary>
    ''' Handles the <see cref="Control.Move"/> event of the form, to keep the floating window docked at the right.
    ''' </summary>
    Private Sub Main_Move(sender As Object, e As EventArgs) Handles MyBase.Move

        If Not Me.isMovingFromSecondaryForm AndAlso Me.IsRunningApplicationsWindowOpen() Then
            Me.runningApplicationsWindow.Location = New Point(Me.Right, Me.Top)
        End If
    End Sub

    ''' <summary>
    ''' Handles the <see cref="Control.Resize"/> event of the form, to hide it in the tray when minimized.
    ''' </summary>
    Private Sub Main_Resize(sender As Object, e As EventArgs) Handles MyBase.Resize

        If Me.WindowState = FormWindowState.Minimized Then
            Me.CloseRunningApplicationsWindow()
            Me.Hide()
        End If
        Me.UpdateTrayMenu()
    End Sub

#End Region

#Region " Control Events "

    ''' <summary>
    ''' Handles the <see cref="RadioButton.CheckedChanged"/> event of the lock type radio buttons.
    ''' </summary>
    Private Sub RadioButtons_LockType_CheckedChanged(sender As Object, e As EventArgs) Handles _
        RadioButton_Lock_Startup.CheckedChanged,
        RadioButton_Lock_Time.CheckedChanged,
        RadioButton_Lock_Application.CheckedChanged

        Dim radioButton As RadioButton = DirectCast(sender, RadioButton)
        If Not radioButton.Checked Then
            Return
        End If

        Me.selectedLockTrigger = Main.GetLockTrigger(radioButton)
        Me.UpdateLockTriggerUi()
    End Sub

    ''' <summary>
    ''' Handles the <see cref="gTimePickerControl.gTimePicker.TimePicked"/> event of the time picker.
    ''' </summary>
    Private Sub GTimePicker1_TimePicked(sender As Object) Handles GTimePicker1.TimePicked
        Me.UpdateScheduledTrigger()
    End Sub

    ''' <summary>
    ''' Handles the <see cref="Control.TextChanged"/> event of the application names text box.
    ''' </summary>
    Private Sub TextBox_Application_Name_TextChanged(sender As Object, e As EventArgs) Handles TextBox_Application_Name.TextChanged

        ' The monitored process may have been removed from the list.
        If Me.runningMonitoredProcess IsNot Nothing AndAlso
           Not ProcessNameList.GetProcessNames(Me.TextBox_Application_Name.Text).Contains(Me.runningMonitoredProcess, StringComparer.OrdinalIgnoreCase) Then
            Me.runningMonitoredProcess = Nothing
        End If
    End Sub

    ''' <summary>
    ''' Handles the <see cref="CButtonLib.CButton.ClickButtonArea"/> event of the running applications button.
    ''' </summary>
    Private Sub Button_Running_applications_ClickButtonArea(sender As Object, e As MouseEventArgs) Handles Button_Running_applications.ClickButtonArea

        If Me.IsRunningApplicationsWindowOpen() Then
            Me.runningApplicationsWindow.Activate()
            Return
        End If

        Me.runningApplicationsWindow = New Running_Applications(Me)
        Me.runningApplicationsWindow.Show(Me)
    End Sub

    ''' <summary>
    ''' Handles the <see cref="CheckBox.CheckedChanged"/> event of the toggle hot key check box.
    ''' </summary>
    Private Sub CheckBox_ToggleKey_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox_ToggleKey.CheckedChanged

        Me.UpdateLockUi()
        If Not Me.isApplyingSettings Then
            Me.RegisterToggleHotkey(showErrors:=True)
        End If
    End Sub

    ''' <summary>
    ''' Handles the <see cref="ComboBox.SelectedIndexChanged"/> event of the toggle hot key combo boxes.
    ''' </summary>
    Private Sub ComboBoxes_ToggleKey_SelectedIndexChanged(sender As Object, e As EventArgs) Handles _
        ComboBox_ToggleKey_SpecialKey.SelectedIndexChanged,
        ComboBox_ToggleKey_NormalKey.SelectedIndexChanged

        If Not Me.isApplyingSettings Then
            Me.RegisterToggleHotkey(showErrors:=True)
        End If
    End Sub

    ''' <summary>
    ''' Handles the <see cref="NumericUpDown.ValueChanged"/> event of the lock interval.
    ''' </summary>
    Private Sub Numeric_Interval_ValueChanged(sender As Object, e As EventArgs) Handles Numeric_Interval.ValueChanged
        ' Applies immediately, also while locked.
        Me.LockTimer.Interval = CInt(Me.Numeric_Interval.Value)
    End Sub

    ''' <summary>
    ''' Handles the <see cref="CheckBox.CheckedChanged"/> event of the "Remember settings" check box.
    ''' </summary>
    Private Sub CheckBox_Remember_settings_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox_Remember_settings.CheckedChanged

        If Not Me.isApplyingSettings AndAlso Me.CheckBox_Remember_settings.Checked Then
            Me.TrySaveSettings()
        End If
    End Sub

    ''' <summary>
    ''' Handles the <see cref="CButtonLib.CButton.ClickButtonArea"/> event of the lock/unlock button.
    ''' </summary>
    Private Sub Button_Lock_Unlock_ClickButtonArea(sender As Object, e As MouseEventArgs) Handles Button_Lock_Unlock.ClickButtonArea
        Me.ToggleLock()
    End Sub

    ''' <summary>
    ''' Handles the <see cref="CustomControls.ThemeToggleSwitch.CheckedChanged"/> event of the dark theme switch.
    ''' </summary>
    Private Sub ToggleSwitch_DarkTheme_CheckedChanged(sender As Object, e As EventArgs) Handles ToggleSwitch_DarkTheme.CheckedChanged

        If Not Me.isApplyingTheme Then
            AppThemeService.SetTheme(If(Me.ToggleSwitch_DarkTheme.Checked, AppTheme.Dark, AppTheme.Light))
        End If
    End Sub

    ''' <summary>
    ''' Handles the <see cref="ToolTip.Draw"/> event of the tool tip, to paint it with the theme colors.
    ''' </summary>
    Private Sub ToolTip_Main_Draw(sender As Object, e As DrawToolTipEventArgs) Handles ToolTip_Main.Draw

        Dim palette As ThemePalette = AppThemeService.CurrentPalette
        Dim backColor As Color = If(palette.IsDark, palette.InputBackground, SystemColors.Info)
        Dim foreColor As Color = If(palette.IsDark, palette.InputForeground, SystemColors.InfoText)

        Using fill As New SolidBrush(backColor)
            e.Graphics.FillRectangle(fill, e.Bounds)
        End Using
        Using pen As New Pen(palette.Border)
            e.Graphics.DrawRectangle(pen, e.Bounds.Left, e.Bounds.Top, e.Bounds.Width - 1, e.Bounds.Height - 1)
        End Using
        TextRenderer.DrawText(e.Graphics, e.ToolTipText, e.Font, e.Bounds, foreColor,
                              TextFormatFlags.HorizontalCenter Or TextFormatFlags.VerticalCenter Or TextFormatFlags.NoPrefix)
    End Sub

#End Region

#Region " Tray Icon Events "

    ''' <summary>
    ''' Handles the <see cref="NotifyIcon.MouseDoubleClick"/> event of the tray icon.
    ''' </summary>
    Private Sub NotifyIcon_SysTray_MouseDoubleClick(sender As Object, e As MouseEventArgs) Handles NotifyIcon_SysTray.MouseDoubleClick

        If e.Button = MouseButtons.Left Then
            Me.ToggleWindowVisibility()
        End If
    End Sub

    ''' <summary>
    ''' Handles the <see cref="ToolStripItem.Click"/> event of the "Restore" and "Hide" tray menu items.
    ''' </summary>
    Private Sub ToolStrip_Restore_Or_Hide_Click(sender As Object, e As EventArgs) Handles _
        ToolStrip_Hide.Click,
        ToolStrip_Restore.Click

        Me.ToggleWindowVisibility()
    End Sub

    ''' <summary>
    ''' Handles the <see cref="ToolStripItem.Click"/> event of the "Lock" tray menu item.
    ''' </summary>
    Private Sub ToolStrip_Lock_Click(sender As Object, e As EventArgs) Handles ToolStrip_Lock.Click

        If Not Me.lockSession.IsActive Then
            Me.StartLock()
        End If
    End Sub

    ''' <summary>
    ''' Handles the <see cref="ToolStripItem.Click"/> event of the "Unlock" tray menu item.
    ''' </summary>
    Private Sub ToolStrip_Unlock_Click(sender As Object, e As EventArgs) Handles ToolStrip_Unlock.Click

        If Me.lockSession.IsActive Then
            Me.StopLock()
        End If
    End Sub

    ''' <summary>
    ''' Handles the <see cref="ToolStripItem.Click"/> event of the "Close" tray menu item.
    ''' </summary>
    Private Sub ToolStrip_Exit_Click(sender As Object, e As EventArgs) Handles ToolStrip_Exit.Click
        Me.Close()
    End Sub

#End Region

#Region " Timer And Hot Key Events "

    ''' <summary>
    ''' Handles the <see cref="Timer.Tick"/> event of the initial delay timer.
    ''' </summary>
    Private Sub DelayTimer_Tick(sender As Object, e As EventArgs) Handles DelayTimer.Tick

        Me.DelayTimer.Stop()
        If Me.lockSession.CompleteDelay() Then
            Me.BeginConfiningCursor()
        End If
    End Sub

    ''' <summary>
    ''' Handles the <see cref="Timer.Tick"/> event of the lock timer.
    ''' </summary>
    Private Sub LockTimer_Tick(sender As Object, e As EventArgs) Handles LockTimer.Tick

        If Me.lockSession.ShouldConfineCursor Then
            ' Computed on every tick, so a change of the screen resolution is honored.
            Dim bounds As Rectangle = Screen.PrimaryScreen.Bounds
            Cursor.Position = New Point(bounds.Left + (bounds.Width \ 2), bounds.Top + (bounds.Height \ 2))
        End If
    End Sub

    ''' <summary>
    ''' Handles the <see cref="Timer.Tick"/> event of the schedule timer.
    ''' </summary>
    Private Sub ScheduleTimer_Tick(sender As Object, e As EventArgs) Handles ScheduleTimer.Tick

        If Me.scheduledTrigger Is Nothing OrElse Not Me.scheduledTrigger.ShouldFire(Date.Now) Then
            Return
        End If

        ' Never toggle (unlock) an already active lock.
        If Not Me.lockSession.IsActive Then
            Me.StartLock()
        End If
    End Sub

    ''' <summary>
    ''' Handles the <see cref="Timer.Tick"/> event of the process monitor timer.
    ''' </summary>
    Private Sub ProcessMonitorTimer_Tick(sender As Object, e As EventArgs) Handles ProcessMonitorTimer.Tick

        If Me.runningMonitoredProcess Is Nothing Then
            For Each processName As String In ProcessNameList.GetProcessNames(Me.TextBox_Application_Name.Text)
                If Main.IsProcessRunning(processName) Then
                    Me.runningMonitoredProcess = processName
                    If Not Me.lockSession.IsActive Then
                        Me.StartLock()
                    End If
                    Exit For
                End If
            Next

        ElseIf Not Main.IsProcessRunning(Me.runningMonitoredProcess) Then
            Me.runningMonitoredProcess = Nothing
            Me.OnMonitoredApplicationExited()
        End If
    End Sub

    ''' <summary>
    ''' Handles the <see cref="GlobalHotkey.Pressed"/> event of the toggle hot key.
    ''' </summary>
    Private Sub ToggleHotkey_Pressed(sender As Object, e As EventArgs) Handles ToggleHotkey.Pressed

        If Not Me.lockSession.TogglePause() Then
            Return
        End If

        Me.ShowBalloon(If(Me.lockSession.State = LockState.Paused, "MouseLock paused...", "MouseLock resumed..."))
    End Sub

    ''' <summary>
    ''' Handles the <see cref="LockSession.StateChanged"/> event.
    ''' </summary>
    Private Sub LockSession_StateChanged(sender As Object, e As EventArgs)
        Me.UpdateLockUi()
    End Sub

    ''' <summary>
    ''' Handles the <see cref="AppThemeService.ThemeChanged"/> event.
    ''' </summary>
    Private Sub AppThemeService_ThemeChanged(sender As Object, e As EventArgs)
        Me.ApplyTheme()
    End Sub

#End Region

#Region " Lock "

    ''' <summary>
    ''' Starts a lock, or stops it if one is active.
    ''' </summary>
    Private Sub ToggleLock()

        If Me.lockSession.IsActive Then
            Me.StopLock()
        Else
            Me.StartLock()
        End If
    End Sub

    ''' <summary>
    ''' Starts a lock, honoring the initial delay.
    ''' </summary>
    ''' <exception cref="InvalidOperationException">Thrown when a lock is already active.</exception>
    Private Sub StartLock()

        Dim delaySeconds As Integer = CInt(Me.Numeric_Delay.Value)
        Dim delayMilliseconds As Integer = UserSettings.SecondsToMilliseconds(delaySeconds)

        If Me.lockSession.Start(hasInitialDelay:=delayMilliseconds > 0) = LockState.Delaying Then
            Me.DelayTimer.Interval = delayMilliseconds
            Me.DelayTimer.Start()
            Me.ShowBalloon($"Sleeping for {delaySeconds.ToString(CultureInfo.CurrentCulture)} seconds before start locking the mouse...")
        Else
            Me.BeginConfiningCursor()
        End If
    End Sub

    ''' <summary>
    ''' Starts keeping the mouse pointer at the center of the screen.
    ''' </summary>
    Private Sub BeginConfiningCursor()

        Me.LockTimer.Interval = CInt(Me.Numeric_Interval.Value)
        Me.LockTimer.Start()
        Me.ShowBalloon("MouseLock started!")
    End Sub

    ''' <summary>
    ''' Stops the active lock.
    ''' </summary>
    Private Sub StopLock()

        Me.DelayTimer.Stop()
        Me.LockTimer.Stop()
        If Me.lockSession.Stop() Then
            Me.ShowBalloon("MouseLock stopped.")
        End If
    End Sub

    ''' <summary>
    ''' Performs the configured action when the monitored application ends.
    ''' </summary>
    Private Sub OnMonitoredApplicationExited()

        Select Case Me.GetApplicationExitAction()
            Case ApplicationExitAction.UnlockMouse
                ' Only unlock; never start a lock that the user stopped manually.
                If Me.lockSession.IsActive Then
                    Me.StopLock()
                End If
            Case ApplicationExitAction.ExitMouseLock
                Me.Close()
        End Select
    End Sub

#End Region

#Region " UI State "

    ''' <summary>
    ''' Updates the controls that depend on the lock state.
    ''' </summary>
    Private Sub UpdateLockUi()

        Dim isActive As Boolean = Me.lockSession.IsActive

        Me.Label_Delay.Enabled = Not isActive
        Me.Numeric_Delay.Enabled = Not isActive
        Me.Label_S.Enabled = Not isActive
        Me.Panel_Lock_Type.Enabled = Not isActive
        Me.CheckBox_ToggleKey.Enabled = Not isActive
        Me.ComboBox_ToggleKey_SpecialKey.Enabled = Not isActive AndAlso Me.CheckBox_ToggleKey.Checked
        Me.ComboBox_ToggleKey_NormalKey.Enabled = Not isActive AndAlso Me.CheckBox_ToggleKey.Checked
        Me.Label2.Enabled = Me.ComboBox_ToggleKey_NormalKey.Enabled

        Dim buttonColor As Color = If(isActive, Color.DarkRed, Color.Green)
        Me.Button_Lock_Unlock.ColorFillBlend.iColor(0) = buttonColor
        Me.Button_Lock_Unlock.ColorFillBlend.iColor(1) = buttonColor
        Me.Button_Lock_Unlock.ColorFillBlend.iColor(2) = buttonColor
        Me.Button_Lock_Unlock.UpdateDimBlends()
        Me.Button_Lock_Unlock.Image = If(isActive, My.Resources.Unlock_64x64, My.Resources.Lock_64x64)
        Me.Button_Lock_Unlock.SideImage = Me.Button_Lock_Unlock.Image
        Me.Button_Lock_Unlock.SideImageSize = New Size(42, 42)
        Me.Button_Lock_Unlock.SideImageAlign = ContentAlignment.TopLeft
        Me.Button_Lock_Unlock.Text = If(isActive, "Unlock", "Lock now")

        Me.UpdateTrayMenu()
    End Sub

    ''' <summary>
    ''' Updates the enabled state of the tray menu items.
    ''' </summary>
    Private Sub UpdateTrayMenu()

        Dim isActive As Boolean = Me.lockSession.IsActive
        Dim isWindowShown As Boolean = Me.Visible AndAlso Me.WindowState <> FormWindowState.Minimized

        Me.ToolStrip_Lock.Enabled = Not isActive
        Me.ToolStrip_Unlock.Enabled = isActive
        Me.ToolStrip_Restore.Enabled = Not isWindowShown
        Me.ToolStrip_Hide.Enabled = isWindowShown
    End Sub

    ''' <summary>
    ''' Updates the controls and monitors that depend on the selected lock trigger.
    ''' </summary>
    Private Sub UpdateLockTriggerUi()

        Dim isScheduled As Boolean = Me.selectedLockTrigger = LockTrigger.ScheduledTime
        Dim isApplication As Boolean = Me.selectedLockTrigger = LockTrigger.ApplicationRunning

        Me.GTimePicker1.Enabled = isScheduled
        Me.Button_Running_applications.Enabled = isApplication
        Me.Label_Application_Name.Enabled = isApplication
        Me.Label_Application_Not_Running.Enabled = isApplication
        Me.ComboBox_Application_Not_Running.Enabled = isApplication
        Me.TextBox_Application_Name.Enabled = isApplication

        If isScheduled Then
            Me.UpdateScheduledTrigger()
            Me.ScheduleTimer.Start()
        Else
            Me.ScheduleTimer.Stop()
            Me.scheduledTrigger = Nothing
        End If

        If isApplication Then
            Me.ProcessMonitorTimer.Start()
        Else
            Me.ProcessMonitorTimer.Stop()
            Me.runningMonitoredProcess = Nothing
            Me.CloseRunningApplicationsWindow()
        End If
    End Sub

    ''' <summary>
    ''' Recreates the scheduled trigger from the time picker value.
    ''' </summary>
    Private Sub UpdateScheduledTrigger()

        Dim timeOfDay As TimeSpan
        If ScheduledTimeTrigger.TryParseTime(Me.GTimePicker1.Time, timeOfDay) Then
            If Me.scheduledTrigger Is Nothing OrElse Me.scheduledTrigger.TimeOfDay <> timeOfDay Then
                Me.scheduledTrigger = New ScheduledTimeTrigger(timeOfDay)
            End If
        Else
            Me.scheduledTrigger = Nothing
        End If
    End Sub

    ''' <summary>
    ''' Shows or hides the main window from the tray.
    ''' </summary>
    Private Sub ToggleWindowVisibility()

        If Me.Visible AndAlso Me.WindowState <> FormWindowState.Minimized Then
            Me.CloseRunningApplicationsWindow()
            Me.Hide()
        Else
            Me.Show()
            Me.WindowState = FormWindowState.Normal
            Me.Activate()
        End If
        Me.UpdateTrayMenu()
    End Sub

    ''' <summary>
    ''' Shows a balloon tip from the tray icon.
    ''' </summary>
    ''' <param name="text">The text to show.</param>
    Private Sub ShowBalloon(text As String)
        Me.NotifyIcon_SysTray.ShowBalloonTip(Main.BalloonTimeout, Main.BalloonTitle, text, ToolTipIcon.Info)
    End Sub

#End Region

#Region " Toggle Hot Key "

    ''' <summary>
    ''' Registers the toggle hot key from the current UI selection, replacing the previous one.
    ''' Nothing is registered when the hot key is disabled or the selection is incomplete.
    ''' </summary>
    ''' <param name="showErrors"><see langword="True"/> to notify the user when the hot key is in use by another application.</param>
    Private Sub RegisterToggleHotkey(showErrors As Boolean)

        Me.UnregisterToggleHotkey()

        If Not Me.CheckBox_ToggleKey.Checked Then
            Return
        End If

        Dim modifiers As HotkeyModifiers
        Dim key As Keys
        If Not ToggleKeyParser.TryParse(Me.ComboBox_ToggleKey_SpecialKey.Text, Me.ComboBox_ToggleKey_NormalKey.Text, modifiers, key) Then
            Return
        End If

        Try
            Me.ToggleHotkey = New GlobalHotkey(modifiers, key)
        Catch ex As HotkeyRegistrationException
            If showErrors Then
                Me.NotifyIcon_SysTray.ShowBalloonTip(Main.BalloonTimeout, Main.BalloonTitle, ex.Message, ToolTipIcon.Warning)
            End If
        End Try
    End Sub

    ''' <summary>
    ''' Unregisters the toggle hot key, if any.
    ''' </summary>
    Private Sub UnregisterToggleHotkey()

        If Me.ToggleHotkey IsNot Nothing Then
            Me.ToggleHotkey.Dispose()
            Me.ToggleHotkey = Nothing
        End If
    End Sub

#End Region

#Region " Running Applications Window "

    ''' <summary>
    ''' Determines whether the running applications window is open.
    ''' </summary>
    ''' <returns><see langword="True"/> if the window is open; otherwise, <see langword="False"/>.</returns>
    Private Function IsRunningApplicationsWindowOpen() As Boolean
        Return Me.runningApplicationsWindow IsNot Nothing AndAlso Not Me.runningApplicationsWindow.IsDisposed
    End Function

    ''' <summary>
    ''' Closes the running applications window, if it is open.
    ''' </summary>
    Private Sub CloseRunningApplicationsWindow()

        If Me.IsRunningApplicationsWindowOpen() Then
            Me.runningApplicationsWindow.Close()
        End If
        Me.runningApplicationsWindow = Nothing
    End Sub

    ''' <summary>
    ''' Moves this window so it stays docked at the left of the running applications window.
    ''' </summary>
    ''' <param name="secondaryWindow">The running applications window.</param>
    Friend Sub FollowSecondaryWindow(secondaryWindow As Form)

        If secondaryWindow Is Nothing Then
            Throw New ArgumentNullException(NameOf(secondaryWindow))
        End If

        Me.isMovingFromSecondaryForm = True
        Try
            Me.Location = New Point(secondaryWindow.Left - Me.Width, secondaryWindow.Top)
        Finally
            Me.isMovingFromSecondaryForm = False
        End Try
    End Sub

#End Region

#Region " Settings "

    ''' <summary>
    ''' Loads the settings file, or returns the default settings when it does not exist or cannot be read.
    ''' </summary>
    ''' <returns>The settings.</returns>
    Private Function LoadSettingsOrDefault() As UserSettings

        If Not File.Exists(Me.settingsFilePath) Then
            Return UserSettings.CreateDefault()
        End If

        Try
            Return UserSettings.Load(Me.settingsFilePath)
        Catch ex As Exception When TypeOf ex Is IOException OrElse TypeOf ex Is UnauthorizedAccessException
            MessageBox.Show(Me, $"The settings file could not be read; the default settings will be used.{Environment.NewLine}{Environment.NewLine}{Me.settingsFilePath}{Environment.NewLine}{ex.Message}",
                            Me.Text, MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return UserSettings.CreateDefault()
        End Try
    End Function

    ''' <summary>
    ''' Applies the settings to the user interface.
    ''' </summary>
    ''' <param name="settings">The settings to apply.</param>
    Private Sub ApplySettings(settings As UserSettings)

        Me.isApplyingSettings = True
        Try
            Me.Numeric_Delay.Value = settings.InitialDelaySeconds
            Me.Numeric_Interval.Value = settings.LockIntervalMilliseconds
            Me.GTimePicker1.Time = settings.Time
            Me.TextBox_Application_Name.Text = settings.Applications
            Me.ComboBox_Application_Not_Running.SelectedIndex = CInt(settings.ApplicationExitAction) - 1
            Me.ComboBox_ToggleKey_SpecialKey.SelectedIndex = Main.IndexOfItem(Me.ComboBox_ToggleKey_SpecialKey, settings.ModifierKey)
            Me.ComboBox_ToggleKey_NormalKey.SelectedIndex = Main.IndexOfItem(Me.ComboBox_ToggleKey_NormalKey, settings.NormalKey)
            Me.CheckBox_ToggleKey.Checked = settings.UseToggleKey
            Me.CheckBox_SysTray.Checked = settings.MinimizeAtStartup
            Me.CheckBox_Remember_settings.Checked = settings.RememberSettings

            Select Case settings.LockTrigger
                Case LockTrigger.Startup
                    Me.RadioButton_Lock_Startup.Checked = True
                Case LockTrigger.ScheduledTime
                    Me.RadioButton_Lock_Time.Checked = True
                Case LockTrigger.ApplicationRunning
                    Me.RadioButton_Lock_Application.Checked = True
                Case Else
                    Me.RadioButton_Lock_Startup.Checked = False
                    Me.RadioButton_Lock_Time.Checked = False
                    Me.RadioButton_Lock_Application.Checked = False
            End Select
            Me.selectedLockTrigger = settings.LockTrigger
        Finally
            Me.isApplyingSettings = False
        End Try

        Me.UpdateLockTriggerUi()
    End Sub

    ''' <summary>
    ''' Captures the current user interface state as settings.
    ''' </summary>
    ''' <returns>The settings.</returns>
    Private Function CaptureSettings() As UserSettings

        Dim timeOfDay As TimeSpan
        Dim time As String = If(ScheduledTimeTrigger.TryParseTime(Me.GTimePicker1.Time, timeOfDay),
                                ScheduledTimeTrigger.FormatTime(timeOfDay), UserSettings.DefaultTime)

        Return New UserSettings() With {
            .InitialDelaySeconds = CInt(Me.Numeric_Delay.Value),
            .LockIntervalMilliseconds = CInt(Me.Numeric_Interval.Value),
            .LockTrigger = Me.selectedLockTrigger,
            .Time = time,
            .Applications = Me.TextBox_Application_Name.Text,
            .ApplicationExitAction = Me.GetApplicationExitAction(),
            .UseToggleKey = Me.CheckBox_ToggleKey.Checked,
            .ModifierKey = If(Me.ComboBox_ToggleKey_SpecialKey.SelectedIndex >= 0, Me.ComboBox_ToggleKey_SpecialKey.Text, UserSettings.DefaultModifierKey),
            .NormalKey = If(Me.ComboBox_ToggleKey_NormalKey.SelectedIndex >= 0, Me.ComboBox_ToggleKey_NormalKey.Text, UserSettings.DefaultNormalKey),
            .MinimizeAtStartup = Me.CheckBox_SysTray.Checked,
            .RememberSettings = Me.CheckBox_Remember_settings.Checked
        }
    End Function

    ''' <summary>
    ''' Saves the settings when "Remember settings" is checked; otherwise deletes the settings file,
    ''' so the next start uses the default settings.
    ''' </summary>
    Private Sub SaveOrDeleteSettings()

        If Me.CheckBox_Remember_settings.Checked Then
            Me.TrySaveSettings()
            Return
        End If

        Try
            If File.Exists(Me.settingsFilePath) Then
                File.Delete(Me.settingsFilePath)
            End If
        Catch ex As Exception When TypeOf ex Is IOException OrElse TypeOf ex Is UnauthorizedAccessException
            Me.ShowSettingsError("deleted", ex)
        End Try
    End Sub

    ''' <summary>
    ''' Saves the settings, notifying the user if the file cannot be written.
    ''' </summary>
    Private Sub TrySaveSettings()

        Try
            Directory.CreateDirectory(Path.GetDirectoryName(Me.settingsFilePath))
            Me.CaptureSettings().Save(Me.settingsFilePath)
        Catch ex As Exception When TypeOf ex Is IOException OrElse TypeOf ex Is UnauthorizedAccessException
            Me.ShowSettingsError("saved", ex)
        End Try
    End Sub

    ''' <summary>
    ''' Notifies the user that the settings file could not be written or deleted.
    ''' </summary>
    ''' <param name="operation">The failed operation ("saved" or "deleted").</param>
    ''' <param name="ex">The exception.</param>
    Private Sub ShowSettingsError(operation As String, ex As Exception)
        MessageBox.Show(Me, $"The settings file could not be {operation}.{Environment.NewLine}{Environment.NewLine}{Me.settingsFilePath}{Environment.NewLine}{ex.Message}",
                        Me.Text, MessageBoxButtons.OK, MessageBoxIcon.Warning)
    End Sub

#End Region

#Region " Theme "

    ''' <summary>
    ''' Applies the current theme to this window, its tray menu and its title bar.
    ''' </summary>
    Private Sub ApplyTheme()

        Dim palette As ThemePalette = AppThemeService.CurrentPalette

        Me.isApplyingTheme = True
        Try
            Me.ToggleSwitch_DarkTheme.Checked = palette.IsDark
        Finally
            Me.isApplyingTheme = False
        End Try

        ThemeManager.Apply(Me, palette, Me.Button_Lock_Unlock, Me.Button_Running_applications)
        ThemeManager.ApplyToToolStrip(Me.ContextMenuStrip_SysTray, palette)

        Dim previousLogo As Bitmap = Me.themedLogo
        Using sourceLogo As Bitmap = My.Resources.Logo
            Me.themedLogo = LogoRecolorizer.Recolor(sourceLogo, ThemePalette.BrandColor, Color.White,
                                                    palette.Background, palette.LogoForeground)
        End Using
        Me.PictureBox_Logo.BackgroundImage = Me.themedLogo
        previousLogo?.Dispose()

        If Me.IsHandleCreated Then
            DwmTitleBar.SetDarkMode(Me, palette.IsDark)
        End If
    End Sub

#End Region

#Region " Helpers "

    ''' <summary>
    ''' Gets the selected action for when the monitored application ends.
    ''' </summary>
    ''' <returns>The action.</returns>
    Private Function GetApplicationExitAction() As ApplicationExitAction

        Dim value As Integer = Me.ComboBox_Application_Not_Running.SelectedIndex + 1
        Return If([Enum].IsDefined(GetType(ApplicationExitAction), value),
                  CType(value, ApplicationExitAction), ApplicationExitAction.UnlockMouse)
    End Function

    ''' <summary>
    ''' Gets the lock trigger stored in the <see cref="Control.Tag"/> of a lock type radio button.
    ''' </summary>
    ''' <param name="radioButton">The radio button.</param>
    ''' <returns>The lock trigger.</returns>
    ''' <exception cref="InvalidOperationException">Thrown when the tag is not a valid lock trigger.</exception>
    Private Shared Function GetLockTrigger(radioButton As RadioButton) As LockTrigger

        Dim value As Integer
        If Not Integer.TryParse(Convert.ToString(radioButton.Tag, CultureInfo.InvariantCulture), NumberStyles.Integer, CultureInfo.InvariantCulture, value) OrElse
           Not [Enum].IsDefined(GetType(LockTrigger), value) Then
            Throw New InvalidOperationException($"The tag of '{radioButton.Name}' is not a valid lock trigger.")
        End If
        Return CType(value, LockTrigger)
    End Function

    ''' <summary>
    ''' Gets the index of a combo box item by its text (case-insensitive).
    ''' </summary>
    ''' <param name="comboBox">The combo box.</param>
    ''' <param name="text">The item text.</param>
    ''' <returns>The item index.</returns>
    ''' <exception cref="ArgumentException">Thrown when the item does not exist.</exception>
    Private Shared Function IndexOfItem(comboBox As ComboBox, text As String) As Integer

        Dim index As Integer = comboBox.FindStringExact(text)
        If index < 0 Then
            Throw New ArgumentException($"'{text}' is not an item of '{comboBox.Name}'.", NameOf(text))
        End If
        Return index
    End Function

    ''' <summary>
    ''' Replaces the items of a combo box.
    ''' </summary>
    ''' <param name="comboBox">The combo box.</param>
    ''' <param name="items">The new items.</param>
    Private Shared Sub SetItems(comboBox As ComboBox, items As IEnumerable(Of String))

        comboBox.BeginUpdate()
        Try
            comboBox.Items.Clear()
            For Each item As String In items
                comboBox.Items.Add(item)
            Next
        Finally
            comboBox.EndUpdate()
        End Try
    End Sub

    ''' <summary>
    ''' Determines whether at least one process with the specified name is running.
    ''' </summary>
    ''' <param name="processName">The process name, without the ".exe" extension.</param>
    ''' <returns><see langword="True"/> if the process is running; otherwise, <see langword="False"/>.</returns>
    Private Shared Function IsProcessRunning(processName As String) As Boolean

        Dim processes As Process() = Process.GetProcessesByName(processName)
        Try
            Return processes.Length <> 0
        Finally
            For Each p As Process In processes
                p.Dispose()
            Next
        End Try
    End Function

#End Region

End Class
