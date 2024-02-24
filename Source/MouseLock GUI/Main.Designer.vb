<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Main
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(Main))
        Dim TimeColors1 As gTimePickerControl.TimeColors = New gTimePickerControl.TimeColors()
        Dim DesignerRectTracker1 As CButtonLib.DesignerRectTracker = New CButtonLib.DesignerRectTracker()
        Dim CBlendItems1 As CButtonLib.cBlendItems = New CButtonLib.cBlendItems()
        Dim DesignerRectTracker2 As CButtonLib.DesignerRectTracker = New CButtonLib.DesignerRectTracker()
        Dim DesignerRectTracker3 As CButtonLib.DesignerRectTracker = New CButtonLib.DesignerRectTracker()
        Dim CBlendItems2 As CButtonLib.cBlendItems = New CButtonLib.cBlendItems()
        Dim DesignerRectTracker4 As CButtonLib.DesignerRectTracker = New CButtonLib.DesignerRectTracker()
        Me.CheckBox_ToggleKey = New System.Windows.Forms.CheckBox()
        Me.TextBox_Application_Name = New System.Windows.Forms.TextBox()
        Me.Label_Application_Name = New System.Windows.Forms.Label()
        Me.ComboBox_ToggleKey_SpecialKey = New System.Windows.Forms.ComboBox()
        Me.ComboBox_ToggleKey_NormalKey = New System.Windows.Forms.ComboBox()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.Label_Application_Not_Running = New System.Windows.Forms.Label()
        Me.CheckBox_Remember_settings = New System.Windows.Forms.CheckBox()
        Me.RadioButton_Lock_Application = New System.Windows.Forms.RadioButton()
        Me.RadioButton_Lock_Time = New System.Windows.Forms.RadioButton()
        Me.RadioButton_Lock_Startup = New System.Windows.Forms.RadioButton()
        Me.ComboBox_Application_Not_Running = New System.Windows.Forms.ComboBox()
        Me.CheckBox_SysTray = New System.Windows.Forms.CheckBox()
        Me.ContextMenuStrip_SysTray = New System.Windows.Forms.ContextMenuStrip(Me.components)
        Me.ToolStrip_Lock = New System.Windows.Forms.ToolStripMenuItem()
        Me.ToolStrip_Unlock = New System.Windows.Forms.ToolStripMenuItem()
        Me.ToolStrip_Restore = New System.Windows.Forms.ToolStripMenuItem()
        Me.ToolStrip_Hide = New System.Windows.Forms.ToolStripMenuItem()
        Me.ToolStrip_Exit = New System.Windows.Forms.ToolStripMenuItem()
        Me.NotifyIcon_SysTray = New System.Windows.Forms.NotifyIcon(Me.components)
        Me.Label_Delay = New System.Windows.Forms.Label()
        Me.Label_Interval = New System.Windows.Forms.Label()
        Me.Numeric_Interval = New System.Windows.Forms.NumericUpDown()
        Me.Numeric_Delay = New System.Windows.Forms.NumericUpDown()
        Me.Panel_Lock_Type = New System.Windows.Forms.Panel()
        Me.GTimePicker1 = New gTimePickerControl.gTimePicker()
        Me.Button_Running_applications = New CButtonLib.CButton()
        Me.Label_S = New System.Windows.Forms.Label()
        Me.Label_MS = New System.Windows.Forms.Label()
        Me.Button_Lock_Unlock = New CButtonLib.CButton()
        Me.PictureBox_Logo = New System.Windows.Forms.PictureBox()
        Me.ContextMenuStrip_SysTray.SuspendLayout()
        CType(Me.Numeric_Interval, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.Numeric_Delay, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.Panel_Lock_Type.SuspendLayout()
        CType(Me.PictureBox_Logo, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'CheckBox_ToggleKey
        '
        Me.CheckBox_ToggleKey.AutoSize = True
        Me.CheckBox_ToggleKey.BackColor = System.Drawing.Color.Transparent
        Me.CheckBox_ToggleKey.Cursor = System.Windows.Forms.Cursors.Hand
        Me.CheckBox_ToggleKey.ForeColor = System.Drawing.Color.Gainsboro
        Me.CheckBox_ToggleKey.Location = New System.Drawing.Point(20, 232)
        Me.CheckBox_ToggleKey.Name = "CheckBox_ToggleKey"
        Me.CheckBox_ToggleKey.Size = New System.Drawing.Size(182, 17)
        Me.CheckBox_ToggleKey.TabIndex = 2
        Me.CheckBox_ToggleKey.Text = "Activate Enable/Disable hotkey :"
        Me.CheckBox_ToggleKey.UseVisualStyleBackColor = False
        '
        'TextBox_Application_Name
        '
        Me.TextBox_Application_Name.BackColor = System.Drawing.Color.Gainsboro
        Me.TextBox_Application_Name.Enabled = False
        Me.TextBox_Application_Name.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.TextBox_Application_Name.ForeColor = System.Drawing.Color.Black
        Me.TextBox_Application_Name.Location = New System.Drawing.Point(153, 78)
        Me.TextBox_Application_Name.Name = "TextBox_Application_Name"
        Me.TextBox_Application_Name.Size = New System.Drawing.Size(145, 20)
        Me.TextBox_Application_Name.TabIndex = 3
        Me.TextBox_Application_Name.Text = "Example1.exe;Example2.exe"
        '
        'Label_Application_Name
        '
        Me.Label_Application_Name.AutoSize = True
        Me.Label_Application_Name.BackColor = System.Drawing.Color.Transparent
        Me.Label_Application_Name.Enabled = False
        Me.Label_Application_Name.ForeColor = System.Drawing.Color.Gainsboro
        Me.Label_Application_Name.Location = New System.Drawing.Point(25, 81)
        Me.Label_Application_Name.Name = "Label_Application_Name"
        Me.Label_Application_Name.Size = New System.Drawing.Size(99, 13)
        Me.Label_Application_Name.TabIndex = 4
        Me.Label_Application_Name.Text = "Application names :"
        '
        'ComboBox_ToggleKey_SpecialKey
        '
        Me.ComboBox_ToggleKey_SpecialKey.BackColor = System.Drawing.Color.FromArgb(CType(CType(0, Byte), Integer), CType(CType(74, Byte), Integer), CType(CType(121, Byte), Integer))
        Me.ComboBox_ToggleKey_SpecialKey.Cursor = System.Windows.Forms.Cursors.Hand
        Me.ComboBox_ToggleKey_SpecialKey.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.ComboBox_ToggleKey_SpecialKey.Enabled = False
        Me.ComboBox_ToggleKey_SpecialKey.ForeColor = System.Drawing.Color.Gainsboro
        Me.ComboBox_ToggleKey_SpecialKey.FormattingEnabled = True
        Me.ComboBox_ToggleKey_SpecialKey.Items.AddRange(New Object() {"NONE", "ALT", "CTRL", "SHIFT"})
        Me.ComboBox_ToggleKey_SpecialKey.Location = New System.Drawing.Point(200, 230)
        Me.ComboBox_ToggleKey_SpecialKey.Name = "ComboBox_ToggleKey_SpecialKey"
        Me.ComboBox_ToggleKey_SpecialKey.Size = New System.Drawing.Size(55, 21)
        Me.ComboBox_ToggleKey_SpecialKey.TabIndex = 5
        '
        'ComboBox_ToggleKey_NormalKey
        '
        Me.ComboBox_ToggleKey_NormalKey.BackColor = System.Drawing.Color.FromArgb(CType(CType(0, Byte), Integer), CType(CType(74, Byte), Integer), CType(CType(121, Byte), Integer))
        Me.ComboBox_ToggleKey_NormalKey.Cursor = System.Windows.Forms.Cursors.Hand
        Me.ComboBox_ToggleKey_NormalKey.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.ComboBox_ToggleKey_NormalKey.Enabled = False
        Me.ComboBox_ToggleKey_NormalKey.ForeColor = System.Drawing.Color.Gainsboro
        Me.ComboBox_ToggleKey_NormalKey.FormattingEnabled = True
        Me.ComboBox_ToggleKey_NormalKey.Items.AddRange(New Object() {"A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z", "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "F1", "F2", "F3", "F4", "F5", "F6", "F7", "F8", "F9", "F10", "F11", "F12"})
        Me.ComboBox_ToggleKey_NormalKey.Location = New System.Drawing.Point(268, 230)
        Me.ComboBox_ToggleKey_NormalKey.Name = "ComboBox_ToggleKey_NormalKey"
        Me.ComboBox_ToggleKey_NormalKey.Size = New System.Drawing.Size(43, 21)
        Me.ComboBox_ToggleKey_NormalKey.TabIndex = 6
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.BackColor = System.Drawing.Color.Transparent
        Me.Label2.ForeColor = System.Drawing.Color.Gainsboro
        Me.Label2.Location = New System.Drawing.Point(255, 233)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(13, 13)
        Me.Label2.TabIndex = 7
        Me.Label2.Text = "+"
        '
        'Label_Application_Not_Running
        '
        Me.Label_Application_Not_Running.AutoSize = True
        Me.Label_Application_Not_Running.BackColor = System.Drawing.Color.Transparent
        Me.Label_Application_Not_Running.Enabled = False
        Me.Label_Application_Not_Running.ForeColor = System.Drawing.Color.Gainsboro
        Me.Label_Application_Not_Running.Location = New System.Drawing.Point(25, 108)
        Me.Label_Application_Not_Running.Name = "Label_Application_Not_Running"
        Me.Label_Application_Not_Running.Size = New System.Drawing.Size(125, 13)
        Me.Label_Application_Not_Running.TabIndex = 10
        Me.Label_Application_Not_Running.Text = "When application ends..."
        '
        'CheckBox_Remember_settings
        '
        Me.CheckBox_Remember_settings.AutoSize = True
        Me.CheckBox_Remember_settings.BackColor = System.Drawing.Color.Transparent
        Me.CheckBox_Remember_settings.Cursor = System.Windows.Forms.Cursors.Hand
        Me.CheckBox_Remember_settings.ForeColor = System.Drawing.Color.Gainsboro
        Me.CheckBox_Remember_settings.Location = New System.Drawing.Point(200, 287)
        Me.CheckBox_Remember_settings.Name = "CheckBox_Remember_settings"
        Me.CheckBox_Remember_settings.Size = New System.Drawing.Size(116, 17)
        Me.CheckBox_Remember_settings.TabIndex = 12
        Me.CheckBox_Remember_settings.Text = "Remember settings"
        Me.CheckBox_Remember_settings.UseVisualStyleBackColor = False
        '
        'RadioButton_Lock_Application
        '
        Me.RadioButton_Lock_Application.AutoSize = True
        Me.RadioButton_Lock_Application.BackColor = System.Drawing.Color.Transparent
        Me.RadioButton_Lock_Application.Cursor = System.Windows.Forms.Cursors.Hand
        Me.RadioButton_Lock_Application.ForeColor = System.Drawing.Color.Gainsboro
        Me.RadioButton_Lock_Application.Location = New System.Drawing.Point(7, 55)
        Me.RadioButton_Lock_Application.Name = "RadioButton_Lock_Application"
        Me.RadioButton_Lock_Application.Size = New System.Drawing.Size(232, 17)
        Me.RadioButton_Lock_Application.TabIndex = 13
        Me.RadioButton_Lock_Application.TabStop = True
        Me.RadioButton_Lock_Application.Tag = "3"
        Me.RadioButton_Lock_Application.Text = "Lock the mouse when application is running"
        Me.RadioButton_Lock_Application.UseVisualStyleBackColor = False
        '
        'RadioButton_Lock_Time
        '
        Me.RadioButton_Lock_Time.AutoSize = True
        Me.RadioButton_Lock_Time.BackColor = System.Drawing.Color.Transparent
        Me.RadioButton_Lock_Time.Cursor = System.Windows.Forms.Cursors.Hand
        Me.RadioButton_Lock_Time.ForeColor = System.Drawing.Color.Gainsboro
        Me.RadioButton_Lock_Time.Location = New System.Drawing.Point(7, 32)
        Me.RadioButton_Lock_Time.Name = "RadioButton_Lock_Time"
        Me.RadioButton_Lock_Time.Size = New System.Drawing.Size(170, 17)
        Me.RadioButton_Lock_Time.TabIndex = 14
        Me.RadioButton_Lock_Time.TabStop = True
        Me.RadioButton_Lock_Time.Tag = "2"
        Me.RadioButton_Lock_Time.Text = "Lock the mouse at certain time"
        Me.RadioButton_Lock_Time.UseVisualStyleBackColor = False
        '
        'RadioButton_Lock_Startup
        '
        Me.RadioButton_Lock_Startup.AutoSize = True
        Me.RadioButton_Lock_Startup.BackColor = System.Drawing.Color.Transparent
        Me.RadioButton_Lock_Startup.Cursor = System.Windows.Forms.Cursors.Hand
        Me.RadioButton_Lock_Startup.ForeColor = System.Drawing.Color.Gainsboro
        Me.RadioButton_Lock_Startup.Location = New System.Drawing.Point(7, 9)
        Me.RadioButton_Lock_Startup.Name = "RadioButton_Lock_Startup"
        Me.RadioButton_Lock_Startup.Size = New System.Drawing.Size(148, 17)
        Me.RadioButton_Lock_Startup.TabIndex = 15
        Me.RadioButton_Lock_Startup.TabStop = True
        Me.RadioButton_Lock_Startup.Tag = "1"
        Me.RadioButton_Lock_Startup.Text = "Lock the mouse at startup"
        Me.RadioButton_Lock_Startup.UseVisualStyleBackColor = False
        '
        'ComboBox_Application_Not_Running
        '
        Me.ComboBox_Application_Not_Running.BackColor = System.Drawing.Color.FromArgb(CType(CType(0, Byte), Integer), CType(CType(74, Byte), Integer), CType(CType(121, Byte), Integer))
        Me.ComboBox_Application_Not_Running.Cursor = System.Windows.Forms.Cursors.Hand
        Me.ComboBox_Application_Not_Running.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.ComboBox_Application_Not_Running.Enabled = False
        Me.ComboBox_Application_Not_Running.ForeColor = System.Drawing.Color.Gainsboro
        Me.ComboBox_Application_Not_Running.FormattingEnabled = True
        Me.ComboBox_Application_Not_Running.Items.AddRange(New Object() {"Unlock mouse", "Kill MouseLock"})
        Me.ComboBox_Application_Not_Running.Location = New System.Drawing.Point(153, 104)
        Me.ComboBox_Application_Not_Running.Name = "ComboBox_Application_Not_Running"
        Me.ComboBox_Application_Not_Running.Size = New System.Drawing.Size(145, 21)
        Me.ComboBox_Application_Not_Running.TabIndex = 16
        '
        'CheckBox_SysTray
        '
        Me.CheckBox_SysTray.AutoSize = True
        Me.CheckBox_SysTray.BackColor = System.Drawing.Color.Transparent
        Me.CheckBox_SysTray.Cursor = System.Windows.Forms.Cursors.Hand
        Me.CheckBox_SysTray.ForeColor = System.Drawing.Color.Gainsboro
        Me.CheckBox_SysTray.Location = New System.Drawing.Point(200, 260)
        Me.CheckBox_SysTray.Name = "CheckBox_SysTray"
        Me.CheckBox_SysTray.Size = New System.Drawing.Size(113, 17)
        Me.CheckBox_SysTray.TabIndex = 18
        Me.CheckBox_SysTray.Text = "Minimize at startup"
        Me.CheckBox_SysTray.UseVisualStyleBackColor = False
        '
        'ContextMenuStrip_SysTray
        '
        Me.ContextMenuStrip_SysTray.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.ToolStrip_Lock, Me.ToolStrip_Unlock, Me.ToolStrip_Restore, Me.ToolStrip_Hide, Me.ToolStrip_Exit})
        Me.ContextMenuStrip_SysTray.Name = "ContextMenuStrip1"
        Me.ContextMenuStrip_SysTray.Size = New System.Drawing.Size(114, 114)
        '
        'ToolStrip_Lock
        '
        Me.ToolStrip_Lock.Image = Global.MouseLock.My.Resources.Resources.Lock
        Me.ToolStrip_Lock.Name = "ToolStrip_Lock"
        Me.ToolStrip_Lock.Size = New System.Drawing.Size(113, 22)
        Me.ToolStrip_Lock.Text = "Lock"
        '
        'ToolStrip_Unlock
        '
        Me.ToolStrip_Unlock.Image = Global.MouseLock.My.Resources.Resources.Unlock
        Me.ToolStrip_Unlock.Name = "ToolStrip_Unlock"
        Me.ToolStrip_Unlock.Size = New System.Drawing.Size(113, 22)
        Me.ToolStrip_Unlock.Text = "Unlock"
        '
        'ToolStrip_Restore
        '
        Me.ToolStrip_Restore.Image = Global.MouseLock.My.Resources.Resources.Restore
        Me.ToolStrip_Restore.Name = "ToolStrip_Restore"
        Me.ToolStrip_Restore.Size = New System.Drawing.Size(113, 22)
        Me.ToolStrip_Restore.Text = "Restore"
        '
        'ToolStrip_Hide
        '
        Me.ToolStrip_Hide.Image = Global.MouseLock.My.Resources.Resources.Hide
        Me.ToolStrip_Hide.Name = "ToolStrip_Hide"
        Me.ToolStrip_Hide.Size = New System.Drawing.Size(113, 22)
        Me.ToolStrip_Hide.Text = "Hide"
        '
        'ToolStrip_Exit
        '
        Me.ToolStrip_Exit.Image = Global.MouseLock.My.Resources.Resources._Exit
        Me.ToolStrip_Exit.Name = "ToolStrip_Exit"
        Me.ToolStrip_Exit.Size = New System.Drawing.Size(113, 22)
        Me.ToolStrip_Exit.Text = "Close"
        '
        'NotifyIcon_SysTray
        '
        Me.NotifyIcon_SysTray.ContextMenuStrip = Me.ContextMenuStrip_SysTray
        Me.NotifyIcon_SysTray.Icon = CType(resources.GetObject("NotifyIcon_SysTray.Icon"), System.Drawing.Icon)
        Me.NotifyIcon_SysTray.Text = "MouseLock"
        Me.NotifyIcon_SysTray.Visible = True
        '
        'Label_Delay
        '
        Me.Label_Delay.AutoSize = True
        Me.Label_Delay.BackColor = System.Drawing.Color.Transparent
        Me.Label_Delay.ForeColor = System.Drawing.Color.Gainsboro
        Me.Label_Delay.Location = New System.Drawing.Point(17, 260)
        Me.Label_Delay.Name = "Label_Delay"
        Me.Label_Delay.Size = New System.Drawing.Size(65, 13)
        Me.Label_Delay.TabIndex = 24
        Me.Label_Delay.Text = "Initial delay :"
        '
        'Label_Interval
        '
        Me.Label_Interval.AutoSize = True
        Me.Label_Interval.BackColor = System.Drawing.Color.Transparent
        Me.Label_Interval.ForeColor = System.Drawing.Color.Gainsboro
        Me.Label_Interval.Location = New System.Drawing.Point(17, 287)
        Me.Label_Interval.Name = "Label_Interval"
        Me.Label_Interval.Size = New System.Drawing.Size(65, 13)
        Me.Label_Interval.TabIndex = 25
        Me.Label_Interval.Text = "Lock delay :"
        '
        'Numeric_Interval
        '
        Me.Numeric_Interval.BackColor = System.Drawing.Color.Gainsboro
        Me.Numeric_Interval.ForeColor = System.Drawing.Color.Black
        Me.Numeric_Interval.Location = New System.Drawing.Point(82, 284)
        Me.Numeric_Interval.Maximum = New Decimal(New Integer() {9999, 0, 0, 0})
        Me.Numeric_Interval.Name = "Numeric_Interval"
        Me.Numeric_Interval.Size = New System.Drawing.Size(56, 20)
        Me.Numeric_Interval.TabIndex = 26
        Me.Numeric_Interval.Value = New Decimal(New Integer() {10, 0, 0, 0})
        '
        'Numeric_Delay
        '
        Me.Numeric_Delay.BackColor = System.Drawing.Color.Gainsboro
        Me.Numeric_Delay.ForeColor = System.Drawing.Color.Black
        Me.Numeric_Delay.Location = New System.Drawing.Point(82, 257)
        Me.Numeric_Delay.Maximum = New Decimal(New Integer() {9999999, 0, 0, 0})
        Me.Numeric_Delay.Name = "Numeric_Delay"
        Me.Numeric_Delay.Size = New System.Drawing.Size(56, 20)
        Me.Numeric_Delay.TabIndex = 27
        '
        'Panel_Lock_Type
        '
        Me.Panel_Lock_Type.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.Panel_Lock_Type.Controls.Add(Me.GTimePicker1)
        Me.Panel_Lock_Type.Controls.Add(Me.Button_Running_applications)
        Me.Panel_Lock_Type.Controls.Add(Me.RadioButton_Lock_Startup)
        Me.Panel_Lock_Type.Controls.Add(Me.TextBox_Application_Name)
        Me.Panel_Lock_Type.Controls.Add(Me.Label_Application_Name)
        Me.Panel_Lock_Type.Controls.Add(Me.ComboBox_Application_Not_Running)
        Me.Panel_Lock_Type.Controls.Add(Me.Label_Application_Not_Running)
        Me.Panel_Lock_Type.Controls.Add(Me.RadioButton_Lock_Application)
        Me.Panel_Lock_Type.Controls.Add(Me.RadioButton_Lock_Time)
        Me.Panel_Lock_Type.Location = New System.Drawing.Point(12, 79)
        Me.Panel_Lock_Type.Name = "Panel_Lock_Type"
        Me.Panel_Lock_Type.Size = New System.Drawing.Size(310, 138)
        Me.Panel_Lock_Type.TabIndex = 30
        '
        'GTimePicker1
        '
        Me.GTimePicker1.ButtonForeColor = System.Drawing.Color.Black
        Me.GTimePicker1.Cursor = System.Windows.Forms.Cursors.Default
        Me.GTimePicker1.Hr24 = True
        Me.GTimePicker1.Location = New System.Drawing.Point(183, 27)
        Me.GTimePicker1.Name = "GTimePicker1"
        Me.GTimePicker1.NullColorA = System.Drawing.Color.LightSteelBlue
        Me.GTimePicker1.NullColorB = System.Drawing.Color.White
        Me.GTimePicker1.NullHatchStyle = System.Drawing.Drawing2D.HatchStyle.WideDownwardDiagonal
        Me.GTimePicker1.NullTextColor = System.Drawing.Color.Black
        Me.GTimePicker1.NullTextInFront = False
        Me.GTimePicker1.ShowMidMins = True
        Me.GTimePicker1.Size = New System.Drawing.Size(75, 23)
        Me.GTimePicker1.TabIndex = 17
        Me.GTimePicker1.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        Me.GTimePicker1.TextBackColor = System.Drawing.Color.White
        Me.GTimePicker1.TextFont = New System.Drawing.Font("Arial", 10.0!)
        Me.GTimePicker1.TextForeColor = System.Drawing.Color.Black
        Me.GTimePicker1.Time = "00:00"
        Me.GTimePicker1.TimeAMPM = gTimePickerControl.gTimePickerCntrl.eTimeAMPM.AM
        TimeColors1.BackGround = System.Drawing.Color.White
        TimeColors1.Box = System.Drawing.Color.White
        TimeColors1.DisplayTime = System.Drawing.Color.Red
        TimeColors1.FaceInner = System.Drawing.Color.White
        TimeColors1.FaceOuter = System.Drawing.Color.LightGoldenrodYellow
        TimeColors1.FrameInner = System.Drawing.Color.AliceBlue
        TimeColors1.FrameOuter = System.Drawing.Color.CornflowerBlue
        TimeColors1.Hour = System.Drawing.Color.DarkBlue
        TimeColors1.HourHand = System.Drawing.Color.DarkBlue
        TimeColors1.Minute = System.Drawing.Color.Blue
        TimeColors1.MinuteHand = System.Drawing.Color.OrangeRed
        TimeColors1.MinutePlus = System.Drawing.Color.LightSlateGray
        TimeColors1.TimeAMPM_OFF = System.Drawing.Color.LightSteelBlue
        TimeColors1.TimeAMPM_ON = System.Drawing.Color.MediumBlue
        Me.GTimePicker1.TimeColors = TimeColors1
        Me.GTimePicker1.TrueHour = True
        '
        'Button_Running_applications
        '
        Me.Button_Running_applications.BorderColor = System.Drawing.Color.Transparent
        DesignerRectTracker1.IsActive = True
        DesignerRectTracker1.TrackerRectangle = CType(resources.GetObject("DesignerRectTracker1.TrackerRectangle"), System.Drawing.RectangleF)
        Me.Button_Running_applications.CenterPtTracker = DesignerRectTracker1
        CBlendItems1.iColor = New System.Drawing.Color() {System.Drawing.Color.Transparent, System.Drawing.Color.Transparent, System.Drawing.Color.Transparent}
        CBlendItems1.iPoint = New Single() {0!, 0.5!, 1.0!}
        Me.Button_Running_applications.ColorFillBlend = CBlendItems1
        Me.Button_Running_applications.Corners.All = 2
        Me.Button_Running_applications.Corners.LowerLeft = 2
        Me.Button_Running_applications.Corners.LowerRight = 2
        Me.Button_Running_applications.Corners.UpperLeft = 2
        Me.Button_Running_applications.Corners.UpperRight = 2
        Me.Button_Running_applications.Cursor = System.Windows.Forms.Cursors.Hand
        Me.Button_Running_applications.FocalPoints.CenterPtX = 0.3125!
        Me.Button_Running_applications.FocalPoints.CenterPtY = 0.6!
        DesignerRectTracker2.IsActive = False
        DesignerRectTracker2.TrackerRectangle = CType(resources.GetObject("DesignerRectTracker2.TrackerRectangle"), System.Drawing.RectangleF)
        Me.Button_Running_applications.FocusPtTracker = DesignerRectTracker2
        Me.Button_Running_applications.Image = Global.MouseLock.My.Resources.Resources.Running_Applications
        Me.Button_Running_applications.ImageIndex = 0
        Me.Button_Running_applications.ImageSize = New System.Drawing.Size(24, 24)
        Me.Button_Running_applications.Location = New System.Drawing.Point(120, 78)
        Me.Button_Running_applications.Name = "Button_Running_applications"
        Me.Button_Running_applications.Size = New System.Drawing.Size(32, 20)
        Me.Button_Running_applications.TabIndex = 34
        Me.Button_Running_applications.Text = ""
        '
        'Label_S
        '
        Me.Label_S.AutoSize = True
        Me.Label_S.BackColor = System.Drawing.Color.Transparent
        Me.Label_S.ForeColor = System.Drawing.Color.Gainsboro
        Me.Label_S.Location = New System.Drawing.Point(140, 260)
        Me.Label_S.Name = "Label_S"
        Me.Label_S.Size = New System.Drawing.Size(12, 13)
        Me.Label_S.TabIndex = 31
        Me.Label_S.Text = "s"
        '
        'Label_MS
        '
        Me.Label_MS.AutoSize = True
        Me.Label_MS.BackColor = System.Drawing.Color.Transparent
        Me.Label_MS.ForeColor = System.Drawing.Color.Gainsboro
        Me.Label_MS.Location = New System.Drawing.Point(140, 286)
        Me.Label_MS.Name = "Label_MS"
        Me.Label_MS.Size = New System.Drawing.Size(20, 13)
        Me.Label_MS.TabIndex = 32
        Me.Label_MS.Text = "ms"
        '
        'Button_Lock_Unlock
        '
        Me.Button_Lock_Unlock.BorderColor = System.Drawing.Color.Black
        DesignerRectTracker3.IsActive = False
        DesignerRectTracker3.TrackerRectangle = CType(resources.GetObject("DesignerRectTracker3.TrackerRectangle"), System.Drawing.RectangleF)
        Me.Button_Lock_Unlock.CenterPtTracker = DesignerRectTracker3
        CBlendItems2.iColor = New System.Drawing.Color() {System.Drawing.Color.Green, System.Drawing.Color.Green, System.Drawing.Color.Green}
        CBlendItems2.iPoint = New Single() {0!, 0.5!, 1.0!}
        Me.Button_Lock_Unlock.ColorFillBlend = CBlendItems2
        Me.Button_Lock_Unlock.Corners.All = 0
        Me.Button_Lock_Unlock.Corners.LowerLeft = 0
        Me.Button_Lock_Unlock.Corners.LowerRight = 0
        Me.Button_Lock_Unlock.Corners.UpperLeft = 0
        Me.Button_Lock_Unlock.Corners.UpperRight = 0
        Me.Button_Lock_Unlock.Cursor = System.Windows.Forms.Cursors.Hand
        Me.Button_Lock_Unlock.Dock = System.Windows.Forms.DockStyle.Bottom
        DesignerRectTracker4.IsActive = False
        DesignerRectTracker4.TrackerRectangle = CType(resources.GetObject("DesignerRectTracker4.TrackerRectangle"), System.Drawing.RectangleF)
        Me.Button_Lock_Unlock.FocusPtTracker = DesignerRectTracker4
        Me.Button_Lock_Unlock.Font = New System.Drawing.Font("Arial", 35.0!, System.Drawing.FontStyle.Bold)
        Me.Button_Lock_Unlock.Image = Global.MouseLock.My.Resources.Resources.Lock_64x64
        Me.Button_Lock_Unlock.ImageAlign = System.Drawing.ContentAlignment.TopRight
        Me.Button_Lock_Unlock.ImageIndex = 0
        Me.Button_Lock_Unlock.ImageSize = New System.Drawing.Size(42, 42)
        Me.Button_Lock_Unlock.Location = New System.Drawing.Point(0, 312)
        Me.Button_Lock_Unlock.Name = "Button_Lock_Unlock"
        Me.Button_Lock_Unlock.SideImage = Global.MouseLock.My.Resources.Resources.Lock_64x64
        Me.Button_Lock_Unlock.SideImageAlign = System.Drawing.ContentAlignment.TopLeft
        Me.Button_Lock_Unlock.SideImageSize = New System.Drawing.Size(42, 42)
        Me.Button_Lock_Unlock.Size = New System.Drawing.Size(334, 50)
        Me.Button_Lock_Unlock.TabIndex = 33
        Me.Button_Lock_Unlock.Text = "Lock now"
        '
        'PictureBox_Logo
        '
        Me.PictureBox_Logo.BackgroundImage = Global.MouseLock.My.Resources.Resources.Logo
        Me.PictureBox_Logo.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom
        Me.PictureBox_Logo.Dock = System.Windows.Forms.DockStyle.Top
        Me.PictureBox_Logo.ErrorImage = Nothing
        Me.PictureBox_Logo.Location = New System.Drawing.Point(0, 0)
        Me.PictureBox_Logo.Name = "PictureBox_Logo"
        Me.PictureBox_Logo.Size = New System.Drawing.Size(334, 70)
        Me.PictureBox_Logo.TabIndex = 21
        Me.PictureBox_Logo.TabStop = False
        '
        'Main
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.Color.FromArgb(CType(CType(0, Byte), Integer), CType(CType(74, Byte), Integer), CType(CType(121, Byte), Integer))
        Me.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center
        Me.ClientSize = New System.Drawing.Size(334, 362)
        Me.Controls.Add(Me.Button_Lock_Unlock)
        Me.Controls.Add(Me.Label_MS)
        Me.Controls.Add(Me.Label_S)
        Me.Controls.Add(Me.Label_Delay)
        Me.Controls.Add(Me.Numeric_Interval)
        Me.Controls.Add(Me.Label_Interval)
        Me.Controls.Add(Me.Panel_Lock_Type)
        Me.Controls.Add(Me.ComboBox_ToggleKey_SpecialKey)
        Me.Controls.Add(Me.ComboBox_ToggleKey_NormalKey)
        Me.Controls.Add(Me.Numeric_Delay)
        Me.Controls.Add(Me.PictureBox_Logo)
        Me.Controls.Add(Me.CheckBox_SysTray)
        Me.Controls.Add(Me.CheckBox_Remember_settings)
        Me.Controls.Add(Me.CheckBox_ToggleKey)
        Me.Controls.Add(Me.Label2)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.MaximizeBox = False
        Me.Name = "Main"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "MouseLock - By ElektroStudios "
        Me.ContextMenuStrip_SysTray.ResumeLayout(False)
        CType(Me.Numeric_Interval, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.Numeric_Delay, System.ComponentModel.ISupportInitialize).EndInit()
        Me.Panel_Lock_Type.ResumeLayout(False)
        Me.Panel_Lock_Type.PerformLayout()
        CType(Me.PictureBox_Logo, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents CheckBox_ToggleKey As System.Windows.Forms.CheckBox
    Friend WithEvents TextBox_Application_Name As System.Windows.Forms.TextBox
    Friend WithEvents Label_Application_Name As System.Windows.Forms.Label
    Friend WithEvents ComboBox_ToggleKey_SpecialKey As System.Windows.Forms.ComboBox
    Friend WithEvents ComboBox_ToggleKey_NormalKey As System.Windows.Forms.ComboBox
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents Label_Application_Not_Running As System.Windows.Forms.Label
    Friend WithEvents CheckBox_Remember_settings As System.Windows.Forms.CheckBox
    Friend WithEvents RadioButton_Lock_Application As System.Windows.Forms.RadioButton
    Friend WithEvents RadioButton_Lock_Time As System.Windows.Forms.RadioButton
    Friend WithEvents RadioButton_Lock_Startup As System.Windows.Forms.RadioButton
    Friend WithEvents ComboBox_Application_Not_Running As System.Windows.Forms.ComboBox
    Friend WithEvents CheckBox_SysTray As System.Windows.Forms.CheckBox
    Friend WithEvents ContextMenuStrip_SysTray As System.Windows.Forms.ContextMenuStrip
    Friend WithEvents NotifyIcon_SysTray As System.Windows.Forms.NotifyIcon
    Friend WithEvents PictureBox_Logo As System.Windows.Forms.PictureBox
    Friend WithEvents Label_Delay As System.Windows.Forms.Label
    Friend WithEvents Label_Interval As System.Windows.Forms.Label
    Friend WithEvents Numeric_Interval As System.Windows.Forms.NumericUpDown
    Friend WithEvents Numeric_Delay As System.Windows.Forms.NumericUpDown
    Friend WithEvents Panel_Lock_Type As System.Windows.Forms.Panel
    Friend WithEvents Label_S As System.Windows.Forms.Label
    Friend WithEvents Label_MS As System.Windows.Forms.Label
    Friend WithEvents GTimePicker1 As gTimePickerControl.gTimePicker
    Friend WithEvents ToolStrip_Lock As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ToolStrip_Unlock As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ToolStrip_Restore As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ToolStrip_Hide As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents Button_Lock_Unlock As CButtonLib.CButton
    Friend WithEvents Button_Running_applications As CButtonLib.CButton
    Friend WithEvents ToolStrip_Exit As System.Windows.Forms.ToolStripMenuItem

End Class
