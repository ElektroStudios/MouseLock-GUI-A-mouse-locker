Option Strict On
Option Explicit On
Option Infer Off

Namespace Core

    ''' <summary>
    ''' Specifies what automatically starts a mouse lock.
    ''' </summary>
    ''' <remarks>
    ''' The numeric values are persisted in the settings file ("LockType" key) and are also
    ''' stored in the <see cref="System.Windows.Forms.Control.Tag"/> of the lock type radio buttons,
    ''' so they must never change.
    ''' </remarks>
    Friend Enum LockTrigger As Integer

        ''' <summary>No automatic lock; the user locks the mouse manually.</summary>
        None = 0

        ''' <summary>Lock the mouse when the application starts.</summary>
        Startup = 1

        ''' <summary>Lock the mouse at a certain time of the day.</summary>
        ScheduledTime = 2

        ''' <summary>Lock the mouse while any of the specified applications is running.</summary>
        ApplicationRunning = 3

    End Enum

    ''' <summary>
    ''' Specifies what to do when the monitored application ends.
    ''' </summary>
    ''' <remarks>
    ''' The numeric values are persisted in the settings file ("IfAPPNotRunning" key),
    ''' and are equal to the combo box item index plus one, so they must never change.
    ''' </remarks>
    Friend Enum ApplicationExitAction As Integer

        ''' <summary>Unlock the mouse.</summary>
        UnlockMouse = 1

        ''' <summary>Close MouseLock.</summary>
        ExitMouseLock = 2

    End Enum

End Namespace
