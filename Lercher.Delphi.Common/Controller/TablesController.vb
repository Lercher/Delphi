Public Class TablesController
    Inherits BaseController

    Function [Get](id As String) As DataTable
        Dim dt = Delphi.Table.Names(id)
        PhysicalModel.ExtendTables(dt)
        Return dt
    End Function
End Class
