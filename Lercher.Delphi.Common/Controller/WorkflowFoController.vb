Imports System.Web.Http
Imports Newtonsoft.Json.Linq

Public Class WorkflowFoController
    Inherits BaseController

    Public Function [Get](id As String, id2 As String, language As String) As WorkflowDefinition
        Dim owner = id
        Dim WorkflowKey = id2
        Return LoadWorkflowDefinition(owner, WorkflowKey, language)
    End Function

    Private Function LoadWorkflowDefinition(owner As String, WorkflowKey As String, language As String) As WorkflowDefinition
        Dim wd = New WorkflowDefinition With {.owner = owner, .workflowkey = WorkflowKey, .language = language}

        wd.workflow = Delphi.Query.ExecuteDatatable(<x>
SELECT W.WORCODE as "key", W.WORDEST as "target", LW.WORLABEL as "label", LW.WORDESCRIPTION as "description"
FROM <%= owner %>.WORKFLOW W
LEFT OUTER JOIN <%= owner %>.LANWORKFLOW LW ON W.WORCODE = LW.WORCODE
AND LW.LANCODE = <p><%= language %></p>
WHERE W.WORCODE=<p><%= WorkflowKey %></p> 
        </x>)

        Return wd
    End Function

    Public Class WorkflowDefinition
        Public Property workflowkey As String
        Public Property language As String
        Public Property owner As String
        Public Property workflow As DataTable
    End Class
End Class
