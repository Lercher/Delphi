
Namespace Delphi
    Public Class Table

        Public Shared Function Names(owner As String) As DataTable
            Dim x =
                <x>
select t.table_name as NAME, COALESCE(c.comments, '===') as DESCRIPTION, t.num_rows
from all_tab_comments c 
inner JOIN sys.dba_tables t on c.table_name = t.table_name and c.owner = t.owner
where Upper(t.owner)=<p><%= owner.ToUpper %></p> 
and not t.table_name like 'V45_%'
and not t.table_name like 'TMP%'
order by t.table_name
                </x>
            Return Query.ExecuteDatatable(x)
        End Function

        Public Shared Function Columns(owner As String, tablename As String) As DataTable
            Dim x =
                <x>
select COLUMN_NAME as NAME, COMMENTS as DESCRIPTION from all_col_comments
where owner=<p><%= owner %></p> and TABLE_NAME=<p><%= tablename %></p>
                </x>
            Return Query.ExecuteDatatable(x)
        End Function

        Public Shared Function HeapTables(owner As String) As String()
            Dim dt = Names(owner)
            Dim nameindex = dt.Columns("NAME").Ordinal
            Dim qy =
                From r In dt.Rows.Cast(Of DataRow)
                Select tablename = r.Item(nameindex).ToString
                Where Not PrimaryKeyColumns(owner, tablename).Any
                Select String.Concat(owner, ".", tablename)
            Return qy.ToArray
        End Function

        Public Shared Function PrimaryKeyColumns(owner As String, tablename As String) As String()
            Dim cached = PKCache.PKs(owner, tablename)
            If cached IsNot Nothing Then Return cached
            Dim x =
                <x>
SELECT c_src.COLUMN_NAME
FROM ALL_CONSTRAINTS c_list
INNER JOIN ALL_CONS_COLUMNS c_src 
ON c_list.CONSTRAINT_NAME = c_src.CONSTRAINT_NAME AND c_list.OWNER = c_src.OWNER
WHERE c_list.CONSTRAINT_TYPE = 'P'
AND c_list.OWNER = <p><%= owner %></p> AND c_list.TABLE_NAME = <p><%= tablename %></p>
ORDER BY c_src.POSITION
                </x>
            Return Query.ExecuteArray(x)
        End Function



        Public Shared Function PointersAndLists(owner As String, tablename As String) As DataTable
            Static cache As New Concurrent.ConcurrentDictionary(Of Tuple(Of String, String), DataTable)
            Return cache.GetOrAdd(Tuple.Create(owner, tablename), AddressOf PointersAndListsImpl)
        End Function

        Private Shared Function PointersAndListsImpl(Owner_Tablename As Tuple(Of String, String)) As DataTable
            Dim owner = Owner_Tablename.Item1
            Dim tablename = Owner_Tablename.Item2
            Query.AssertIdentifier(owner, NameOf(owner))
            Query.AssertIdentifier(tablename, NameOf(tablename))
            Dim cached = FKCache.FKs(owner, tablename)
            If cached IsNot Nothing Then Return cached

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
  c_list.OWNER = <p><%= owner %></p> AND c_list.TABLE_NAME = <p><%= tablename %></p>
OR
  c_dest.OWNER = <p><%= owner %></p> AND c_dest.TABLE_NAME = <p><%= tablename %></p>
)
ORDER BY c_list.TABLE_NAME, c_dest.TABLE_NAME, c_list.CONSTRAINT_NAME, c_src.POSITION
                </x>

            Return Query.ExecuteDatatable(x)
        End Function

        Private Shared Function isEqual(d1 As DataTable, d2 As DataTable) As Boolean
            Dim a = asString(d1)
            Dim b = asString(d2)
            If a = b Then Return True
            Dim linesA = Split(a, ControlChars.CrLf)
            Dim linesB = Split(b, ControlChars.CrLf)
            For i = 0 To Math.Min(linesA.Count, linesB.Count) - 1
                If linesA(i) = linesB(i) Then Continue For
                Console.WriteLine("line {0,4}: {1,-40} {2,-40}", i + 1, linesA(i), linesB(i))
            Next
            Return False
        End Function

        Private Shared Function asString(d As DataTable) As String
            d.TableName = "d1"
            Dim sb = New Text.StringBuilder
            Using sw = New IO.StringWriter(sb)
                d.WriteXml(sw)
            End Using
            Return sb.ToString
        End Function


    End Class
End Namespace
