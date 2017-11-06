Imports System.Reflection
Imports Microsoft.Owin.Hosting

Public Module MainModule

    Public Sub Main(args As String())
        Console.WriteLine("This is Delphi, an Oracle for Data. 2016-2017 M. Lercher")
        Console.WriteLine(My.Computer.FileSystem.CurrentDirectory)
        Dim options = New Delphi.Common.Delphi.ConnectionOptions
        if Not CommandLine.Parser.Default.ParseArguments(args, options) Then
            'Console.WriteLine(options.GetUsage())
            Environment.ExitCode = 99
            Return
        End If

        Delphi.Common.Delphi.Query.Connection = options
        Console.Title = String.Format("{0} on {1} - Delphi", Delphi.Common.Delphi.Query.Connection.UserID, Delphi.Common.Delphi.Query.Connection.Host)
        Console.WriteLine()
        If options.nocache Then
            Console.WriteLine("nocache option (-x) is set, skipping prepopulation of the FK cache")
        Else
            Console.WriteLine("Prepopulating the foreign key cache for '{0}'. This can take some time (at least 15s in the LAN and 40s remote) ...", Delphi.Common.Delphi.Query.Connection.UserID)
            Console.WriteLine(Delphi.Common.Delphi.FKCache.Prepopulate())
        End If
        Console.WriteLine()
        Dim url = String.Format("http://+:{0}/delphi", options.LocalPort)
        Try
            Using app = WebApp.Start(Of Common.Startup)(url)
                Console.WriteLine("Listening on {0}/...", url)
                Console.WriteLine()
                Dim cmd = ""
                Do
                    Console.Write("b - open browser. Press Enter to stop ... ")
                    cmd = Console.ReadLine()
                    Select Case cmd
                        Case "b"
                            StartBrowser
                    End Select
                Loop Until String.IsNullOrEmpty(cmd)
            End Using
        Catch ex As TargetInvocationException
            Console.WriteLine(ex.InnerException.Message)
            Console.Write("Press Enter to stop ... ")
            Console.ReadLine()
        End Try
    End Sub

    Public Sub StartBrowser()
        System.Diagnostics.Process.Start(String.Format("http://localhost:{0}/delphi/#!?owner={1}", Delphi.Common.Delphi.Query.Connection.LocalPort, Delphi.Common.Delphi.Query.Connection.UserID))
    End Sub
End Module
