Imports System.Drawing.Drawing2D
Imports System.Drawing
Imports System.ComponentModel
Imports System.Reflection
Imports System.Text.Json
Imports System.Text.Json.Serialization
Imports System.Windows.Forms
Imports System.IO
Imports System.Linq
Imports Vortice.Direct3D
Imports Vortice.Direct3D11
Imports Vortice.DXGI

Namespace Electropad
    Public Class MainForm
        Inherits Form

        Private ReadOnly project As New CircuitProject()
        Private ReadOnly canvas As New CircuitCanvas()
            Private ReadOnly statusLabel As New Label()
        Private ReadOnly selectionLabel As New Label()
        Private ReadOnly referenceBox As New TextBox()
        Private ReadOnly valueBox As New TextBox()
        Private ReadOnly materialCombo As New ComboBox()
        Private ReadOnly materialStateLabel As New Label()
        Private ReadOnly colorCombo As New ComboBox()
        Private ReadOnly lengthSlider As New TrackBar()
        Private ReadOnly radiusSlider As New TrackBar()
        Private voltageLabel As Label
        Private currentLabel As Label
        Private wireResistanceLabel As Label
        Private totalResistanceLabel As Label
        Private powerLabel As Label
        Private selected As CircuitComponent
        Private selectedWire As CircuitWire
        Private assemblyTree As TreeView
        Private simulationRunning As Boolean
        Private breadboardMode As Boolean
        Private render3D As Boolean

        Public Sub New()
            Text = "Electropad / Untitled circuit"
            Width = 1440
            Height = 900
            MinimumSize = New Size(1100, 680)
            BackColor = Color.FromArgb(15, 25, 30)
            ForeColor = Color.FromArgb(242, 245, 247)
            Font = New Font("Segoe UI", 9)

            project.Components.AddRange({
                New CircuitComponent With {.Kind = "Battery", .Reference = "V1", .Value = 9, .Color = "Blue", .X = 130, .Y = 150, .LengthMm = 18, .BodyDiameterMm = 10, .HeightMm = 35},
                New CircuitComponent With {.Kind = "Resistor", .Reference = "R1", .Value = 220, .Color = "Copper", .X = 365, .Y = 150, .LengthMm = 10, .BodyDiameterMm = 4, .HeightMm = 4},
                New CircuitComponent With {.Kind = "LED", .Reference = "D1", .Value = 2, .Color = "Amber", .X = 590, .Y = 295, .LengthMm = 5, .BodyDiameterMm = 5, .HeightMm = 8},
                New CircuitComponent With {.Kind = "Ground", .Reference = "GND1", .Value = 0, .Color = "Green", .X = 130, .Y = 295, .LengthMm = 5, .BodyDiameterMm = 5, .HeightMm = 4}
            })
            project.Wires.AddRange({
                New CircuitWire With {.Reference = "W1", .StartReference = "V1", .EndReference = "R1", .LengthMm = 42, .RadiusMm = 2, .Material = "Copper"},
                New CircuitWire With {.Reference = "W2", .StartReference = "R1", .EndReference = "D1", .LengthMm = 38, .RadiusMm = 2, .Material = "Copper"},
                New CircuitWire With {.Reference = "W3", .StartReference = "D1", .EndReference = "GND1", .LengthMm = 35, .RadiusMm = 2, .Material = "Copper"}
            })
            selected = project.Components(1)
            BuildUi()
            UpdateInspector()
        End Sub

        Private Sub BuildUi()
            Dim header = New Panel With {.Dock = DockStyle.Top, .Height = 60, .BackColor = Color.FromArgb(21, 33, 39), .Padding = New Padding(20, 0, 20, 0)}
            Dim brand = New Label With {.Text = "ELECTROPAD  /  CIRCUIT STUDIO", .Dock = DockStyle.Left, .Width = 300, .TextAlign = ContentAlignment.MiddleLeft, .Font = New Font("Segoe UI", 13, FontStyle.Bold), .ForeColor = Color.FromArgb(85, 224, 208)}
            header.Controls.Add(brand)
            Dim importButton = MakeButton("IMPORT .CEX", AddressOf ImportProject)
            importButton.Dock = DockStyle.Right
            Dim exportButton = MakeButton("EXPORT", AddressOf ExportProject)
            exportButton.Dock = DockStyle.Right
            Dim runButton = MakeButton("RUN SIMULATION", AddressOf Simulate)
            runButton.Dock = DockStyle.Right
            runButton.BackColor = Color.FromArgb(85, 224, 208)
            runButton.ForeColor = Color.FromArgb(15, 25, 30)
            header.Controls.Add(importButton)
            header.Controls.Add(exportButton)
            header.Controls.Add(runButton)
            Dim exchangeButton = MakeButton("2D ↔ 3D EXCHANGE", AddressOf ShowExchange)
            exchangeButton.Dock = DockStyle.Right
            header.Controls.Add(exchangeButton)

            Dim left = New Panel With {.Dock = DockStyle.Left, .Width = 220, .BackColor = Color.FromArgb(20, 33, 39), .Padding = New Padding(16)}
            Controls.Add(left)
            AddPalette(left)

            Dim right = New Panel With {.Dock = DockStyle.Right, .Width = 290, .BackColor = Color.FromArgb(20, 33, 39), .Padding = New Padding(16)}
            Controls.Add(right)
            AddInspector(right)

            canvas.Dock = DockStyle.Fill
            canvas.BackColor = Color.FromArgb(15, 25, 30)
            canvas.Project = project
            canvas.OnComponentSelected = AddressOf SelectComponent
            canvas.OnComponentAdded = AddressOf AddComponent
            canvas.OnWireEdit = AddressOf EditWire
            canvas.OnAddWire = AddressOf AddWire
            canvas.OnComponentDelete = AddressOf DeleteComponent
            canvas.OnComponentContextMenu = AddressOf ShowComponentCanvasMenu
            canvas.OnWireSelected = AddressOf SelectWire
            canvas.OnWireContextMenu = AddressOf ShowWireCanvasMenu
            canvas.Render3D = render3D
            Controls.Add(canvas)

            statusLabel.Dock = DockStyle.Bottom
            statusLabel.Height = 27
            statusLabel.Padding = New Padding(16, 6, 0, 0)
            statusLabel.BackColor = Color.FromArgb(10, 18, 22)
            statusLabel.ForeColor = Color.FromArgb(111, 133, 142)
            statusLabel.Text = "●  LOCAL PROJECT                         Ready · 4 components · 4 connections"
            Controls.Add(statusLabel)
        End Sub

        Private Sub AddPalette(parent As Panel)
            parent.Controls.Add(HeaderLabel("COMPONENTS"))
            parent.Controls.Add(HeaderLabel("Click to add to workspace", 10, Color.FromArgb(95, 115, 125)))
            For Each kind In {"Resistor", "LED", "Battery", "Switch", "Ground", "Capacitor", "Inductor", "Diode", "Transistor", "OpAmp", "IC", "Fuse", "Potentiometer", "Speaker", "Motor", "Connector"}
                Dim button = MakeButton("+  " & kind, Sub() AddComponent(kind))
                button.Dock = DockStyle.Top
                button.Height = 42
                button.TextAlign = ContentAlignment.MiddleLeft
                button.Margin = New Padding(0, 0, 0, 7)
                button.Tag = kind
                parent.Controls.Add(button)
            Next
            parent.Controls.Add(HeaderLabel("TOOLS", 22))
            Dim breadboard = MakeButton("BREADBOARD VIEW", AddressOf ToggleBreadboard)
            breadboard.Dock = DockStyle.Top
            parent.Controls.Add(breadboard)
            Dim threeD = MakeButton("3D INSPECTOR", AddressOf ShowThreeD)
            threeD.Dock = DockStyle.Top
            parent.Controls.Add(threeD)
            Dim xView = MakeButton("X VIEW", AddressOf SetXView)
            xView.Dock = DockStyle.Top
            parent.Controls.Add(xView)
            Dim yView = MakeButton("Y VIEW", AddressOf SetYView)
            yView.Dock = DockStyle.Top
            parent.Controls.Add(yView)
            Dim zView = MakeButton("Z VIEW", AddressOf SetZView)
            zView.Dock = DockStyle.Top
            parent.Controls.Add(zView)
            Dim testPoint = MakeButton("ADD TEST POINT", AddressOf AddTestPoint)
            testPoint.Dock = DockStyle.Top
            parent.Controls.Add(testPoint)
            Dim addWireButton = MakeButton("ADD WIRE", AddressOf AddWire)
            addWireButton.Dock = DockStyle.Top
            parent.Controls.Add(addWireButton)
            Dim arrangeButton = MakeButton("AUTO ARRANGE", AddressOf AutoArrange)
            arrangeButton.Dock = DockStyle.Top
            parent.Controls.Add(arrangeButton)
            Dim validateButton = MakeButton("VALIDATE DESIGN", AddressOf ValidateDesign)
            validateButton.Dock = DockStyle.Top
            parent.Controls.Add(validateButton)
            Dim bomButton = MakeButton("BILL OF MATERIALS", AddressOf ShowBillOfMaterials)
            bomButton.Dock = DockStyle.Top
            parent.Controls.Add(bomButton)
            assemblyTree = BuildComponentTree()
            parent.Controls.Add(assemblyTree)
        End Sub

        Private Function BuildComponentTree() As TreeView
            Dim tree = New TreeView With {.Dock = DockStyle.Top, .Height = 180, .BackColor = Color.FromArgb(15, 25, 30), .ForeColor = Color.FromArgb(220, 229, 232), .BorderStyle = BorderStyle.FixedSingle, .ShowLines = True, .FullRowSelect = True, .HideSelection = False, .HotTracking = True, .ShowNodeToolTips = True}
            Dim root = tree.Nodes.Add("Electropad assembly")
            root.NodeFont = New Font("Segoe UI", 9, FontStyle.Bold)
            For Each component In project.Components
                Dim node = root.Nodes.Add(component.Reference & "  /  " & component.Kind & If(component.Hidden, "  [HIDDEN]", ""))
                node.Tag = component
                node.ToolTipText = "Right-click for edit, duplicate, hide/restore, or delete"
            Next
            For Each wire In project.Wires
                Dim node = root.Nodes.Add(wire.Reference & "  /  Wire  " & wire.LengthMm.ToString("0.#") & " mm  /  " & wire.Material)
                node.Tag = wire
                node.ToolTipText = "Material: " & wire.Material & " | Right-click for material and mechanical data"
                node.ToolTipText = "Double-click a wire on the canvas to edit dimensions"
            Next
            root.Expand()
            AddHandler tree.AfterSelect, Sub(sender, args)
                                             If args.Node Is Nothing OrElse args.Node Is root Then Return
                                             If TypeOf args.Node.Tag Is CircuitComponent Then
                                                 SelectComponent(DirectCast(args.Node.Tag, CircuitComponent))
                                             ElseIf TypeOf args.Node.Tag Is CircuitWire Then
                                                 SelectWire(DirectCast(args.Node.Tag, CircuitWire))
                                             End If
                                         End Sub
            AddHandler tree.NodeMouseClick, Sub(sender, args)
                                                 If args.Button = MouseButtons.Right AndAlso args.Node.Tag IsNot Nothing Then
                                                     tree.SelectedNode = args.Node
                                                     ShowAssemblyMenu(tree, args.Node)
                                                 End If
                                             End Sub
            Return tree
        End Function

        Private Sub AddInspector(parent As Panel)
            parent.Controls.Add(HeaderLabel("INSPECTOR"))
            selectionLabel.AutoSize = False
            selectionLabel.Height = 42
            selectionLabel.Font = New Font("Segoe UI", 12, FontStyle.Bold)
            parent.Controls.Add(selectionLabel)
            parent.Controls.Add(HeaderLabel("IDENTITY", 8))
            AddField(parent, "Reference", referenceBox)
            AddField(parent, "Value", valueBox)
            Dim apply = MakeButton("APPLY NAME & VALUE", AddressOf ApplyIdentity)
            apply.Dock = DockStyle.Top
            parent.Controls.Add(apply)
            parent.Controls.Add(HeaderLabel("APPEARANCE", 22))
            AddCombo(parent, "Component colour", colorCombo, {"Copper", "Teal", "Amber", "Violet"})
            AddCombo(parent, "Circuit material", materialCombo, MaterialCatalog.Materials.Select(Function(item) item.Name).Concat({"Options"}).ToArray())
            AddHandler materialCombo.SelectedIndexChanged, AddressOf MaterialSelectionChanged
            materialStateLabel.Text = "ACTIVE MATERIAL: " & project.Material
            materialStateLabel.Dock = DockStyle.Top
            materialStateLabel.Height = 24
            materialStateLabel.ForeColor = Color.FromArgb(85, 224, 208)
            materialStateLabel.Font = New Font("Segoe UI", 8, FontStyle.Bold)
            parent.Controls.Add(materialStateLabel)
            Dim thermalButton = MakeButton("SHOW THERMAL DATA", AddressOf ShowThermalData)
            thermalButton.Dock = DockStyle.Top
            parent.Controls.Add(thermalButton)
            parent.Controls.Add(HeaderLabel("WIRE GEOMETRY", 22))
            AddSlider(parent, "Wire length", lengthSlider, 1, 100, 42)
            AddSlider(parent, "Wire radius / boldness", radiusSlider, 1, 8, 2)
            parent.Controls.Add(HeaderLabel("TEST POINT  TP1", 22))
            voltageLabel = MetricLabel(parent, "Voltage", "0.00 V")
            currentLabel = MetricLabel(parent, "Current", "0.00 mA")
            wireResistanceLabel = MetricLabel(parent, "Wire resistance", "0.041 Ω")
            totalResistanceLabel = MetricLabel(parent, "Total resistance", "0.000000 Ω")
            powerLabel = MetricLabel(parent, "Power", "0.000000 W")
            Dim view3D = MakeButton("OPEN 3D INSPECTOR", AddressOf ShowThreeD)
            view3D.Dock = DockStyle.Top
            view3D.ForeColor = Color.FromArgb(85, 224, 208)
            parent.Controls.Add(view3D)
        End Sub

        Private Function HeaderLabel(text As String, Optional top As Integer = 0, Optional color As Color = Nothing) As Label
            Dim label = New Label With {.Text = text, .Dock = DockStyle.Top, .Height = 29 + top, .Padding = New Padding(0, top, 0, 0), .Font = New Font("Segoe UI", 8, FontStyle.Bold), .ForeColor = If(color = Nothing, Color.FromArgb(145, 160, 170), color)}
            Return label
        End Function

        Private Sub AddField(parent As Panel, caption As String, field As TextBox)
            Dim row = New Panel With {.Dock = DockStyle.Top, .Height = 31}
            row.Controls.Add(New Label With {.Text = caption, .Dock = DockStyle.Left, .Width = 120, .ForeColor = Color.FromArgb(145, 160, 170), .TextAlign = ContentAlignment.MiddleLeft})
            field.Dock = DockStyle.Fill
            field.BackColor = Color.FromArgb(15, 25, 30)
            field.ForeColor = Color.White
            row.Controls.Add(field)
            parent.Controls.Add(row)
        End Sub

        Private Sub AddCombo(parent As Panel, caption As String, combo As ComboBox, values As String())
            Dim row = New Panel With {.Dock = DockStyle.Top, .Height = 34}
            row.Controls.Add(New Label With {.Text = caption, .Dock = DockStyle.Left, .Width = 120, .ForeColor = Color.FromArgb(145, 160, 170), .TextAlign = ContentAlignment.MiddleLeft})
            combo.Items.AddRange(values)
            combo.Dock = DockStyle.Fill
            combo.DropDownStyle = ComboBoxStyle.DropDownList
            combo.BackColor = Color.FromArgb(15, 25, 30)
            combo.ForeColor = Color.FromArgb(242, 245, 247)
            combo.FlatStyle = FlatStyle.Flat
            row.Controls.Add(combo)
            parent.Controls.Add(row)
            AddHandler combo.SelectedIndexChanged, Sub() canvas.Invalidate()
        End Sub

        Private Sub AddSlider(parent As Panel, caption As String, slider As TrackBar, minimum As Integer, maximum As Integer, value As Integer)
            Dim label = New Label With {.Text = caption, .Dock = DockStyle.Top, .Height = 20, .ForeColor = Color.FromArgb(145, 160, 170)}
            parent.Controls.Add(label)
            slider.Minimum = minimum : slider.Maximum = maximum : slider.Value = value : slider.Dock = DockStyle.Top : slider.Height = 32 : slider.TickStyle = TickStyle.None
            parent.Controls.Add(slider)
            AddHandler slider.ValueChanged, Sub() UpdateWireValues()
        End Sub

        Private Function MetricLabel(parent As Panel, caption As String, initial As String) As Label
            Dim label = New Label With {.Text = caption & "                                      " & initial, .Dock = DockStyle.Top, .Height = 26, .ForeColor = Color.FromArgb(145, 160, 170)}
            parent.Controls.Add(label)
            Return label
        End Function

        Private Function MakeButton(text As String, click As Action) As Button
            Dim button = New Button With {.Text = text, .Height = 36, .FlatStyle = FlatStyle.Flat, .BackColor = Color.FromArgb(33, 50, 58), .ForeColor = Color.FromArgb(242, 245, 247), .Font = New Font("Segoe UI", 9, FontStyle.Bold), .Padding = New Padding(10, 0, 10, 0), .Cursor = Cursors.Hand, .UseVisualStyleBackColor = False}
            AddHandler button.Click, Sub() click()
            button.FlatAppearance.BorderColor = Color.FromArgb(44, 62, 69)
            button.FlatAppearance.MouseOverBackColor = Color.FromArgb(48, 76, 82)
            button.FlatAppearance.MouseDownBackColor = Color.FromArgb(85, 150, 145)
            Return button
        End Function

        Private Function ComponentPrefix(kind As String) As String
            Select Case kind
                Case "Resistor" : Return "R"
                Case "LED", "Diode" : Return "D"
                Case "Battery" : Return "V"
                Case "Ground" : Return "GND"
                Case "Capacitor" : Return "C"
                Case "Inductor" : Return "L"
                Case "Transistor" : Return "Q"
                Case "OpAmp", "IC" : Return "U"
                Case "Fuse" : Return "F"
                Case "Potentiometer" : Return "RV"
                Case "Speaker" : Return "SP"
                Case "Motor" : Return "M"
                Case Else : Return "J"
            End Select
        End Function

        Private Function NextReference(kind As String) As String
            Dim prefix = ComponentPrefix(kind)
            Dim number = 1
            While project.Components.Any(Function(item) item.Reference = prefix & number)
                number += 1
            End While
            Return prefix & number
        End Function

        Private Function ComponentSymbol(kind As String) As String
            Select Case kind
                Case "Resistor" : Return "R"
                Case "LED" : Return "LED"
                Case "Battery" : Return "BAT"
                Case "Switch" : Return "SW"
                Case "Ground" : Return "GND"
                Case "Capacitor" : Return "C"
                Case "Inductor" : Return "L"
                Case "Diode" : Return "D"
                Case "Transistor" : Return "Q"
                Case "OpAmp" : Return "OP"
                Case "IC" : Return "IC"
                Case "Fuse" : Return "F"
                Case "Potentiometer" : Return "POT"
                Case "Speaker" : Return "SPK"
                Case "Motor" : Return "M"
                Case Else : Return "CON"
            End Select
        End Function

        Private Sub RefreshAssemblyTree()
            If assemblyTree Is Nothing Then Return
            Dim selectedReference = If(selected Is Nothing, Nothing, selected.Reference)
            assemblyTree.Nodes.Clear()
            Dim root = assemblyTree.Nodes.Add("Electropad assembly")
            root.NodeFont = New Font("Segoe UI", 9, FontStyle.Bold)
            For Each component In project.Components
                Dim node = root.Nodes.Add(component.Reference & "  /  " & component.Kind & If(component.Hidden, "  [HIDDEN]", ""))
                node.Tag = component
                If component.Reference = selectedReference Then assemblyTree.SelectedNode = node
            Next
            For Each wire In project.Wires
                Dim node = root.Nodes.Add(wire.Reference & "  /  Wire  " & wire.LengthMm.ToString("0.#") & " mm")
                node.Tag = wire
            Next
            root.Expand()
        End Sub

        Private Sub ShowAssemblyMenu(tree As TreeView, node As TreeNode)
            Dim component = TryCast(node.Tag, CircuitComponent)
            Dim wire = TryCast(node.Tag, CircuitWire)
            Dim menu = NewContextMenu()
            Dim deleteItem = menu.Items.Add("Delete")
            AddHandler deleteItem.Click, Sub()
                                            If component IsNot Nothing Then
                                                DeleteComponent(component)
                                            Else
                                                DeleteWire(wire)
                                            End If
                                        End Sub
            If component IsNot Nothing Then
                Dim hideItem = menu.Items.Add(If(component.Hidden, "Restore", "Hide"))
                AddHandler hideItem.Click, Sub()
                                                component.Hidden = Not component.Hidden
                                                RefreshAssemblyTree()
                                                canvas.Invalidate()
                                            End Sub
                Dim duplicateItem = menu.Items.Add("Duplicate")
                AddHandler duplicateItem.Click, Sub() DuplicateComponent(component)
                Dim editItem = menu.Items.Add("Edit value")
                AddHandler editItem.Click, Sub() EditSelectedValue(component)
            ElseIf wire IsNot Nothing Then
                Dim editItem = menu.Items.Add("Edit dimensions")
                AddHandler editItem.Click, Sub() EditWire(wire)
                Dim materialItem = menu.Items.Add("Apply material")
                AddHandler materialItem.Click, Sub() ApplyWireMaterial(wire)
                Dim mechanicalItem = menu.Items.Add("View mechanical")
                AddHandler mechanicalItem.Click, Sub() ShowMechanicalData(wire)
                Dim duplicateItem = menu.Items.Add("Duplicate")
                AddHandler duplicateItem.Click, Sub() DuplicateWire(wire)
            End If
            menu.Show(tree, tree.PointToClient(Cursor.Position))
        End Sub

        Private Function NewContextMenu() As ContextMenuStrip
            Dim menu = New ContextMenuStrip With {.BackColor = Color.FromArgb(27, 42, 48), .ForeColor = Color.FromArgb(242, 245, 247), .ShowImageMargin = False}
            Return menu
        End Function

        Private Sub ShowComponentCanvasMenu(component As CircuitComponent, location As Point)
            If component Is Nothing Then Return
            selected = component
            UpdateInspector()
            Dim menu = NewContextMenu()
            Dim editItem = menu.Items.Add("Edit value")
            AddHandler editItem.Click, Sub() EditSelectedValue(component)
            Dim duplicateItem = menu.Items.Add("Duplicate")
            AddHandler duplicateItem.Click, Sub() DuplicateComponent(component)
            Dim hideItem = menu.Items.Add(If(component.Hidden, "Restore", "Hide"))
            AddHandler hideItem.Click, Sub()
                                            component.Hidden = Not component.Hidden
                                            RefreshAssemblyTree()
                                            canvas.Invalidate()
                                        End Sub
            Dim deleteItem = menu.Items.Add("Delete")
            AddHandler deleteItem.Click, Sub() DeleteComponent(component)
            menu.Show(canvas, location)
        End Sub

        Private Sub ShowWireCanvasMenu(wire As CircuitWire, location As Point)
            If wire Is Nothing Then Return
            SelectWire(wire)
            Dim menu = NewContextMenu()
            Dim editItem = menu.Items.Add("Edit dimensions")
            AddHandler editItem.Click, Sub() EditWire(wire)
            Dim materialItem = menu.Items.Add("Apply material")
            AddHandler materialItem.Click, Sub() ApplyWireMaterial(wire)
            Dim mechanicalItem = menu.Items.Add("View mechanical")
            AddHandler mechanicalItem.Click, Sub() ShowMechanicalData(wire)
            Dim deleteItem = menu.Items.Add("Delete")
            AddHandler deleteItem.Click, Sub() DeleteWire(wire)
            menu.Show(canvas, location)
        End Sub

        Private Sub DeleteComponent(component As CircuitComponent)
            project.Components.Remove(component)
            project.Wires.RemoveAll(Function(wire) wire.StartReference = component.Reference OrElse wire.EndReference = component.Reference)
            If selected Is component Then selected = project.Components.FirstOrDefault()
            UpdateInspector()
            RefreshAssemblyTree()
            canvas.Invalidate()
        End Sub

        Private Sub DeleteWire(wire As CircuitWire)
            If wire Is Nothing Then Return
            project.Wires.Remove(wire)
            RefreshAssemblyTree()
            canvas.Invalidate()
        End Sub

        Private Sub SelectWire(wire As CircuitWire)
            selectedWire = wire
            If wire Is Nothing Then Return
            selectionLabel.Text = wire.Reference & "  /  WIRE  /  " & wire.Material
            statusLabel.Text = "●  WIRE SELECTED                         " & wire.Reference & " · " & wire.LengthMm.ToString("0.###") & " mm · " & wire.RadiusMm.ToString("0.###") & " mm radius"
            canvas.Invalidate()
        End Sub

        Private Sub ApplyWireMaterial(wire As CircuitWire)
            If wire Is Nothing Then Return
            Using manager As New MaterialManagerDialog(wire.Material)
                If manager.ShowDialog(Me) = DialogResult.OK AndAlso manager.SelectedMaterial IsNot Nothing Then
                    Using host As New CommandHostDialog(manager.SelectedMaterial)
                        host.ShowDialog(Me)
                    End Using
                    wire.Material = manager.SelectedMaterial.Name
                    RefreshAssemblyTree()
                    SelectWire(wire)
                End If
            End Using
        End Sub

        Private Sub ShowMechanicalData(wire As CircuitWire)
            If wire Is Nothing Then Return
            Using mechanical As New MechanicalManagerDialog(wire)
                mechanical.ShowDialog(Me)
            End Using
        End Sub

        Private Sub DuplicateComponent(component As CircuitComponent)
            Dim copy = New CircuitComponent With {.Kind = component.Kind, .Reference = NextReference(component.Kind), .Value = component.Value, .Color = component.Color, .X = component.X + 28, .Y = component.Y + 28, .Z = component.Z, .RotationX = component.RotationX, .RotationY = component.RotationY, .RotationZ = component.RotationZ, .LengthMm = component.LengthMm, .BodyDiameterMm = component.BodyDiameterMm, .HeightMm = component.HeightMm, .Symbol = component.Symbol}
            project.Components.Add(copy)
            selected = copy
            UpdateInspector()
            RefreshAssemblyTree()
            canvas.Invalidate()
        End Sub

        Private Sub DuplicateWire(wire As CircuitWire)
            If wire Is Nothing Then Return
            project.Wires.Add(New CircuitWire With {.Reference = "W" & (project.Wires.Count + 1), .StartReference = wire.StartReference, .EndReference = wire.EndReference, .LengthMm = wire.LengthMm, .RadiusMm = wire.RadiusMm, .Material = wire.Material})
            RefreshAssemblyTree()
            canvas.Invalidate()
        End Sub

        Private Sub AddComponent(kind As String)
            Dim reference = NextReference(kind)
            Dim component = New CircuitComponent With {.Kind = kind, .Reference = reference, .Value = If(kind = "Resistor", 220, If(kind = "Battery", 9, 0)), .X = 270 + (project.Components.Count Mod 2) * 220, .Y = 100 + (project.Components.Count Mod 3) * 125, .LengthMm = If(kind = "Battery", 18, If(kind = "Resistor", 10, 5)), .BodyDiameterMm = If(kind = "Battery", 10, If(kind = "Resistor", 4, 5)), .HeightMm = If(kind = "Battery", 35, If(kind = "LED", 8, 4)), .Symbol = ComponentSymbol(kind)}
            project.Components.Add(component)
            selected = component
            UpdateInspector()
            canvas.Invalidate()
            RefreshAssemblyTree()
            statusLabel.Text = "●  LOCAL PROJECT                         Added " & component.Reference & " · " & project.Components.Count & " components"
        End Sub

        Private Sub SelectComponent(component As CircuitComponent)
            selected = component
            UpdateInspector()
        End Sub

        Private Sub EditSelectedValue(component As CircuitComponent)
            If component Is Nothing Then Return
            Using dialog As New ValueEditorDialog(component)
                If dialog.ShowDialog(Me) = DialogResult.OK Then
                    selected = component
                    UpdateInspector()
                    canvas.Invalidate()
                End If
            End Using
        End Sub

        Private Sub AddWire()
            If selected Is Nothing Then Return
            Dim target = project.Components.FirstOrDefault(Function(item) item IsNot selected AndAlso Not item.Hidden)
            If target Is Nothing Then Return
            Dim number = project.Wires.Count + 1
            project.Wires.Add(New CircuitWire With {.Reference = "W" & number, .StartReference = selected.Reference, .EndReference = target.Reference, .LengthMm = project.WireLength, .RadiusMm = project.WireRadius, .Material = project.Material})
            RefreshAssemblyTree()
            canvas.Invalidate()
            statusLabel.Text = "●  LOCAL PROJECT                         Added W" & number & " · " & selected.Reference & " to " & target.Reference
        End Sub

        Private Sub AutoArrange()
            Dim visible = project.Components.Where(Function(item) Not item.Hidden).ToList()
            For index = 0 To visible.Count - 1
                visible(index).X = 90 + (index Mod 3) * 220
                visible(index).Y = 105 + (index \ 3) * 125
            Next
            canvas.Invalidate()
            statusLabel.Text = "●  DESIGN TOOLS                         Components arranged on a 220 x 125 grid"
        End Sub

        Private Sub ValidateDesign()
            Dim issues = CircuitServices.ComponentReferenceIssues(project)
            Using dialog As New ValidationDialog(issues)
                dialog.ShowDialog(Me)
            End Using
        End Sub

        Private Sub ShowBillOfMaterials()
            Using dialog As New BillOfMaterialsDialog(project)
                dialog.ShowDialog(Me)
            End Using
        End Sub

        Private Sub EditWire(wire As CircuitWire)
            If wire Is Nothing Then Return
            Using dialog As New WireDimensionDialog(wire)
                If dialog.ShowDialog(Me) = DialogResult.OK Then
                    project.WireLength = wire.LengthMm
                    project.WireRadius = wire.RadiusMm
                    RefreshAssemblyTree()
                    canvas.Invalidate()
                End If
            End Using
        End Sub

        Private Sub MaterialSelectionChanged(sender As Object, args As EventArgs)
            If materialCombo.SelectedItem Is Nothing Then Return
            If CStr(materialCombo.SelectedItem) = "Options" Then
                Dim currentName = project.Material
                Using manager As New MaterialManagerDialog(currentName)
                    If manager.ShowDialog(Me) = DialogResult.OK Then
                        ApplyMaterialThroughCommandHost(manager.SelectedMaterial)
                    Else
                        materialCombo.SelectedItem = currentName
                    End If
                End Using
            Else
                project.Material = CStr(materialCombo.SelectedItem)
                materialStateLabel.Text = "ACTIVE MATERIAL: " & project.Material
                UpdateWireValues()
            End If
        End Sub

        Private Sub ShowThermalData()
            Using thermal As New ThermalDataDialog(MaterialCatalog.Find(project.Material))
                thermal.ShowDialog(Me)
            End Using
        End Sub

        Private Sub ApplyMaterialThroughCommandHost(material As MaterialSpec)
            If material Is Nothing Then Return
            Using host As New CommandHostDialog(material)
                host.ShowDialog(Me)
            End Using
            project.Material = material.Name
            materialCombo.SelectedItem = material.Name
            materialStateLabel.Text = "ACTIVE MATERIAL: " & project.Material
            UpdateWireValues()
            statusLabel.Text = "●  MATERIAL APPLIED                         " & material.Name & " · " & material.Resistivity.ToString("0.######") & " Ω·mm²/m"
        End Sub

        Private Sub UpdateInspector()
            If selected Is Nothing Then Return
            selectionLabel.Text = selected.Reference & "  /  " & selected.Kind.ToUpperInvariant()
            referenceBox.Text = selected.Reference
            valueBox.Text = selected.Value.ToString("0.##")
            colorCombo.SelectedItem = If(selected.Color = "Blue", "Teal", selected.Color)
            materialCombo.SelectedItem = project.Material
            materialStateLabel.Text = "ACTIVE MATERIAL: " & project.Material
            UpdateWireValues()
        End Sub

        Private Sub ApplyIdentity()
            If selected Is Nothing Then Return
            Dim oldReference = selected.Reference
            selected.Reference = referenceBox.Text.Trim()
            For Each wire In project.Wires
                If wire.StartReference = oldReference Then wire.StartReference = selected.Reference
                If wire.EndReference = oldReference Then wire.EndReference = selected.Reference
            Next
            Double.TryParse(valueBox.Text, selected.Value)
            RefreshAssemblyTree()
            UpdateInspector()
            canvas.Invalidate()
        End Sub

        Private Sub UpdateWireValues()
            project.WireLength = If(lengthSlider.Value = 0, 1, lengthSlider.Value)
            project.WireRadius = If(radiusSlider.Value = 0, 1, radiusSlider.Value)
            Dim material = MaterialCatalog.Find(project.Material)
            Dim resistance = TotalWireResistance(material)
            Dim sourceVoltage = If(project.Components.FirstOrDefault(Function(item) item.Kind = "Battery")?.Value, 0)
            Dim componentResistance = If(selected Is Nothing OrElse selected.Kind <> "Resistor", 0, Math.Max(0, selected.Value))
            Dim totalResistance = componentResistance + resistance
            Dim current = If(totalResistance <= 0, 0, sourceVoltage / totalResistance)
            wireResistanceLabel.Text = "Wire resistance                              " & resistance.ToString("0.000000") & " Ω"
            totalResistanceLabel.Text = "Total resistance                            " & totalResistance.ToString("0.000000") & " Ω"
            voltageLabel.Text = "Voltage                                      " & sourceVoltage.ToString("0.000000") & " V"
            currentLabel.Text = "Current                                      " & (current * 1000).ToString("0.000000") & " mA"
            powerLabel.Text = "Power                                        " & (sourceVoltage * current).ToString("0.000000") & " W"
            canvas.Invalidate()
        End Sub

        Private Function WireResistance(material As MaterialSpec, lengthMm As Double, radiusMm As Double) As Double
            Return CircuitServices.WireResistance(material, lengthMm, radiusMm)
        End Function

        Private Function TotalWireResistance(material As MaterialSpec) As Double
            Return CircuitServices.TotalWireResistance(project)
        End Function

        Private Sub Simulate()
            simulationRunning = Not simulationRunning
            If simulationRunning Then
                Dim resistor = project.Components.FirstOrDefault(Function(item) item.Kind = "Resistor")
                Dim battery = project.Components.FirstOrDefault(Function(item) item.Kind = "Battery")
                Dim resistance = If(resistor Is Nothing OrElse resistor.Value <= 0, 220, resistor.Value)
                Dim voltage = If(battery Is Nothing, 9, battery.Value)
                Dim wireOhms = TotalWireResistance(MaterialCatalog.Find(project.Material))
                Dim totalResistance = resistance + wireOhms
                Dim current = If(totalResistance <= 0, 0, voltage / totalResistance)
                voltageLabel.Text = "Voltage                                      " & voltage.ToString("0.000000") & " V"
                currentLabel.Text = "Current                                      " & (current * 1000).ToString("0.000000") & " mA"
                wireResistanceLabel.Text = "Wire resistance                              " & wireOhms.ToString("0.000000") & " Ω"
                totalResistanceLabel.Text = "Total resistance                            " & totalResistance.ToString("0.000000") & " Ω"
                powerLabel.Text = "Power                                        " & (voltage * current).ToString("0.000000") & " W"
                statusLabel.Text = "●  SIMULATION RUNNING                     Ohm's law solved · test point TP1 active"
            Else
                UpdateWireValues()
                statusLabel.Text = "●  LOCAL PROJECT                         Simulation paused · test point remains calculated"
            End If
            canvas.Invalidate()
        End Sub

        Private Sub ToggleBreadboard()
            breadboardMode = Not breadboardMode
            canvas.BreadboardMode = breadboardMode
            statusLabel.Text = If(breadboardMode, "●  BREADBOARD VIEW                         Rails mapped · drag components to sockets", "●  SCHEMATIC VIEW                         Ready · 4 connections")
            canvas.Invalidate()
        End Sub

        Private Sub AddTestPoint()
            statusLabel.Text = "●  TEST POINT TP1                         Click a node on the canvas to probe voltage, current and wire resistance"
        End Sub

        Private Sub ShowThreeD()
            render3D = True
            breadboardMode = False
            canvas.Render3D = True
            canvas.BreadboardMode = False
            statusLabel.Text = "●  3D INSPECTOR                         Isometric solid view · " & project.Components.Count & " bodies · " & project.Material
            canvas.Invalidate()
        End Sub

        Private Sub SetXView()
            canvas.SetCameraView("X")
            ShowThreeD()
        End Sub

        Private Sub SetYView()
            canvas.SetCameraView("Y")
            ShowThreeD()
        End Sub

        Private Sub SetZView()
            canvas.SetCameraView("Z")
            ShowThreeD()
        End Sub

        Private Sub ShowExchange()
    Using dialog As New ExchangeDialog(project, Sub(outputMode)
                                                 If outputMode = "3D" Then
                                                     render3D = True
                                                     canvas.Render3D = True
                                                     canvas.BreadboardMode = False
                                                     breadboardMode = False
                                                     statusLabel.Text = "●  EXCHANGE COMPLETE                     Converted 2D schematic to 3D scene"
                                                     canvas.Invalidate()
                                                 Else
                                                     ' Trigger the custom terminal execution window for 2D downgrades
                                                     Using cdmHost As New ConversionExchange2DForm(project)
                                                         If cdmHost.ShowDialog(Me) = DialogResult.OK Then
                                                             render3D = False
                                                             canvas.Render3D = False
                                                             canvas.BreadboardMode = False
                                                             breadboardMode = False
                                                             statusLabel.Text = "●  EXCHANGE COMPLETE                     Reverted 3D bodies to 2D Schematic view"
                                                             canvas.Invalidate()
                                                         End If
                                                     End Using
                                                 End If
                                             End Sub)
        dialog.ShowDialog(Me)
    End Using
