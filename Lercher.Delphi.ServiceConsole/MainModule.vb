﻿Imports System.Reflection
Imports Microsoft.Owin.Hosting
Imports Lercher.Delphi.Common.Delphi

Public Module MainModule

    Public Sub Main(args As String())
        Console.WriteLine("This is Delphi, an Oracle for Data. 2016-2017 M. Lercher")
        Console.WriteLine(My.Computer.FileSystem.CurrentDirectory)
        Query.Connection = New Delphi.Common.Delphi.ConnectionOptions
        If Not CommandLine.Parser.Default.ParseArguments(args, Query.Connection) Then
            'Console.WriteLine(options.GetUsage())
            Environment.ExitCode = 99
            Return
        End If

        Console.Title = String.Format("{0} on {1} - Delphi", Query.Connection.UserID, Query.Connection.Host)
        Console.WriteLine()
        Console.WriteLine(Console.Title)

        Try
            Console.WriteLine("Loading physical model files from: {0}", Query.Connection.Modeldirectory)
            Console.WriteLine(Common.PhysicalModel.LoadAllPdmFrom(Query.Connection.Modeldirectory))
        Catch ex As Exception
            Console.WriteLine(ex.Message)
            Console.WriteLine("Physical model file ignored b/c of previous error")
        End Try

        If Query.Connection.nocache Then
            Console.WriteLine("nocache option (-x) is set, skipping prepopulation of the PK/FK cache")
        Else
            System.Threading.ThreadPool.QueueUserWorkItem(Sub() FillCaches())
        End If
        Console.WriteLine()
        Dim url = String.Format("http://+:{0}/delphi", Query.Connection.LocalPort)
        Try
            Using app = WebApp.Start(Of Common.Startup)(url)
                Console.WriteLine("Listening on {0}/...", url)
                Console.WriteLine()
                Dim cmd = ""
                Do
                    Console.Write("b - open browser, c - clear. Press Enter to stop ... ")
                    cmd = Console.ReadLine()
                    Select Case cmd
                        Case "b"
                            StartBrowser()
                        Case "c"
                            Console.Clear()
                    End Select
                Loop Until String.IsNullOrEmpty(cmd)
            End Using
        Catch ex As TargetInvocationException
            Console.WriteLine("When opening {0}: {1}", url, ex.InnerException.Message)
            Console.Write("Press Enter to stop ... ")
            Console.ReadLine()
        End Try
    End Sub

    Private Sub FillCaches()
        Console.WriteLine("Prefilling some caches. This can take some time (at least 20s in the LAN and 60s remote), consider using -x option.")
        Console.WriteLine("Prepopulating the primary key cache for '{0}' ...", Query.Connection.UserID)
        Console.WriteLine(Delphi.Common.Delphi.PKCache.Prepopulate())
        Console.WriteLine("Prepopulating the foreign key cache for '{0}' ...", Query.Connection.UserID)
        Console.WriteLine(Delphi.Common.Delphi.FKCache.Prepopulate())
        Console.WriteLine()
        Console.WriteLine("Checking for Heap tables, i.e. without PK ...")
        Dim hts = Delphi.Common.Delphi.Table.HeapTables(Query.Connection.UserID)
        Dim fc = Console.ForegroundColor
        Console.ForegroundColor = ConsoleColor.Red
        Dim n = 0
        For Each ht In hts
            n += 1
            Console.WriteLine("{0,4}. {1}", n, ht)
        Next
        Console.ForegroundColor = fc
        Console.WriteLine()
        Console.Write("b - open browser, c - clear. Press Enter to stop ... ")
    End Sub

    Public Sub StartBrowser()
        System.Diagnostics.Process.Start(String.Format("http://localhost:{0}/delphi/#!?owner={1}", Query.Connection.LocalPort, Query.Connection.UserID))
    End Sub
End Module
