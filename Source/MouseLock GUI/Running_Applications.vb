Imports MouseLock.Main

Public Class Running_Applications

    ' Dim Running_Applications_Timer As New Timer
    ' Dim Listview_is_busy As Boolean = False
    Dim CheckFlag As Boolean = False

#Region " Load / Close / Move "

    ' Load
    Private Sub Running_Applications_Load(sender As Object, e As EventArgs) Handles Me.Load
        Me.Location = New Point(Main.Location.X + (Main.Width), Main.Location.Y)
        AnimateWindow(Me.Handle, 500, Animation.Show_Fade)
        ' Me.Running_Applications_Timer.Interval = 5000
        ' AddHandler Me.Running_Applications_Timer.Tick, AddressOf Me.Running_Applications_Timer_Tick
        Me.Load_Listview()
    End Sub

    ' Close
    Private Sub Running_Applications_FormClosing(sender As Object, e As FormClosingEventArgs) Handles Me.FormClosing
        AnimateWindow(Me.Handle, 500, Animation.Hide_Fade)
        '  Me.Running_Applications_Timer.Enabled = False
        '  Me.Running_Applications_Timer.Dispose()
    End Sub

    ' Move event
    Private Sub Form2_Move(ByVal sender As Object, ByVal e As System.EventArgs) Handles MyBase.Move
        Main.Moving_From_Secondary_Form = True
        Main.Location = New Point(Me.Left - (Main.Width), Me.Top)
        Main.Moving_From_Secondary_Form = False
    End Sub

#End Region

#Region " Buttons "

    ' Button Select None
    Private Sub Button_Select_None_ClickButtonArea(Sender As Object, e As MouseEventArgs) Handles Button_Select_None.ClickButtonArea

        Me.ListView_Running_Applications.BeginUpdate()
        For Each Item In Me.ListView_Running_Applications.CheckedItems
            ' Application.DoEvents()
            Item.checked = False
        Next
        Me.ListView_Running_Applications.EndUpdate()
    End Sub

    ' Button_Close
    Private Sub Button_Close_ClickButtonArea(Sender As Object, e As MouseEventArgs) Handles Button_Close.ClickButtonArea
        ' Me.Running_Applications_Timer.Stop()
        Me.Close()
    End Sub

#End Region

#Region " ListView "

    ' Listview
    Private Sub Load_Listview()
        Dim ProcessesName As New List(Of String)

        Me.ListView_Running_Applications.BeginUpdate()
        Me.ListView_Running_Applications.Items.Clear()
        ProcessesName.Clear()
        For Each App As Process In Process.GetProcesses()
            ' Application.DoEvents()
            If Not ProcessesName.Contains(App.ProcessName.ToLower & ".exe") Then
                If Main.TextBox_Application_Name.Text.ToLower.Contains(App.ProcessName.ToLower & ".exe") Then
                    Me.CheckFlag = True
                    Me.ListView_Running_Applications.Items.Add(App.ProcessName & ".exe").Checked = True
                    Me.CheckFlag = False
                Else
                    Me.ListView_Running_Applications.Items.Add(App.ProcessName & ".exe")
                End If
                ProcessesName.Add(App.ProcessName.ToLower & ".exe")
            End If
        Next

        Me.ListView_Running_Applications.EndUpdate()
        ' Me.Running_Applications_Timer.Start()
    End Sub

    ' Listview [ Checked event ]
    Private Sub ListView_Running_Applications_ItemCheck(sender As Object, e As ItemCheckEventArgs) Handles ListView_Running_Applications.ItemCheck

        'MsgBox(Main.Current_Executable_Name)

        If Not Me.CheckFlag Then
            If Main.TextBox_Application_Name.Text.ToLower.Contains(sender.Items(e.Index).Text.ToLower) Then
                'If (Main.Current_Executable_Name & ".exe") = sender.Items(e.Index).Text.ToLower Then Main.Current_Executable_Name = Nothing
                Main.TextBox_Application_Name.Text = Main.TextBox_Application_Name.Text.ToLower.Replace(sender.Items(e.Index).Text.ToLower, "")
            Else
                Main.TextBox_Application_Name.Text += ";" & sender.Items(e.Index).Text
                Main.Monitor_Executables()
            End If
        End If
    End Sub

    ' Restric resizing
    Private Sub ListView_Running_Applications_ColumnWidthChanging(sender As Object, e As ColumnWidthChangingEventArgs) Handles ListView_Running_Applications.ColumnWidthChanging
        e.Cancel = True
        e.NewWidth = sender.Columns(e.ColumnIndex).Width
    End Sub

    ' Running_Applications_Timer_Tick
    'Private Sub Running_Applications_Timer_Tick()
    '    If Not Me.Listview_is_busy Then
    '        Me.Running_Applications_Timer.Stop()
    '        Me.Load_Listview()
    '    End If
    'End Sub

    ' Listview is busy
    'Private Sub ListView_Running_Applications_Mouse1(sender As Object, e As MouseEventArgs) Handles _
    '    ListView_Running_Applications.MouseDown,
    '    ListView_Running_Applications.MouseClick

    '    Me.Listview_is_busy = True
    'End Sub
    'Private Sub ListView_Running_Applications_Mouse2(sender As Object, e As EventArgs) Handles ListView_Running_Applications.MouseCaptureChanged
    '    Me.Listview_is_busy = True
    'End Sub

    '' Listview is not busy
    'Private Sub ListView_Running_Applications_Mouse3(sender As Object, e As EventArgs) Handles _
    'ListView_Running_Applications.Leave,
    'ListView_Running_Applications.MouseLeave

    '    Me.Listview_is_busy = False
    'End Sub
    'Private Sub ListView_Running_Applications_Mouse4(sender As Object, e As MouseEventArgs) Handles ListView_Running_Applications.MouseUp
    '    Me.Listview_is_busy = False
    'End Sub

    Private Sub CButton1_ClickButtonArea(Sender As Object, e As MouseEventArgs) Handles CButton1.ClickButtonArea
        Load_Listview()
    End Sub

#End Region

End Class