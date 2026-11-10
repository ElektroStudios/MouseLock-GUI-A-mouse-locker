Option Strict On
Option Explicit On
Option Infer Off

Imports System
Imports System.Collections.Generic
Imports System.Diagnostics
Imports System.Linq
Imports System.Runtime.InteropServices
Imports System.Text.RegularExpressions
Imports System.Threading
Imports System.Timers
Imports System.Windows.Forms

''' <summary>
''' Main module for the MouseLock application.
''' </summary>
Friend Module MainModule

#Region " 01. Native Methods (P/Invokes) "

    ''' <summary>
    ''' Moves the cursor to the specified screen coordinates.
    ''' </summary>
    ''' <param name="x">The new x-coordinate of the cursor, in screen coordinates.</param>
    ''' <param name="y">The new y-coordinate of the cursor, in screen coordinates.</param>
    ''' <returns>True if successful; otherwise, False.</returns>
    <DllImport("user32.dll", SetLastError:=True)>
    Private Function SetCursorPos(ByVal x As Integer, ByVal y As Integer) As Boolean
    End Function

    ''' <summary>
    ''' Retrieves the specified system metric or system configuration setting.
    ''' </summary>
    ''' <param name="nIndex">The system metric or configuration setting to be retrieved.</param>
    ''' <returns>The requested system metric or configuration setting.</returns>
    <DllImport("user32.dll", SetLastError:=True)>
    Private Function GetSystemMetrics(ByVal nIndex As Integer) As Integer
    End Function

    Private Const SM_CXSCREEN As Integer = 0
    Private Const SM_CYSCREEN As Integer = 1

#End Region

#Region " 02. Fields "

    Private isRunning As Boolean = False
    Private executableName As String = String.Empty
    Private toggleKey As Keys = Keys.None

    ' Assuming 'Shortcut' is your custom class for Global Hotkeys.
    Private WithEvents ToggleKeyGlobal As Shortcut = Nothing

    Private ReadOnly executableTimer As New System.Timers.Timer()
    Private ReadOnly lockTimer As New System.Timers.Timer()
    Private lockInterval As Integer = 10
    Private lockSleep As Integer = 3000 ' Default 3 seconds in milliseconds

    Private ReadOnly screenCenterX As Integer = (GetSystemMetrics(SM_CXSCREEN) \ 2)
    Private ReadOnly screenCenterY As Integer = (GetSystemMetrics(SM_CYSCREEN) \ 2)

    Private ReadOnly numberRegex As New Regex("\D", RegexOptions.Compiled)
    Private ReadOnly commandLineArguments As List(Of String) = GetCommandLineArguments()

#End Region

#Region " 03. Initialization & Arguments "

    ''' <summary>
    ''' Retrieves the command-line arguments for the current application.
    ''' </summary>
    ''' <returns>A list of strings containing the command-line arguments.</returns>
    Public Function GetCommandLineArguments() As List(Of String)
#If DEBUG Then
        Dim debugArguments As String = "Notepad.exe -Sleep 5 -Interval 50 -Key CTRL+C"
        Return debugArguments.Split(" "c).ToList()
#Else
        Return My.Application.CommandLineArgs.ToList()
