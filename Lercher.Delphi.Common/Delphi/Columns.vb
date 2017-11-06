
Namespace Delphi
    Public Class Columns

        Public Shared Function SearchAll(owner As String, searchfor As String) As DataTable
            Dim lk = String.Format("%{0}%", searchfor.ToUpperInvariant())
            Dim x =
                <x>
SELECT TABLE_NAME as "tablename", COLUMN_NAME as "columnname", COALESCE(COMMENTS, '===') as "comments"
FROM SYS.ALL_COL_COMMENTS
WHERE owner=<p><%= owner %></p> 
AND ( UPPER(COLUMN_NAME) LIKE <p><%= lk %></p> OR UPPER(COMMENTS) LIKE :p2 ) 
ORDER BY TABLE_NAME, COLUMN_NAME
                </x>
            Return Query.ExecuteDatatable(x)
        End Function

    End Class
End Namespace
