Imports System.Globalization
Imports System.Threading
Imports System.IO
Imports System.ComponentModel
Imports MySql.Data.MySqlClient

Imports System.Diagnostics

public function check_error(byval room as string) as integer
    
    Dim thermometer = hs.GetIniSetting(room,"Thermometer","0","thermostat.ini")
    Dim heater as String = get_heater (room)
    dim returnvalue = 0
    Dim strSQL As New System.Text.StringBuilder
    strSQL.Append("SELECT f.number, f.lastchange, f.devicename from readings f inner join (select deviceID, max(lastchange) as lc from readings where deviceID='" & heater & "' and number=100 group by deviceID) g on (f.lastchange >= DATE_ADD(g.lc, INTERVAL 15 MINUTE)) where f.deviceID='" & thermometer & "' ORDER BY lastchange DESC")
    Dim dbconn As MySqlConnection = New MySqlConnection("Database=homeseer;Data Source=localhost;User Id=homeseer;Password=homeseer")
    try 
        dbconn.Open()
        Dim sqlCmd As MySqlCommand = New MySqlCommand(strSQL.ToString)    
        sqlCmd.Connection=dbconn
        Dim reader As MySqlDataReader
        reader=sqlCmd.ExecuteReader()
        dim index = 0
        dim last_value = 1000
        while reader.read()
            if reader("number") >= last_value then
                hs.WriteLog("coldcheck", "temp decrease")
                index = index +1
            else
                hs.WriteLog("coldcheck", "temp reset")
                index = 0
            end if
            last_value = reader ("number")
            if index >2 then
                hs.WriteLog("coldcheck", "threshold reached")
                returnvalue= 1
            end if
        end while
    
    Catch ex As Exception 
        hs.WriteLog("temperature", ex.Message) 
    Finally 
        dbconn.close ()
        dbconn = Nothing 
    End Try 
    return  returnvalue
end function

Public Function ann(ByVal room As string, byval heater_status as integer) 
    Dim compiler As New Process()
    dim lookahead = 4
    Dim time As DateTime = DateTime.Now
    dim remaining =12 - 12*Cint(time.tostring("mm"))/60
    REM hs.WriteLog("ann","remaining " & remaining)
    dim target_string as string = ""
    dim interval = double_from_string(hs.GetIniSetting(a(i), "interval","1","thermostat.ini"))
    dim i as integer
    for i = 0 to lookahead*12
        dim target =get_temperature_target(room, time.addminutes (i*5))
            target_string = target_string & " " & convert_double(target)
    next i
    REM hs.WriteLog("ann",target_string)
    Dim decision as double
    dim status as integer = 0
    If heater_status >0 then
        status = 1
    end if
    
    Dim nfi As System.Globalization.NumberFormatInfo = New System.Globalization.CultureInfo("en-US", False).NumberFormat
    Try
        compiler.StartInfo.FileName = "c:\Python26\python.exe"
        compiler.StartInfo.Arguments = "ann_thermostat\ann_thermostat_rate.py " & room & " " & interval & " " & status & " " &  convert_double (get_temperature (room)) & " "  & target_string
        compiler.StartInfo.UseShellExecute = False
        compiler.StartInfo.RedirectStandardOutput = True
        compiler.StartInfo.RedirectStandardError = True
        compiler.StartInfo.CreateNoWindow = True 
        'AddHandler compiler.OutputDataReceived, AddressOf received
        compiler.Start()
        'compiler.BeginOutputReadLine()
        Dim output =compiler.StandardOutput.ReadToEnd()
        REM hs.WriteLog("ann",output)
        hs.WriteLog("ann stderr",compiler.StandardError.ReadToEnd())
        decision =Convert.ToDouble(output,nfi)
        
        
        
        compiler.WaitForExit()
    Catch e As Exception
        hs.WriteLog("ann",(e.Message))
    End Try 
    return decision
end Function


