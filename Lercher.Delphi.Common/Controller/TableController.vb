Imports System.Web.Http
Imports Newtonsoft.Json.Linq

Public Class TableController
    Inherits BaseController

    Function [Get](id As String, id2 As String) As Delphi.TableData.RichtableDescription
        Return Delphi.TableData.RichValues(id, id2, pk:=Nothing)
    End Function

    Function Post(id As String, id2 As String, <FromBody> link As LinkDef) As Delphi.TableData.RichtableDescription
        Dim isForwardDirection = link.direction.ToLowerInvariant.StartsWith("f")
        Return Delphi.TableData.FollowFK(id, id2, link.key, link.pk, isForwardDirection)
    End Function

    Public Class LinkDef
        Public Property direction As String 
        Public Property key As String
        Public Property pk As PKDef()
    End Class

    Public Class PKDef
        Public Property key As String
        Public Property value As String
    End Class

End Class