#End If
    End Function

    ''' <summary>
    ''' Displays the syntax help and ASCII art in the console.
    ''' </summary>
    Private Sub ShowHelp()
        Dim helpText As String = <a><![CDATA[
                                                                     .-"-.
                                                                    / .--. \
                                                                   / /    \ \
 ___ ___   ___   __ __  _____   ___  _       ___      __  __  _    | |    | |
|   T   T /   \ |  T  T/ ___/  /  _]| T     /   \    /  ]|  l/ ]   | |.-""-.|
| _   _ |Y     Y|  |  (   \_  /  [_ | |    Y     Y  /  / |  ' /   ///`.::::.`\
|  \_/  ||  O  ||  |  |\__  TY    _]| l___ |  O  | /  /  |    \  ||| ::/  \:: ;
|   |   ||     ||  :  |/  \ ||   [_ |     T|     |/   \_ |     Y ||; ::\__/:: ;
|   |   |l     !l     |\    ||     T|     |l     !\     ||  .  |  \\\ '::::' /
l___j___j \___/  \__,_j \___jl_____jl_____j \___/  \____jl__j\_j   `=':-..-'` 

 By Elektro H@cker

[+] Syntax:
    MouseLock {Executable Name}

[+] Syntax (using optional features):
    MouseLock {Executable Name} -S {Seconds} -I {Milliseconds} -K {Key}

[+] Options:
 -S (or) -Sleep
    The seconds to wait until MouseLock will being started. Default is "3".
 -I (or) -Interval
    The milliseconds between the mouse will locks again. Default is "10".
 -K (or) -Key
    The key for toggle between Enabled/Disabled while MouseLock is running.
    Valid keys: A-Z, 0-9 (Numpad) and F0-F12 (Combinable with ALT, CTRL or SHIFT).

[+] Examples:
    MouseLock Game.exe -Sleep 10 -Interval 500 -Key ALT+Z
]]></a>.Value

        Console.WriteLine(helpText)
    End Sub

    ''' <summary>
    ''' Parses and validates the command-line arguments. Fails fast on invalid input.
    ''' </summary>
    Private Sub ParseArguments()
        If (commandLineArguments.Count = 0) Then
            ShowHelp()
            Throw New ArgumentException("No command-line arguments were provided.")
        End If

        Dim targetExecutable As String = commandLineArguments(0)
        If (Not targetExecutable.ToLower().EndsWith(".exe")) Then
            Throw New ArgumentException($"Bad executable name provided: {targetExecutable}. Must end with .exe")
        End If

        executableName = targetExecutable.Substring(0, targetExecutable.Length - 4)

        For i As Integer = 0 To commandLineArguments.Count - 1
            Dim currentArg As String = commandLineArguments(i).ToLower()

            ' Sleep
            If (currentArg = "-s" OrElse currentArg = "-sleep") Then
                Dim seconds As Integer
                If (Not Integer.TryParse(commandLineArguments(i + 1), seconds)) Then
                    Throw New ArgumentException($"Invalid numeric argument for -Sleep: {commandLineArguments(i + 1)}")
                End If
                lockSleep = seconds * 1000 ' Convert to milliseconds
            End If

            ' Interval
            If (currentArg = "-i" OrElse currentArg = "-interval") Then
                If (Not Integer.TryParse(commandLineArguments(i + 1), lockInterval)) Then
                    Throw New ArgumentException($"Invalid numeric argument for -Interval: {commandLineArguments(i + 1)}")
                End If
            End If

            ' Key
            If (currentArg = "-k" OrElse currentArg = "-key") Then
                Dim keyArg As String = commandLineArguments(i + 1).ToLower()
                Try
                    If (keyArg.StartsWith("alt+")) Then
                        toggleKey = DirectCast([Enum].Parse(GetType(Keys), keyArg.Replace("alt+", ""), True), Keys)
                        ToggleKeyGlobal = Shortcut.Create(Shortcut.Modifier.Alt, toggleKey)
                    ElseIf (keyArg.StartsWith("ctrl+")) Then
                        toggleKey = DirectCast([Enum].Parse(GetType(Keys), keyArg.Replace("ctrl+", ""), True), Keys)
                        ToggleKeyGlobal = Shortcut.Create(Shortcut.Modifier.Ctrl, toggleKey)
                    ElseIf (keyArg.StartsWith("shift+")) Then
                        toggleKey = DirectCast([Enum].Parse(GetType(Keys), keyArg.Replace("shift+", ""), True), Keys)
                        ToggleKeyGlobal = Shortcut.Create(Shortcut.Modifier.Shift, toggleKey)
                    Else
                        toggleKey = If((Not IsStringNumeric(commandLineArguments(i + 1))),
                            DirectCast([Enum].Parse(GetType(Keys), commandLineArguments(i + 1), True), Keys),
                            DirectCast([Enum].Parse(GetType(Keys), CStr(CInt(commandLineArguments(i + 1)) + 96), False), Keys))
                        ToggleKeyGlobal = Shortcut.Create(Shortcut.Modifier.None, toggleKey)
                    End If
                Catch ex As Exception
                    Throw New ArgumentException($"Failed to parse -Key argument: {commandLineArguments(i + 1)}", ex)
                End Try
            End If
        Next
    End Sub

#End Region