Public Sub Main(ByVal Parms As Object) 
    Dim rooms As String = hs.GetIniSetting("Global","rooms","0","thermostat.ini")
    Dim a() As String
    a= Split (rooms, ",")
    Dim time As DateTime = DateTime.Now
    Dim i As Integer
    Dim message_base as String = hs.GetIniSetting("Global","aradmin", "", "thermostat.ini") & "&ttl=300&message=note=:="
    Dim webClient As New System.Net.WebClient

    For i = 0 To UBound(a) 
        Dim current_target as Double = get_temperature_target(a(i), time)
        Dim temperature as Double = get_temperature (a(i))
        Dim heater as String = get_heater (a(i))
        Dim interval as Double = get_interval(a(i))
        Dim pattern As String = "dd.MM.yyyy HH:mm:ss"
        Dim last_change As DateTime=DateTime.ParseExact(hs.DeviceLastChangeRef(heater), pattern, CultureInfo.CurrentCulture)
        dim holdoff=Cint(hs.GetIniSetting(a(i), "holdoff","60","thermostat.ini"))*60
        dim since_last = (time- last_change).totalseconds
        hs.WriteLog("Thermostat", "Time since last change is " & since_last)
        hs.WriteLog("Thermostat", "Current temperature in room " & a(i) & " is " & temperature & " with target " & current_target & " and interval " & interval)
        Dim decision =ann(a(i), hs.DeviceValue(heater))
        hs.WriteLog("Thermostat", "ANN decision is "  & decision)
        if hs.DeviceValue(heater)= 100 then
            if check_error (a(i)) = 1 then
                hs.WriteLog("thermostat","Detected cooling for " & a(i))
                Dim result As String = webClient.DownloadString(message_base & time.ToString() & ": Detected cooling for " & a(i))
                hs.saveIniSetting(a(i),"failed",datetime.now().ToString(),"thermostat.ini")
                hs.sendemail ("frankose@ifi.uio.no", "frankose@ifi.uio.no", "", "", "cooling detected", time.ToString() & ": Detected cooling for " & a(i), "")
                hs.SetDeviceValueByRef(heater, 0, True)
            end if
        end if
        if decision >= 0.5 and hs.DeviceValue(heater)= 0 then
            if since_last > holdoff and hs.GetIniSetting(a(i),"failed", "0", "thermostat.ini") = "0" then
                hs.SetDeviceValueByRef(heater, 100, True)
                hs.WriteLog("Thermostat", "Switching on heater in room " & a(i))
                If temperature < (current_target - interval) then
                
                    hs.WriteLog("Thermostat", "Regular aagrees")
                else
                    hs.WriteLog("Thermostat", "Regular disagrees")
                end if
                Dim result As String = webClient.DownloadString(message_base & time.ToString() & ": Switching on heater in " & a(i) & "(" & temperature & " with target " & current_target & ")")
            else
                hs.WriteLog("Thermostat", "Wants to turn on, but only " & since_last & " seconds since it was switched off, and failed is " & hs.GetIniSetting(a(i),"failed", "0", "thermostat.ini"))
            end if
        elseif decision < 0.5 and hs.DeviceValue(heater)= 100 then
            if since_last > holdoff then
        
                hs.SetDeviceValueByRef(heater, 0, True)
                hs.WriteLog("Thermostat", "Switching off heater in room " & a(i))
                If temperature > (current_target - interval) then
                
                    hs.WriteLog("Thermostat", "Regular aagrees")
                else
                    hs.WriteLog("Thermostat", "Regular disagrees")
                end if
                Dim result As String = webClient.DownloadString(message_base & time.ToString() & ": Switching off heater in " & a(i) & "(" & temperature & " with target " & current_target & ")")
            else
                hs.WriteLog("Thermostat", "Wants to turn off, but only " & since_last & " seconds since it was switched on")
            end if
        REM elseif decision >= 0.5 and hs.DeviceValue(heater) = 100 and temperature > (current_target + interval+1) then
            REM hs.SetDeviceValueByRef(heater, 0, True)
            REM hs.WriteLog("Thermostat", "Switching off (override) heater in room " & a(i))
            REM Dim result As String = webClient.DownloadString(message_base & time.ToString() & ": Switching off (override) heater in " & a(i) & "(" & temperature & " with target " & current_target & ")")
        end if
        REM If temperature < (current_target - interval) then
            REM If hs.DeviceValue(heater)= 0 then
                REM hs.SetDeviceValueByRef(heater, 100, True)
                REM hs.WriteLog("Thermostat", "Switching on heater in room " & a(i))
                REM if  decision >0.5 then
                    REM hs.WriteLog("Thermostat", "ANN aagrees")
                REM else
                    REM hs.WriteLog("Thermostat", "ANN disagrees")
                 REM end if
                REM Dim result As String = webClient.DownloadString(message_base & time.ToString() & ": Switching on heater in " & a(i))
            REM end if
        REM elseif temperature > (current_target + interval) then
            REM If hs.DeviceValue(heater)= 100 then
                REM hs.SetDeviceValueByRef(heater, 0, True)
                REM hs.WriteLog("Thermostat", "Switching off heater in room " & a(i))
                REM if  decision <0.5 then
                    REM hs.WriteLog("Thermostat", "ANN aagrees")
                REM else
                    REM hs.WriteLog("Thermostat", "ANN disagrees")
                 REM end if
                REM Dim result As String = webClient.DownloadString(message_base & time.ToString() & ": Switching off heater in " & a(i))
            REM end if
        REM End If
    Next i
