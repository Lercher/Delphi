# Delphi

An oracle for data given in my employer's typical layout.


## Build

* Clone the repository
* Get and install the oracle client for .net and Visual Studio (this will  be quite cumbersome).
  I have "Oracle Developer Tools for Visual Studio 2015" Version 12.1.2400 on my machine.
* Open the solution file in Visual Studio 2015
* Execute "Restore Nuget Packages" on the Solution
* Set Lercher.Delphi.ServiceConsole as the startup project
* Configure the appropriate command line options in "My Project/Debug/Command line arguments" (see below)
* Check Tools/Options/Typescript/Project/General - Automatically compile typescript files 
  which are not part of a project, and look for "Output(s) generated successfully." in the 
  status bar after saving *.ts files
* Hit F5 and pray, it should say something like:

    This is Delphi, an Oracle for Data. 2016 M. Lercher
    Listening on http://+:9001/delphi/...
    
    b - open browser. Press Enter to stop ...

* Press b and Enter, a browser window should open now


## Run

Runs as a console application (Lercher.Delphi.ServiceConsole.exe) and then serves on http://+:9001/Delphi
You have to provide several commandline options to specify the connection to an oracle database at startup.

    -p, --port           (Default: 1521) Port number of the oracle connection
    -h, --host           Required. Server name of the oracle database
    -s, --sid            Required. SID of the oracle database
    -u, --user           Required. oracle user
    -c, --credentials    Required. password to access the oracle database

### Example

To connect to your favourite locally installed xe oracle edition user SYSTEM, start:

    Lercher.Delphi.ServiceConsole.exe -p 1521 -h localhost -s xe -u SYSTEM -c you-know-your-password
