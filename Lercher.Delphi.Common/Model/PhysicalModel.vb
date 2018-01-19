Imports System.Xml.Xsl

Public Class PhysicalModel
    Private Const TRANSFORM_XSLT As String = "Lercher.Delphi.Common.TransformPhysicalModel.xslt"
    Private Shared ReadOnly transform As XslCompiledTransform
    Private Shared pms As New List(Of Xml.XmlDocument)

    Shared Sub New()
        transform = New XslCompiledTransform()
        Using s = System.Reflection.Assembly.GetExecutingAssembly.GetManifestResourceStream(TRANSFORM_XSLT)
            Using xr = Xml.XmlReader.Create(s)
                transform.Load(xr)
            End Using
        End Using
    End Sub

    Public Shared Function LoadAllPdmFrom(path As String) As String
        Dim r = New List(Of String)
        Dim di = New IO.DirectoryInfo(path)
        For Each pdm In di.GetFiles("*.pdm")
            Dim x = New Xml.XmlDocument()
            x.Load(pdm.FullName)
            Dim nav As Xml.XPath.XPathNavigator = x.CreateNavigator
            Dim y = New Xml.XmlDocument(nav.NameTable)
            Using xw = y.CreateNavigator.AppendChild
                transform.Transform(nav, xw)
            End Using
            pms.Add(y)
            r.Add(pdm.FullName)
        Next
        Return String.Join(ControlChars.CrLf, r)
    End Function

    Public Class Info
        Public Name As String
        Public Type As String
        Public Comment As String = ""
        Public Annotation As String = ""
    End Class

    Public Shared Function GetTableInfo(Table As String) As Info
        Dim info = New Info With {.Name = Table, .Type = "table"}
        Dim xpath = String.Format("/physical/model/tables/table[name='{0}']", Table)
        For Each pm In pms
            Dim n = TryCast(pm.SelectSingleNode(xpath), Xml.XmlElement)
            If n IsNot Nothing Then
                info.Comment = value(n, "comment")
                info.Annotation = value(n, "annotation")
                Return info
            End If
        Next
        Return info
    End Function

    Public Shared Function GetFieldInfo(Table As String, field As String) As Info
        Dim info = New Info With {.Name = Table & "." & field}
        Dim xpath = String.Format("/physical/model/tables/table[name='{0}']/fields/field[name='{1}']", Table, field)
        For Each pm In pms
            Dim n = TryCast(pm.SelectSingleNode(xpath), Xml.XmlElement)
            If n IsNot Nothing Then
                info.Type = value(n, "type")
                info.Comment = value(n, "comment")
                info.Annotation = value(n, "annotation")
                Return info
            End If
        Next
        Return info
    End Function

    Private Shared Function value(e As Xml.XmlElement, child As String) As String
        Dim n = e.SelectSingleNode(child)
        If n Is Nothing Then Return String.Empty
        Return n.InnerText
    End Function

    Public Shared Sub ExtendTables(dt As DataTable)
        dt.Columns.Add("pdmComment", GetType(String))
        dt.Columns.Add("pdmAnnotation", GetType(String))
        If Not pms.Any Then Return
        For Each r In dt.Rows.Cast(Of DataRow)
            'select t.table_name as NAME, COALESCE(c.comments, '===') as DESCRIPTION, t.num_rows
            Dim name = r!NAME.ToString()
            Dim comment = r!DESCRIPTION.ToString()
            Dim info = GetTableInfo(name)
            r!pdmComment = info.Comment
            r!pdmAnnotation = info.Annotation
        Next
    End Sub

    Public Shared Sub ExtendColumns(table As String, dt As DataTable)
        dt.Columns.Add("pdmType", GetType(String))
        dt.Columns.Add("pdmComment", GetType(String))
        dt.Columns.Add("pdmAnnotation", GetType(String))
        If Not pms.Any Then Return
        For Each r In dt.Rows.Cast(Of DataRow)
            'select COLUMN_NAME as NAME, COMMENTS as DESCRIPTION
            Dim name = r!NAME.ToString()
            Dim info = GetFieldInfo(table, name)
            r!pdmType = info.Type
            r!pdmComment = info.Comment
            r!pdmAnnotation = info.Annotation
        Next
    End Sub
End Class
