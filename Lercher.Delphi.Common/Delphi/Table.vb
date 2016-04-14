
Namespace Delphi
    Public Class Table

        Public shared Function Names(owner As String) As DataTable
            Dim x =
                <x>
select table_name as NAME, COALESCE(comments, '===') as DESCRIPTION
from all_tab_comments
where Upper(owner)=<p><%= owner.ToUpper %></p> 
and not table_name like 'V45_%'
and not table_name like 'TMP%'
order by table_name
                </x>
            return Query.ExecuteDatatable(x)
        End Function

        Public Shared Function Columns(owner as string, tablename as string) As DataTable
            Dim x = 
                <x>
select COLUMN_NAME as NAME, COMMENTS as DESCRIPTION from all_col_comments
where owner=<p><%= owner %></p> and TABLE_NAME=<p><%= tablename %></p>
                </x>
            return Query.ExecuteDatatable(x)
        End Function

        Public Shared Function PrimaryKeyColumns(owner as string, tablename as string) as string()
            dim x =
                <x>
SELECT c_src.COLUMN_NAME
FROM ALL_CONSTRAINTS c_list
INNER JOIN ALL_CONS_COLUMNS c_src 
ON c_list.CONSTRAINT_NAME = c_src.CONSTRAINT_NAME AND c_list.OWNER = c_src.OWNER
WHERE c_list.CONSTRAINT_TYPE = 'P'
AND c_list.OWNER = <p><%= owner %></p> AND c_list.TABLE_NAME = <p><%= tablename %></p>
ORDER BY c_src.POSITION
                </x>
            return Query.ExecuteArray(x)
        End Function



        Public Shared Function PointersAndLists(owner As String, tablename As string) As DataTable
            Query.AssertIdentifier(owner, NameOf(owner))
            Query.AssertIdentifier(tablename, NameOf(tablename))
            Dim x = <x>
SELECT
    c_list.CONSTRAINT_NAME as key,
    c_list.TABLE_NAME as srctable,
    c_src.COLUMN_NAME as srccolumn,
    c_dest.TABLE_NAME as desttable,
    c_dest.COLUMN_NAME as destcolumn
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
ORDER BY c_src.TABLE_NAME, c_dest.TABLE_NAME, c_list.CONSTRAINT_NAME, c_src.POSITION
                </x>
            return Query.ExecuteDatatable(x)
        End Function





    End Class
End Namespace
