Public Class Main

#Region " Vars "

    ' INI File
    Dim INI As String = ".\MouseLock.ini"

    ' Lock is running?
    Public Running As Boolean = False
    Public Paused As Boolean = False

    ' Menu options
    Dim Lock_Sleep As Integer
    Dim Lock_Interval As Integer
    Dim Lock_Type As Short
    Dim Time As String
    Dim Executable_Names As New List(Of String)
    Dim IfAppNotRunning As Short
    Dim Use_ToggleKey As Boolean
    Dim SysTray As Boolean
    Dim Remember_Settings As Boolean

    ' Timers
    Dim Sleep_Timer As New Timer
    Dim Lock_Timer As New Timer
    Dim Time_Timer As New Timer
    Dim Executable_Timer As New Timer

    ' Exe for Lock Type 2
    Public Current_Executable_Name As String = Nothing

    ' ToggleKey class
    Dim Toggle_Key As System.Windows.Forms.Keys
    Dim WithEvents Toggle_Key_Global As Shortcut

    ' Screen positions
    Dim Screen_Center_X As Short = (Screen.PrimaryScreen.Bounds.Width / 2)
    Dim Screen_Center_Y As Short = (Screen.PrimaryScreen.Bounds.Height / 2)

    ' FormDocking
    Public Moving_From_Secondary_Form As Boolean = False

#End Region

