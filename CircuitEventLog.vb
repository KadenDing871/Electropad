Imports System.Linq

Namespace Electropad
    Public Enum EventSeverity
        Information
        Notice
        Warning
        Failure
    End Enum

    Public Class DesignEvent
        Public Property Timestamp As DateTime = DateTime.Now
        Public Property Severity As EventSeverity
        Public Property Source As String
        Public Property Message As String
        Public Property Reference As String
        Public Overrides Function ToString() As String
            Return Timestamp.ToString("HH:mm:ss") & " [" & Severity.ToString().ToUpperInvariant() & "] " & Source & ": " & Message
        End Function
    End Class

    Public Class DesignEventLog
        Private ReadOnly entries As New List(Of DesignEvent)
        Public Event EventAdded(entry As DesignEvent)
        Public Sub Write(severity As EventSeverity, source As String, message As String, Optional reference As String = "")
            Dim entry = New DesignEvent With {.Severity = severity, .Source = source, .Message = message, .Reference = reference}
            entries.Add(entry)
            RaiseEvent EventAdded(entry)
        End Sub
        Public Sub Info(source As String, message As String, Optional reference As String = "")
            Write(EventSeverity.Information, source, message, reference)
        End Sub
        Public Sub Notice(source As String, message As String, Optional reference As String = "")
            Write(EventSeverity.Notice, source, message, reference)
        End Sub
        Public Sub Warn(source As String, message As String, Optional reference As String = "")
            Write(EventSeverity.Warning, source, message, reference)
        End Sub
        Public Sub Fail(source As String, message As String, Optional reference As String = "")
            Write(EventSeverity.Failure, source, message, reference)
        End Sub
        Public Function All() As IEnumerable(Of DesignEvent)
            Return entries.ToArray()
        End Function
        Public Function Since(moment As DateTime) As IEnumerable(Of DesignEvent)
            Return entries.Where(Function(item) item.Timestamp >= moment).ToArray()
        End Function
        Public Function BySeverity(severity As EventSeverity) As IEnumerable(Of DesignEvent)
            Return entries.Where(Function(item) item.Severity = severity).ToArray()
        End Function
        Public Function ForReference(reference As String) As IEnumerable(Of DesignEvent)
            Return entries.Where(Function(item) item.Reference.Equals(reference, StringComparison.OrdinalIgnoreCase)).ToArray()
        End Function
        Public Function Count() As Integer
            Return entries.Count
        End Function
        Public Function ErrorCount() As Integer
            Return entries.Where(Function(item) item.Severity = EventSeverity.Failure).Count()
        End Function
        Public Function WarningCount() As Integer
            Return entries.Where(Function(item) item.Severity = EventSeverity.Warning).Count()
        End Function
        Public Function ExportText() As String
            Return String.Join(Environment.NewLine, entries.Select(Function(item) item.ToString()))
        End Function
        Public Sub Clear()
            entries.Clear()
        End Sub
    End Class
End Namespace
