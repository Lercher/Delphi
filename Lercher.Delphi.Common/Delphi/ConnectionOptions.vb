Imports CommandLine

Namespace Delphi
    Public Class ConnectionOptions
        <[Option]("p"c, "port", DefaultValue:=1521, HelpText:="Port number of the oracle connection", Required:=False)>
        Public Property Port as integer

        <[Option]("h"c, "host", HelpText:="Server name of the oracle database", Required:=True)>
        Public Property Host As String

        <[Option]("s"c, "sid", HelpText:="SID of the oracle database", Required:=True)>
        Public Property SID As String

        <[Option]("u"c, "user", HelpText:="oracle user", Required:=True)>
        Public property UserID As String

        <[Option]("c"c, "credentials", HelpText:="password to access the oracle database", Required:=true)>
        Public Property Password As String

        <[Option]("x"c, "nocache", DefaultValue:=False, HelpText:="skip the startup FK query", Required:=False)>
        Public Property nocache As Boolean

        <[Option]("w"c, "webport", DefaultValue:=9001, HelpText:="Local port number for the web app", Required:=False)>
        Public Property LocalPort As Integer

        <[Option]("l"c, "limit", DefaultValue:=5000, HelpText:="Limit oracle server results to this much rows, 0=unlimited", Required:=False)>
        Public Property LimitRows As Integer

        <HelpOption>
        Public Function GetUsage() As String
            Return CommandLine.Text.HelpText.AutoBuild(Me).ToString
        End Function
    End Class
End Namespace
