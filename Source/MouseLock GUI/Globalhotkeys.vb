#Region " GlobalHotkeys Class "

Class Shortcut
    Inherits NativeWindow
    Implements IDisposable

#Region " GlobalHotkeys Declarations "
    Protected Declare Function UnregisterHotKey Lib "user32.dll" (ByVal handle As IntPtr, ByVal id As Integer) As Boolean
    Protected Declare Function RegisterHotKey Lib "user32.dll" (ByVal handle As IntPtr, ByVal id As Integer, ByVal modifier As Integer, ByVal vk As Integer) As Boolean

    Event Press(ByVal sender As Object, ByVal e As HotKeyEventArgs)
    Protected EventArgs As HotKeyEventArgs, ID As Integer

    Enum Modifier As Integer
        None = 0
        Alt = 1
        Ctrl = 2
        Shift = 4
    End Enum
    Class HotKeyEventArgs
        Inherits EventArgs
        Property Modifier As Shortcut.Modifier
        Property Key As Keys
    End Class
    Class RegisteredException
        Inherits Exception
        Protected Const s As String = "Shortcut combination is in use."
        Sub New()
            MyBase.New(s)
        End Sub
    End Class
#End Region

#Region " GlobalHotkeys IDisposable "
    Private disposed As Boolean
    Protected Overridable Sub Dispose(ByVal disposing As Boolean)
        If Not Me.disposed Then UnregisterHotKey(Me.Handle, Me.ID)
        Me.disposed = True
    End Sub
    Protected Overrides Sub Finalize()
        Me.Dispose(False)
        MyBase.Finalize()
    End Sub
    Sub Dispose() Implements IDisposable.Dispose
        Me.Dispose(True)
        GC.SuppressFinalize(Me)
    End Sub
#End Region

#Region " GlobalHotkeys debugger "
    <DebuggerStepperBoundary()>
    Sub New(ByVal modifier As Modifier, ByVal key As Keys)
        Me.CreateHandle(New CreateParams)
        Me.ID = Me.GetHashCode()
        Me.EventArgs = New HotKeyEventArgs With {.Key = key, .Modifier = modifier}
        If Not RegisterHotKey(Me.Handle, Me.ID, modifier, key) Then Throw New RegisteredException
    End Sub
    Shared Function Create(ByVal modifier As Modifier, ByVal key As Keys) As Shortcut
        Return New Shortcut(modifier, key)
    End Function

    Protected Sub New()
    End Sub
    Protected Overrides Sub WndProc(ByRef m As Message)
        Select Case m.Msg
            Case 786
                RaiseEvent Press(Me, Me.EventArgs)
            Case Else
                MyBase.WndProc(m)
        End Select
    End Sub
#End Region

End Class

#End Region
