Imports System.Linq

Namespace Electropad
    Public Class DesignMetadata
        Public Property Author As String = Environment.UserName
        Public Property Organization As String = ""
        Public Property Description As String = ""
        Public Property Revision As String = "A"
        Public Property PartNumber As String = "ELECTROPAD-001"
        Public Property Units As String = "millimeters"
        Public Property Created As DateTime = DateTime.Now
        Public Property Modified As DateTime = DateTime.Now
        Public Property Tags As New List(Of String)
        Public Property Notes As New List(Of String)
        Public Function AddTag(tag As String) As Boolean
            If String.IsNullOrWhiteSpace(tag) OrElse Tags.Any(Function(item) item.Equals(tag, StringComparison.OrdinalIgnoreCase)) Then Return False
            Tags.Add(tag.Trim())
            Modified = DateTime.Now
            Return True
        End Function
        Public Function RemoveTag(tag As String) As Boolean
            Dim found = Tags.FirstOrDefault(Function(item) item.Equals(tag, StringComparison.OrdinalIgnoreCase))
            If found Is Nothing Then Return False
            Tags.Remove(found)
            Modified = DateTime.Now
            Return True
        End Function
        Public Function AddNote(note As String) As Boolean
            If String.IsNullOrWhiteSpace(note) Then Return False
            Notes.Add(note.Trim())
            Modified = DateTime.Now
            Return True
        End Function
        Public Function Summary() As String
            Return PartNumber & " rev " & Revision & " / " & Author & " / " & Units
        End Function
    End Class

    Public Class DesignChange
        Public Property Timestamp As DateTime = DateTime.Now
        Public Property Author As String
        Public Property Description As String
        Public Property Category As String
        Public Property IsApproved As Boolean
    End Class

    Public Class DesignChangeLog
        Private ReadOnly changes As New List(Of DesignChange)
        Public Sub Add(author As String, description As String, category As String)
            changes.Add(New DesignChange With {.Author = author, .Description = description, .Category = category})
        End Sub
        Public Function Approve(description As String) As Boolean
            Dim change = changes.LastOrDefault(Function(item) item.Description = description)
            If change Is Nothing Then Return False
            change.IsApproved = True
            Return True
        End Function
        Public Function All() As IEnumerable(Of DesignChange)
            Return changes.ToArray()
        End Function
        Public Function Pending() As IEnumerable(Of DesignChange)
            Return changes.Where(Function(item) Not item.IsApproved).ToArray()
        End Function
        Public Function Count() As Integer
            Return changes.Count
        End Function
    End Class
End Namespace
