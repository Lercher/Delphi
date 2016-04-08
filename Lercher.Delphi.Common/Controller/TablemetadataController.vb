
Public Class TablemetadataController
    Inherits BaseController

    Function [Get](id As String, id2 As String) As DataTable
        Return Delphi.Table.PointersAndLists(id, id2)
    End Function
End Class