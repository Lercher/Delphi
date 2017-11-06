Imports Owin
Imports System.Web.Http
Imports Microsoft.Owin
Imports Microsoft.Owin.Cors
Imports Microsoft.Owin.StaticFiles
Imports Microsoft.Owin.FileSystems

Public Class Startup
    Public Sub Configuration(app As IAppBuilder)
        Dim config = New HttpConfiguration
        config.Routes.MapHttpRoute("API", "api/{controller}/{id}/{id2}", New With {.id = RouteParameter.Optional, .id2 = RouteParameter.Optional})
        app.UseWebApi(config)
        app.UseCors(CorsOptions.AllowAll)

        ' http://odetocode.com/blogs/scott/archive/2014/02/10/building-a-simple-file-server-with-owin-and-katana.aspx
        ' http://docs.asp.net/en/latest/fundamentals/static-files.html
        Dim webroot = IO.Path.Combine(IO.Path.GetDirectoryName(Reflection.Assembly.GetExecutingAssembly.Location), "wwwroot")
#If DEBUG Then
        webroot = "\daten\Lercher.Delphi\Lercher.Delphi.Common\wwwroot"
#End If

        Console.WriteLine("Serving static files from: {0}", webroot)
        AppBuilderUseExtensions.Use(app,
            Async Function(context As IOwinContext, nxt As Func(Of Task)) As Task
                Await nxt()
                context.Response.Headers.Add("Cache-Control", {"max-age=0"}) ' For Chrome to honor ETags
            End Function
        )
        app.UseFileServer(New FileServerOptions() With {.FileSystem = New PhysicalFileSystem(webroot)})
    End Sub

End Class
