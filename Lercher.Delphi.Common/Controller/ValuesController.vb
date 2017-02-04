Imports System.Web.Http
Imports Newtonsoft.Json.Linq

Public Class ValuesController
    Inherits BaseController

    Public Function [Get](id As String, id2 As String) As String()
        Dim owner = id
        Select Case id2.ToUpperInvariant
            Case "LANGUAGES"
                Return Languages(owner)
            Case "TARGETS"
                Return WorkflowTargets(owner)
            Case "WORKFLOWS"
                Return WorkflowKeys(owner)
        End Select
        Throw New NotImplementedException("No values for " & id2)
    End Function

    Private Function Languages(owner As String) As String()
        Dim x =
            <x>SELECT LANCODE FROM <%= owner %>.LANGUE ORDER BY LANCODE</x>
        Return Delphi.Query.ExecuteArray(x)
    End Function

    Private Function WorkflowTargets(ByVal owner As String) As String()
        Dim x =
            <x>SELECT DISTINCT PHADEST FROM <%= owner %>.PHASE ORDER BY PHADEST</x>
        Return Delphi.Query.ExecuteArray(x)
    End Function

    Private Function WorkflowKeys(owner As String) As String()
        Dim x =
            <x>SELECT WORCODE FROM <%= owner %>.WORKFLOW ORDER BY WORCODE</x>
        Return Delphi.Query.ExecuteArray(x)
    End Function
End Class
