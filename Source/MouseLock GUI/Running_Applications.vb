Option Strict On
Option Explicit On
Option Infer Off

#Region " Imports "

Imports MouseLock.Core
Imports MouseLock.Interop
Imports MouseLock.Theming

#End Region

''' <summary>
''' Floating window, docked at the right of the main window, that lists the running applications
''' and lets the user check the ones that must be monitored.
''' </summary>
Public Class Running_Applications

#Region " Constants "

    ''' <summary>
    ''' The duration of the fade in/out animation, in milliseconds.
    ''' </summary>
    Private Const FadeDuration As Integer = 500

#End Region

#Region " Fields "

    ''' <summary>The main window that owns the list of application names.</summary>
    Private ReadOnly mainWindow As Main

    ''' <summary>Flag that suppresses the <see cref="ListView.ItemCheck"/> handler while the list is being populated.</summary>
    Private isPopulating As Boolean

#End Region

#Region " Constructors "

    ''' <summary>
    ''' Initializes a new instance of the <see cref="Running_Applications"/> class.
    ''' </summary>
    ''' <remarks>Parameterless constructor required by the Windows Forms designer; the window cannot be shown
    ''' without a main window, use <see cref="Running_Applications.New(Main)"/> instead.</remarks>
    Public Sub New()
        Me.InitializeComponent()
    End Sub

    ''' <summary>
    ''' Initializes a new instance of the <see cref="Running_Applications"/> class.
    ''' </summary>
    ''' <param name="mainWindow">The main window that owns the list of application names.</param>
    ''' <exception cref="ArgumentNullException">Thrown when <paramref name="mainWindow"/> is <see langword="Nothing"/>.</exception>
    Friend Sub New(mainWindow As Main)

        If mainWindow Is Nothing Then
            Throw New ArgumentNullException(NameOf(mainWindow))
        End If

        Me.InitializeComponent()
        Me.mainWindow = mainWindow
        AddHandler AppThemeService.ThemeChanged, AddressOf Me.AppThemeService_ThemeChanged
    End Sub

#End Region

#Region " Form Events "

    ''' <summary>
    ''' Applies the dark title bar as soon as the native window exists (it has no effect from the Load event).
    ''' </summary>
    ''' <param name="e">The event data.</param>
    Protected Overrides Sub OnHandleCreated(e As EventArgs)
        MyBase.OnHandleCreated(e)
        DwmTitleBar.SetDarkMode(Me, AppThemeService.Current = AppTheme.Dark)
    End Sub

    ''' <summary>
    ''' Handles the <see cref="Form.Load"/> event of the form.
    ''' </summary>
    Private Sub Running_Applications_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        If Me.mainWindow Is Nothing Then
            Throw New InvalidOperationException($"{NameOf(Running_Applications)} must be created with its main window.")
        End If

        Me.ApplyTheme()
        Me.Location = New Point(Me.mainWindow.Right, Me.mainWindow.Top)
        Me.LoadListView()
        NativeMethods.AnimateWindow(Me.Handle, Running_Applications.FadeDuration, AnimateWindowFlags.Blend)
    End Sub

    ''' <summary>
    ''' Handles the <see cref="Form.FormClosing"/> event of the form.
    ''' </summary>
    Private Sub Running_Applications_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing

        If Me.Visible Then
            NativeMethods.AnimateWindow(Me.Handle, Running_Applications.FadeDuration, AnimateWindowFlags.Blend Or AnimateWindowFlags.Hide)
        End If
    End Sub

    ''' <summary>
    ''' Handles the <see cref="Form.FormClosed"/> event of the form.
    ''' </summary>
    Private Sub Running_Applications_FormClosed(sender As Object, e As FormClosedEventArgs) Handles MyBase.FormClosed
        RemoveHandler AppThemeService.ThemeChanged, AddressOf Me.AppThemeService_ThemeChanged
    End Sub

    ''' <summary>
    ''' Handles the <see cref="Control.Move"/> event of the form, to keep the main window docked at the left.
    ''' </summary>
    Private Sub Running_Applications_Move(sender As Object, e As EventArgs) Handles MyBase.Move

        If Me.mainWindow IsNot Nothing AndAlso Me.Visible Then
            Me.mainWindow.FollowSecondaryWindow(Me)
        End If
    End Sub

    ''' <summary>
    ''' Handles the <see cref="AppThemeService.ThemeChanged"/> event.
    ''' </summary>
    Private Sub AppThemeService_ThemeChanged(sender As Object, e As EventArgs)
        Me.ApplyTheme()
    End Sub

