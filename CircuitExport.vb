Imports System.Globalization
Imports System.Text

Namespace Electropad
    Public Class ExportPackage
        Public Property ProjectName As String
        Public Property CreatedAt As DateTime
        Public Property ComponentCount As Integer
        Public Property WireCount As Integer
        Public Property Material As String
        Public Property AnalysisSummary As String
        Public Property Records As New List(Of String)
        Public Function ToCsv() As String
            Dim builder As New StringBuilder()
            builder.AppendLine("Type,Reference,Kind,Value,Material,LengthMm,RadiusMm,Hidden")
            For Each record In Records
                builder.AppendLine(record)
            Next
            Return builder.ToString()
        End Function
    End Class

    Public Module CircuitExport
        Public Function CreatePackage(project As CircuitProject, name As String) As ExportPackage
            Dim analyzer As New CircuitAnalyzer()
            Dim analysis = analyzer.Analyze(project)
            Dim package As New ExportPackage With {.ProjectName = name, .CreatedAt = DateTime.Now, .ComponentCount = project.Components.Count, .WireCount = project.Wires.Count, .Material = project.Material, .AnalysisSummary = analysis.Summary}
            For Each component In project.Components
                package.Records.Add(String.Join(",", "Component", Quote(component.Reference), Quote(component.Kind), component.Value.ToString(CultureInfo.InvariantCulture), Quote(component.Color), component.LengthMm.ToString(CultureInfo.InvariantCulture), "", component.Hidden.ToString()))
            Next
            For Each wire In project.Wires
                package.Records.Add(String.Join(",", "Wire", Quote(wire.Reference), Quote(wire.StartReference & "->" & wire.EndReference), "", Quote(wire.Material), wire.LengthMm.ToString(CultureInfo.InvariantCulture), wire.RadiusMm.ToString(CultureInfo.InvariantCulture), wire.Hidden.ToString()))
            Next
            Return package
        End Function
        Public Function SaveCsv(project As CircuitProject, name As String, path As String) As Boolean
            Try
                IO.File.WriteAllText(path, CreatePackage(project, name).ToCsv(), Encoding.UTF8)
                Return True
            Catch
                Return False
            End Try
        End Function
        Public Function ExportComponentList(project As CircuitProject) As String
            Dim builder As New StringBuilder()
            builder.AppendLine("Electropad component list")
            builder.AppendLine("Generated: " & DateTime.Now.ToString("u"))
            For Each group In project.Components.GroupBy(Function(item) item.Kind).OrderBy(Function(item) item.Key)
                builder.AppendLine(group.Key & ": " & group.Count())
            Next
            Return builder.ToString()
        End Function
        Public Function Quote(value As String) As String
            Dim quoteMark = ChrW(34)
            If value Is Nothing Then Return quoteMark & quoteMark
            Return quoteMark & value.Replace(quoteMark, quoteMark & quoteMark) & quoteMark
        End Function
    End Module
End Namespace