End Sub


        Private Sub ExportProject()
            Using dialog As New SaveFileDialog With {.Filter = "Circuit Exchange (*.cex)|*.cex|JSON (*.json)|*.json", .FileName = "untitled.cex"}
                If dialog.ShowDialog() = DialogResult.OK Then File.WriteAllText(dialog.FileName, JsonSerializer.Serialize(project, New JsonSerializerOptions With {.WriteIndented = True}))
            End Using
        End Sub

        Private Sub ImportProject()
            Using dialog As New OpenFileDialog With {.Filter = "Circuit Exchange (*.cex;*.json)|*.cex;*.json"}
                If dialog.ShowDialog() = DialogResult.OK Then
                    Dim loaded = JsonSerializer.Deserialize(Of CircuitProject)(File.ReadAllText(dialog.FileName))
                    If loaded IsNot Nothing Then
                        project.Material = loaded.Material : project.WireLength = loaded.WireLength : project.WireRadius = loaded.WireRadius
                            project.Components.Clear() : project.Components.AddRange(loaded.Components)
                            project.Wires.Clear()
                            If loaded.Wires IsNot Nothing Then project.Wires.AddRange(loaded.Wires)
                        selected = project.Components.FirstOrDefault()
                        RefreshAssemblyTree() : UpdateInspector() : canvas.Invalidate()
                    End If
                End If
            End Using
        End Sub
    End Class

    Public Class CircuitCanvas
        Inherits Panel
        <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
        Public Property Project As CircuitProject
        <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
        Public Property Render3D As Boolean
        <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
        Public Property BreadboardMode As Boolean
        <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
        Public Property OnComponentSelected As Action(Of CircuitComponent)
        <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
        Public Property OnComponentAdded As Action(Of String)
        <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
        Public Property OnWireEdit As Action(Of CircuitWire)
        <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
        Public Property OnAddWire As Action
        <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
        Public Property OnComponentDelete As Action(Of CircuitComponent)
        <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
        Public Property OnComponentContextMenu As Action(Of CircuitComponent, Point)
        <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
        Public Property OnWireSelected As Action(Of CircuitWire)
        <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
        Public Property OnWireContextMenu As Action(Of CircuitWire, Point)

        Private d3dRenderer As Direct3D11Renderer
        Private ReadOnly gridPen As New Pen(Color.FromArgb(28, 48, 56), 1)
        Private ReadOnly canvasLabelFont As New Font("Segoe UI", 9, FontStyle.Bold)
        Private ReadOnly canvasLabelBrush As New SolidBrush(Color.FromArgb(55, 82, 92))
        Private ReadOnly componentTitleFont As New Font("Segoe UI", 9, FontStyle.Bold)
        Private ReadOnly componentValueFont As New Font("Segoe UI", 15, FontStyle.Bold)
        Private ReadOnly solidLabelFont As New Font("Segoe UI", 8, FontStyle.Bold)
        Private ReadOnly dimensionFont As New Font("Segoe UI", 8, FontStyle.Bold)

        Public Sub New()
            DoubleBuffered = True
            d3dRenderer = New Direct3D11Renderer(Me)
            AddHandler MouseDown, AddressOf CanvasMouseDown
            AddHandler MouseMove, AddressOf CanvasMouseMove
            AddHandler MouseUp, AddressOf CanvasMouseUp
            AddHandler MouseWheel, AddressOf CanvasMouseWheel
            AddHandler DoubleClick, AddressOf CanvasDoubleClick
            AddHandler KeyDown, AddressOf CanvasKeyDown
            AddHandler KeyUp, AddressOf CanvasKeyUp
            TabStop = True
        End Sub

        Protected Overrides Sub OnHandleDestroyed(e As EventArgs)
            d3dRenderer.Dispose()
            MyBase.OnHandleDestroyed(e)
        End Sub

        Protected Overrides Sub Dispose(disposing As Boolean)
            If disposing Then
                d3dRenderer.Dispose()
                gridPen.Dispose()
                canvasLabelFont.Dispose()
                canvasLabelBrush.Dispose()
                componentTitleFont.Dispose()
                componentValueFont.Dispose()
                solidLabelFont.Dispose()
                dimensionFont.Dispose()
            End If
            MyBase.Dispose(disposing)
        End Sub

        Protected Overrides Sub OnResize(e As EventArgs)
            MyBase.OnResize(e)
            If d3dRenderer IsNot Nothing Then d3dRenderer.Resize()
        End Sub

        Private dragging As Boolean
        Private dragOffset As Point
        Private orbiting As Boolean
        Private orbitStart As Point
        Private yaw As Single = -0.45F
        Private pitch As Single = 0.55F
        Private zoom As Single = 1.0F
        Private lastMousePosition As Point
        Private spaceHeld As Boolean
        Private dragMoved As Boolean

        Public Sub SetCameraView(viewName As String)
            Select Case viewName.ToUpperInvariant()
                Case "X"
                    yaw = 0.0F
                    pitch = 0.55F
                Case "Y"
                    yaw = CSng(Math.PI / 2)
                    pitch = 0.55F
                Case "Z"
                    yaw = -0.45F
                    pitch = 1.3F
            End Select
            Invalidate()
        End Sub

        Private Sub CanvasMouseDown(sender As Object, args As MouseEventArgs)
    If Project Is Nothing Then Return
    lastMousePosition = args.Location
    Focus()
