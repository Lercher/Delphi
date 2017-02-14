Imports System.Web.Http
Imports Newtonsoft.Json.Linq

Public Class EventController
    Inherits BaseController

    Public Function [Get](id As String, id2 As String, language As String, code As string) As EventDefinition
        Return LoadEventDefinition(id, id2, language, code)
    End Function

    Private Function LoadEventDefinition(owner As String, target As String, language As String, code As string) As EventDefinition
        Dim ed = New EventDefinition With {.owner = owner, .target = target, .language = language, .code = code}

        ed.events = Delphi.Query.ExecuteDatatable(<x>
SELECT E.*, LE.TEVLIBELLE as "label"
FROM <%= owner %>.TEVENEMENT E
LEFT OUTER JOIN <%= owner %>.LANTEVENEMENT LE 
ON E.TEVDEST = LE.TEVDEST        
AND E.TACCODE = LE.TACCODE      
AND E.TMOMODULE = LE.TMOMODULE   
AND E.TMFFONCTION = LE.TMFFONCTION
AND LE.LANCODE = <p><%= language %></p>
WHERE E.TEVDEST = <p><%= target %></p>
AND E.TACCODE = <p><%= code %></p>
ORDER BY E.TMFFONCTION, E.TACCODE, E.TMOMODULE
        </x>)

        Return ed
    End Function

    Public Class EventDefinition
        Public Property target As String
        Public Property language As String
        Public Property owner As String
        Public Property code As String 
        Public Property events As DataTable
    End Class
End Class