#Region " Form Load/Close/Resize "

    ' Load
    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        If IO.File.Exists(".\MouseLock.ini") Then Me.Load_User_settings() Else Me.Load_Default_settings()

        If Me.SysTray Then
            Me.WindowState = FormWindowState.Minimized
            Me.ShowInTaskbar = False
            Me.ToolStrip_Hide.Enabled = False
        End If

        Me.ToolStrip_Unlock.Enabled = False

        If Me.Lock_Type = 1 Then
            Me.RadioButton_Lock_Startup.Checked = True
            Me.ToolStrip_Lock.Enabled = False
            Me.ToolStrip_Unlock.Enabled = True
            Me.DoLock_Or_DoUnlock()
        ElseIf Me.Lock_Type = 2 Then
            Me.RadioButton_Lock_Time.Checked = True
        ElseIf Me.Lock_Type = 3 Then
            Me.RadioButton_Lock_Application.Checked = True
        End If

    End Sub

    ' Close
    Private Sub Form1_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        Me.NotifyIcon_SysTray.Visible = False
        Me.NotifyIcon_SysTray.Dispose()
        If Me.Remember_Settings Then Me.Save_Settings()
    End Sub

    ' Move
    Private Sub Form1_Move(ByVal sender As Object, ByVal e As System.EventArgs) Handles MyBase.Move
        If Not Me.Moving_From_Secondary_Form Then Running_Applications.Location = New Point(Me.Right, Me.Top)
    End Sub

    ' Minimize
    Public Sub Form1_Resize(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Resize
        If Me.WindowState = FormWindowState.Minimized Then
            If Running_Applications.Visible Then Running_Applications.Close()
            Me.Hide()
            Me.ToolStrip_Hide.Enabled = False
            Me.ToolStrip_Restore.Enabled = True
            Me.ShowInTaskbar = False
        Else
            Me.ToolStrip_Restore.Enabled = False
            Me.ToolStrip_Hide.Enabled = True
            Me.ShowInTaskbar = True
        End If
    End Sub

#End Region

#Region " Controls "

    ' Numeric Delay
    Private Sub Numeric_Delay_ValueChanged(sender As Object, e As EventArgs) Handles Numeric_Delay.ValueChanged
        Me.Lock_Sleep = Me.Get_Milliseconds(sender.value)
    End Sub

    ' Numeric Interval
    Private Sub Numeric_Interval_ValueChanged(sender As Object, e As EventArgs) Handles Numeric_Interval.ValueChanged
        Me.Lock_Interval = sender.value
    End Sub

    ' ComboBoxes ToggleKey
    Private Sub ComboBox_ToggleKey_SpecialKey_SelectedIndexChanged(sender As Object, e As EventArgs) Handles _
        ComboBox_ToggleKey_SpecialKey.SelectedIndexChanged,
        ComboBox_ToggleKey_NormalKey.SelectedIndexChanged

        Try : Select Case Me.ComboBox_ToggleKey_SpecialKey.Text
                Case "ALT"
                    Me.Toggle_Key = [Enum].Parse(GetType(System.Windows.Forms.Keys), Me.ComboBox_ToggleKey_NormalKey.Text, True)
                    Me.Toggle_Key_Global = Shortcut.Create(Shortcut.Modifier.Alt, Me.Toggle_Key)
                Case "CTRL"
                    Me.Toggle_Key = [Enum].Parse(GetType(System.Windows.Forms.Keys), Me.ComboBox_ToggleKey_NormalKey.Text, True)
                    Me.Toggle_Key_Global = Shortcut.Create(Shortcut.Modifier.Ctrl, Me.Toggle_Key)
                Case "SHIFT"
                    Me.Toggle_Key = [Enum].Parse(GetType(System.Windows.Forms.Keys), Me.ComboBox_ToggleKey_NormalKey.Text, True)
                    Me.Toggle_Key_Global = Shortcut.Create(Shortcut.Modifier.Shift, Me.Toggle_Key)
                Case "NONE"
                    Me.Toggle_Key = If(Not Me.String_Is_Numeric(Me.ComboBox_ToggleKey_NormalKey.Text),
                        DirectCast([Enum].Parse(GetType(System.Windows.Forms.Keys), Me.ComboBox_ToggleKey_NormalKey.Text, True), Keys),
                        DirectCast([Enum].Parse(GetType(System.Windows.Forms.Keys), (Me.ComboBox_ToggleKey_NormalKey.Text + 96), False), Keys))
                    Me.Toggle_Key_Global = Shortcut.Create(Shortcut.Modifier.None, Me.Toggle_Key)

            End Select
        Catch : End Try

    End Sub

    ' Radio Buttons
    Private Sub RadioButtons_CheckedChanged(sender As Object, e As EventArgs) Handles _
        RadioButton_Lock_Startup.CheckedChanged,
        RadioButton_Lock_Time.CheckedChanged,
        RadioButton_Lock_Application.CheckedChanged

        Me.Lock_Type = sender.tag

        If Me.Lock_Type = 2 Then
            Me.Enable_Control(Me.GTimePicker1)
            Me.Monitor_Time()
        ElseIf Not Me.Lock_Type = 2 Then
            Me.Time_Timer.Stop()
            Me.Disable_Control(Me.GTimePicker1)
        End If

        If Me.Lock_Type = 3 Then
            Me.Enable_Control(Me.Button_Running_applications)
            Me.Enable_Control(Me.Label_Application_Name)
            Me.Enable_Control(Me.Label_Application_Not_Running)
            Me.Enable_Control(Me.ComboBox_Application_Not_Running)
            Me.Enable_Control(Me.TextBox_Application_Name)
            Me.Monitor_Executables()
        ElseIf Not Me.Lock_Type = 3 Then

            Me.Disable_Control(Me.Button_Running_applications)
            Me.Disable_Control(Me.Label_Application_Name)
            Me.Disable_Control(Me.Label_Application_Not_Running)
            Me.Disable_Control(Me.ComboBox_Application_Not_Running)
            Me.Disable_Control(Me.TextBox_Application_Name)
            Me.Executable_Timer.Stop()
        End If

    End Sub

    ' Button Running applications
    Private Sub Button_Running_applications_Click(sender As Object, e As EventArgs) Handles Button_Running_applications.ClickButtonArea
        Running_Applications.Show()
    End Sub




#Region " Animate Window "

    ' [ Animate Window ]
    '
    ' // By ElektroStudios
    '
    ' Examples :
    ' AnimateWindow(Me.Handle, 1500, Animation.Show_Fade)
    ' AnimateWindow(Me.Handle, 1500, Animation.Hide_Center)

    Public Declare Function AnimateWindow Lib "user32" (ByVal hwnd As IntPtr, ByVal dwtime As Long, ByVal dwflags As Animation) As Boolean

    Public Enum Animation As Integer

        Show_Left_To_Right = 1
        Show_Right_To_left = 2
        Show_Top_To_Bottom = 4
        Show_Bottom_to_top = 8
        Show_Corner_Left_UP = 5
        Show_Corner_Left_Down = 9
        Show_Corner_Right_UP = 6
        Show_Corner_Right_Down = 10
        Show_Center = 16
        Show_Fade = 524288

        Hide_Left_To_Right = 1 Or 65536
        Hide_Right_To_left = 2 Or 65536
        Hide_Top_To_Bottom = 4 Or 65536
        Hide_Bottom_to_top = 8 Or 65536
        Hide_Corner_Left_UP = 5 Or 65536
        Hide_Corner_Left_Down = 9 Or 65536
        Hide_Corner_Right_UP = 6 Or 65536
        Hide_Corner_Right_Down = 10 Or 65536
        Hide_Center = 16 Or 65536
        Hide_Fade = 524288 Or 65536

    End Enum

#End Region





    ' ComboBox If APP is not running...
    Private Sub ComboBox_Application_Not_Running_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ComboBox_Application_Not_Running.SelectedIndexChanged
        Me.IfAppNotRunning = sender.selectedindex + 1
    End Sub

    ' GTimer picker
    Private Sub GTimePicker1_TimePicked(sender As Object) Handles GTimePicker1.TimePicked
        Me.Time = sender.time
    End Sub

    ' CheckBox ToggleKey
    Private Sub CheckBox_ToggleKey_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox_ToggleKey.CheckedChanged
        If sender.checked Then
            Me.Enable_Control(Me.ComboBox_ToggleKey_NormalKey)
            Me.Enable_Control(Me.ComboBox_ToggleKey_SpecialKey)
        Else
            Me.Disable_Control(Me.ComboBox_ToggleKey_NormalKey)
            Me.Disable_Control(Me.ComboBox_ToggleKey_SpecialKey)
        End If
    End Sub

    ' CheckBox SysTray
    Private Sub CheckBox_SysTray_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox_SysTray.CheckedChanged
        If sender.checked Then Me.SysTray = True Else Me.SysTray = False
    End Sub

    ' CheckBox Remember settings
    Private Sub CheckBox_Remember_settings_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox_Remember_settings.CheckedChanged
        If sender.checked Then Me.Remember_Settings = True : Me.Save_Settings()
    End Sub

    ' Textbox app names
    Private Sub TextBox_Application_Name_TextChanged(sender As Object, e As EventArgs) Handles TextBox_Application_Name.TextChanged
        Me.Executable_Names.Clear()
        Try
            For Each token In sender.text.split(";") : Me.Executable_Names.Add(token) : Next
        Catch
            sender.text += ";"
        End Try
        sender.Text = sender.Text.Replace(";;", ";")

        If Me.Lock_Type = 3 Then Me.Monitor_Executables()
    End Sub

    ' Button Lock/Unlock
    Private Sub Button_Lock_Unlock_Click(sender As Object, e As EventArgs) Handles Button_Lock_Unlock.ClickButtonArea
        Me.DoLock_Or_DoUnlock()
    End Sub

#End Region

#Region " SysTray "

    ' NotityIcon [ Double-Click ]
    Private Sub NotifyIcon1_MouseDoubleClick(sender As Object, e As MouseEventArgs) Handles NotifyIcon_SysTray.MouseDoubleClick
        If Me.Visible = True And Not Me.ShowInTaskbar = False Then
            Me.Hide()
            Running_Applications.Close()
            Me.ToolStrip_Hide.Enabled = False
            Me.ToolStrip_Restore.Enabled = True
        ElseIf Me.Visible = False OrElse Me.ShowInTaskbar = False Then
            Me.Show()
            Me.WindowState = FormWindowState.Normal
            Me.ToolStrip_Hide.Enabled = True
            Me.ToolStrip_Restore.Enabled = False
            Me.ShowInTaskbar = True
        End If
    End Sub

    ' ToolStrip [ Restore Or Hide ]
    Private Sub NotifyIcon_Restore_Or_Hide(sender As Object, e As EventArgs) Handles _
    ToolStrip_Hide.Click,
    ToolStrip_Restore.Click

        If Me.Visible = True And Not Me.ShowInTaskbar = False Then
            Me.Hide()
            Running_Applications.Close()
            Me.ToolStrip_Hide.Enabled = False
            Me.ToolStrip_Restore.Enabled = True
        ElseIf Me.Visible = False OrElse Me.ShowInTaskbar = False Then
            Me.Show()
            Me.WindowState = FormWindowState.Normal
            Me.ShowInTaskbar = True
            Me.ToolStrip_Hide.Enabled = True
            Me.ToolStrip_Restore.Enabled = False
        End If

    End Sub

    ' ToolStrip [ Lock Unlock ]
    Private Sub ToolStrip_Lock_Unlock_Click(sender As Object, e As EventArgs) Handles _
        ToolStrip_Lock.Click,
        ToolStrip_Unlock.Click

        If sender.name = "ToolStrip_Lock" Then
            Me.DoLock_Or_DoUnlock()
            Me.ToolStrip_Lock.Enabled = False
            Me.ToolStrip_Unlock.Enabled = True
        Else
            Me.DoLock_Or_DoUnlock()
            Me.ToolStrip_Lock.Enabled = True
            Me.ToolStrip_Unlock.Enabled = False
        End If

    End Sub

    ' ToolStrip [ Exit ]
    Private Sub ToolStrip_Exit_Click(sender As Object, e As EventArgs) Handles ToolStrip_Exit.Click
        Application.Exit()
    End Sub

#End Region

#Region " Misc Functions/Methods "

    ' Save Settings
    Private Sub Save_Settings()
        Me.Write_Text_To_File(Me.INI, "Delay=" & Me.Numeric_Delay.Value & vbNewLine, False)
        Me.Write_Text_To_File(Me.INI, "Interval=" & Me.Numeric_Interval.Value & vbNewLine, True)
        Me.Write_Text_To_File(Me.INI, "LockType=" & Me.Lock_Type & vbNewLine, True)
        Me.Write_Text_To_File(Me.INI, "Time=" & Me.GTimePicker1.Time & vbNewLine, True)
        Me.Write_Text_To_File(Me.INI, "APPS=" & Me.TextBox_Application_Name.Text & vbNewLine, True)
        Me.Write_Text_To_File(Me.INI, "IfAPPNotRunning=" & Me.ComboBox_Application_Not_Running.SelectedIndex + 1 & vbNewLine, True)
        Me.Write_Text_To_File(Me.INI, "UseToggleKey=" & Me.CheckBox_ToggleKey.Checked & vbNewLine, True)
        Me.Write_Text_To_File(Me.INI, "SpecialKey=" & Me.ComboBox_ToggleKey_SpecialKey.Text & vbNewLine, True)
        Me.Write_Text_To_File(Me.INI, "NormalKey=" & Me.ComboBox_ToggleKey_NormalKey.Text & vbNewLine, True)
        Me.Write_Text_To_File(Me.INI, "SysTray=" & Me.CheckBox_SysTray.Checked & vbNewLine, True)
        Me.Write_Text_To_File(Me.INI, "RememberSettings=" & Me.CheckBox_Remember_settings.Checked, True)
    End Sub

    ' Load default settings
    Private Sub Load_Default_settings()
        Me.Numeric_Delay.Value = 0
        Me.Numeric_Interval.Value = 10
        Me.ComboBox_Application_Not_Running.SelectedIndex = 0
        Me.CheckBox_ToggleKey.Checked = True
        Me.ComboBox_ToggleKey_SpecialKey.SelectedIndex = 0
        Me.ComboBox_ToggleKey_NormalKey.SelectedIndex = 36
    End Sub

    ' Load user settings
    Private Sub Load_User_settings()
        Dim xRead As IO.StreamReader
        xRead = IO.File.OpenText(Me.INI)
        Do Until xRead.EndOfStream

            Dim Line = xRead.ReadLine().ToLower

            If Line.StartsWith("delay=") Then Me.Numeric_Delay.Value = Line.Replace("delay=", "")
            If Line.StartsWith("interval=") Then Me.Numeric_Interval.Value = Line.Replace("interval=", "")
            If Line.StartsWith("locktype=") Then Me.Lock_Type = Line.Replace("locktype=", "")
            If Line.StartsWith("time=") Then Me.GTimePicker1.Time = Line.Replace("time=", "")
            If Line.StartsWith("apps=") Then Me.TextBox_Application_Name.Text = Line.Replace("apps=", "")
            If Line.StartsWith("ifappnotrunning=") Then Me.ComboBox_Application_Not_Running.SelectedIndex = Line.Replace("ifappnotrunning=", "") - 1
            If Line.StartsWith("usetogglekey=") Then Me.CheckBox_ToggleKey.Checked = Line.Replace("usetogglekey=", "")
            If Line.StartsWith("specialkey=") Then Me.ComboBox_ToggleKey_SpecialKey.Text = Line.Replace("specialkey=", "")
            If Line.StartsWith("normalkey=") Then Me.ComboBox_ToggleKey_NormalKey.Text = Line.Replace("normalkey=", "")
            If Line.StartsWith("systray=") Then Me.CheckBox_SysTray.Checked = Line.Replace("systray=", "")
            If Line.StartsWith("remembersettings=") Then Me.CheckBox_Remember_settings.Checked = Line.Replace("remembersettings=", "")

        Loop

        xRead.Close()
        xRead.Dispose()

    End Sub

    ' Write Text To File
    Public Sub Write_Text_To_File(ByVal Text_File As String, ByVal Text As String, ByVal Append_Text As Boolean)
        Try : My.Computer.FileSystem.WriteAllText(Text_File, Text, Append_Text)
        Catch : End Try
    End Sub

    ' Get Milliseconds
    Private Function Get_Milliseconds(ByVal Seconds As Integer) As Integer
        Dim Time_Span As New TimeSpan(TimeSpan.TicksPerSecond * Seconds)
        Return Time_Span.TotalMilliseconds
    End Function

    ' String Is Numeric
    Private Function String_Is_Numeric(ByVal Value As String) As Boolean
        Dim Number_RegEx As New System.Text.RegularExpressions.Regex("\D")
        If Number_RegEx.IsMatch(Value) Then Return False Else Return True
    End Function

    ' Process Is Running
    Private Function Process_Is_Running(ByVal Process_Name As String) As Boolean
        Dim myProcess As Process() = Process.GetProcessesByName(Process_Name)
        If Not myProcess.Length = 0 Then Return True Else Return False
    End Function

    ' Enable Control
    Private Sub Enable_Control(ByVal Control As Control)
        Try
            If Not Control.Enabled Then Control.Enabled = True
        Catch ex As Exception
            Throw New Exception(ex.Message)
        End Try
    End Sub

    ' Disable Control
    Private Sub Disable_Control(ByVal Control As Control)
        Try
            If Control.Enabled Then Control.Enabled = False
        Catch ex As Exception
            Throw New Exception(ex.Message)
        End Try
    End Sub

    ' Change Cbutton Color
    Private Sub Change_Cbutton_Color(ByVal Button_Name As CButtonLib.CButton,
                                  ByVal Color1 As Color,
                                  ByVal Color2 As Color,
                                  ByVal Color3 As Color)

        Button_Name.ColorFillBlend.iColor(0) = Color1
        Button_Name.ColorFillBlend.iColor(1) = Color2
        Button_Name.ColorFillBlend.iColor(2) = Color3
        Button_Name.UpdateDimBlends()

    End Sub

    ' Sleep
    Private Sub Sleep()
        Me.Sleep_Timer.Interval = Me.Lock_Sleep
        AddHandler Me.Sleep_Timer.Tick, AddressOf Me.Sleep_Timer_Tick
        Me.NotifyIcon_SysTray.ShowBalloonTip(10, "MouseLock", "Sleeping for " & (Me.Lock_Sleep \ 1000) & " seconds before start locking the mouse...", ToolTipIcon.Info)
        Me.Sleep_Timer.Start()
    End Sub

    ' Monitor Time
    Private Sub Monitor_Time()
        Me.Time_Timer.Interval = 1000
        AddHandler Me.Time_Timer.Tick, AddressOf Me.Time_Timer_Tick
        Me.Time_Timer.Start()
    End Sub

    ' Monitor_Executables
    Public Sub Monitor_Executables()
        Me.Executable_Timer.Interval = 1000
        AddHandler Me.Executable_Timer.Tick, AddressOf Me.Executable_Timer_Tick
        Me.Executable_Timer.Start()
    End Sub

    ' Monitor ToggleKey
    Private Sub Toggle_Key_Global_Press(ByVal s As Object, ByVal e As Shortcut.HotKeyEventArgs) Handles Toggle_Key_Global.Press
        Select Case Me.Running
            Case False
                Me.Running = True
                Me.NotifyIcon_SysTray.ShowBalloonTip(10, "MouseLock", "MouseLock resumed...", ToolTipIcon.Info)
            Case True
                Me.Paused = True
                Me.Running = False
                Me.NotifyIcon_SysTray.ShowBalloonTip(10, "MouseLock", "MouseLock paused...", ToolTipIcon.Info)
                Me.Lock_Timer.Interval = Me.Lock_Interval
        End Select
    End Sub

    ' Terminate
    Private Sub Terminate()
        If Me.Lock_Type = 2 Then
            Me.Lock_Timer.Stop()
            Me.Executable_Timer.Stop()
        End If
        If Me.Lock_Type = 3 Then
            Select Case Me.IfAppNotRunning
                Case 1
                    Me.Lock_Timer.Stop()
                    Me.Executable_Timer.Stop()
                    Me.RadioButton_Lock_Application.Checked = False : Me.RadioButton_Lock_Application.Checked = True
                Case 2
                    Application.Exit()
            End Select
        End If
    End Sub

    ' DoLock Or DoUnlock
    Private Sub DoLock_Or_DoUnlock()
        If Not Me.Running And Not Me.Paused Then
            Me.Disable_Control(Me.Label_Delay)
            Me.Disable_Control(Me.Numeric_Delay)
            Me.Disable_Control(Me.Label_S)
            Me.Disable_Control(Me.Panel_Lock_Type)
            Me.Disable_Control(Me.CheckBox_ToggleKey)
            Me.Disable_Control(Me.ComboBox_ToggleKey_NormalKey)
            Me.Disable_Control(Me.ComboBox_ToggleKey_SpecialKey)
            Me.Running = True
            Me.Paused = False
            Me.Change_Cbutton_Color(Me.Button_Lock_Unlock, Color.DarkRed, Color.DarkRed, Color.DarkRed)
            Me.Button_Lock_Unlock.UpdateDimBlends()
            Me.Button_Lock_Unlock.Image = My.Resources.Unlock_64x64
            Me.Button_Lock_Unlock.SideImage = My.Resources.Unlock_64x64
            Me.Button_Lock_Unlock.SideImageSize = New Point(42, 42)
            Me.Button_Lock_Unlock.SideImageAlign = ContentAlignment.TopLeft
            Me.Button_Lock_Unlock.Text = "Unlock"
            If Not Me.Lock_Sleep = 0 Then Me.Sleep() Else Me.Lock()
        ElseIf Me.Running Or Me.Paused Then
            Me.Enable_Control(Me.Label_Delay)
            Me.Enable_Control(Me.Numeric_Delay)
            Me.Enable_Control(Me.Label_S)
            Me.Enable_Control(Me.Panel_Lock_Type)
            Me.Enable_Control(Me.CheckBox_ToggleKey)
            Me.Enable_Control(Me.ComboBox_ToggleKey_NormalKey)
            Me.Enable_Control(Me.ComboBox_ToggleKey_SpecialKey)
            Me.Running = False
            Me.Paused = False
            Me.Terminate()
            Me.NotifyIcon_SysTray.ShowBalloonTip(10, "MouseLock", "MouseLock stopped.", ToolTipIcon.Info)
            Me.Change_Cbutton_Color(Me.Button_Lock_Unlock, Color.Green, Color.Green, Color.Green)
            Me.Button_Lock_Unlock.Image = My.Resources.Lock_64x64
            Me.Button_Lock_Unlock.SideImage = My.Resources.Lock_64x64
            Me.Button_Lock_Unlock.SideImageSize = New Point(42, 42)
            Me.Button_Lock_Unlock.SideImageAlign = ContentAlignment.TopLeft
            Me.Button_Lock_Unlock.Text = "Lock now"
        End If
    End Sub

    ' Lock
    Private Sub Lock()
        Me.NotifyIcon_SysTray.ShowBalloonTip(10, "MouseLock", "MouseLock started!", ToolTipIcon.Info)
        Me.Running = True
        AddHandler Me.Lock_Timer.Tick, AddressOf Me.Lock_Tick
        Me.Lock_Timer.Interval = Me.Lock_Interval
        Me.Lock_Timer.Start()
    End Sub

    ' Sleep timer tick
    Private Sub Sleep_Timer_Tick(sender As Object, e As EventArgs)
        sender.stop()
        If Me.Running Or Me.Paused Then Me.Lock()
    End Sub

    ' Lock timer tick
    Private Sub Lock_Tick(sender As Object, e As EventArgs)
        Me.Lock_Timer.Interval = Me.Lock_Interval
        If Me.Running Then Cursor.Position = New Point(Me.Screen_Center_X, Me.Screen_Center_Y)
    End Sub

    ' Time timer tick
    Private Sub Time_Timer_Tick()
        If Date.Now.ToShortTimeString.ToString() = Me.Time Then
            Me.Time_Timer.Stop()
            Me.DoLock_Or_DoUnlock()
        End If
    End Sub

    ' Executable Timer Tick
    Private Sub Executable_Timer_Tick()
        If Me.Current_Executable_Name = Nothing Then
            For Each x In Me.Executable_Names
                Application.DoEvents()
                If Not x.Length = 0 And x.ToLower.EndsWith(".exe") Then
                    If Me.Process_Is_Running(x.Substring(0, x.Length - 4)) Then
                        Me.Current_Executable_Name = x.Substring(0, x.Length - 4)
                        If Not Me.Running Then Me.DoLock_Or_DoUnlock()
                        'DoLock_Or_DoUnlock()
                        Exit For
                    End If
                End If
            Next
        Else
            If Not Me.Process_Is_Running(Me.Current_Executable_Name) Then
                Me.Current_Executable_Name = Nothing
                Me.Executable_Timer.Stop()
                Me.DoLock_Or_DoUnlock()
            End If
        End If

    End Sub

#End Region

End Class