Public Sub Main(ByVal Parms As Object) 
Dim message_base as String = hs.GetIniSetting("Global","aradmin", "", "thermostat.ini") & "&ttl=300&message=si=:="
hs.WriteLog("Speaking", message_base & get_weather())
Dim webClient As New System.Net.WebClient
Dim result As String = webClient.DownloadString(message_base & get_weather())
end sub

function get_direction () as string
    dim text as string = hs.DeviceString("3898")
    if text.length >5 then
        return text
    else
        return text
    end if
end function

function get_weather() as string
Dim nfi As System.Globalization.NumberFormatInfo = New System.Globalization.CultureInfo("en-US", False).NumberFormat
    dim temperature as double = Convert.ToDouble((hs.DeviceValueEx("7975")), nfi)
    dim humidity = Convert.ToDouble((hs.DeviceValueEx("9540")), nfi).tostring(nfi)
    dim speed = Convert.ToDouble((hs.DeviceValueEx("1227")), nfi).tostring(nfi)
    dim prefix as string
    if temperature <0 then
        prefix = "minus"
        temperature = temperature*-1
    End if
    return "Det er  " & prefix & " " & temperature.tostring(nfi) & " grader ute med " & humidity & " prosent luftfuktighet. Vindhastigheten er " & speed & " meter i sekundet fra " & hs.DeviceValue("3898") & " grader"
    REM return "The temperature is " & prefix & " " & temperature.tostring(nfi) & " degrees Celsius with a humidity of " & humidity & " percent. The wind speed is " & speed & " metres per second and it is blowing from " & hs.DeviceValue("3898") & " degrees"
end function    