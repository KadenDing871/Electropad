Imports System.Linq

Namespace Electropad
    Public Class ComponentDefinition
        Public Property Kind As String
        Public Property Prefix As String
        Public Property Symbol As String
        Public Property DefaultValue As Double
        Public Property DefaultLengthMm As Double
        Public Property DefaultDiameterMm As Double
        Public Property DefaultHeightMm As Double
        Public Property Category As String
        Public Property Description As String
        Public Property Pins As Integer
        Public Property IsPolarized As Boolean
        Public Property IsPassive As Boolean
        Public Property IsMechanical As Boolean
        Public Overrides Function ToString() As String
            Return Kind & "  [" & Symbol & "]"
        End Function
    End Class

    Public Module ComponentLibrary
        Public ReadOnly Definitions As New List(Of ComponentDefinition) From {
            New ComponentDefinition With {.Kind = "Resistor", .Prefix = "R", .Symbol = "R", .DefaultValue = 220, .DefaultLengthMm = 10, .DefaultDiameterMm = 4, .DefaultHeightMm = 4, .Category = "Passive", .Description = "Fixed current-limiting resistor", .Pins = 2, .IsPassive = True},
            New ComponentDefinition With {.Kind = "Capacitor", .Prefix = "C", .Symbol = "C", .DefaultValue = 0.000001, .DefaultLengthMm = 8, .DefaultDiameterMm = 5, .DefaultHeightMm = 8, .Category = "Passive", .Description = "Energy storage capacitor", .Pins = 2, .IsPassive = True},
            New ComponentDefinition With {.Kind = "Inductor", .Prefix = "L", .Symbol = "L", .DefaultValue = 0.001, .DefaultLengthMm = 12, .DefaultDiameterMm = 6, .DefaultHeightMm = 8, .Category = "Passive", .Description = "Magnetic energy storage coil", .Pins = 2, .IsPassive = True},
            New ComponentDefinition With {.Kind = "LED", .Prefix = "D", .Symbol = "LED", .DefaultValue = 2, .DefaultLengthMm = 5, .DefaultDiameterMm = 5, .DefaultHeightMm = 8, .Category = "Semiconductor", .Description = "Light emitting diode", .Pins = 2, .IsPolarized = True},
            New ComponentDefinition With {.Kind = "Diode", .Prefix = "D", .Symbol = "D", .DefaultValue = 0.7, .DefaultLengthMm = 6, .DefaultDiameterMm = 3, .DefaultHeightMm = 4, .Category = "Semiconductor", .Description = "One-way semiconductor junction", .Pins = 2, .IsPolarized = True},
            New ComponentDefinition With {.Kind = "Transistor", .Prefix = "Q", .Symbol = "Q", .DefaultValue = 100, .DefaultLengthMm = 5, .DefaultDiameterMm = 4, .DefaultHeightMm = 6, .Category = "Semiconductor", .Description = "Three-terminal switching device", .Pins = 3},
            New ComponentDefinition With {.Kind = "OpAmp", .Prefix = "U", .Symbol = "OP", .DefaultValue = 100000, .DefaultLengthMm = 12, .DefaultDiameterMm = 8, .DefaultHeightMm = 4, .Category = "Integrated", .Description = "Operational amplifier", .Pins = 5},
            New ComponentDefinition With {.Kind = "IC", .Prefix = "U", .Symbol = "IC", .DefaultValue = 0, .DefaultLengthMm = 15, .DefaultDiameterMm = 8, .DefaultHeightMm = 4, .Category = "Integrated", .Description = "Integrated circuit package", .Pins = 8},
            New ComponentDefinition With {.Kind = "Battery", .Prefix = "V", .Symbol = "BAT", .DefaultValue = 9, .DefaultLengthMm = 18, .DefaultDiameterMm = 10, .DefaultHeightMm = 35, .Category = "Source", .Description = "DC voltage source", .Pins = 2, .IsPolarized = True},
            New ComponentDefinition With {.Kind = "Ground", .Prefix = "GND", .Symbol = "GND", .DefaultValue = 0, .DefaultLengthMm = 5, .DefaultDiameterMm = 5, .DefaultHeightMm = 4, .Category = "Reference", .Description = "Circuit reference node", .Pins = 1},
            New ComponentDefinition With {.Kind = "Switch", .Prefix = "S", .Symbol = "SW", .DefaultValue = 0, .DefaultLengthMm = 10, .DefaultDiameterMm = 5, .DefaultHeightMm = 4, .Category = "Control", .Description = "Manual open or close contact", .Pins = 2, .IsMechanical = True},
            New ComponentDefinition With {.Kind = "Fuse", .Prefix = "F", .Symbol = "F", .DefaultValue = 1, .DefaultLengthMm = 12, .DefaultDiameterMm = 4, .DefaultHeightMm = 4, .Category = "Protection", .Description = "Overcurrent protection link", .Pins = 2, .IsPassive = True},
            New ComponentDefinition With {.Kind = "Potentiometer", .Prefix = "RV", .Symbol = "POT", .DefaultValue = 10000, .DefaultLengthMm = 14, .DefaultDiameterMm = 10, .DefaultHeightMm = 12, .Category = "Control", .Description = "Adjustable three-terminal resistor", .Pins = 3, .IsMechanical = True},
            New ComponentDefinition With {.Kind = "Speaker", .Prefix = "SP", .Symbol = "SPK", .DefaultValue = 8, .DefaultLengthMm = 20, .DefaultDiameterMm = 20, .DefaultHeightMm = 8, .Category = "Output", .Description = "Electrodynamic audio transducer", .Pins = 2},
            New ComponentDefinition With {.Kind = "Motor", .Prefix = "M", .Symbol = "M", .DefaultValue = 12, .DefaultLengthMm = 25, .DefaultDiameterMm = 14, .DefaultHeightMm = 14, .Category = "Output", .Description = "Rotary electromechanical actuator", .Pins = 2, .IsMechanical = True},
            New ComponentDefinition With {.Kind = "Connector", .Prefix = "J", .Symbol = "CON", .DefaultValue = 0, .DefaultLengthMm = 10, .DefaultDiameterMm = 8, .DefaultHeightMm = 5, .Category = "Interface", .Description = "External connection header", .Pins = 2, .IsMechanical = True}
        }
        Public Function Find(kind As String) As ComponentDefinition
            Return Definitions.FirstOrDefault(Function(item) item.Kind = kind)
        End Function
        Public Function ByCategory(category As String) As IEnumerable(Of ComponentDefinition)
            Return Definitions.Where(Function(item) item.Category = category)
        End Function
        Public Function Categories() As IEnumerable(Of String)
            Return Definitions.Select(Function(item) item.Category).Distinct().OrderBy(Function(item) item)
        End Function
        Public Function Create(kind As String, reference As String, x As Integer, y As Integer) As CircuitComponent
            Dim definition = Find(kind)
            If definition Is Nothing Then Return Nothing
            Return New CircuitComponent With {.Kind = definition.Kind, .Reference = reference, .Value = definition.DefaultValue, .X = x, .Y = y, .LengthMm = definition.DefaultLengthMm, .BodyDiameterMm = definition.DefaultDiameterMm, .HeightMm = definition.DefaultHeightMm, .Symbol = definition.Symbol}
        End Function
        Public Function Search(text As String) As IEnumerable(Of ComponentDefinition)
            If String.IsNullOrWhiteSpace(text) Then Return Definitions
            Return Definitions.Where(Function(item) item.Kind.IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0 OrElse item.Description.IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0)
        End Function
    End Module
End Namespace
