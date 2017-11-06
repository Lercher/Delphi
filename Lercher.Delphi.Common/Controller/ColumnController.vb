Imports System.Web.Http
Imports Newtonsoft.Json.Linq


Public Class ColumnController
    Inherits BaseController

    Function [Get](id As String, id2 As String) As DataTable
        Return Delphi.Columns.SearchAll(owner:=id, searchfor:=id2)
    End Function

End Class
