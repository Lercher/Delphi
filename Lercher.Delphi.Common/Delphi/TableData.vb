Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq

Namespace Delphi
    Public Class TableData
        Public Shared Function Values(owner As string, tablename As string) As DataTable
            Query.AssertIdentifier(owner, NameOf(owner))
            Query.AssertIdentifier(tablename, NameOf(tablename))
            Dim x =
                <x>
                    select * 
                    from <%= owner %>.<%= tablename %>
                    <%= If(Query.Connection.LimitRows > 0, String.Format(" where ROWNUM <= {0} ", Query.Connection.LimitRows), " ") %>
                    order by 1
                </x>
            Dim result = Query.ExecuteDatatable(x)
            If Query.Connection.LimitRows > 0 Then
                Console.WriteLine("Limit of max. {0:n0} rows applied to the last query (see -l option).", Query.Connection.LimitRows)
            Else
                Console.WriteLine("Warning: Unlimited number rows was requested.")
            End If
            Return result
        End Function

        Public Shared Function ValuesOf(owner As String, tablename As String, pk As IEnumerable(Of TableController.PKDef)) As DataTable
            If pk Is Nothing OrElse Not pk.Any Then Throw New ApplicationException(String.Format("Can't query TableData.ValuesOf for table {0}.{1} with missing PK and therefor no PK values.", owner, tablename))
            Query.AssertIdentifier(owner, NameOf(owner))
            Query.AssertIdentifier(tablename, NameOf(tablename))
            Dim x =
                <x>
                    select * 
                    from <%= owner %>.<%= tablename %>
                    where ROWNUM &gt; -1
                    <%= From p In pk Select
                       <text> AND <%= p.key %> = <p><%= p.value %></p></text>
                    %>
                </x>
            Return Query.ExecuteDatatable(x)
        End Function

        Public Shared Function FollowFK(owner As String, tablename As string, fkname as string, pk as TableController.PKDef(), isForwardDirection As Boolean) As RichtableDescription
            Dim metadata = Table.PointersAndLists(owner, tablename)
            Dim md = JsonConvert.SerializeObject(metadata)
            Dim FKs = JsonConvert.DeserializeObject(of fk())(md)
            Dim qy =
                from f in FKs
                where f.key = fkname
                Group By f.srctable, f.desttable
                Into alle = Group
            dim fk = qy.FirstOrDefault
            If fk is Nothing then Throw new ArgumentException("Named foreign key not found: " & fkname, NameOf(fkname))
            Dim newpk = new List(of TableController.PKDef)
            Dim original = ValuesOf(owner, tablename, pk)
            for each c in fk.alle
                dim cc = new TableController.PKDef() With {.key = If (isForwardDirection, c.destcolumn, c.srccolumn)}
                Dim o As Object = original.Rows(0).Item(If (not isForwardDirection, c.destcolumn, c.srccolumn))
                Select case o.GetType
                    Case GetType(DateTime)
                        dim dt = DirectCast(o, DateTime)
                        ' This example displays the following output to the console:
                        '       d: 6/15/2008
                        '       D: Sunday, June 15, 2008
                        '       f: Sunday, June 15, 2008 9:15 PM
                        '       F: Sunday, June 15, 2008 9:15:07 PM
                        '       g: 6/15/2008 9:15 PM
                        '       G: 6/15/2008 9:15:07 PM
                        '       m: June 15
                        '       o: 2008-06-15T21:15:07.0000000
                        '       R: Sun, 15 Jun 2008 21:15:07 GMT
                        '       s: 2008-06-15T21:15:07
                        '       t: 9:15 PM
                        '       T: 9:15:07 PM
                        '       u: 2008-06-15 21:15:07Z
                        '       U: Monday, June 16, 2008 4:15:07 AM
                        '       y: June, 2008
                        '       
                        '       'h:mm:ss.ff t': 9:15:07.00 P
                        '       'd MMM yyyy': 15 Jun 2008
                        '       'HH:mm:ss.f': 21:15:07.0
                        '       'dd MMM HH:mm:ss': 15 Jun 21:15:07
                        '       '\Mon\t\h\: M': Month: 6
                        '       'HH:mm:ss.ffffzzz': 21:15:07.0000-07:00
                        cc.value = dt.ToString("s") ' s: 2008-06-15T21:15:07
                    case Else
                        cc.value = o.ToString
                End Select
                newpk.Add(cc)
            Next

            if isForwardDirection Then
                Return RichValues(owner, fk.desttable, newpk)
            Else
                Return RichValues(owner, fk.srctable, newpk)
            End If
        End Function

        Public Shared Function RichValues(owner As String, tablename As string, pk as IEnumerable(Of TableController.PKDef)) As RichtableDescription
            Dim info = PhysicalModel.GetTableInfo(tablename)
            Dim r = New RichtableDescription With {.owner = owner, .table = tablename, .pdmComment = info.Comment, .pdmAnnotation = info.Annotation}
            If pk Is Nothing Then
                r.values = Values(owner, tablename)
            Else
                r.values = ValuesOf(owner, tablename, pk)
            end If
            r.metadata = Table.PointersAndLists(owner, tablename)
            r.columns = Table.Columns(owner, tablename)
            PhysicalModel.ExtendColumns(tablename, r.columns)
            r.pk = Table.PrimaryKeyColumns(owner, tablename)
            r.description = Table.Description(owner, tablename)
            Dim md = JsonConvert.SerializeObject(r.metadata)
            Dim FKs = JsonConvert.DeserializeObject(of fk())(md)
            Dim qy =
                    from f in FKs
                    Group By f.srctable, f.key, f.desttable
                    Into alle = Group
            for each q in qy.Where(Function(f) f.srctable = tablename)
                Dim key = new JProperty("key", q.key)
                Dim source = new JProperty("source", q.srctable)
                Dim destination = new JProperty("destination", q.desttable)
                Dim matchcolumn = New JProperty("matchcolumn", new JArray(from c in q.alle Select new JObject(new JProperty("src", c.srccolumn), new JProperty("dest", c.destcolumn))))
                r.links(q.key) = new JObject(source, destination, matchcolumn, key)
            Next
            for each q in qy.Where(Function(f) f.desttable = tablename)
                Dim key = new JProperty("key", q.key)
                Dim source = new JProperty("source", q.srctable)
                Dim destination = new JProperty("destination", q.desttable)
                Dim matchcolumn = New JProperty("matchcolumn", new JArray(from c in q.alle Select new JObject(new JProperty("src", c.srccolumn), new JProperty("dest", c.destcolumn))))
                r.details(q.key) = new JObject(source, destination, matchcolumn, key)
            Next
            Return r
        End Function

        Public Class FK
            Public Property key as String
            Public Property srctable as String
            Public Property desttable as string
            Public Property srccolumn as string
            Public Property destcolumn as String
        End Class

        #Disable Warning S2365 ' Properties should not be based on arrays - OK because RichtableDescription/pk is just a WebApi parameter object
        Public Class RichtableDescription
            Public Property owner As String
            Public Property table As String
            Public Property description As String
            Public Property pdmComment As String
            Public Property pdmAnnotation As String
            Public Property values As DataTable
            Public Property metadata As DataTable
            Public Property columns As DataTable
            Public Property links As New JObject
            Public Property details As New JObject
            Public Property pk as string()
        End Class
        #Enable Warning S2365 ' Properties should not be based on arrays

    End Class
End Namespace
