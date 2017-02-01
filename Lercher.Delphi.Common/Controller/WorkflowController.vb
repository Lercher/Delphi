Imports System.Web.Http
Imports Newtonsoft.Json.Linq

Public Class WorkflowController
    Inherits BaseController

    Public Function [Get](id As String, id2 As String, language As String) As WorkflowDefinition
        Dim owner = id
        Dim target = id2
        Return LoadWorkflowDefinition(owner, target, language)
    End Function

    Private Function LoadWorkflowDefinition(owner As String, target As String, language As String) As WorkflowDefinition
        Dim wd = New WorkflowDefinition With {.owner = owner, .target = target, .language = language}

        wd.phases = Delphi.Query.ExecuteDatatable(<x>
SELECT P.PHACODE as "phase", P.PHAORDRE as "order", LP.PHALIBELLE as "label"
FROM <%= owner %>.PHASE P
LEFT OUTER JOIN <%= owner %>.LANPHASE LP ON P.PHACODE = LP.PHACODE
AND P.PHADEST = LP.PHADEST
AND LP.LANCODE = <p><%= language %></p>
WHERE P.PHADEST=<p><%= target %></p> 
ORDER BY P.PHAORDRE
        </x>)

        wd.steps = Delphi.Query.ExecuteDatatable(<x>
SELECT J.JALCODE as "step", J.JALFLAGINTERNE as "internal", LJ.JALLIBELLE as "label"
FROM <%= owner %>.JALON J 
LEFT OUTER JOIN <%= owner %>.LANJALON LJ ON J.JALCODE = LJ.JALCODE
AND LJ.LANCODE = <p><%= language %></p>
ORDER BY J.JALCODE
        </x>)

        wd.relations = Delphi.Query.ExecuteDatatable(<x>
SELECT PHACODE as "phase", JALCODE as "step" 
FROM <%= owner %>.PHAJAL 
WHERE PHADEST=<p><%= target %></p> 
ORDER BY PHACODE, JALCODE
        </x>)

        Return wd
    End Function

    Public Class WorkflowDefinition
        Public Property target As String
        Public Property language As String
        Public Property owner As String
        Public Property phases As DataTable
        Public Property steps As DataTable
        Public Property relations As DataTable
    End Class
End Class
