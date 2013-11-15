Public Sub speak(ByVal text As string) 
Dim message_base as String = hs.GetIniSetting("Global","aradmin", "", "thermostat.ini") & "&ttl=300&message=say=:="
Dim webClient As New System.Net.WebClient
hs.WriteLog("Speaking", message_base & text)
Dim result As String = webClient.DownloadString(message_base & text)
end sub

Public Sub snakk(ByVal text As string) 
Dim message_base as String = hs.GetIniSetting("Global","aradmin", "", "thermostat.ini") & "&ttl=300&message=si=:="
Dim webClient As New System.Net.WebClient
Dim result As String = webClient.DownloadString(message_base & text)
end sub
