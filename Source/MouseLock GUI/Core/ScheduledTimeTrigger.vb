Option Strict On
Option Explicit On
Option Infer Off

#Region " Imports "

Imports System.Globalization

#End Region

Namespace Core

    ''' <summary>
    ''' Decides when a daily scheduled lock must fire.
    ''' </summary>
    ''' <remarks>
    ''' The trigger fires at most once per calendar day, during the configured minute,
    ''' regardless of how many times per minute <see cref="ScheduledTimeTrigger.ShouldFire(Date)"/> is polled.
    ''' The comparison is done on numeric hour/minute values, so it does not depend on the current culture
    ''' (e.g. "9:05" vs "09:05" vs "9:05 AM").
    ''' </remarks>
    Friend NotInheritable Class ScheduledTimeTrigger

#Region " Fields "

        ''' <summary>
        ''' The accepted time formats, parsed with the invariant culture.
        ''' </summary>
        Private Shared ReadOnly TimeFormats As String() = {"H:mm", "HH:mm", "H:mm:ss", "HH:mm:ss", "h:mm tt", "hh:mm tt", "h:mmtt", "hh:mmtt"}

        ''' <summary>
        ''' The date on which the trigger fired for the last time, if any.
        ''' </summary>
        Private lastFiredDate As Date?

#End Region

#Region " Properties "

        ''' <summary>
        ''' Gets the time of the day (hours and minutes) at which the trigger fires.
        ''' </summary>
        Friend ReadOnly Property TimeOfDay As TimeSpan

#End Region

#Region " Constructors "

        ''' <summary>
        ''' Initializes a new instance of the <see cref="ScheduledTimeTrigger"/> class.
        ''' </summary>
        ''' <param name="timeOfDay">The time of the day. Seconds and fractions are ignored.</param>
        ''' <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="timeOfDay"/> is negative or not less than 24 hours.</exception>
        Friend Sub New(timeOfDay As TimeSpan)

            If timeOfDay < TimeSpan.Zero OrElse timeOfDay >= TimeSpan.FromDays(1) Then
                Throw New ArgumentOutOfRangeException(NameOf(timeOfDay), timeOfDay, "The time of the day must be in the range 00:00 - 23:59.")
            End If
            Me.TimeOfDay = New TimeSpan(timeOfDay.Hours, timeOfDay.Minutes, 0)
        End Sub

#End Region

#Region " Public Methods "

        ''' <summary>
        ''' Determines whether the trigger must fire at the specified moment, and if so, marks it as fired for that day.
        ''' </summary>
        ''' <param name="now">The current local date and time.</param>
        ''' <returns><see langword="True"/> the first time it is called during the configured minute of a given day;
        ''' otherwise, <see langword="False"/>.</returns>
        Friend Function ShouldFire(now As Date) As Boolean

            If now.Hour <> Me.TimeOfDay.Hours OrElse now.Minute <> Me.TimeOfDay.Minutes Then
                Return False
            End If
            If Me.lastFiredDate.HasValue AndAlso Me.lastFiredDate.Value = now.Date Then
                Return False
            End If

            Me.lastFiredDate = now.Date
            Return True
        End Function

        ''' <summary>
        ''' Tries to parse a time of the day, as returned by the time picker control (e.g. "00:00", "9:05", "09:05 PM").
        ''' </summary>
        ''' <param name="text">The text to parse.</param>
        ''' <param name="result">When this method returns <see langword="True"/>, contains the parsed time of the day.</param>
        ''' <returns><see langword="True"/> if <paramref name="text"/> is a valid time; otherwise, <see langword="False"/>.</returns>
        Friend Shared Function TryParseTime(text As String, ByRef result As TimeSpan) As Boolean

            result = TimeSpan.Zero
            If String.IsNullOrWhiteSpace(text) Then
                Return False
            End If

            Dim value As Date
            If Not Date.TryParseExact(text.Trim(), ScheduledTimeTrigger.TimeFormats, CultureInfo.InvariantCulture,
                                      DateTimeStyles.NoCurrentDateDefault, value) Then
                Return False
            End If

            result = New TimeSpan(value.Hour, value.Minute, 0)
            Return True
        End Function

        ''' <summary>
        ''' Formats a time of the day as "HH:mm", the format used by the time picker and the settings file.
        ''' </summary>
        ''' <param name="timeOfDay">The time of the day.</param>
        ''' <returns>The formatted time.</returns>
        Friend Shared Function FormatTime(timeOfDay As TimeSpan) As String
            Return timeOfDay.ToString("hh\:mm", CultureInfo.InvariantCulture)
        End Function

#End Region

    End Class

End Namespace
