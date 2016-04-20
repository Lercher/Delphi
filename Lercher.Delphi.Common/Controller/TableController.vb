Imports System.Web.Http
Imports Newtonsoft.Json.Linq


Public Class TableController
    Inherits BaseController

    Function [Get](id As String, id2 As String) As Delphi.TableData.RichtableDescription
        Return Delphi.TableData.RichValues(owner:=id, tablename:=id2, pk:=Nothing)
    End Function

    Function Post(id As String, id2 As String, <FromBody> link As LinkDef) As Delphi.TableData.RichtableDescription
        If link.direction.ToLowerInvariant = "pk" Then
            Return Delphi.TableData.RichValues(owner:=id, tablename:=id2, pk:=link.pk)
        End If
        Dim isForwardDirection = link.direction.ToLowerInvariant.StartsWith("f")
        Return Delphi.TableData.FollowFK(owner:=id, tablename:=id2, fkname:=link.key, pk:=link.pk, isForwardDirection:=isForwardDirection)
    End Function

    #Disable Warning S2365 ' Properties should not be based on arrays - OK because LinkDef/pk is just a WebApi parameter object
    Public Class LinkDef
        Public Property direction As String
        Public Property key As String
        Public Property pk As PKDef()
    End Class
    #Enable Warning S2365 ' Properties should not be based on arrays

    Public Class PKDef
        Public Property key As String
        Public Property value As String
    End Class

End Class
