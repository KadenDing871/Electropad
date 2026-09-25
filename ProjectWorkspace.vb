Imports System.Linq

Namespace Electropad
    Public Class WorkspaceDocument
        Public Property Name As String
        Public Property Project As New CircuitProject()
        Public Property IsDirty As Boolean
        Public Property LastSaved As DateTime
        Public Property ActiveView As String = "2D"
        Public Property SelectedReference As String
        Public Sub MarkDirty()
            IsDirty = True
        End Sub
        Public Sub MarkSaved()
            IsDirty = False
            LastSaved = DateTime.Now
        End Sub
        Public Function FindComponent(reference As String) As CircuitComponent
            Return Project.Components.FirstOrDefault(Function(item) item.Reference = reference)
        End Function
        Public Function FindWire(reference As String) As CircuitWire
            Return Project.Wires.FirstOrDefault(Function(item) item.Reference = reference)
        End Function
        Public Function VisibleComponents() As IEnumerable(Of CircuitComponent)
            Return Project.Components.Where(Function(item) Not item.Hidden)
        End Function
        Public Function VisibleWires() As IEnumerable(Of CircuitWire)
            Return Project.Wires.Where(Function(item) Not item.Hidden)
        End Function
    End Class

    Public Class WorkspaceManager
        Private ReadOnly documents As New List(Of WorkspaceDocument)
        Private activeIndex As Integer = -1
        Public ReadOnly Property Count As Integer
            Get
                Return documents.Count
            End Get
        End Property
        Public ReadOnly Property Active As WorkspaceDocument
            Get
                If activeIndex < 0 OrElse activeIndex >= documents.Count Then Return Nothing
                Return documents(activeIndex)
            End Get
        End Property
        Public Function NewDocument(name As String) As WorkspaceDocument
            Dim document = New WorkspaceDocument With {.Name = If(String.IsNullOrWhiteSpace(name), "Untitled", name)}
            documents.Add(document)
            activeIndex = documents.Count - 1
            Return document
        End Function
        Public Function OpenDocument(document As WorkspaceDocument) As Integer
            If document Is Nothing Then Return -1
            documents.Add(document)
            activeIndex = documents.Count - 1
            Return activeIndex
        End Function
        Public Function Activate(index As Integer) As Boolean
            If index < 0 OrElse index >= documents.Count Then Return False
            activeIndex = index
            Return True
        End Function
        Public Function Activate(name As String) As Boolean
            Dim index = documents.FindIndex(Function(item) item.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
            Return Activate(index)
        End Function
        Public Function Close(index As Integer, allowDirty As Boolean) As Boolean
            If index < 0 OrElse index >= documents.Count Then Return False
            If documents(index).IsDirty AndAlso Not allowDirty Then Return False
            documents.RemoveAt(index)
            If documents.Count = 0 Then
                activeIndex = -1
            Else
                activeIndex = Math.Min(index, documents.Count - 1)
            End If
            Return True
        End Function
        Public Function CloseActive(allowDirty As Boolean) As Boolean
            Return Close(activeIndex, allowDirty)
        End Function
        Public Function AllDocuments() As IEnumerable(Of WorkspaceDocument)
            Return documents.ToArray()
        End Function
        Public Function DirtyDocuments() As IEnumerable(Of WorkspaceDocument)
            Return documents.Where(Function(item) item.IsDirty).ToArray()
        End Function
        Public Function RenameActive(newName As String) As Boolean
            If Active Is Nothing OrElse String.IsNullOrWhiteSpace(newName) Then Return False
            Active.Name = newName.Trim()
            Active.MarkDirty()
            Return True
        End Function
    End Class
End Namespace