Dim hit As CircuitComponent = Nothing
If Render3D Then
    hit = Find3DComponentAt(args.Location)
Else
    hit = Project.Components.FirstOrDefault(Function(item) Not item.Hidden AndAlso New Rectangle(item.X, item.Y, 155, 72).Contains(args.Location))
End If

    Dim wireHit = FindWireAt(args.Location)
    If args.Button = MouseButtons.Right AndAlso hit IsNot Nothing Then
        selectedComponent = hit
        OnComponentSelected?.Invoke(hit)
        OnComponentContextMenu?.Invoke(hit, args.Location)
        Return
    End If
    If args.Button = MouseButtons.Right AndAlso wireHit IsNot Nothing Then
        OnWireSelected?.Invoke(wireHit)
        OnWireContextMenu?.Invoke(wireHit, args.Location)
        Return
    End If
    If Render3D AndAlso args.Button = MouseButtons.Middle Then
        orbiting = True
        orbitStart = args.Location
        Return
    End If
    If hit IsNot Nothing Then
        selectedComponent = hit
        selectedWire = Nothing
        OnComponentSelected?.Invoke(hit)
        If args.Button = MouseButtons.Left Then
            dragging = True
            dragMoved = False
            ' FIX: Calculate mouse offset relative to the component's top-left corner
            dragOffset = New Point(args.X - hit.X, args.Y - hit.Y)
        End If
    ElseIf wireHit IsNot Nothing AndAlso args.Button = MouseButtons.Left Then
        selectedWire = wireHit
        OnWireSelected?.Invoke(wireHit)
        Invalidate()
    End If
