Option Strict On
Option Explicit On
Option Infer Off

Namespace Core

    ''' <summary>
    ''' Specifies the state of a <see cref="LockSession"/>.
    ''' </summary>
    Friend Enum LockState As Integer

        ''' <summary>The mouse is not locked.</summary>
        Idle = 0

        ''' <summary>A lock was requested and the initial delay is elapsing.</summary>
        Delaying = 1

        ''' <summary>The mouse is locked at the center of the screen.</summary>
        Locked = 2

        ''' <summary>The lock is active but temporarily suspended by the toggle hot key.</summary>
        Paused = 3

    End Enum

    ''' <summary>
    ''' Deterministic state machine that models the life cycle of a mouse lock:
    ''' <see cref="LockState.Idle"/> → (<see cref="LockState.Delaying"/>) → <see cref="LockState.Locked"/> ⇄ <see cref="LockState.Paused"/> → <see cref="LockState.Idle"/>.
    ''' </summary>
    ''' <remarks>
    ''' This class contains no timers nor UI code; the owner form drives the transitions
    ''' and reacts to the resulting state. That keeps the behavior fully unit-testable.
    ''' </remarks>
    Friend NotInheritable Class LockSession

#Region " Properties "

        ''' <summary>
        ''' Gets the current state.
        ''' </summary>
        Friend ReadOnly Property State As LockState = LockState.Idle

        ''' <summary>
        ''' Gets a value indicating whether a lock is in progress (any state other than <see cref="LockState.Idle"/>).
        ''' </summary>
        Friend ReadOnly Property IsActive As Boolean
            Get
                Return Me.State <> LockState.Idle
            End Get
        End Property

        ''' <summary>
        ''' Gets a value indicating whether the mouse pointer must be kept at the center of the screen right now.
        ''' </summary>
        Friend ReadOnly Property ShouldConfineCursor As Boolean
            Get
                Return Me.State = LockState.Locked
            End Get
        End Property

#End Region

#Region " Events "

        ''' <summary>
        ''' Occurs when <see cref="LockSession.State"/> changes.
        ''' </summary>
        Friend Event StateChanged As EventHandler

#End Region

#Region " Public Methods "

        ''' <summary>
        ''' Starts a new lock.
        ''' </summary>
        ''' <param name="hasInitialDelay"><see langword="True"/> to enter <see cref="LockState.Delaying"/> first;
        ''' <see langword="False"/> to lock immediately.</param>
        ''' <returns>The new state.</returns>
        ''' <exception cref="InvalidOperationException">Thrown when a lock is already active.</exception>
        Friend Function Start(hasInitialDelay As Boolean) As LockState

            If Me.IsActive Then
                Throw New InvalidOperationException($"Cannot start a lock while the session is in the '{Me.State}' state.")
            End If

            Me.SetState(If(hasInitialDelay, LockState.Delaying, LockState.Locked))
            Return Me.State
        End Function

        ''' <summary>
        ''' Notifies that the initial delay has elapsed.
        ''' </summary>
        ''' <returns><see langword="True"/> if the session moved to <see cref="LockState.Locked"/>;
        ''' <see langword="False"/> if the session was no longer delaying (e.g. it was stopped meanwhile).</returns>
        Friend Function CompleteDelay() As Boolean

            If Me.State <> LockState.Delaying Then
                Return False
            End If

            Me.SetState(LockState.Locked)
            Return True
        End Function

        ''' <summary>
        ''' Toggles between <see cref="LockState.Locked"/> and <see cref="LockState.Paused"/>.
        ''' </summary>
        ''' <returns><see langword="True"/> if the state changed; <see langword="False"/> if the session
        ''' is <see cref="LockState.Idle"/> or <see cref="LockState.Delaying"/>, where pausing has no meaning.</returns>
        Friend Function TogglePause() As Boolean

            Select Case Me.State
                Case LockState.Locked
                    Me.SetState(LockState.Paused)
                    Return True
                Case LockState.Paused
                    Me.SetState(LockState.Locked)
                    Return True
                Case Else
                    Return False
            End Select
        End Function

        ''' <summary>
        ''' Stops the lock, whatever its current state.
        ''' </summary>
        ''' <returns><see langword="True"/> if a lock was active; otherwise, <see langword="False"/>.</returns>
        Friend Function [Stop]() As Boolean

            If Not Me.IsActive Then
                Return False
            End If

            Me.SetState(LockState.Idle)
            Return True
        End Function

#End Region

#Region " Private Methods "

        ''' <summary>
        ''' Sets the state and raises <see cref="LockSession.StateChanged"/>.
        ''' </summary>
        ''' <param name="value">The new state.</param>
        Private Sub SetState(value As LockState)

            If Me._State = value Then
                Return
            End If
            Me._State = value
            RaiseEvent StateChanged(Me, EventArgs.Empty)
        End Sub

#End Region

    End Class

End Namespace
