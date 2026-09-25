Imports System.Linq

Namespace Electropad
    Public Class CircuitComponent
        Public Property Kind As String = "Resistor"
        Public Property Reference As String = "R1"
        Public Property Value As Double = 220
        Public Property Color As String = "Copper"
        Public Property X As Integer = 360
        Public Property Y As Integer = 190
        Public Property Z As Integer = 0
        Public Property RotationX As Single
        Public Property RotationY As Single
        Public Property RotationZ As Single
        Public Property LengthMm As Double = 10
        Public Property BodyDiameterMm As Double = 4
        Public Property HeightMm As Double = 3
        Public Property Hidden As Boolean
        Public Property Symbol As String = "R"
    End Class

    Public Class CircuitWire
        Public Property Reference As String = "W1"
        Public Property StartReference As String
        Public Property EndReference As String
        Public Property LengthMm As Double = 42
        Public Property RadiusMm As Double = 2
        Public Property Material As String = "Copper"
        Public Property Hidden As Boolean
    End Class

    Public Class CircuitProject
        Public Property Material As String = "FR-4 board"
        Public Property WireLength As Double = 42
        Public Property WireRadius As Double = 2
        Public Property Components As New List(Of CircuitComponent)
        Public Property Wires As New List(Of CircuitWire)
    End Class

    Public Class MaterialSpec
        Public Property Name As String
        Public Property Resistivity As Double
        Public Property Density As Double
        Public Property ThermalConductivity As Double
        Public Property MaximumTemperature As Double
        Public Property Hardness As Double
        Public Property TensileStrength As Double
        Public Property YieldStrength As Double
        Public Property ElasticModulus As Double

        Public Overrides Function ToString() As String
            Return Name
        End Function
    End Class

    Public Module MaterialCatalog
        Public ReadOnly Materials As New List(Of MaterialSpec) From {
            New MaterialSpec With {.Name = "FR-4 board", .Resistivity = 1.0E+12, .Density = 1.85, .ThermalConductivity = 0.3, .MaximumTemperature = 130, .Hardness = 80, .TensileStrength = 310, .YieldStrength = 250, .ElasticModulus = 22},
            New MaterialSpec With {.Name = "Solderless", .Resistivity = 1.0E+12, .Density = 1.2, .ThermalConductivity = 0.2, .MaximumTemperature = 85},
            New MaterialSpec With {.Name = "Aluminium", .Resistivity = 0.0282, .Density = 2.7, .ThermalConductivity = 237, .MaximumTemperature = 660},
            New MaterialSpec With {.Name = "Copper", .Resistivity = 0.01724, .Density = 8.96, .ThermalConductivity = 401, .MaximumTemperature = 1085, .Hardness = 35, .TensileStrength = 220, .YieldStrength = 70, .ElasticModulus = 117},
            New MaterialSpec With {.Name = "Silver", .Resistivity = 0.0159, .Density = 10.49, .ThermalConductivity = 429, .MaximumTemperature = 962},
            New MaterialSpec With {.Name = "Gold", .Resistivity = 0.02214, .Density = 19.3, .ThermalConductivity = 318, .MaximumTemperature = 1064},
            New MaterialSpec With {.Name = "Tin", .Resistivity = 0.109, .Density = 7.31, .ThermalConductivity = 67, .MaximumTemperature = 232},
            New MaterialSpec With {.Name = "Lead", .Resistivity = 0.22, .Density = 11.34, .ThermalConductivity = 35, .MaximumTemperature = 327},
            New MaterialSpec With {.Name = "Nickel", .Resistivity = 0.0699, .Density = 8.91, .ThermalConductivity = 91, .MaximumTemperature = 1455},
            New MaterialSpec With {.Name = "Iron", .Resistivity = 0.0961, .Density = 7.87, .ThermalConductivity = 80, .MaximumTemperature = 1538},
            New MaterialSpec With {.Name = "Steel", .Resistivity = 0.143, .Density = 7.85, .ThermalConductivity = 50, .MaximumTemperature = 1370},
            New MaterialSpec With {.Name = "Stainless steel", .Resistivity = 0.72, .Density = 8.0, .ThermalConductivity = 16, .MaximumTemperature = 1400},
            New MaterialSpec With {.Name = "Tungsten", .Resistivity = 0.0528, .Density = 19.25, .ThermalConductivity = 174, .MaximumTemperature = 3422},
            New MaterialSpec With {.Name = "Titanium", .Resistivity = 0.42, .Density = 4.51, .ThermalConductivity = 22, .MaximumTemperature = 1668},
            New MaterialSpec With {.Name = "Chromium", .Resistivity = 0.129, .Density = 7.19, .ThermalConductivity = 94, .MaximumTemperature = 1907},
            New MaterialSpec With {.Name = "Zinc", .Resistivity = 0.059, .Density = 7.14, .ThermalConductivity = 116, .MaximumTemperature = 420},
            New MaterialSpec With {.Name = "Brass", .Resistivity = 0.065, .Density = 8.5, .ThermalConductivity = 120, .MaximumTemperature = 930},
            New MaterialSpec With {.Name = "Bronze", .Resistivity = 0.087, .Density = 8.8, .ThermalConductivity = 60, .MaximumTemperature = 950},
            New MaterialSpec With {.Name = "Graphite", .Resistivity = 13.0, .Density = 2.2, .ThermalConductivity = 140, .MaximumTemperature = 3650},
            New MaterialSpec With {.Name = "Carbon film", .Resistivity = 1000000, .Density = 2.0, .ThermalConductivity = 4, .MaximumTemperature = 155},
            New MaterialSpec With {.Name = "Nichrome", .Resistivity = 1.1, .Density = 8.4, .ThermalConductivity = 11, .MaximumTemperature = 1400},
            New MaterialSpec With {.Name = "Manganin", .Resistivity = 0.43, .Density = 8.4, .ThermalConductivity = 22, .MaximumTemperature = 400},
            New MaterialSpec With {.Name = "Constantan", .Resistivity = 0.49, .Density = 8.9, .ThermalConductivity = 19, .MaximumTemperature = 1210},
            New MaterialSpec With {.Name = "Silicon", .Resistivity = 2300, .Density = 2.33, .ThermalConductivity = 149, .MaximumTemperature = 1414},
            New MaterialSpec With {.Name = "Germanium", .Resistivity = 0.46, .Density = 5.32, .ThermalConductivity = 60, .MaximumTemperature = 938},
            New MaterialSpec With {.Name = "GaAs", .Resistivity = 1.0E+08, .Density = 5.32, .ThermalConductivity = 55, .MaximumTemperature = 1238},
            New MaterialSpec With {.Name = "Ceramic", .Resistivity = 1.0E+14, .Density = 3.8, .ThermalConductivity = 24, .MaximumTemperature = 2000},
            New MaterialSpec With {.Name = "Glass", .Resistivity = 1.0E+12, .Density = 2.5, .ThermalConductivity = 1.1, .MaximumTemperature = 1500},
            New MaterialSpec With {.Name = "PTFE", .Resistivity = 1.0E+18, .Density = 2.2, .ThermalConductivity = 0.25, .MaximumTemperature = 260},
            New MaterialSpec With {.Name = "Polyimide", .Resistivity = 1.0E+16, .Density = 1.42, .ThermalConductivity = 0.2, .MaximumTemperature = 400}
        }

        Public Function Find(name As String) As MaterialSpec
            Dim found = Materials.FirstOrDefault(Function(item) item.Name = name)
            Dim material = If(found, Materials(0))
            If material.Hardness <= 0 Then
                material.Hardness = Math.Max(10, material.Density * 8)
                material.TensileStrength = Math.Max(20, material.Density * 35)
                material.YieldStrength = material.TensileStrength * 0.55
                material.ElasticModulus = Math.Max(1, material.ThermalConductivity * 0.3)
            End If
            Return material
        End Function
    End Module
End Namespace
