Namespace Delphi
    Public Class PKCache
        Private Shared ReadOnly PKsByOwner As New Concurrent.ConcurrentDictionary(Of String, IEnumerable(Of KeyValuePair(Of String, String)))
        Private Shared Enabled As Boolean = False

        ''' <summary>
        ''' We guess that the UserID of the connection will be the user for the schema information as well
        ''' </summary>
        Public Shared Function Prepopulate() As String
            Try
                Dim sw = Stopwatch.StartNew
                Dim result = PKs(Delphi.Query.Connection.UserID)
                sw.Stop()
                Enabled = True
                Return String.Format("Loaded {0:n0} primary key infos in {1}", result.Count, sw.Elapsed)
            Catch ex As Exception
                Return ex.Message
            End Try
        End Function

        Public Shared Function PKs(owner As String) As IEnumerable(Of KeyValuePair(Of String, String))
            Return PKsByOwner.GetOrAdd(owner, AddressOf QueryPK)
        End Function

        Public Shared Function PKs(owner As String, tablename As String) As String()
            If Not Enabled Then Return Nothing
            Dim all = PKs(owner)
            Dim qy = From pk In all Where pk.Key = tablename Select pk.Value  ' Don't order by!
            Return qy.ToArray
        End Function

        Public Shared Function QueryPK(owner As String) As IEnumerable(Of KeyValuePair(Of String, String))
            Dim x =
                <x>
SELECT c_list.TABLE_NAME, c_src.COLUMN_NAME
FROM ALL_CONSTRAINTS c_list
INNER JOIN ALL_CONS_COLUMNS c_src 
ON c_list.CONSTRAINT_NAME = c_src.CONSTRAINT_NAME AND c_list.OWNER = c_src.OWNER
WHERE c_list.CONSTRAINT_TYPE = 'P'
AND c_list.OWNER = <p><%= owner %></p>
ORDER BY c_list.TABLE_NAME, c_src.POSITION
                </x>
            Dim dt = Delphi.Query.ExecuteDatatable(x)
            Dim qy = From r In dt.Rows.Cast(Of DataRow) Select New KeyValuePair(Of String, String)(r.Item(0).ToString, r.Item(1).ToString)
            Return qy.ToArray
        End Function
    End Class
End Namespace

'--- because of: ---

'  :p1 = 'TRREADY45'
'  :p2 = 'ACTADRESSE'
'SELECT c_src.COLUMN_NAME
'FROM ALL_CONSTRAINTS c_list
'INNER JOIN ALL_CONS_COLUMNS c_src
'ON c_list.CONSTRAINT_NAME = c_src.CONSTRAINT_NAME AND c_list.OWNER = c_src.OWNER
'WHERE c_list.CONSTRAINT_TYPE = 'P'
'AND c_list.OWNER = :p1 AND c_list.TABLE_NAME = :p2
'ORDER BY c_src.POSITION
'--- 2 line(s) affected. Processing time: 00:00:04.5805686