End Sub

Private Sub CanvasMouseMove(sender As Object, args As MouseEventArgs)
    lastMousePosition = args.Location
    If orbiting Then
        yaw += (args.X - orbitStart.X) * 0.01F
        pitch = Math.Max(0.15F, Math.Min(1.35F, pitch + (args.Y - orbitStart.Y) * 0.01F))
        orbitStart = args.Location
        Invalidate()
    ElseIf dragging Then
        Dim hit = Project.Components.FirstOrDefault(Function(item) item Is selectedComponent)
        If hit IsNot Nothing Then
            If Render3D Then
                Dim deltaX = args.X - lastMousePosition.X
                Dim deltaY = args.Y - lastMousePosition.Y
                hit.X = Math.Max(-200, Math.Min(1200, hit.X + deltaX))
                hit.Z = Math.Max(-400, Math.Min(400, hit.Z - deltaY))
            Else
                ' FIX: Directly assign new location tracking the cursor position minus the starting grab offset
                dragMoved = True
                hit.X = Math.Max(0, args.X - dragOffset.X)
                hit.Y = Math.Max(0, args.Y - dragOffset.Y)
            End If
            Invalidate()
        End If
    End If
End Sub


        Private selectedComponent As CircuitComponent
        Private selectedWire As CircuitWire

        Private Sub CanvasMouseUp(sender As Object, args As MouseEventArgs)
            dragging = False
            orbiting = False
            dragMoved = False
        End Sub

        Private Sub CanvasMouseWheel(sender As Object, args As MouseEventArgs)
            If Not Render3D Then Return
            zoom = Math.Max(0.55F, Math.Min(2.4F, zoom + If(args.Delta > 0, 0.1F, -0.1F)))
            Invalidate()
        End Sub

        Private Sub CanvasDoubleClick(sender As Object, args As EventArgs)
            If Project Is Nothing Then Return
            Dim wire = FindWireAt(lastMousePosition)
            If wire IsNot Nothing Then
                selectedWire = wire
                OnWireSelected?.Invoke(wire)
                OnWireEdit?.Invoke(wire)
            End If
        End Sub

        Private Sub CanvasKeyDown(sender As Object, args As KeyEventArgs)
            If Render3D AndAlso selectedComponent IsNot Nothing Then
                Dim stepSize = If(args.Shift, 10, 1)
                Select Case args.KeyCode
                    Case Keys.Up
                        selectedComponent.Z += stepSize
                    Case Keys.Down
                        If spaceHeld Then
                            selectedComponent.Y -= stepSize
                        Else
                            selectedComponent.Z -= stepSize
                        End If
                    Case Keys.Left
                        selectedComponent.X += stepSize
                    Case Keys.Right
                        selectedComponent.X -= stepSize
                    Case Keys.Space
                        spaceHeld = True
                        selectedComponent.Y += stepSize
                    Case Else
                        GoTo HandleDelete
                End Select
                Invalidate()
                args.Handled = True
                Return
            End If
