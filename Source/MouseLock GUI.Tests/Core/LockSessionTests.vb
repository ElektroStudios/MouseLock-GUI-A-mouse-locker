Option Strict On
Option Explicit On
Option Infer Off

#Region " Imports "

Imports MouseLock.Core

#End Region

Namespace Core

    ''' <summary>
    ''' Unit tests for <see cref="LockSession"/>.
    ''' </summary>
    <TestClass>
    Public NotInheritable Class LockSessionTests

        <TestMethod>
        Public Sub NewSession_IsIdle()
            Dim session As New LockSession()

            Assert.AreEqual(LockState.Idle, session.State)
            Assert.IsFalse(session.IsActive)
            Assert.IsFalse(session.ShouldConfineCursor)
        End Sub

        <TestMethod>
        Public Sub Start_WithoutDelay_LocksImmediately()
            Dim session As New LockSession()

            Dim result As LockState = session.Start(hasInitialDelay:=False)

            Assert.AreEqual(LockState.Locked, result)
            Assert.AreEqual(LockState.Locked, session.State)
            Assert.IsTrue(session.IsActive)
            Assert.IsTrue(session.ShouldConfineCursor)
        End Sub

        <TestMethod>
        Public Sub Start_WithDelay_EntersDelaying_AndDoesNotConfineCursor()
            Dim session As New LockSession()

            Dim result As LockState = session.Start(hasInitialDelay:=True)

            Assert.AreEqual(LockState.Delaying, result)
            Assert.IsTrue(session.IsActive)
            Assert.IsFalse(session.ShouldConfineCursor)
        End Sub

        <TestMethod>
        Public Sub Start_WhenAlreadyActive_Throws()
            Dim session As New LockSession()
            session.Start(hasInitialDelay:=False)

            Assert.ThrowsException(Of InvalidOperationException)(Sub() session.Start(hasInitialDelay:=False))
        End Sub

        <TestMethod>
        Public Sub CompleteDelay_WhenDelaying_Locks()
            Dim session As New LockSession()
            session.Start(hasInitialDelay:=True)

            Assert.IsTrue(session.CompleteDelay())
            Assert.AreEqual(LockState.Locked, session.State)
        End Sub

        <TestMethod>
        Public Sub CompleteDelay_AfterStop_DoesNotLock()
            ' Regression: in v1.1 an initial delay that elapsed after the user pressed "Unlock" re-locked the mouse.
            Dim session As New LockSession()
            session.Start(hasInitialDelay:=True)
            session.Stop()

            Assert.IsFalse(session.CompleteDelay())
            Assert.AreEqual(LockState.Idle, session.State)
        End Sub

        <TestMethod>
        Public Sub CompleteDelay_WhenLocked_ReturnsFalse()
            Dim session As New LockSession()
            session.Start(hasInitialDelay:=False)

            Assert.IsFalse(session.CompleteDelay())
            Assert.AreEqual(LockState.Locked, session.State)
        End Sub

        <TestMethod>
        Public Sub TogglePause_AlternatesBetweenLockedAndPaused()
            Dim session As New LockSession()
            session.Start(hasInitialDelay:=False)

            Assert.IsTrue(session.TogglePause())
            Assert.AreEqual(LockState.Paused, session.State)
            Assert.IsTrue(session.IsActive)
            Assert.IsFalse(session.ShouldConfineCursor)

            Assert.IsTrue(session.TogglePause())
            Assert.AreEqual(LockState.Locked, session.State)
            Assert.IsTrue(session.ShouldConfineCursor)
        End Sub

        <TestMethod>
        Public Sub TogglePause_WhenIdle_DoesNothing()
            ' Regression: in v1.1 pressing the hot key while idle flagged the lock as running
            ' without starting it, so the next click on "Lock now" unlocked instead.
            Dim session As New LockSession()

            Assert.IsFalse(session.TogglePause())
            Assert.AreEqual(LockState.Idle, session.State)
            Assert.IsFalse(session.IsActive)
        End Sub

        <TestMethod>
        Public Sub TogglePause_WhenDelaying_DoesNothing()
            Dim session As New LockSession()
            session.Start(hasInitialDelay:=True)

            Assert.IsFalse(session.TogglePause())
            Assert.AreEqual(LockState.Delaying, session.State)
        End Sub

        <TestMethod>
        Public Sub Stop_FromEveryActiveState_ReturnsToIdle()
            For Each pause As Boolean In {False, True}
                For Each delay As Boolean In {False, True}
                    Dim session As New LockSession()
                    session.Start(delay)
                    If pause Then
                        session.CompleteDelay()
                        session.TogglePause()
                    End If

                    Assert.IsTrue(session.Stop(), $"delay={delay}, pause={pause}")
                    Assert.AreEqual(LockState.Idle, session.State)
                Next
            Next
        End Sub

        <TestMethod>
        Public Sub Stop_WhenIdle_ReturnsFalse()
            Dim session As New LockSession()
            Assert.IsFalse(session.Stop())
        End Sub

        <TestMethod>
        Public Sub Stop_ThenStart_Works()
            Dim session As New LockSession()
            session.Start(hasInitialDelay:=False)
            session.Stop()

            Assert.AreEqual(LockState.Locked, session.Start(hasInitialDelay:=False))
        End Sub

        <TestMethod>
        Public Sub StateChanged_IsRaisedOncePerTransition()
            Dim session As New LockSession()
            Dim states As New List(Of LockState)
            AddHandler session.StateChanged, Sub(sender As Object, e As EventArgs) states.Add(session.State)

            session.Start(hasInitialDelay:=True)
            session.CompleteDelay()
            session.TogglePause()
            session.TogglePause()
            session.Stop()
            session.Stop()          ' No-op: no event.
            session.TogglePause()   ' No-op: no event.

            CollectionAssert.AreEqual({LockState.Delaying, LockState.Locked, LockState.Paused, LockState.Locked, LockState.Idle}, states)
        End Sub

    End Class

End Namespace
