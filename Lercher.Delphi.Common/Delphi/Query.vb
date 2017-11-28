Imports Oracle.ManagedDataAccess.Client
Imports System.Xml.Linq
Imports System.Text.RegularExpressions
Imports CommandLine

Namespace Delphi

    Public Class Query
        Private Const STR_PARAM_FORMAT_COMMAND As String = ":p{0}"
        Private Const STR_PARAM_FORMAT_VALUE As String = "p{0}"

        Private Shared ReadOnly IdentifierExpression As Regex = New Regex("^[a-z][a-z0-9$_]{0,30}$", RegexOptions.Compiled Or RegexOptions.IgnoreCase)
        '2012-06-15T00:00:00
        Private Shared ReadOnly DatetimeExpression As Regex = New Regex("^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}$", RegexOptions.Compiled Or RegexOptions.IgnoreCase)

        Public Shared Connection As ConnectionOptions
        Public Shared ReadOnly TimingStatistics As New Concurrent.ConcurrentDictionary(Of Integer, Integer)
        Public Shared ReadOnly TimingClassDivisor As Double = 100.0 'ms

        Public Shared Sub AssertIdentifier(s As String, paramname As String)
            If IdentifierExpression.IsMatch(s) Then Return
            Throw New ArgumentException("Identifier expected", paramname)
        End Sub

        Private Shared Function isDatetime(s As String) As Boolean
            Return DatetimeExpression.IsMatch(s)
        End Function

        Public Shared Function ExecuteDatatable(cmdText As String, params() As String) As DataTable
            Dim sw = Stopwatch.StartNew
            Try
                Dim cb = New OracleConnectionStringBuilder()
                cb.DataSource = String.Format("{0}:{1}/{2}", Connection.Host, Connection.Port, Connection.SID)
                cb.UserID = Connection.UserID
                cb.Password = "***"
                cb.ConnectionTimeout = 5
                Diagnostics.Trace.WriteLine("")
                Diagnostics.Trace.WriteLine("")
                Diagnostics.Trace.WriteLine(String.Format("Connecting to Oracle DB: {0} ...", cb))
                cb.Password = Connection.Password
                Dim cs = cb.ConnectionString

                Using con = New OracleConnection(cs)
                    con.Open()
                    Diagnostics.Trace.WriteLine("Connected.")
                    Using cmd = con.CreateCommand()
                        cmd.CommandText = cmdText
                        cmd.BindByName = True ' see http://stackoverflow.com/questions/7493028/ora-01008-not-all-variables-bound-they-are-bound/34597586#34597586
                        Dim n = 0
                        For Each p In params
                            n += 1
                            Dim pn = String.Format(STR_PARAM_FORMAT_VALUE, n)
                            If isDatetime(p) Then
                                Dim dt = DateTime.Parse(p, Globalization.CultureInfo.InvariantCulture, Globalization.DateTimeStyles.RoundtripKind)
                                cmd.Parameters.Add(pn, OracleDbType.Date).Value = dt
                            Else
                                cmd.Parameters.Add(pn, OracleDbType.NVarchar2, 250).Value = p
                            End If
                            Diagnostics.Trace.WriteLine(String.Format("  :{0} = '{1}'", pn, p))
                        Next
                        Diagnostics.Trace.WriteLine(cmd.CommandText)
                        Dim tbl = New DataTable
                        Dim da = New OracleDataAdapter(cmd)
                        da.Fill(tbl)
                        sw.Stop()
                        Dim stats = RecordTiming(sw.Elapsed)
                        Diagnostics.Trace.WriteLine(String.Format("----- {0:n0} line(s) affected. Processing time: {1}. Stats: {2}", tbl.Rows.Count, sw.Elapsed, stats))
                        Return tbl
                    End Using
                End Using
            Catch ex As OracleException
                Dim msg = String.Format("{0} in {1}", ex.Message, cmdText)
                Diagnostics.Trace.TraceError(msg)
                Console.Beep()
                Throw New ApplicationException(msg, ex)
            Finally
                sw.Stop()
            End Try
        End Function

        Private Shared Function RecordTiming(ts As TimeSpan) As String
            Try
                Dim el = ts.TotalMilliseconds
                Const more = 5000
                Dim discrete = Math.Min(more, CInt(Math.Round(el / TimingClassDivisor) * TimingClassDivisor))
                TimingStatistics.AddOrUpdate(discrete, 1, Function(k, v) v + 1)
                Dim qy = From kv In TimingStatistics Order By kv.Key Select s = String.Format(If(kv.Key = more, "{1:n0} more", "{1:n0}x{0:n0}ms"), kv.Key, kv.Value)
                Return Join(qy.ToArray, " | ")
            Finally : End Try
        End Function


        Public Shared Function ExecuteDatatable(cmdText As XElement) As DataTable
            Dim sb As New System.Text.StringBuilder
            Dim p = New List(Of String)
            AppendNodes(sb, p, cmdText.Nodes)
            Return ExecuteDatatable(sb.ToString, p.ToArray)
        End Function

        Private Shared Sub AppendNodes(ByVal sb As System.Text.StringBuilder, ByVal p As System.Collections.Generic.List(Of String), ByVal nodes As IEnumerable(Of XNode))
            For Each n In nodes
                If TypeOf n Is XText Then
                    Dim t = DirectCast(n, XText)
                    sb.Append(t.Value)
                ElseIf TypeOf n Is XElement Then
                    Dim e = DirectCast(n, XElement)
                    If e.Name.LocalName = "text" Then
                        AppendNodes(sb, p, e.Nodes)
                    Else
                        p.Add(e.Value)
                        sb.AppendFormat(STR_PARAM_FORMAT_COMMAND, p.Count)
                    End If
                End If
            Next
        End Sub

        Public Shared Function ExecuteArray(cmdText As XElement) As String()
            Dim dt = ExecuteDatatable(cmdText)
            Dim list = New List(Of String)
            For Each r As DataRow In dt.Rows
                list.Add(r.Field(Of String)(0))
            Next
            Return list.ToArray
        End Function
    End Class
End Namespace
