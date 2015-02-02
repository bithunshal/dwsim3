﻿Imports DWSIM.DWSIM.SimulationObjects.UnitOps.Auxiliary
Imports Microsoft.MSDN.Samples.GraphicObjects
Imports System.Linq
Public Class FlowsheetUOEditorForm

    Public fsuo As DWSIM.SimulationObjects.UnitOps.Flowsheet

    Private loaded As Boolean = True

    Private Sub FlowsheetUOEditorForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        If fsuo.Initialized Then
            btnInitialize.Enabled = True
            lblInit.Text = DWSIM.App.GetLocalString("FlowsheetInitialized")
            UpdateLinks()
            UpdateProps()
            loaded = True
        Else
            btnInitialize.Enabled = True
            lblInit.Text = DWSIM.App.GetLocalString("FlowsheetNotInitialized")
        End If

        cbMassTransferMode.SelectedIndex = fsuo.MassTransferMode

    End Sub

    Private Sub btnInitialize_Click(sender As Object, e As EventArgs) Handles btnInitialize.Click

        fsuo.InitializeFlowsheet(fsuo.SimulationFile)

        If fsuo.Initialized Then
            btnInitialize.Enabled = True
            lblInit.Text = DWSIM.App.GetLocalString("FlowsheetInitializationSuccess")
        Else
            btnInitialize.Enabled = True
            lblInit.Text = DWSIM.App.GetLocalString("FlowsheetInitializationError")
        End If

        UpdateLinks()
        UpdateProps()

        loaded = True

    End Sub

    Sub UpdateLinks()

        Dim i As Integer
        Dim connectedfrom, connectedto As String

        Dim cb As New DataGridViewComboBoxCell

        cb.Items.Add("")
        For Each ms As GraphicObject In fsuo.Fsheet.Collections.MaterialStreamCollection.Values
            cb.Items.Add(ms.Tag)
        Next

        dgvInputLinks.Columns(1).CellTemplate = cb
        dgvOutputLinks.Columns(1).CellTemplate = cb

        With dgvInputLinks.Rows
            .Clear()
            For i = 0 To 9
                If fsuo.GraphicObject.InputConnectors(i).IsAttached Then
                    connectedfrom = " (" & fsuo.GraphicObject.InputConnectors(i).AttachedConnector.AttachedFrom.Tag & ")"
                Else
                    connectedfrom = ""
                End If
                If fsuo.InputConnections(i) <> "" Then
                    Dim obj As GraphicObject = fsuo.Fsheet.Collections.MaterialStreamCollection(fsuo.InputConnections(i))
                    If Not obj Is Nothing Then connectedto = obj.Tag Else connectedto = ""
                Else
                    connectedto = ""
                End If
                .Add(New Object() {DWSIM.App.GetLocalString("Correntedeentrada" & (i + 1).ToString) & connectedfrom, connectedto})
            Next
        End With

        With dgvOutputLinks.Rows
            .Clear()
            For i = 0 To 9
                If fsuo.GraphicObject.OutputConnectors(i).IsAttached Then
                    connectedto = " (" & fsuo.GraphicObject.OutputConnectors(i).AttachedConnector.AttachedTo.Tag & ")"
                Else
                    connectedto = ""
                End If
                If fsuo.OutputConnections(i) <> "" Then
                    Dim obj As GraphicObject = fsuo.Fsheet.Collections.MaterialStreamCollection(fsuo.OutputConnections(i))
                    If Not obj Is Nothing Then connectedfrom = obj.Tag Else connectedfrom = ""
                Else
                    connectedfrom = ""
                End If
                .Add(New Object() {DWSIM.App.GetLocalString("Correntedesaida" & (i + 1).ToString) & connectedto, connectedfrom})
            Next
        End With

    End Sub

    Private Sub dgv_CellValueChanged(ByVal sender As System.Object, ByVal e As System.Windows.Forms.DataGridViewCellEventArgs) Handles dgvInputLinks.CellValueChanged
        If e.RowIndex >= 0 And e.ColumnIndex = 1 And loaded Then
            Dim obj As GraphicObject = fsuo.Fsheet.GetFlowsheetGraphicObject(dgvInputLinks.Rows(e.RowIndex).Cells(1).Value)
            If Not obj Is Nothing Then fsuo.InputConnections(e.RowIndex) = obj.Name Else fsuo.InputConnections(e.RowIndex) = ""
        End If
    End Sub

    Private Sub dgv_CellValueChanged2(ByVal sender As System.Object, ByVal e As System.Windows.Forms.DataGridViewCellEventArgs) Handles dgvOutputLinks.CellValueChanged
        If e.RowIndex >= 0 And e.ColumnIndex = 1 And loaded Then
            Dim obj As GraphicObject = fsuo.Fsheet.GetFlowsheetGraphicObject(dgvOutputLinks.Rows(e.RowIndex).Cells(1).Value)
            If Not obj Is Nothing Then fsuo.OutputConnections(e.RowIndex) = obj.Name Else fsuo.OutputConnections(e.RowIndex) = ""
        End If
    End Sub

    Sub UpdateProps()

        Dim cbc2 = New DataGridViewComboBoxCell
        cbc2.Sorted = True
        cbc2.MaxDropDownItems = 10
        cbc2.Items.Add("")
        For Each obj As SimulationObjects_BaseClass In fsuo.Fsheet.Collections.ObjectCollection.Values
            cbc2.Items.Add(obj.GraphicObject.Tag)
        Next

        dgvInputPars.Columns(1).CellTemplate = cbc2
        dgvOutputPars.Columns(1).CellTemplate = cbc2

        With dgvInputPars.Rows
            .Clear()
            For Each ip In fsuo.InputParams.Values
                If fsuo.Fsheet.Collections.ObjectCollection.ContainsKey(ip.ObjectID) Then
                    .Add(New Object() {ip.ID, fsuo.Fsheet.Collections.ObjectCollection(ip.ObjectID).GraphicObject.Tag, DWSIM.App.GetPropertyName(ip.ObjectProperty)})
                    Dim cbc As DataGridViewComboBoxCell = .Item(.Count - 1).Cells(2)
                    cbc.Items.Clear()
                    Dim props As String()
                    props = Me.ReturnProperties(.Item(.Count - 1).Cells(1).Value, False)
                    For Each prop As String In props
                        cbc.Items.Add(DWSIM.App.GetPropertyName(prop))
                    Next
                    .Item(.Count - 1).Cells(1).Tag = ip.ObjectID
                End If
            Next
        End With

        With dgvOutputPars.Rows
            .Clear()
            For Each ip In fsuo.OutputParams.Values
                If fsuo.Fsheet.Collections.ObjectCollection.ContainsKey(ip.ObjectID) Then
                    .Add(New Object() {ip.ID, fsuo.Fsheet.Collections.ObjectCollection(ip.ObjectID).GraphicObject.Tag, DWSIM.App.GetPropertyName(ip.ObjectProperty)})
                    Dim cbc As DataGridViewComboBoxCell = .Item(.Count - 1).Cells(2)
                    cbc.Items.Clear()
                    Dim props As String()
                    props = Me.ReturnProperties(.Item(.Count - 1).Cells(1).Value, True)
                    For Each prop As String In props
                        cbc.Items.Add(DWSIM.App.GetPropertyName(prop))
                    Next
                    .Item(.Count - 1).Cells(1).Tag = ip.ObjectID
                End If
            Next
        End With

    End Sub

    Private Function ReturnProperties(ByVal objectTAG As String, ByVal dependent As Boolean) As String()

        For Each obj As SimulationObjects_BaseClass In fsuo.Fsheet.Collections.ObjectCollection.Values
            If objectTAG = obj.GraphicObject.Tag Then
                If dependent Then
                    Return obj.GetProperties(SimulationObjects_BaseClass.PropertyType.ALL)
                Else
                    Return obj.GetProperties(SimulationObjects_BaseClass.PropertyType.WR)
                End If
                Exit Function
            End If
        Next

        Return Nothing

    End Function

    Private Function ReturnObject(ByVal objectTAG As String) As SimulationObjects_BaseClass

        For Each obj As SimulationObjects_BaseClass In fsuo.Fsheet.Collections.ObjectCollection.Values
            If objectTAG = obj.GraphicObject.Tag Then
                Return obj
                Exit Function
            End If
        Next

        Return Nothing

    End Function

    Private Function ReturnPropertyID(ByVal objectID As String, ByVal propTAG As String) As String

        Dim props As String() = fsuo.Fsheet.Collections.ObjectCollection(objectID).GetProperties(SimulationObjects_BaseClass.PropertyType.ALL)
        For Each prop As String In props
            If DWSIM.App.GetPropertyName(prop) = propTAG Then
                Return prop
            End If
        Next

        Return Nothing

    End Function

    Private Sub ToolStripButton1_Click(sender As Object, e As EventArgs) Handles ToolStripButton1.Click
        Me.dgvInputPars.Rows.Add()
        Me.dgvInputPars.Rows(Me.dgvInputPars.Rows.Count - 1).Cells(0).Value = Guid.NewGuid.ToString
    End Sub

    Private Sub ToolStripButton3_Click(sender As Object, e As EventArgs) Handles ToolStripButton3.Click
        Me.dgvOutputPars.Rows.Add()
        Me.dgvOutputPars.Rows(Me.dgvOutputPars.Rows.Count - 1).Cells(0).Value = Guid.NewGuid.ToString
    End Sub

    Private Sub dgvInputPars_CellValueChanged(sender As Object, e As DataGridViewCellEventArgs) Handles dgvInputPars.CellValueChanged

        If e.RowIndex >= 0 Then
            Select Case e.ColumnIndex
                Case 1
                    Dim obj As GraphicObject = fsuo.Fsheet.GetFlowsheetGraphicObject(dgvInputPars.Rows(e.RowIndex).Cells(1).Value)
                    If Not obj Is Nothing Then
                        If fsuo.InputParams.ContainsKey(Me.dgvInputPars.Rows(e.RowIndex).Cells(0).Value) Then
                            fsuo.InputParams(Me.dgvInputPars.Rows(e.RowIndex).Cells(0).Value).ObjectID = obj.Name
                        Else
                            fsuo.InputParams.Add(Me.dgvInputPars.Rows(e.RowIndex).Cells(0).Value, New FlowsheetUOParameter() With
                                                                                                  {.ID = Me.dgvInputPars.Rows(e.RowIndex).Cells(0).Value,
                                                                                                   .ObjectID = obj.Name,
                                                                                                   .ObjectProperty = ""})
                        End If
                    End If
                    Dim cbc As DataGridViewComboBoxCell = Me.dgvInputPars.Rows(e.RowIndex).Cells(e.ColumnIndex + 1)
                    cbc.Items.Clear()
                    With cbc.Items
                        If Me.dgvInputPars.Rows(e.RowIndex).Cells(e.ColumnIndex).Value.ToString <> "" Then
                            Dim props As String()
                            props = Me.ReturnProperties(Me.dgvInputPars.Rows(e.RowIndex).Cells(1).Value, False)
                            For Each prop As String In props
                                .Add(DWSIM.App.GetPropertyName(prop))
                            Next
                        End If
                    End With
                Case 2
                    If Not Me.dgvInputPars.Rows(e.RowIndex).Cells(e.ColumnIndex).Value Is Nothing Then
                        Dim props As String() = Me.ReturnProperties(Me.dgvInputPars.Rows(e.RowIndex).Cells(1).Value, False)
                        For Each prop As String In props
                            If DWSIM.App.GetPropertyName(prop) = Me.dgvInputPars.Rows(e.RowIndex).Cells(e.ColumnIndex).Value.ToString Then
                                If fsuo.InputParams.ContainsKey(Me.dgvInputPars.Rows(e.RowIndex).Cells(0).Value) Then
                                    fsuo.InputParams(Me.dgvInputPars.Rows(e.RowIndex).Cells(0).Value).ObjectProperty = prop
                                Else
                                    fsuo.InputParams.Add(Me.dgvInputPars.Rows(e.RowIndex).Cells(0).Value, New FlowsheetUOParameter() With
                                                                                                          {.ID = Me.dgvInputPars.Rows(e.RowIndex).Cells(0).Value,
                                                                                                           .ObjectID = Me.dgvInputPars.Rows(e.RowIndex).Cells(1).Tag,
                                                                                                           .ObjectProperty = prop})
                                End If
                            End If
                        Next
                    End If
            End Select
        End If

    End Sub

    Private Sub dgvOutputPars_CellValueChanged(sender As Object, e As DataGridViewCellEventArgs) Handles dgvOutputPars.CellValueChanged

        If e.RowIndex >= 0 Then
            Select Case e.ColumnIndex
                Case 1
                    Dim obj As GraphicObject = fsuo.Fsheet.GetFlowsheetGraphicObject(dgvOutputPars.Rows(e.RowIndex).Cells(1).Value)
                    If Not obj Is Nothing Then
                        If fsuo.OutputParams.ContainsKey(Me.dgvOutputPars.Rows(e.RowIndex).Cells(0).Value) Then
                            fsuo.OutputParams(Me.dgvOutputPars.Rows(e.RowIndex).Cells(0).Value).ObjectID = obj.Name
                        Else
                            fsuo.OutputParams.Add(Me.dgvOutputPars.Rows(e.RowIndex).Cells(0).Value, New FlowsheetUOParameter() With
                                                                                                  {.ID = Me.dgvOutputPars.Rows(e.RowIndex).Cells(0).Value,
                                                                                                   .ObjectID = obj.Name,
                                                                                                   .ObjectProperty = ""})
                        End If
                    End If
                    Dim cbc As DataGridViewComboBoxCell = Me.dgvOutputPars.Rows(e.RowIndex).Cells(e.ColumnIndex + 1)
                    cbc.Items.Clear()
                    With cbc.Items
                        If Me.dgvOutputPars.Rows(e.RowIndex).Cells(e.ColumnIndex).Value.ToString <> "" Then
                            Dim props As String()
                            props = Me.ReturnProperties(Me.dgvOutputPars.Rows(e.RowIndex).Cells(1).Value, True)
                            For Each prop As String In props
                                .Add(DWSIM.App.GetPropertyName(prop))
                            Next
                        End If
                    End With
                Case 2
                    If Not Me.dgvOutputPars.Rows(e.RowIndex).Cells(e.ColumnIndex).Value Is Nothing Then
                        Dim props As String() = Me.ReturnProperties(Me.dgvOutputPars.Rows(e.RowIndex).Cells(1).Value, True)
                        For Each prop As String In props
                            If DWSIM.App.GetPropertyName(prop) = Me.dgvOutputPars.Rows(e.RowIndex).Cells(e.ColumnIndex).Value.ToString Then
                                If fsuo.OutputParams.ContainsKey(Me.dgvOutputPars.Rows(e.RowIndex).Cells(0).Value) Then
                                    fsuo.OutputParams(Me.dgvOutputPars.Rows(e.RowIndex).Cells(0).Value).ObjectProperty = prop
                                Else
                                    fsuo.OutputParams.Add(Me.dgvOutputPars.Rows(e.RowIndex).Cells(0).Value, New FlowsheetUOParameter() With
                                                                                                          {.ID = Me.dgvOutputPars.Rows(e.RowIndex).Cells(0).Value,
                                                                                                           .ObjectID = Me.dgvOutputPars.Rows(e.RowIndex).Cells(1).Tag,
                                                                                                           .ObjectProperty = prop})
                                End If
                            End If
                        Next
                    End If
            End Select
        End If

    End Sub

    Private Sub cbMassTransferMode_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cbMassTransferMode.SelectedIndexChanged
        If loaded Then fsuo.MassTransferMode = cbMassTransferMode.SelectedIndex
    End Sub

    Private Sub ToolStripButton2_Click(sender As Object, e As EventArgs) Handles ToolStripButton2.Click
        Dim id As String = dgvInputPars.SelectedRows(0).Cells(0).Value
        If fsuo.InputParams.ContainsKey(id) Then fsuo.InputParams.Remove(id)
        dgvInputPars.Rows.Remove(dgvInputPars.SelectedRows(0))
    End Sub

    Private Sub ToolStripButton4_Click(sender As Object, e As EventArgs) Handles ToolStripButton4.Click
        Dim id As String = dgvOutputPars.SelectedRows(0).Cells(0).Value
        If fsuo.OutputParams.ContainsKey(id) Then fsuo.OutputParams.Remove(id)
        dgvOutputPars.Rows.Remove(dgvOutputPars.SelectedRows(0))
    End Sub

    Private Sub ToolStripButton5_Click(sender As Object, e As EventArgs) Handles ToolStripButton5.Click
        Dim id As String = dgvInputPars.SelectedRows(0).Cells(0).Value
        Clipboard.SetText("[I][" & id & "]")
    End Sub

    Private Sub ToolStripButton6_Click(sender As Object, e As EventArgs) Handles ToolStripButton6.Click
        Dim id As String = dgvOutputPars.SelectedRows(0).Cells(0).Value
        Clipboard.SetText("[O][" & id & "]")
    End Sub

End Class