Imports System.Web.Http
Imports Newtonsoft.Json.Linq

Public Class RuleController
    Inherits BaseController

    Public Function [Get](id As String, id2 As String, language As String) As RuleDescription
        Dim owner = id
        Dim RuleID = id2
        Return LoadRule(owner, RuleID, language)
    End Function


    Private Function LoadRule(ByVal owner As String, ByVal ruleID As String, ByVal language As String) As RuleDescription
        Dim rd = New RuleDescription With {.owner = owner, .key = ruleID, .language = language}

        rd.rule = Delphi.Query.ExecuteDatatable(<x>
SELECT R.RULID as "rulid", R.RULCODE as "code", R.RULTARGET as "target", R.RULSTATUS as "status", R.RULTYPE as "type"
        , R.RULOPERATOR as "operator"
        , R.RULLOWERLIMIT as "lowerlimit"
        , R.RULUPPERLIMIT as "upperlimit"
        , R.CRIIDFIRST as "firstcriterion" <!-- Or better firstvalue? -->
        , R.RULIDFIRST as "firstrule"
        , R.CRIIDSECONDMIN as "secondcriterionmin"
        , R.CRIIDSECONDMAX as "secondcriterionmax"
        , R.RULFLAGCASSIOPAE as "cassiopae"
        , R.RULGROUP as "group"
        , LR.RULLABEL as "label"
FROM <%= owner %>.RULE R
LEFT OUTER JOIN <%= owner %>.LANRULE LR ON R.RULID = LR.RULID
AND LR.LANCODE = <p><%= language %></p>
WHERE R.RULID=<p><%= ruleID %></p>
        </x>)

        rd.values = Delphi.Query.ExecuteDatatable(<x>
SELECT V.RVAVALUE as "value", V.RULIDVALUE as "rule", V.CRIID as "criterion"
FROM <%= owner %>.RULVALUE V
WHERE V.RULID=<p><%= ruleID %></p>
ORDER BY V.RVAORDRE
        </x>)

        Return rd
    End Function

    Public Class RuleDescription
        Public Property key As String
        Public Property language As String
        Public Property owner As String
        Public Property rule As DataTable
        Public Property values As DataTable
    End Class
End Class
