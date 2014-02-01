' This is the startup script
' It is run once when HomeSeer starts up
' 
' You may also have Startup.vb and it will be run instead of this script.
'
sub Main(parm as object)
    
	hs.WriteLog("Startup", "Scripting is OK and is now running Startup.vb")
	    
	' Speak - uncomment the next line if you want HomeSeer to speak
	'         at startup.
	hs.Speak("Welcome to Home-Seer", True)
	' speak the port the web server is running on
    Dim port As String = hs.GetINISetting("Settings", "gWebSvrPort", "")
    If port <> "80" Then
        hs.Speak("Web server port number is " & port)
    End If

    ' You may add your own commands to this script.
    ' See the scripting section of the HomeSeer help system for more information.
    ' You may access help by going to your HomeSeer website and clicking the HELP button,
    ' or by pointing your browser to the /help page of your HomeSeer system.
    hs.SaveIniSetting("Global", "reset", "1.0", "thermostat.ini")
End Sub