end sub

Function get_temperature(Byval room as String) as Double
    Dim thermometer as String =hs.GetIniSetting(room,"Thermometer","0","thermostat.ini")
    if thermometer = "0" then
        hs.WriteLog("Thermostat", "Unknown thermometer for room " & room)
        return 100.0
    End If
    return CDbl(hs.DeviceValueEx(thermometer))
End Function

Function get_heater(Byval room as String) as String
    Dim heater as String =hs.GetIniSetting(room,"Heater","0","thermostat.ini")
    if heater = "0" then
        hs.WriteLog("Thermostat", "Unknown heater for room " & room)
        return ""
    End If
    return heater
End Function

Function get_interval(Byval room as String) as Double
    Dim culture As CultureInfo
    culture = CultureInfo.CreateSpecificCulture("en-US")
    Dim style As NumberStyles
    style = NumberStyles.AllowDecimalPoint
    Dim interval as String =hs.GetIniSetting(room,"Interval","0.0","thermostat.ini")
    Dim d as Double
    if interval = "0.0" or (Double.TryParse(interval, style, culture,d) = False) Then
        hs.WriteLog("Thermostat", "Unknown interval for room " & room)
        return 0.0
    End If
    
    return d
End Function

Function get_temperature_target(Byval room as String, ByVal time as DateTime) as Double
    Dim current_target_integer as Double
    Dim dateTimeFormats As DateTimeFormatInfo
    dim out = hs.GetIniSetting("Global", "return_time","0","thermostat.ini")
    if out <> "0" then
        dim return_time = datetime.parse(out)
        if DateTime.Compare(time, return_time) <0 then
            ' time is before return_time
            'hs.WriteLog("Thermostat", "Returning away temp")
            return double_from_string (hs.GetIniSetting("Global", "away_temperature","10.0","thermostat.ini"))
        end if
    end if
    Dim hour= time.tostring("HH")
    dateTimeFormats = New CultureInfo("en-US").DateTimeFormat
    Dim culture As CultureInfo
    culture = CultureInfo.CreateSpecificCulture("en-US")
    Dim style As NumberStyles
    style = NumberStyles.AllowDecimalPoint
    Dim current_target As String = hs.GetIniSetting(room,"temperature_" & time.ToString("ddd", dateTimeFormats) & "_" & hour,"0","thermostat.ini")
    If current_target = "0" or (Double.TryParse(current_target, style, culture,current_target_integer) = False) Then
        hs.SaveIniSetting(room,"temperature_" & time.ToString("ddd", dateTimeFormats) & "_" & hour,"15","thermostat.ini")
        current_target_integer = 15.0
    End If
    return current_target_integer
end Function

Function double_from_string (Byval de as String) as Double
    Dim culture As CultureInfo
    culture = CultureInfo.CreateSpecificCulture("en-US")
    Dim style As NumberStyles
    style = NumberStyles.AllowDecimalPoint
    Dim d as Double
    Double.TryParse(de, style, culture,d)
    return d
end function   

Function convert_double (Byval d as Double) as String
    Dim nfi As System.Globalization.NumberFormatInfo = New System.Globalization.CultureInfo("en-US", False).NumberFormat
    return Convert.ToDouble(d, nfi).tostring(nfi)
end function