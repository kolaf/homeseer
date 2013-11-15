Imports MySql.Data.MySqlClient 
Imports System.Globalization

Public Const SENSOR_LIST As String = "7975/temp, 6214/windspeed, 1227/windspeed, 9540/humidity, 3898/winddir, 262/temp, 9533/temp, 9882/temp, 4126/humidity, 5101/humidity, 6621/humidity, 7772/temp, 6902/humidity, 7679/heater, 8967/light, 8379/heater, 2356/heater" 
Public Const CONN As String = "Database=homeseer;Data Source=localhost;User Id=homeseer;Password=homeseer" 

Public Sub Main(ByVal Parms As Object) 
    Dim s As New MySqlConnection 
    Dim c As MySqlCommand 
    Dim sensors As String = SENSOR_LIST 
    Dim ary() As String 
    Dim i As Integer 
    Dim nfi As System.Globalization.NumberFormatInfo = New System.Globalization.CultureInfo("en-US", False).NumberFormat
    ary = Split(sensors, ",") 
    s.ConnectionString = CONN 
    Try 
        s.Open() 
        For i = 0 To UBound(ary) 
            Dim words as string() =ary(i).split("/")
            Dim ID as String = words(0)
            Dim sensor as String = words(1)

                dim room = get_device_room (ID)
                REM hs.WriteLog("temperature", "found room: " & room)
                
                dim target = get_temperature_target (room, datetime.now())
                REM hs.WriteLog("temperature", "Logging target " & target)
                ''ugh - Z-Wave temperature is xxxx whereas RFXCOM temperatures are xxx 
                Dim pattern As String = "dd.MM.yyyy HH:mm:ss"
                Dim parsedDate As DateTime=DateTime.ParseExact(hs.DeviceLastChangeRef(ID), pattern, CultureInfo.CurrentCulture)
                c = New MySqlCommand("select count(*) from readings where deviceID='" & ID & "' and lastchange='" & parsedDate.toString("yyyy-MM-dd HH:mm:ss", CultureInfo.CurrentCulture) & "'") 
                   
                c.Connection=s
                Dim results as Integer
                results = c.ExecuteScalar()
                'hs.WriteLog("temperature", "Number of results found for " & hs.DeviceName(ID) & ": " & results) 
                if results = 0 then
                    hs.WriteLog("temperature", "Recording " & hs.DeviceName(ID) & " results")
                    c = New MySqlCommand("INSERT INTO readings (deviceID, devicename, number, type, lastchange, target) VALUES('" & ID & "', '" & hs.DeviceName(ID) & "'," & Convert.ToDouble((hs.DeviceValueEx(ID)), nfi).tostring(nfi) & ",'" & sensor & "','"& parsedDate.toString("yyyy-MM-dd HH:mm:ss", CultureInfo.CurrentCulture) & "','" & target & "')" )                    
                    c.Connection=s
                    c.ExecuteNonQuery() 
                end if
        Next i 


    Catch ex As Exception 
        hs.WriteLog("temperature", ex.Message) 
    Finally 
        s.Close() 
        c = Nothing 
        s = Nothing 
    End Try 
End Sub  

function  get_device_room(byval  sensor as string)
    dim  rooms =hs.GetIniSetting("Global","rooms","0.0","thermostat.ini")
    dim  thermostats =hs.GetIniSetting("Global","thermometers","0.0","thermostat.ini")
    REM hs.writeLog("plot_thermostat", "rooms " & rooms)
    REM hs.writeLog("plot_thermostat", "device " & sensor)
    REM hs.writeLog("plot_thermostat", "devices " & thermostats)
    dim a() = split (rooms, ",")
    dim b() = split (thermostats, ",")
    dim i as integer
    for i = 0 to UBound(b) 
        if Cint(b(i)) =  Cint(sensor) then
            return a(i)
        end if
    next i
    return ""
end function


Function get_temperature_target(Byval room as String, ByVal time as DateTime) as string
    Dim current_target_integer as string
    Dim dateTimeFormats As DateTimeFormatInfo
    dim out = hs.GetIniSetting("Global", "return_time","0","thermostat.ini")
    if out <> "0" then
        dim return_time = datetime.parse(out)
        if DateTime.Compare(time, return_time) <0 then
            ' time is before return_time
            'hs.WriteLog("Thermostat", "Returning away temp")
            return hs.GetIniSetting("Global", "away_temperature","10.0","thermostat.ini")
        end if
    end if
    Dim hour= time.tostring("HH")
    dateTimeFormats = New CultureInfo("en-US").DateTimeFormat
    Dim culture As CultureInfo
    culture = CultureInfo.CreateSpecificCulture("en-US")
    Dim style As NumberStyles
    style = NumberStyles.AllowDecimalPoint
    Dim current_target As String = hs.GetIniSetting(room,"temperature_" & time.ToString("ddd", dateTimeFormats) & "_" & hour,"0","thermostat.ini")
    return current_target
end Function

protected Function double_from_string (Byval de as String) as Double
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
