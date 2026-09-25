Imports System.Text
Imports System.Linq

Namespace Electropad
    Public Class ProjectReport
        Public Property Title As String
        Public Property GeneratedAt As DateTime
        Public Property Lines As New List(Of String)
        Public Function ToText() As String
            Return String.Join(Environment.NewLine, Lines)
        End Function
    End Class

    Public Module CircuitReports
        Public Function BuildProjectReport(project As CircuitProject) As ProjectReport
            Dim report As New ProjectReport With {.Title = "Electropad Project Report", .GeneratedAt = DateTime.Now}
            report.Lines.Add(report.Title)
            report.Lines.Add(New String("="c, report.Title.Length))
            report.Lines.Add("Generated: " & report.GeneratedAt.ToString("u"))
            report.Lines.Add("Board material: " & project.Material)
            report.Lines.Add("Component count: " & project.Components.Count)
            report.Lines.Add("Wire count: " & project.Wires.Count)
            report.Lines.Add("")
            report.Lines.Add("COMPONENTS")
            For Each group In project.Components.GroupBy(Function(item) item.Kind).OrderBy(Function(item) item.Key)
                report.Lines.Add(group.Key.PadRight(18) & group.Count().ToString().PadLeft(4))
            Next
            report.Lines.Add("")
            report.Lines.Add("WIRES")
            For Each wire In project.Wires
                report.Lines.Add(wire.Reference.PadRight(8) & wire.StartReference & " -> " & wire.EndReference & "  " & wire.LengthMm.ToString("0.###") & " mm  " & wire.Material)
            Next
            Return report
        End Function
        Public Function BuildMaterialReport(material As MaterialSpec) As ProjectReport
            Dim report As New ProjectReport With {.Title = material.Name & " Material Report", .GeneratedAt = DateTime.Now}
            report.Lines.Add(report.Title)
            report.Lines.Add(New String("="c, report.Title.Length))
            report.Lines.Add("Resistivity: " & material.Resistivity.ToString("0.######") & " Ω·mm²/m")
            report.Lines.Add("Density: " & material.Density.ToString("0.###") & " g/cm³")
            report.Lines.Add("Hardness: " & material.Hardness.ToString("0.###") & " HB")
            report.Lines.Add("Tensile strength: " & material.TensileStrength.ToString("0.###") & " MPa")
            report.Lines.Add("Yield strength: " & material.YieldStrength.ToString("0.###") & " MPa")
            report.Lines.Add("Elastic modulus: " & material.ElasticModulus.ToString("0.###") & " GPa")
            report.Lines.Add("Thermal conductivity: " & material.ThermalConductivity.ToString("0.###") & " W/m·K")
            report.Lines.Add("Maximum temperature: " & material.MaximumTemperature.ToString("0.#") & " °C")
            Return report
        End Function
        Public Function BuildAnalysisReport(result As AnalysisResult) As ProjectReport
            Dim report As New ProjectReport With {.Title = "Electrical Analysis Report", .GeneratedAt = DateTime.Now}
            report.Lines.Add(report.Title)
            report.Lines.Add(New String("="c, report.Title.Length))
            report.Lines.Add("Voltage: " & result.Voltage.ToString("0.######") & " V")
            report.Lines.Add("Current: " & result.CurrentMilliamps.ToString("0.######") & " mA")
            report.Lines.Add("Wire resistance: " & result.WireResistance.ToString("0.######") & " Ω")
            report.Lines.Add("Component resistance: " & result.ComponentResistance.ToString("0.######") & " Ω")
            report.Lines.Add("Total resistance: " & result.TotalResistance.ToString("0.######") & " Ω")
            report.Lines.Add("Power: " & result.PowerWatts.ToString("0.######") & " W")
            For Each warning In result.Warnings
                report.Lines.Add("WARNING: " & warning)
            Next
            Return report
        End Function
        Public Function Save(report As ProjectReport, path As String) As Boolean
            Try
                IO.File.WriteAllText(path, report.ToText(), Encoding.UTF8)
                Return True
            Catch
                Return False
            End Try
        End Function
    End Module
End Namespace
