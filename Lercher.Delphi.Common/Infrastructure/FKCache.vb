Namespace Delphi
    Public Class FKCache
        Private Shared ReadOnly FKsByOwner As New Concurrent.ConcurrentDictionary(Of String, DataTable)
        Private Shared Enabled As Boolean = False

        ''' <summary>
        ''' We guess that the UserID of the connection will be the user for the schema information as well
        ''' </summary>
        Public Shared Function Prepopulate() As String
            Try
                Dim sw = Stopwatch.StartNew
                Dim result = FKs(Delphi.Query.Connection.UserID)
                sw.Stop()
                Enabled = True
                Return String.Format("Loaded {0:n0} foreign key infos in {1}", result.Rows.Count, sw.Elapsed)
            Catch ex As Exception
                Return ex.Message
            End Try
        End Function

        Public Shared Function FKs(owner As String) As DataTable
            Return FKsByOwner.GetOrAdd(owner, AddressOf QueryFK)
        End Function

        Public Shared Function FKs(owner As String, tablename As String) As DataTable
            If Not Enabled Then Return Nothing
            Dim all = FKs(owner)
            Dim view = New DataView(all)
            view.RowFilter = String.Format("srctable='{0}' or desttable='{0}'", tablename)
            view.Sort = "srctable, desttable, key, position"
            Return view.ToTable
        End Function

        Public Shared Function QueryFK(owner As String) As DataTable
            Dim x = <x>
SELECT
    c_list.CONSTRAINT_NAME as key,
    c_list.TABLE_NAME as srctable,
    c_src.COLUMN_NAME as srccolumn,
    c_dest.TABLE_NAME as desttable,
    c_dest.COLUMN_NAME as destcolumn,
    c_src.POSITION as position
FROM ALL_CONSTRAINTS c_list
INNER JOIN ALL_CONS_COLUMNS c_src ON c_list.CONSTRAINT_NAME = c_src.CONSTRAINT_NAME AND c_list.OWNER = c_src.OWNER
INNER JOIN ALL_CONS_COLUMNS c_dest ON c_list.R_CONSTRAINT_NAME = c_dest.CONSTRAINT_NAME AND c_list.R_OWNER = c_dest.OWNER
WHERE c_list.CONSTRAINT_TYPE = 'R'
AND c_src.POSITION = c_dest.POSITION
AND
(
  c_list.OWNER = <p><%= owner %></p>
  OR
  c_list.R_OWNER = <p><%= owner %></p>
)
                </x>
            ' Don't ORDER BY this on the server, it probably don't have space for this on disk
            ' ORDER BY c_src.TABLE_NAME, c_dest.TABLE_NAME, c_list.CONSTRAINT_NAME, c_src.POSITION
            Return Delphi.Query.ExecuteDatatable(x)
        End Function
    End Class
End Namespace
