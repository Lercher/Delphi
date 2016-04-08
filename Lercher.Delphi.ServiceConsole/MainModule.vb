Imports System.Reflection
Imports Microsoft.Owin.Hosting

Public Module MainModule

    Public Sub Main(args As String())
        Console.WriteLine("This is Delphi, an Oracle for Data. 2016 M. Lercher")
        Dim options = New Delphi.Common.Delphi.ConnectionOptions
        if Not CommandLine.Parser.Default.ParseArguments(args, options) Then
            'Console.WriteLine(options.GetUsage())
            Environment.ExitCode = 99
            Return
        End If

        Delphi.Common.Delphi.Query.Connection = options
        Dim url = "http://+:9001/delphi"
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
        System.Diagnostics.Process.Start("http://localhost:9001/delphi/")
    End Sub
End Module