#End Region

#Region " Control Events "

    ''' <summary>
    ''' Handles the <see cref="CButtonLib.CButton.ClickButtonArea"/> event of the "Select none" button.
    ''' </summary>
    Private Sub Button_Select_None_ClickButtonArea(sender As Object, e As MouseEventArgs) Handles Button_Select_None.ClickButtonArea

        Me.ListView_Running_Applications.BeginUpdate()
        Try
            For Each item As ListViewItem In Me.ListView_Running_Applications.CheckedItems.Cast(Of ListViewItem)().ToArray()
                item.Checked = False
            Next
        Finally
            Me.ListView_Running_Applications.EndUpdate()
        End Try
    End Sub

    ''' <summary>
    ''' Handles the <see cref="CButtonLib.CButton.ClickButtonArea"/> event of the "Refresh" button.
    ''' </summary>
    Private Sub Button_Refresh_ClickButtonArea(sender As Object, e As MouseEventArgs) Handles Button_Refresh.ClickButtonArea
        Me.LoadListView()
    End Sub

    ''' <summary>
    ''' Handles the <see cref="CButtonLib.CButton.ClickButtonArea"/> event of the "Close" button.
    ''' </summary>
    Private Sub Button_Close_ClickButtonArea(sender As Object, e As MouseEventArgs) Handles Button_Close.ClickButtonArea
        Me.Close()
    End Sub

    ''' <summary>
    ''' Handles the <see cref="ListView.ItemCheck"/> event of the list, adding or removing the application name.
    ''' </summary>
    Private Sub ListView_Running_Applications_ItemCheck(sender As Object, e As ItemCheckEventArgs) Handles ListView_Running_Applications.ItemCheck

        If Me.isPopulating Then
            Return
        End If

        Dim executableName As String = Me.ListView_Running_Applications.Items(e.Index).Text
        Me.mainWindow.ApplicationListText = If(e.NewValue = CheckState.Checked,
                                               ProcessNameList.Add(Me.mainWindow.ApplicationListText, executableName),
                                               ProcessNameList.Remove(Me.mainWindow.ApplicationListText, executableName))
    End Sub

    ''' <summary>
    ''' Handles the <see cref="ListView.ColumnWidthChanging"/> event of the list, to prevent resizing the column.
    ''' </summary>
    Private Sub ListView_Running_Applications_ColumnWidthChanging(sender As Object, e As ColumnWidthChangingEventArgs) Handles ListView_Running_Applications.ColumnWidthChanging

        e.Cancel = True
        e.NewWidth = Me.ListView_Running_Applications.Columns(e.ColumnIndex).Width
    End Sub

#End Region

#Region " Private Methods "

    ''' <summary>
    ''' Fills the list with the distinct names of the running processes, checking the monitored ones.
    ''' </summary>
    Private Sub LoadListView()

        Dim executableNames As New SortedSet(Of String)(StringComparer.OrdinalIgnoreCase)
        For Each runningProcess As Process In Process.GetProcesses()
            Using runningProcess
                executableNames.Add($"{runningProcess.ProcessName}{ProcessNameList.ExecutableExtension}")
            End Using
        Next

        Dim monitoredText As String = Me.mainWindow.ApplicationListText
        Me.isPopulating = True
        Me.ListView_Running_Applications.BeginUpdate()
        Try
            Me.ListView_Running_Applications.Items.Clear()
            For Each executableName As String In executableNames
                Dim item As New ListViewItem(executableName) With {
                    .Checked = ProcessNameList.Contains(monitoredText, executableName)
                }
                Me.ListView_Running_Applications.Items.Add(item)
            Next
        Finally
            Me.ListView_Running_Applications.EndUpdate()
            Me.isPopulating = False
        End Try
    End Sub

    ''' <summary>
    ''' Applies the current theme to this window and its title bar.
    ''' </summary>
    Private Sub ApplyTheme()

        Dim palette As ThemePalette = AppThemeService.CurrentPalette
        ThemeManager.Apply(Me, palette)
        If Me.IsHandleCreated Then
            DwmTitleBar.SetDarkMode(Me, palette.IsDark)
        End If
    End Sub

#End Region

End Class