HandleDelete:
            If args.KeyCode = Keys.Delete AndAlso selectedComponent IsNot Nothing Then
                OnComponentDelete?.Invoke(selectedComponent)
                selectedComponent = Nothing
                args.Handled = True
            End If
        End Sub

        Private Sub CanvasKeyUp(sender As Object, args As KeyEventArgs)
            If args.KeyCode = Keys.Space Then spaceHeld = False
        End Sub

        Private Function Find3DComponentAt(point As Point) As CircuitComponent
            Dim best As CircuitComponent = Nothing
            Dim bestDistance = Double.MaxValue
            For Each component In Project.Components.Where(Function(item) Not item.Hidden)
                Dim center = ProjectComponentPoint(component)
                Dim distance = Math.Sqrt((point.X - center.X) ^ 2 + (point.Y - center.Y) ^ 2)
                Dim tolerance = Math.Max(28, ComponentVisualWidth(component) / 2)
                If distance <= tolerance AndAlso distance < bestDistance Then
                    best = component
                    bestDistance = distance
                End If
            Next
            Return best
        End Function

        Private Function FindWireAt(point As Point) As CircuitWire
            For Each wire In Project.Wires
                If wire.Hidden Then Continue For
                Dim first = Project.Components.FirstOrDefault(Function(item) item.Reference = wire.StartReference)
                Dim second = Project.Components.FirstOrDefault(Function(item) item.Reference = wire.EndReference)
                If first Is Nothing OrElse second Is Nothing OrElse first.Hidden OrElse second.Hidden Then Continue For
                Dim firstPoint = If(Render3D, ProjectConnectionPoint(first, True), New PointF(first.X + 155, first.Y + 36))
                Dim secondPoint = If(Render3D, ProjectConnectionPoint(second, False), New PointF(second.X, second.Y + 36))
                If DistanceToSegment(point, Point.Round(firstPoint), Point.Round(secondPoint)) <= Math.Max(7, wire.RadiusMm * 2) Then Return wire
            Next
            Return Nothing
        End Function

        Private Function ProjectComponentPoint(component As CircuitComponent) As PointF
            Dim origin = New PointF(Math.Max(260, Width / 2 - 220), Math.Max(175, Height / 2 - 170))
            Dim boardWidth = Math.Min(760, Math.Max(520, Width - 180)) * zoom
            Dim boardHeight = Math.Min(410, Math.Max(300, Height - 190)) * zoom
            Dim sourceWidth = Math.Max(720, Width - 330)
            Dim sourceHeight = Math.Max(480, Height - 260)
            Dim localX = (component.X / sourceWidth - 0.5F) * boardWidth
            Dim localY = (component.Y / sourceHeight - 0.5F) * boardHeight
            Dim rotatedX = localX * CSng(Math.Cos(yaw)) - localY * CSng(Math.Sin(yaw))
            Dim rotatedY = localX * CSng(Math.Sin(yaw)) + localY * CSng(Math.Cos(yaw))
            Return New PointF(origin.X + boardWidth / 2 + rotatedX, origin.Y + boardHeight / 2 + 72 * zoom + rotatedY * CSng(Math.Sin(pitch)) - component.Z * zoom)
        End Function

        Private Function ComponentVisualWidth(component As CircuitComponent) As Single
            Return CSng(Math.Max(24, component.LengthMm * 4.5) * zoom)
        End Function

        Private Function ProjectConnectionPoint(component As CircuitComponent, rightSide As Boolean) As PointF
            Dim center = ProjectComponentPoint(component)
            Dim direction = If(rightSide, 1.0F, -1.0F)
            Return New PointF(center.X + direction * ComponentVisualWidth(component) / 2, center.Y)
        End Function

        Private Function DistanceToSegment(point As Point, startPoint As Point, endPoint As Point) As Double
            Dim dx = endPoint.X - startPoint.X
            Dim dy = endPoint.Y - startPoint.Y
            If dx = 0 AndAlso dy = 0 Then Return Math.Sqrt((point.X - startPoint.X) ^ 2 + (point.Y - startPoint.Y) ^ 2)
            Dim t = Math.Max(0, Math.Min(1, ((point.X - startPoint.X) * dx + (point.Y - startPoint.Y) * dy) / CDbl(dx * dx + dy * dy)))
            Dim closestX = startPoint.X + t * dx
            Dim closestY = startPoint.Y + t * dy
            Return Math.Sqrt((point.X - closestX) ^ 2 + (point.Y - closestY) ^ 2)
        End Function

        Protected Overrides Sub OnPaint(e As PaintEventArgs)
            MyBase.OnPaint(e)
            If Render3D Then d3dRenderer.Render(Color.FromArgb(15, 25, 30))
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias
            If Not Render3D Then
                For x = 0 To Width Step 28 : e.Graphics.DrawLine(gridPen, x, 0, x, Height) : Next
                For y = 0 To Height Step 28 : e.Graphics.DrawLine(gridPen, 0, y, Width, y) : Next
            End If
            If BreadboardMode Then
                DrawBreadboard(e.Graphics)
                DrawWires(e.Graphics)
                For Each component In Project.Components : DrawComponent(e.Graphics, component) : Next
            ElseIf Render3D Then
                DrawIsometricScene(e.Graphics)
            Else
                DrawWires(e.Graphics)
                For Each component In Project.Components : DrawComponent(e.Graphics, component) : Next
            End If
            e.Graphics.DrawString(If(BreadboardMode, "BREADBOARD VIEW", If(Render3D, "3D ISOMETRIC VIEW", "2D SCHEMATIC CANVAS")), canvasLabelFont, canvasLabelBrush, 24, 22)
        End Sub

        Private Sub DrawIsometricScene(graphics As Graphics)
            Dim origin = New PointF(Math.Max(260, Width / 2 - 220), Math.Max(175, Height / 2 - 170))
            Dim boardDepth As Single = 34 * zoom
            Dim boardWidth As Single = Math.Min(760, Math.Max(520, Width - 180)) * zoom
            Dim boardHeight As Single = Math.Min(410, Math.Max(300, Height - 190)) * zoom
            graphics.Clear(Color.FromArgb(12, 20, 25))
            Using floorShadow As New SolidBrush(Color.FromArgb(80, 0, 0, 0))
                graphics.FillEllipse(floorShadow, origin.X - 120, origin.Y + boardHeight + 30, boardWidth + 260, 54 * zoom)
            End Using
            Dim top = {origin, New PointF(origin.X + boardWidth, origin.Y), New PointF(origin.X + boardWidth - 95, origin.Y + boardDepth), New PointF(origin.X - 95, origin.Y + boardDepth)}
            Dim front = {top(3), top(2), New PointF(top(2).X, top(2).Y + boardHeight), New PointF(top(3).X, top(3).Y + boardHeight)}
            Dim side = {top(1), top(2), New PointF(top(2).X, top(2).Y + boardHeight), New PointF(top(1).X, top(1).Y + boardHeight)}
            Using boardBrush As New SolidBrush(Color.FromArgb(42, 93, 72)), frontBrush As New SolidBrush(Color.FromArgb(25, 53, 43)), sideBrush As New SolidBrush(Color.FromArgb(22, 45, 38)), edgePen As New Pen(Color.FromArgb(85, 224, 208), 1.5F)
                graphics.FillPolygon(boardBrush, top) : graphics.FillPolygon(frontBrush, front) : graphics.FillPolygon(sideBrush, side)
                graphics.DrawPolygon(edgePen, top) : graphics.DrawPolygon(edgePen, front) : graphics.DrawPolygon(edgePen, side)
            End Using
            Dim railPen As New Pen(Color.FromArgb(255, 184, 74), Math.Max(1, 2 * zoom))
            graphics.DrawLine(railPen, top(3).X + 12, top(3).Y + 9, top(2).X + 12, top(2).Y + 9)
            graphics.DrawLine(railPen, top(3).X + 12, top(3).Y + boardHeight - 12, top(2).X + 12, top(2).Y + boardHeight - 12)
            railPen.Dispose()
            Using holeBrush As New SolidBrush(Color.FromArgb(145, 194, 150)), holeHighlight As New SolidBrush(Color.FromArgb(205, 240, 185))
                For row = 0 To 11
                    For column = 0 To 25
                        Dim point = New PointF(origin.X + 22 + column * (boardWidth / 27) - row * 4 * zoom, origin.Y + 20 + row * (boardHeight / 15) + column * 1.6F * zoom)
                        Dim diameter = Math.Max(2, 4 * zoom)
                        graphics.FillEllipse(holeBrush, point.X, point.Y, diameter, diameter)
                        graphics.FillEllipse(holeHighlight, point.X + diameter * 0.2F, point.Y + diameter * 0.15F, diameter * 0.35F, diameter * 0.35F)
                    Next
                Next
            End Using
            DrawSolidWires(graphics)
            For Each component In Project.Components
                If component.Hidden Then Continue For
                DrawSolidBody(graphics, component, ProjectComponentPoint(component), zoom)
            Next
        End Sub

        Private Sub DrawSolidWires(graphics As Graphics)
            For Each modelWire In Project.Wires
                If modelWire.Hidden Then Continue For
                Dim first = Project.Components.FirstOrDefault(Function(item) item.Reference = modelWire.StartReference)
                Dim second = Project.Components.FirstOrDefault(Function(item) item.Reference = modelWire.EndReference)
                If first Is Nothing OrElse second Is Nothing OrElse first.Hidden OrElse second.Hidden Then Continue For
                Dim firstPoint = ProjectConnectionPoint(first, True)
                Dim secondPoint = ProjectConnectionPoint(second, False)
                Dim wireColor = If(modelWire Is selectedWire, Color.FromArgb(255, 184, 74), Color.FromArgb(85, 224, 208))
                Using shadowPen As New Pen(Color.FromArgb(100, 0, 0, 0), CSng(Math.Max(3, modelWire.RadiusMm * 3.2F * zoom))), wirePen As New Pen(wireColor, CSng(Math.Max(2, modelWire.RadiusMm * 2.5F * zoom)))
                    graphics.DrawLine(shadowPen, firstPoint.X + 2, firstPoint.Y + 4, secondPoint.X + 2, secondPoint.Y + 4)
                    graphics.DrawLine(wirePen, firstPoint, secondPoint)
                    Using endpoint As New SolidBrush(ControlPaint.Light(wireColor, 0.35F))
                        graphics.FillEllipse(endpoint, firstPoint.X - wirePen.Width, firstPoint.Y - wirePen.Width, wirePen.Width * 2, wirePen.Width * 2)
                        graphics.FillEllipse(endpoint, secondPoint.X - wirePen.Width, secondPoint.Y - wirePen.Width, wirePen.Width * 2, wirePen.Width * 2)
                    End Using
                End Using
            Next
        End Sub

        Private Sub DrawSolidBody(graphics As Graphics, component As CircuitComponent, position As PointF, scale As Single)
            Dim componentColor As Color = If(component.Kind = "Resistor", Color.FromArgb(217, 124, 99), If(component.Kind = "LED", Color.FromArgb(255, 184, 74), If(component.Kind = "Battery", Color.FromArgb(108, 181, 213), If(component.Kind = "Ground", Color.FromArgb(127, 204, 157), Color.FromArgb(168, 138, 227)))))
            Dim width = CSng(Math.Max(24, component.LengthMm * 4.5) * scale)
            Dim height = CSng(Math.Max(18, component.BodyDiameterMm * 6) * scale)
            If component.Kind = "Battery" Then height = CSng(54 * scale)
            position = New PointF(position.X - width / 2, position.Y + height / 2)
            Dim depth As Single = 13 * scale
            Dim front = {position, New PointF(position.X + width, position.Y), New PointF(position.X + width, position.Y - height), New PointF(position.X, position.Y - height)}
            Dim top = {front(3), front(2), New PointF(front(2).X + depth, front(2).Y - depth), New PointF(front(3).X + depth, front(3).Y - depth)}
            Using shadow As New SolidBrush(Color.FromArgb(80, 0, 0, 0)), frontBrush As New SolidBrush(componentColor), topBrush As New SolidBrush(ControlPaint.Light(componentColor, 0.35F)), sideBrush As New SolidBrush(ControlPaint.Dark(componentColor, 0.35F)), pen As New Pen(Color.FromArgb(230, 245, 245), Math.Max(1, scale))
                graphics.FillEllipse(shadow, position.X - 5 * scale, position.Y + 2 * scale, width + 18 * scale, 12 * scale)
                If component.Kind = "Resistor" Then
                    graphics.FillPolygon(sideBrush, {New PointF(position.X + width, position.Y - height / 2), New PointF(position.X + width + depth, position.Y - height / 2 - depth), New PointF(position.X + width + depth, position.Y - depth), New PointF(position.X + width, position.Y)})
                    graphics.FillEllipse(topBrush, position.X, position.Y - height, width, height)
                    graphics.FillRectangle(frontBrush, position.X, position.Y - height / 2, width, height / 2)
                    graphics.DrawEllipse(pen, position.X, position.Y - height, width, height)
                    For band = 1 To 4
                        Dim bandX = position.X + width * (0.18F + band * 0.14F)
                        Using bandPen As New Pen(If(band Mod 2 = 0, Color.FromArgb(245, 215, 125), Color.FromArgb(100, 45, 35)), Math.Max(2, 3 * scale))
                            graphics.DrawLine(bandPen, bandX, position.Y - height * 0.72F, bandX, position.Y - height * 0.08F)
                        End Using
                    Next
                    Using lead As New Pen(Color.Silver, Math.Max(1, 2 * scale))
                        graphics.DrawLine(lead, position.X - 18 * scale, position.Y - height / 2, position.X, position.Y - height / 2)
                        graphics.DrawLine(lead, position.X + width, position.Y - height / 2, position.X + width + 18 * scale, position.Y - height / 2)
                    End Using
                ElseIf component.Kind = "LED" Then
                    graphics.FillEllipse(topBrush, position.X, position.Y - height, width, height)
                    graphics.FillRectangle(frontBrush, position.X, position.Y - height / 2, width, height / 2)
                    graphics.DrawEllipse(pen, position.X, position.Y - height, width, height)
                    graphics.DrawLine(Pens.Silver, position.X + width * 0.35F, position.Y, position.X + width * 0.35F, position.Y + 14 * scale)
                    graphics.DrawLine(Pens.Silver, position.X + width * 0.65F, position.Y, position.X + width * 0.65F, position.Y + 14 * scale)
                ElseIf component.Kind = "Capacitor" Then
                    graphics.FillRectangle(frontBrush, position.X, position.Y - height * 0.65F, width, height * 0.3F)
                    graphics.DrawLine(pen, position.X + width * 0.35F, position.Y - height, position.X + width * 0.35F, position.Y)
                    graphics.DrawLine(pen, position.X + width * 0.65F, position.Y - height, position.X + width * 0.65F, position.Y)
                ElseIf component.Kind = "Inductor" Then
                    Using coilPen As New Pen(ControlPaint.Light(componentColor, 0.25F), Math.Max(2, 3 * scale))
                        For coil = 0 To 4
                            graphics.DrawArc(coilPen, position.X + coil * width / 5, position.Y - height * 0.8F, width / 5 + 4 * scale, height * 0.7F, 0, 180)
                        Next
                    End Using
                ElseIf component.Kind = "Diode" Then
                    graphics.FillRectangle(frontBrush, position.X, position.Y - height * 0.55F, width, height * 0.16F)
                    graphics.DrawLine(pen, position.X + width * 0.65F, position.Y - height * 0.8F, position.X + width * 0.65F, position.Y - height * 0.1F)
                ElseIf component.Kind = "Transistor" OrElse component.Kind = "OpAmp" OrElse component.Kind = "IC" Then
                    graphics.FillPolygon(frontBrush, front)
                    graphics.FillPolygon(topBrush, top)
                    graphics.DrawPolygon(pen, front)
                    graphics.DrawPolygon(pen, top)
                    For pin = 1 To Math.Min(6, If(component.Kind = "IC", 4, 3))
                        Dim pinX = position.X + width * pin / (Math.Min(6, If(component.Kind = "IC", 4, 3)) + 1)
                        graphics.DrawLine(Pens.Silver, pinX, position.Y, pinX, position.Y + 12 * scale)
                    Next
                ElseIf component.Kind = "Battery" Then
                    graphics.FillEllipse(topBrush, position.X, position.Y - height, width, height * 0.38F)
                    graphics.FillRectangle(frontBrush, position.X, position.Y - height * 0.81F, width, height * 0.62F)
                    graphics.FillEllipse(frontBrush, position.X, position.Y - height * 0.38F, width, height * 0.38F)
                    graphics.DrawEllipse(pen, position.X, position.Y - height, width, height * 0.38F)
                Else
                    graphics.FillPolygon(frontBrush, front) : graphics.FillPolygon(topBrush, top) : graphics.DrawPolygon(pen, front) : graphics.DrawPolygon(pen, top)
                End If
            End Using
            graphics.DrawString(component.Reference & "  " & component.LengthMm.ToString("0.#") & " mm", solidLabelFont, Brushes.White, position.X, position.Y - height - 24 * scale)
        End Sub

        Private Sub DrawWires(graphics As Graphics)
            If Project.Components.Count < 2 Then Return
            Dim pixelsPerMillimeter = 2.5F
            For Each modelWire In Project.Wires
                If modelWire.Hidden Then Continue For
                Dim first = Project.Components.FirstOrDefault(Function(item) item.Reference = modelWire.StartReference)
                Dim second = Project.Components.FirstOrDefault(Function(item) item.Reference = modelWire.EndReference)
                If first Is Nothing OrElse second Is Nothing OrElse first.Hidden OrElse second.Hidden Then Continue For
                Dim wireColor = If(modelWire Is selectedWire, Color.FromArgb(255, 184, 74), Color.FromArgb(85, 224, 208))
                Using wire As New Pen(wireColor, CSng(Math.Max(1, modelWire.RadiusMm * pixelsPerMillimeter)))
                    graphics.DrawLine(wire, first.X + 155, first.Y + 36, second.X, second.Y + 36)
                End Using
            Next
            graphics.DrawString(Project.WireLength.ToString("0.#") & " mm  /  " & (Project.WireRadius * 2).ToString("0.#") & " mm dia", dimensionFont, Brushes.WhiteSmoke, 24, Height - 42)
            If selectedWire IsNot Nothing Then
                graphics.DrawString("Selected wire: " & selectedWire.Reference & "  /  " & selectedWire.Material, dimensionFont, Brushes.WhiteSmoke, 24, Height - 62)
            End If
        End Sub

        Private Sub DrawComponent(graphics As Graphics, component As CircuitComponent)
            If component.Hidden Then Return
            Dim componentColor As Color = If(component.Kind = "Resistor", Color.FromArgb(217, 124, 99), If(component.Kind = "LED", Color.FromArgb(255, 184, 74), If(component.Kind = "Battery", Color.FromArgb(108, 181, 213), If(component.Kind = "Ground", Color.FromArgb(127, 204, 157), Color.FromArgb(168, 138, 227)))))
            Dim bounds = New Rectangle(component.X, component.Y, 155, 72)
            Using fill As New SolidBrush(Color.FromArgb(45, 45, 40)), outline As New Pen(componentColor, 1)
                graphics.FillRectangle(fill, bounds) : graphics.DrawRectangle(outline, bounds)
            End Using
            Using titleBrush As New SolidBrush(componentColor)
                graphics.DrawString(component.Reference & "  /  " & component.Kind.ToUpperInvariant() & "  [" & component.Symbol & "]", componentTitleFont, titleBrush, component.X + 12, component.Y + 10)
                graphics.DrawString(If(component.Kind = "LED", "RED", If(component.Kind = "Ground", "0 V", If(component.Kind = "Battery", component.Value.ToString("0.0") & " V", component.Value.ToString("0") & " Ω"))), componentValueFont, Brushes.WhiteSmoke, component.X + 12, component.Y + 30)
            End Using
            Using symbolPen As New Pen(componentColor, 2)
                Dim symbolX = component.X + 116
                Dim symbolY = component.Y + 44
                If component.Symbol = "C" Then
                    graphics.DrawLine(symbolPen, symbolX, symbolY - 12, symbolX, symbolY + 12)
                    graphics.DrawLine(symbolPen, symbolX + 9, symbolY - 12, symbolX + 9, symbolY + 12)
                ElseIf component.Symbol = "R" Then
                    graphics.DrawRectangle(symbolPen, symbolX, symbolY - 7, 18, 14)
                ElseIf component.Symbol = "LED" OrElse component.Symbol = "D" Then
                    graphics.DrawLine(symbolPen, symbolX, symbolY, symbolX + 18, symbolY)
                    graphics.DrawLine(symbolPen, symbolX + 5, symbolY - 7, symbolX + 5, symbolY + 7)
                    graphics.DrawLine(symbolPen, symbolX + 12, symbolY - 7, symbolX + 12, symbolY + 7)
                Else
                    graphics.DrawEllipse(symbolPen, symbolX, symbolY - 9, 18, 18)
                End If
            End Using
        End Sub

        Private Sub DrawBreadboard(graphics As Graphics)
            Using board As New SolidBrush(Color.FromArgb(35, 55, 42)), holes As New SolidBrush(Color.FromArgb(112, 151, 116))
                graphics.FillRectangle(board, 70, 90, Math.Max(600, Width - 150), Math.Min(440, Height - 150))
                For row = 0 To 9
                    For column = 0 To 29
                        graphics.FillEllipse(holes, 95 + column * 22, 120 + row * 30, 5, 5)
                    Next
                Next
            End Using
        End Sub
    End Class

    Public Class ValueEditorDialog
        Inherits Form

        Private ReadOnly component As CircuitComponent
        Private ReadOnly valueBox As New TextBox()

        Public Sub New(source As CircuitComponent)
            component = source
            Text = "550C Value editor (cve.exe)"
            Width = 420
            Height = 190
            StartPosition = FormStartPosition.CenterParent
            FormBorderStyle = FormBorderStyle.FixedDialog
            MaximizeBox = False
            MinimizeBox = False
            BackColor = Color.FromArgb(20, 33, 39)
            ForeColor = Color.WhiteSmoke
            Dim title = New Label With {.Text = "550C VALUE EDITOR", .Dock = DockStyle.Top, .Height = 45, .Padding = New Padding(18, 14, 0, 0), .Font = New Font("Segoe UI", 12, FontStyle.Bold), .ForeColor = Color.FromArgb(85, 224, 208)}
            Controls.Add(title)
            Dim caption = New Label With {.Text = component.Reference & " / " & component.Kind & " value", .Dock = DockStyle.Top, .Height = 28, .Padding = New Padding(18, 4, 0, 0)}
            Controls.Add(caption)
            valueBox.Text = component.Value.ToString("0.##")
            valueBox.Dock = DockStyle.Top
            valueBox.Margin = New Padding(18, 0, 18, 0)
            Controls.Add(valueBox)
            Dim save = New Button With {.Text = "APPLY", .DialogResult = DialogResult.OK, .Dock = DockStyle.Bottom, .Height = 36}
            AddHandler save.Click, Sub() Double.TryParse(valueBox.Text, component.Value)
            Controls.Add(save)
            AcceptButton = save
        End Sub
    End Class

    Public Class WireDimensionDialog
        Inherits Form

        Private ReadOnly wire As CircuitWire
        Private ReadOnly lengthBox As New TextBox()
        Private ReadOnly radiusBox As New TextBox()

        Public Sub New(source As CircuitWire)
            wire = source
            Text = "550C Wire dimension editor (cve.exe)"
            Width = 430
            Height = 220
            StartPosition = FormStartPosition.CenterParent
            FormBorderStyle = FormBorderStyle.FixedDialog
            MaximizeBox = False
            MinimizeBox = False
            BackColor = Color.FromArgb(20, 33, 39)
            ForeColor = Color.WhiteSmoke
            Dim title = New Label With {.Text = "WIRE DIMENSIONS", .Dock = DockStyle.Top, .Height = 44, .Padding = New Padding(18, 14, 0, 0), .Font = New Font("Segoe UI", 12, FontStyle.Bold), .ForeColor = Color.FromArgb(85, 224, 208)}
            Controls.Add(title)
            Dim lengthLabel = New Label With {.Text = "Length (mm)", .Dock = DockStyle.Top, .Height = 25, .Padding = New Padding(18, 4, 0, 0)}
            Controls.Add(lengthLabel)
            lengthBox.Text = wire.LengthMm.ToString("0.##")
            lengthBox.Dock = DockStyle.Top
            Controls.Add(lengthBox)
            Dim radiusLabel = New Label With {.Text = "Radius (mm)", .Dock = DockStyle.Top, .Height = 25, .Padding = New Padding(18, 4, 0, 0)}
            Controls.Add(radiusLabel)
            radiusBox.Text = wire.RadiusMm.ToString("0.##")
            radiusBox.Dock = DockStyle.Top
            Controls.Add(radiusBox)
            Dim save = New Button With {.Text = "APPLY", .DialogResult = DialogResult.OK, .Dock = DockStyle.Bottom, .Height = 36}
            AddHandler save.Click, Sub()
                                       Double.TryParse(lengthBox.Text, wire.LengthMm)
                                       Double.TryParse(radiusBox.Text, wire.RadiusMm)
                                   End Sub
            Controls.Add(save)
            AcceptButton = save
        End Sub
    End Class

    Public Class MaterialManagerDialog
        Inherits Form

        Private ReadOnly materialList As New ListBox()
        Private ReadOnly details As New Label()
        Private selectedMaterialValue As MaterialSpec

        Public ReadOnly Property SelectedMaterial As MaterialSpec
            Get
                Return selectedMaterialValue
            End Get
        End Property

        Public Sub New(currentName As String)
            Text = "550C Material Manager (cmm.exe)"
            Width = 620
            Height = 500
            StartPosition = FormStartPosition.CenterParent
            FormBorderStyle = FormBorderStyle.FixedDialog
            MaximizeBox = False
            MinimizeBox = False
            BackColor = Color.FromArgb(20, 33, 39)
            ForeColor = Color.WhiteSmoke

            Dim title = New Label With {.Text = "550C MATERIAL MANAGER", .Dock = DockStyle.Top, .Height = 48, .Padding = New Padding(20, 15, 0, 0), .Font = New Font("Segoe UI", 14, FontStyle.Bold), .ForeColor = Color.FromArgb(85, 224, 208)}
            Controls.Add(title)
            Dim content = New TableLayoutPanel With {.Dock = DockStyle.Fill, .ColumnCount = 2, .RowCount = 1, .Padding = New Padding(16), .BackColor = Color.FromArgb(20, 33, 39)}
            content.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 245))
            content.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100))
            content.RowStyles.Add(New RowStyle(SizeType.Percent, 100))
            Controls.Add(content)
            materialList.Dock = DockStyle.Fill
            materialList.Width = 245
            materialList.BackColor = Color.FromArgb(15, 25, 30)
            materialList.ForeColor = Color.WhiteSmoke
            materialList.BorderStyle = BorderStyle.FixedSingle
            For Each material In MaterialCatalog.Materials
                materialList.Items.Add(material)
            Next
            Dim current = MaterialCatalog.Find(currentName)
            materialList.SelectedItem = current
            AddHandler materialList.SelectedIndexChanged, AddressOf UpdateDetails
            content.Controls.Add(materialList, 0, 0)

            details.Dock = DockStyle.Fill
            details.Padding = New Padding(22)
            details.Font = New Font("Segoe UI", 10)
            details.ForeColor = Color.FromArgb(220, 229, 232)
            content.Controls.Add(details, 1, 0)

            Dim thermal = New Button With {.Text = "SHOW THERMAL DATA", .Dock = DockStyle.Bottom, .Height = 38, .BackColor = Color.FromArgb(33, 50, 58), .ForeColor = Color.WhiteSmoke, .FlatStyle = FlatStyle.Flat}
            AddHandler thermal.Click, Sub()
                                          If materialList.SelectedItem IsNot Nothing Then
                                              Using dialog As New ThermalDataDialog(DirectCast(materialList.SelectedItem, MaterialSpec))
                                                  dialog.ShowDialog(Me)
                                              End Using
                                          End If
                                      End Sub
            Controls.Add(thermal)
            Dim apply = New Button With {.Text = "APPLY MATERIAL", .Dock = DockStyle.Bottom, .Height = 42, .BackColor = Color.FromArgb(85, 224, 208), .ForeColor = Color.FromArgb(15, 25, 30), .FlatStyle = FlatStyle.Flat}
            AddHandler apply.Click, AddressOf ApplyMaterial
            Controls.Add(apply)
            UpdateDetails()
        End Sub

        Private Sub UpdateDetails(sender As Object, args As EventArgs)
            UpdateDetails()
        End Sub

        Private Sub UpdateDetails()
            If materialList.SelectedItem Is Nothing Then Return
            Dim material = DirectCast(materialList.SelectedItem, MaterialSpec)
            details.Text = material.Name & Environment.NewLine & Environment.NewLine &
                           "Electrical resistivity:  " & material.Resistivity.ToString("0.######") & " Ω·mm²/m" & Environment.NewLine &
                           "Density:                " & material.Density.ToString("0.###") & " g/cm³" & Environment.NewLine &
                           "Hardness:               " & material.Hardness.ToString("0.###") & " HB" & Environment.NewLine &
                           "Tensile strength:       " & material.TensileStrength.ToString("0.###") & " MPa" & Environment.NewLine &
                           "Yield strength:         " & material.YieldStrength.ToString("0.###") & " MPa" & Environment.NewLine &
                           "Thermal conductivity:   " & material.ThermalConductivity.ToString("0.###") & " W/m·K" & Environment.NewLine &
                           "Maximum temperature:    " & material.MaximumTemperature.ToString("0.#") & " °C"
        End Sub

        Private Sub ApplyMaterial(sender As Object, args As EventArgs)
            If materialList.SelectedItem Is Nothing Then Return
            Dim material = DirectCast(materialList.SelectedItem, MaterialSpec)
            selectedMaterialValue = material
            DialogResult = DialogResult.OK
            Close()
        End Sub
    End Class

    Public Class ThermalDataDialog
        Inherits Form

        Public Sub New(material As MaterialSpec)
            Text = "550C ThermalDynamic Manager (ctdm.exe)"
            Width = 520
            Height = 360
            StartPosition = FormStartPosition.CenterParent
            FormBorderStyle = FormBorderStyle.FixedDialog
            MaximizeBox = False
            MinimizeBox = False
            BackColor = Color.FromArgb(20, 33, 39)
            ForeColor = Color.WhiteSmoke
            Dim title = New Label With {.Text = "550C THERMALDYNAMIC MANAGER", .Dock = DockStyle.Top, .Height = 50, .Padding = New Padding(20, 15, 0, 0), .Font = New Font("Segoe UI", 13, FontStyle.Bold), .ForeColor = Color.FromArgb(255, 184, 74)}
            Controls.Add(title)
            Dim data = New Label With {.Text = "MATERIAL: " & material.Name & Environment.NewLine & Environment.NewLine &
                                       "Thermal conductivity     " & material.ThermalConductivity.ToString("0.###") & " W/m·K" & Environment.NewLine &
                                       "Maximum service temp     " & material.MaximumTemperature.ToString("0.#") & " °C" & Environment.NewLine &
                                       "Estimated thermal class   " & If(material.MaximumTemperature >= 1000, "HIGH", If(material.MaximumTemperature >= 300, "MEDIUM", "LOW")) & Environment.NewLine & Environment.NewLine &
                                       "Thermal data loaded for the selected material.", .Dock = DockStyle.Fill, .Padding = New Padding(24), .Font = New Font("Segoe UI", 11)}
            Controls.Add(data)
            Dim closeButton = New Button With {.Text = "CLOSE", .Dock = DockStyle.Bottom, .Height = 38, .DialogResult = DialogResult.OK}
            Controls.Add(closeButton)
            AcceptButton = closeButton
        End Sub
    End Class

    Public Class MechanicalManagerDialog
        Inherits Form

        Public Sub New(wire As CircuitWire)
            Dim material = MaterialCatalog.Find(wire.Material)
            Text = "550C Mechanical Manager (cmm-mech.exe)"
            Width = 560
            Height = 420
            StartPosition = FormStartPosition.CenterParent
            FormBorderStyle = FormBorderStyle.FixedDialog
            MaximizeBox = False
            MinimizeBox = False
            BackColor = Color.FromArgb(20, 33, 39)
            ForeColor = Color.WhiteSmoke
            Dim title = New Label With {.Text = "550C MECHANICAL MANAGER", .Dock = DockStyle.Top, .Height = 50, .Padding = New Padding(20, 15, 0, 0), .Font = New Font("Segoe UI", 13, FontStyle.Bold), .ForeColor = Color.FromArgb(255, 184, 74)}
            Controls.Add(title)
            Dim data = New Label With {.Text = "WIRE: " & wire.Reference & Environment.NewLine &
                                       "MATERIAL: " & material.Name & Environment.NewLine & Environment.NewLine &
                                       "Length                  " & wire.LengthMm.ToString("0.###") & " mm" & Environment.NewLine &
                                       "Radius                  " & wire.RadiusMm.ToString("0.###") & " mm" & Environment.NewLine &
                                       "Density                 " & material.Density.ToString("0.###") & " g/cm³" & Environment.NewLine &
                                       "Hardness                " & material.Hardness.ToString("0.###") & " HB" & Environment.NewLine &
                                       "Tensile strength        " & material.TensileStrength.ToString("0.###") & " MPa" & Environment.NewLine &
                                       "Yield strength          " & material.YieldStrength.ToString("0.###") & " MPa" & Environment.NewLine &
                                       "Elastic modulus         " & material.ElasticModulus.ToString("0.###") & " GPa" & Environment.NewLine &
                                       "Thermal conductivity    " & material.ThermalConductivity.ToString("0.###") & " W/m·K", .Dock = DockStyle.Fill, .Padding = New Padding(24), .Font = New Font("Segoe UI", 10)}
            Controls.Add(data)
            Dim closeButton = New Button With {.Text = "CLOSE", .Dock = DockStyle.Bottom, .Height = 38, .DialogResult = DialogResult.OK}
            Controls.Add(closeButton)
            AcceptButton = closeButton
        End Sub
    End Class

    Public Class CommandHostDialog
        Inherits Form

        Private ReadOnly material As MaterialSpec
        Private ReadOnly commandTimer As New Timer With {.Interval = 20}
        Private ReadOnly progress As New ProgressBar()
        Private ReadOnly detail As New Label()
        Private commandNumber As Integer

        Public Sub New(source As MaterialSpec)
            material = source
            Text = "550C Command Host (ccmdhost.exe)"
            Width = 520
            Height = 220
            StartPosition = FormStartPosition.CenterParent
            FormBorderStyle = FormBorderStyle.FixedDialog
            ControlBox = False
            BackColor = Color.FromArgb(20, 33, 39)
            ForeColor = Color.WhiteSmoke
            Dim title = New Label With {.Text = "550C COMMAND HOST", .Dock = DockStyle.Top, .Height = 48, .Padding = New Padding(20, 15, 0, 0), .Font = New Font("Segoe UI", 13, FontStyle.Bold), .ForeColor = Color.FromArgb(85, 224, 208)}
            Controls.Add(title)
            detail.Text = "Preparing 100 material commands..."
            detail.Dock = DockStyle.Top
            detail.Height = 38
            detail.Padding = New Padding(20, 8, 0, 0)
            Controls.Add(detail)
            progress.Dock = DockStyle.Top
            progress.Height = 26
            progress.Maximum = 100
            Controls.Add(progress)
            AddHandler commandTimer.Tick, AddressOf RunCommand
            AddHandler Shown, Sub() commandTimer.Start()
        End Sub

        Private Sub RunCommand(sender As Object, args As EventArgs)
            commandNumber += 1
            progress.Value = commandNumber
            detail.Text = "Executing command " & commandNumber.ToString("000") & " / 100 for " & material.Name
            If commandNumber >= 100 Then
                commandTimer.Stop()
                DialogResult = DialogResult.OK
                Close()
            End If
        End Sub
    End Class

    Public Class ValidationDialog
        Inherits Form

        Public Sub New(issues As List(Of String))
            Text = "Electropad Design Validation"
            Width = 620
            Height = 380
            StartPosition = FormStartPosition.CenterParent
            BackColor = Color.FromArgb(20, 33, 39)
            ForeColor = Color.WhiteSmoke
            Dim title = New Label With {.Text = If(issues.Count = 0, "DESIGN VALIDATION PASSED", "DESIGN VALIDATION ISSUES"), .Dock = DockStyle.Top, .Height = 52, .Padding = New Padding(20, 15, 0, 0), .Font = New Font("Segoe UI", 14, FontStyle.Bold), .ForeColor = If(issues.Count = 0, Color.FromArgb(85, 224, 208), Color.FromArgb(255, 184, 74))}
            Controls.Add(title)
            Dim list = New ListBox With {.Dock = DockStyle.Fill, .BackColor = Color.FromArgb(15, 25, 30), .ForeColor = Color.FromArgb(220, 229, 232), .BorderStyle = BorderStyle.FixedSingle}
            If issues.Count = 0 Then
                list.Items.Add("No duplicate references, broken wire endpoints, or invalid dimensions found.")
            Else
                For Each issue In issues
                    list.Items.Add("• " & issue)
                Next
            End If
            Controls.Add(list)
            Dim closeButton = New Button With {.Text = "CLOSE", .Dock = DockStyle.Bottom, .Height = 38, .DialogResult = DialogResult.OK}
            Controls.Add(closeButton)
            AcceptButton = closeButton
        End Sub
    End Class

    Public Class BillOfMaterialsDialog
        Inherits Form

        Public Sub New(project As CircuitProject)
            Text = "Electropad Bill of Materials"
            Width = 720
            Height = 480
            StartPosition = FormStartPosition.CenterParent
            BackColor = Color.FromArgb(20, 33, 39)
            ForeColor = Color.WhiteSmoke
            Dim title = New Label With {.Text = "BILL OF MATERIALS", .Dock = DockStyle.Top, .Height = 52, .Padding = New Padding(20, 15, 0, 0), .Font = New Font("Segoe UI", 14, FontStyle.Bold), .ForeColor = Color.FromArgb(85, 224, 208)}
            Controls.Add(title)
            Dim grid = New DataGridView With {.Dock = DockStyle.Fill, .ReadOnly = True, .AllowUserToAddRows = False, .AllowUserToDeleteRows = False, .AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, .BackgroundColor = Color.FromArgb(15, 25, 30), .BorderStyle = BorderStyle.FixedSingle, .RowHeadersVisible = False}
            grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(33, 50, 58)
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.WhiteSmoke
            grid.EnableHeadersVisualStyles = False
            grid.Columns.Add("Reference", "Reference")
            grid.Columns.Add("Type", "Type")
            grid.Columns.Add("Value", "Value")
            grid.Columns.Add("Material", "Material")
            For Each component In project.Components.Where(Function(item) Not item.Hidden)
                grid.Rows.Add(component.Reference, component.Kind, component.Value.ToString("0.###"), component.Color)
            Next
            Controls.Add(grid)
            Dim summary = New Label With {.Text = project.Components.Count & " components   ·   " & project.Wires.Count & " wires   ·   Board material: " & project.Material, .Dock = DockStyle.Bottom, .Height = 34, .Padding = New Padding(20, 8, 0, 0), .ForeColor = Color.FromArgb(145, 160, 170)}
            Controls.Add(summary)
            Dim closeButton = New Button With {.Text = "CLOSE", .Dock = DockStyle.Bottom, .Height = 38, .DialogResult = DialogResult.OK}
            Controls.Add(closeButton)
            AcceptButton = closeButton
        End Sub
    End Class

        Public Class ExchangeDialog
        Inherits Form

        Private ReadOnly project As CircuitProject
        Private ReadOnly completed As Action(Of String)
        Private ReadOnly targetCombo As New ComboBox()
        Private ReadOnly progress As New ProgressBar()
        Private ReadOnly detail As New Label()
        Private ReadOnly convertButton As New Button()
        Private ReadOnly conversionTimer As New Timer With {.Interval = 90}
        Private conversionValue As Integer

        Public Sub New(sourceProject As CircuitProject, onCompleted As Action(Of String))
            project = sourceProject
            completed = onCompleted
            Text = "2D ↔ 3D Exchange"
            Width = 500
            Height = 330
            StartPosition = FormStartPosition.CenterParent
            BackColor = Color.FromArgb(20, 33, 39)
            ForeColor = Color.FromArgb(242, 245, 247)
            FormBorderStyle = FormBorderStyle.FixedDialog
            MaximizeBox = False
            MinimizeBox = False
            BuildDialog()
            AddHandler conversionTimer.Tick, AddressOf ConvertTick
        End Sub

        Private Sub BuildDialog()
            Dim title = New Label With {.Text = "2D ↔ 3D EXCHANGE", .Dock = DockStyle.Top, .Height = 48, .Padding = New Padding(22, 16, 0, 0), .Font = New Font("Segoe UI", 14, FontStyle.Bold), .ForeColor = Color.FromArgb(85, 224, 208)}
            Controls.Add(title)
            Dim description = New Label With {.Text = "Convert the current schematic into editable solid bodies and preserve component identity, materials, and wire geometry.", .Dock = DockStyle.Top, .Height = 55, .Padding = New Padding(22, 0, 22, 0), .ForeColor = Color.FromArgb(145, 160, 170)}
            Controls.Add(description)
            Dim source = New Label With {.Text = "SOURCE     2D SCHEMATIC   ·   " & project.Components.Count & " COMPONENTS", .Dock = DockStyle.Top, .Height = 30, .Padding = New Padding(22, 4, 0, 0), .ForeColor = Color.FromArgb(210, 220, 223)}
            Controls.Add(source)
            targetCombo.Items.AddRange({"3D SOLID SCENE", "2D SCHEMATIC"})
            targetCombo.SelectedIndex = 0
            targetCombo.DropDownStyle = ComboBoxStyle.DropDownList
            targetCombo.Dock = DockStyle.Top
            targetCombo.Margin = New Padding(22, 0, 22, 0)
            Controls.Add(targetCombo)
            progress.Dock = DockStyle.Top
            progress.Height = 22
            progress.Margin = New Padding(22, 12, 22, 0)
            progress.Minimum = 0
            progress.Maximum = 100
            Controls.Add(progress)
            detail.Text = "Ready to map nodes, extrude bodies, and route wires."
            detail.Dock = DockStyle.Top
            detail.Height = 35
            detail.Padding = New Padding(22, 8, 0, 0)
            detail.ForeColor = Color.FromArgb(145, 160, 170)
            Controls.Add(detail)
            convertButton.Text = "CONVERT"
            convertButton.Dock = DockStyle.Bottom
            convertButton.Height = 42
            convertButton.Margin = New Padding(22, 0, 22, 16)
            convertButton.BackColor = Color.FromArgb(85, 224, 208)
            convertButton.ForeColor = Color.FromArgb(15, 25, 30)
            convertButton.FlatStyle = FlatStyle.Flat
            AddHandler convertButton.Click, AddressOf StartConversion
            Controls.Add(convertButton)
        End Sub

        Private Sub StartConversion(sender As Object, args As EventArgs)
            conversionValue = 0
            progress.Value = 0
            convertButton.Enabled = False
            targetCombo.Enabled = False
            detail.Text = "Preparing geometry conversion..."
            conversionTimer.Start()
        End Sub

        Private Sub ConvertTick(sender As Object, args As EventArgs)
    conversionValue = Math.Min(100, conversionValue + 20)
    progress.Value = conversionValue
    detail.Text = "Processing geometry layout transform allocations..."
    
    If conversionValue >= 100 Then
        conversionTimer.Stop()
        
        ' FIX 1: Cache the selected dropdown index BEFORE closing the form
        Dim chosenIndex As Integer = targetCombo.SelectedIndex
        
        DialogResult = DialogResult.OK
        Close()
        
        ' FIX 2: Execute the callback block AFTER the form window unloads cleanly
        If chosenIndex = 0 Then
            completed("3D")
        Else
            completed("2D")
        End If
    End If
