﻿Imports System.Web.Http
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
SELECT W.WORCODE as "key", W.WORDEST as "target"
        , LW.WORLABEL as "label", LW.WORDESCRIPTION as "description"
FROM <%= owner %>.WORKFLOW W
LEFT OUTER JOIN <%= owner %>.LANWORKFLOW LW ON W.WORCODE = LW.WORCODE
AND LW.LANCODE = <p><%= language %></p>
WHERE W.WORCODE=<p><%= WorkflowKey %></p>
                                                    </x>)

        wd.steps = Delphi.Query.ExecuteDatatable(<x>
SELECT 
        S.WSTORDER as "order", S.WSTEXECUTIONORDER as "executionorder", S.WSTTYPE as "type"
        , S.WSTACTIONTYPE as "actiontype", S.WSTACTIONCODE as "action", S.WSTACTIONMODE as "actionmode"
        , S.WSTFLAGEXECUTEONCE as "executeonce", S.WSTEXECUTIONMODE as "executionmode"
        , S.RULIDJUMPAUTO as "autojumprule"
        , LS.WSTLABEL as "label", LS.WSTDESCRIPTION as "description"
FROM <%= owner %>.WORSTEP S
LEFT OUTER JOIN <%= owner %>.LANWORSTEP LS ON S.WORCODE = LS.WORCODE
AND S.WSTORDER = LS.WSTORDER
AND LS.LANCODE = <p><%= language %></p>
WHERE S.WORCODE=<p><%= WorkflowKey %></p> 
ORDER BY S.WSTORDER
        </x>)

        wd.consequences = Delphi.Query.ExecuteDatatable(<x>
SELECT 
        C.WSTORDER as "order", C.WSCORDER as "consequenceorder"
        , C.WSCACTIONTYPE as "actiontype", C.WSCACTIONCODE as "action", C.WSCACTIONMODE as "actionmode"
        , C.WSCFLAGMAIL as "mail"
        , C.WORCODEDEST as "otherworkflow"
        , LC.WSTLABEL as "label", LC.WSTDESCRIPTION as "description"
FROM <%= owner %>.WSTCONSEQUENCE C
LEFT OUTER JOIN <%= owner %>.LANWSTCONSEQUENCE LC ON C.WORCODE = LC.WORCODE
AND C.WSTORDER = LC.WSTORDER
AND C.WSCORDER = LC.WSCORDER
AND LC.LANCODE = <p><%= language %></p>
WHERE C.WORCODE=<p><%= WorkflowKey %></p> 
ORDER BY C.WSTORDER, C.WSCORDER
        </x>)

                wd.jumps = Delphi.Query.ExecuteDatatable(<x>
SELECT 
        J.WSTORDER as "order"
        , J.WSTORDERDEST as "targetorder"
        , J.WORCODEDEST as "targetworkflow"
        , J.RULID as "rule"
FROM <%= owner %>.WSTJUMP J
WHERE J.WORCODE=<p><%= WorkflowKey %></p> 
ORDER BY J.WSTORDER, J.WORCODEDEST, J.WSTORDERDEST
        </x>)

        Return wd
    End Function

    Public Class WorkflowDefinition
        Public Property workflowkey As String
        Public Property language As String
        Public Property owner As String
        Public Property workflow As DataTable
        Public Property steps As DataTable
        Public Property jumps As DataTable
        Public Property consequences As DataTable
    End Class
End Class