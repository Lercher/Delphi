Public Class TablesController
    Inherits BaseController

    Function [Get](id As String) As DataTable
        Return Delphi.Table.Names(id)
    End Function
End Class
