Public Class HomeController
    Inherits BaseController

    Public Function [Get]() As String
        Return "Hello world!" 
    End Function

    Public Function [Get](id As string) As String
        Return "Hello " & id & "!"
    End Function
End Class