#Region " 04. Core Logic "

    ''' <summary>
    ''' Entry point of the application.
    ''' </summary>
    Public Sub Main()
        Console.Title = "MouseLock"

        Try
            ParseArguments()
            SleepBeforeLock()
            StartLocking()
        Catch ex As Exception
            Console.ForegroundColor = ConsoleColor.Red
            Console.WriteLine($"[!] FATAL ERROR: {ex.Message}")
            Console.ResetColor()
            Console.WriteLine(Environment.NewLine & "Press any key to exit...")
            Console.ReadKey(True)
            Environment.Exit(1)
        End Try
    End Sub

    ''' <summary>
    ''' Pauses execution for the designated sleep duration before locking.
    ''' </summary>
    Private Sub SleepBeforeLock()
        Console.Title = "MouseLock   (Status: Sleeping)"
        Console.WriteLine($"Sleeping for {(lockSleep \ 1000)} seconds before starting to lock the mouse...")

        Dim secondsPassed As Integer = 0
        Dim targetSeconds As Integer = (lockSleep \ 1000)

        While (secondsPassed < targetSeconds)
            Thread.CurrentThread.Join(1000)
            secondsPassed += 1
        End While
    End Sub

    ''' <summary>
    ''' Initiates the locking mechanism if the target process is running.
    ''' </summary>
    Private Sub StartLocking()
        If (IsProcessRunning(executableName)) Then
            AddHandler lockTimer.Elapsed, AddressOf OnLockTick
            AddHandler executableTimer.Elapsed, AddressOf OnExecutableTick

            lockTimer.Interval = lockInterval
            lockTimer.Start()
            executableTimer.Start()

            ' isRunning controls whether the cursor is actively forced to the center.
            isRunning = True

            Console.WriteLine("MouseLock started!")
            Console.Title = "MouseLock   (Status: Running)"

            ' This replaces your CPU-burning While(isRunning) loop.
            ' It keeps the application alive and processes Hotkey Windows Messages natively.
            Application.Run()
        Else
            TerminateApplication()
        End If
    End Sub

    ''' <summary>
    ''' Handles the tick event to lock the cursor position to the center of the screen.
    ''' </summary>
    Private Sub OnLockTick(ByVal sender As Object, ByVal e As ElapsedEventArgs)
        If (isRunning) Then
            SetCursorPos(screenCenterX, screenCenterY)
        End If
    End Sub

    ''' <summary>
    ''' Monitors the target process and terminates the lock if the process exits.
    ''' </summary>
    Private Sub OnExecutableTick(ByVal sender As Object, ByVal e As ElapsedEventArgs)
        If (Not IsProcessRunning(executableName)) Then
            TerminateApplication()
        End If
    End Sub

    ''' <summary>
    ''' Toggles the active state of the mouse lock when the global hotkey is pressed.
    ''' </summary>
    Private Sub OnToggleKeyGlobalPress(ByVal sender As Object, ByVal e As Shortcut.HotKeyEventArgs) Handles ToggleKeyGlobal.Press
        If (Not isRunning) Then
            isRunning = True
            Console.WriteLine("MouseLock enabled with the ToggleKey.")
            Console.Title = "MouseLock   (Status: Running)"
            lockTimer.Start()
        Else
            isRunning = False
            Console.WriteLine("MouseLock disabled with the ToggleKey.")
            Console.Title = "MouseLock   (Status: Paused)"
            lockTimer.Stop()

            ' Your inner While (Not isRunning) loop was completely deleted here.
            ' It is no longer needed to "pause" the app because Application.Run() 
            ' handles the thread state effortlessly in the background.
        End If
    End Sub

#End Region

#Region " 05. Utility Methods "

    ''' <summary>
    ''' Checks if any process with the specified name is currently running.
    ''' </summary>
    ''' <param name="processName">The name of the process to evaluate.</param>
    ''' <returns>True if at least one instance is running; otherwise, False.</returns>
    Private Function IsProcessRunning(ByVal processName As String) As Boolean
        Dim runningProcesses As Process() = Process.GetProcessesByName(processName)
        Return (runningProcesses.Length > 0)
    End Function

    ''' <summary>
    ''' Determines whether the provided string contains only non-numeric characters.
    ''' </summary>
    ''' <param name="value">The string to validate.</param>
    ''' <returns>True if the string contains only numbers; otherwise, False.</returns>
    Private Function IsStringNumeric(ByVal value As String) As Boolean
        Return Not numberRegex.IsMatch(value)
    End Function

    ''' <summary>
    ''' Safely terminates the mouse lock timers and exits the application gracefully.
    ''' </summary>
    Private Sub TerminateApplication()
        isRunning = False
        lockTimer.Stop()
        executableTimer.Stop()

        Console.WriteLine($"Application '{executableName}.exe' is not running.")
        Console.WriteLine("MouseLock finished.")
        Console.Title = "MouseLock   (Status: Finished)"

        Console.WriteLine(Environment.NewLine & "Press any key to exit...")

        ' Need to use Environment.Exit because this method can be called 
        ' from the ExecutableTimer thread, bypassing standard message pumps.
        Console.ReadKey(True)
        Environment.Exit(0)
    End Sub

#End Region

End Module