End Sub

    End Class


    Public NotInheritable Class Direct3D11Renderer
        Implements IDisposable

        Private ReadOnly target As Control
        Private device As ID3D11Device
        Private context As ID3D11DeviceContext
        Private swapChain As IDXGISwapChain
        Private renderTarget As ID3D11RenderTargetView
        Private clearMethod As MethodInfo
        Private initialized As Boolean
        Private initializationAttempted As Boolean

        Public Sub New(renderTargetControl As Control)
            target = renderTargetControl
        End Sub

        Public ReadOnly Property IsInitialized As Boolean
            Get
                Return initialized
            End Get
        End Property

        Public Function Initialize() As Boolean
            If initialized OrElse target.IsDisposed OrElse Not target.IsHandleCreated Then Return initialized
            If initializationAttempted Then Return False
            initializationAttempted = True
            Try
                Dim description = New SwapChainDescription With {
                    .BufferCount = 2,
                    .BufferDescription = New ModeDescription(Math.Max(1, target.ClientSize.Width), Math.Max(1, target.ClientSize.Height), New Rational(60, 1), Format.B8G8R8A8_UNorm),
                    .BufferUsage = Usage.RenderTargetOutput,
                    .OutputWindow = target.Handle,
                    .SampleDescription = New SampleDescription(1, 0),
                    .Windowed = True,
                    .SwapEffect = SwapEffect.Discard
                }
                Dim result = D3D11.D3D11CreateDeviceAndSwapChain(Nothing, DriverType.Hardware, DeviceCreationFlags.BgraSupport, Nothing, description, swapChain, device, Nothing, context)
                If result.Failure OrElse device Is Nothing OrElse context Is Nothing OrElse swapChain Is Nothing Then Return False
                clearMethod = context.GetType().GetMethod("ClearRenderTargetView", BindingFlags.Instance Or BindingFlags.Public, Nothing, {GetType(ID3D11RenderTargetView), GetType(Vortice.Mathematics.Color4)}, Nothing)
                CreateRenderTarget()
                initialized = renderTarget IsNot Nothing
                Return initialized
            Catch
                DisposeResources()
                Return False
            End Try
        End Function

        Public Sub Render(clearColor As System.Drawing.Color)
            If Not Initialize() OrElse renderTarget Is Nothing Then Return
            Try
                Dim color = New Vortice.Mathematics.Color4(clearColor.R / 255.0F, clearColor.G / 255.0F, clearColor.B / 255.0F, 1.0F)
                If clearMethod IsNot Nothing Then clearMethod.Invoke(context, {renderTarget, color})
                context.OMSetRenderTargets(renderTarget)
                swapChain.Present(0, PresentFlags.None)
            Catch
                DisposeResources()
            End Try
        End Sub

        Public Sub Resize()
            If Not initialized OrElse swapChain Is Nothing Then Return
            Try
                renderTarget?.Dispose()
                renderTarget = Nothing
                swapChain.ResizeBuffers(2, Math.Max(1, target.ClientSize.Width), Math.Max(1, target.ClientSize.Height), Format.B8G8R8A8_UNorm, SwapChainFlags.None)
                CreateRenderTarget()
            Catch
                DisposeResources()
            End Try
        End Sub

        Private Sub CreateRenderTarget()
            Dim backBuffer = swapChain.GetBuffer(Of ID3D11Texture2D)(0)
            renderTarget = device.CreateRenderTargetView(backBuffer)
            backBuffer.Dispose()
        End Sub

        Private Sub DisposeResources()
            renderTarget?.Dispose()
            swapChain?.Dispose()
            context?.Dispose()
            device?.Dispose()
            renderTarget = Nothing
            swapChain = Nothing
            context = Nothing
            device = Nothing
            initialized = False
        End Sub

        Public Sub Dispose() Implements IDisposable.Dispose
            DisposeResources()
            GC.SuppressFinalize(Me)
        End Sub
    End Class
    Public Class ConversionExchange2DForm
    Inherits Form

    Private ReadOnly project As CircuitProject
    Private ReadOnly processingTimer As New Timer With {.Interval = 35}
    Private ReadOnly terminalProgress As New ProgressBar()
    Private ReadOnly consoleDetailLabel As New Label()
    Private commandIndex As Integer

    ' A pipeline array of mock system processing instructions to flash across the UI
    Private ReadOnly consoleCommands As String() = {
        "INIT: Initializing 550C 2D3D Manager (cdm.exe)...",
        "SYS: Checking DXGI core pipelines...",
        "SYS: Halting Direct3D11 render contextual threads...",
        "GEOM: Disassembling isometric canvas polygons...",
        "GEOM: Flattening component vertex projection matrices...",
        "PROJ: Flattening 'Battery' body diameter vector array...",
        "PROJ: Extracting structural schematic reference 'V1'...",
        "PROJ: Flattening 'Resistor' physical casing dimensions...",
        "PROJ: Extracting copper banded ohm metric structural layout...",
        "PROJ: Flattening amber 'LED' node paths...",
        "PROJ: Compressing ground node 'GND1' to schematic grid coordinates...",
        "WIRE: Recalculating copper wire gauge trace trajectories...",
        "WIRE: Re-evaluating resistivity algorithms on a flat grid...",
        "MATH: Solving absolute circuit nodal matrices via Ohm's Law...",
        "GRID: Snapping components to structural 220 x 125 vector grid...",
        "SYS: Releasing swapchain memory handles safely...",
        "SYS: Flushing Direct3D runtime layout caches...",
        "VIEW: Repainting 2D schematic drawing elements...",
        "SUCCESS: 2D geometry mapping pipeline complete."
    }

    Public Sub New(sourceProject As CircuitProject)
        project = sourceProject
        
        ' Form Setup matching your application's UI theme style
        Text = "550C 2D3D Manager (cdm.exe)"
        Width = 560
        Height = 240
        StartPosition = FormStartPosition.CenterParent
        FormBorderStyle = FormBorderStyle.FixedDialog
        ControlBox = False
        BackColor = Color.FromArgb(20, 33, 39)
        ForeColor = Color.WhiteSmoke

        Dim titleHeader = New Label With {
            .Text = "550C 2D3D REVERSION CONSOLE", 
            .Dock = DockStyle.Top, 
            .Height = 48, 
            .Padding = New Padding(20, 15, 0, 0), 
            .Font = New Font("Segoe UI", 12, FontStyle.Bold), 
            .ForeColor = Color.FromArgb(255, 184, 74)
        }
        Controls.Add(titleHeader)

        consoleDetailLabel.Text = "Awaiting inversion pipeline start..."
        consoleDetailLabel.Dock = DockStyle.Top
        consoleDetailLabel.Height = 45
        consoleDetailLabel.Padding = New Padding(20, 8, 20, 0)
        consoleDetailLabel.Font = New Font("Consolas", 9, FontStyle.Regular)
        consoleDetailLabel.ForeColor = Color.FromArgb(170, 190, 200)
        Controls.Add(consoleDetailLabel)

        terminalProgress.Dock = DockStyle.Top
        terminalProgress.Height = 28
        terminalProgress.Margin = New Padding(20, 0, 20, 0)
        terminalProgress.Maximum = 100
        Controls.Add(terminalProgress)

        AddHandler processingTimer.Tick, AddressOf ProcessNextCommand
        AddHandler Shown, Sub() processingTimer.Start()
    End Sub

    Private Sub ProcessNextCommand(sender As Object, args As EventArgs)
        commandIndex += 1
        
        ' Limit progress bar bounds to matching 100% capacity safely
        If commandIndex <= 100 Then
            terminalProgress.Value = commandIndex
        End If

        ' Distribute console command messages fluidly across progress steps
        Dim messageSelector As Integer = Math.Min(consoleCommands.Length - 1, CInt(Math.Floor((commandIndex / 100.0) * consoleCommands.Length)))
        consoleDetailLabel.Text = "cmd_exec: " & consoleCommands(messageSelector)

        If commandIndex >= 100 Then
            processingTimer.Stop()
            DialogResult = DialogResult.OK
            Close()
        End If
    End Sub
End Class

End Namespace

