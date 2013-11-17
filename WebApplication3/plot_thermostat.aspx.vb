IMPORTs System.Web.UI.WebControls
Imports Scheduler
Imports System.Globalization
Imports MySql.Data.MySqlClient

Public class plot_thermostat
    Inherits basic


function  get_device_room(byval  device as string, byval  my_room as string)
    dim  rooms =hs.GetIniSetting("Global","rooms","0.0","thermostat.ini")
    dim  heaters =hs.GetIniSetting("Global",device,"0.0","thermostat.ini")
    REM hs.writeLog("plot_thermostat", "rooms " & rooms)
    REM hs.writeLog("plot_thermostat", "device " & device)
    REM hs.writeLog("plot_thermostat", "devices " & heaters)
    dim a() = split (rooms, ",")
    dim b() = split (heaters, ",")
    dim i as integer
    for i = 0 to UBound(a) 
        if a (i) =  my_room then
            return b(i)
        end if
    next i
    return ""
end function

    Public plot_interval As Integer

Function  get_room_data(byval column as string, byval device as string)  as string
        Dim room = ViewState("room")

        If Trim(Request.QueryString("plot_interval")) = "" Then
            plot_interval = 24
        Else
            plot_interval = CInt(Request.QueryString("plot_interval"))

        End If
    dim thermometer = get_device_room (device, room)
    REM hs.writeLog("plot_thermostat", thermometer)
    Dim strSQL As New System.Text.StringBuilder
        strSQL.Append("SELECT lastchange as last, " & column & ", devicename FROM readings WHERE  deviceID = '" & thermometer & "' and lastchange >= DATE_SUB(NOW(),INTERVAL " & plot_interval & " HOUR) ORDER BY lastchange ASC")
    REM hs.writeLog("plot_thermostat", strsql.tostring ())
        Dim dbconn As MySqlConnection = New MySqlConnection("Database=homeseer;Data Source=localhost;User Id=homeseer;Password=homeseer")
    dbconn.Open()
	Dim sqlCmd As MySqlCommand = New MySqlCommand(strSQL.ToString)    
	sqlCmd.Connection=dbconn
    Dim reader As MySqlDataReader
    dim output as string = "["
    dim index = 0
    reader=sqlCmd.ExecuteReader()
    dim last_value as string= "-1"
    while reader.read()
        if index >  0 then
            output = output & ", "
        end if
        if not IsDbNull(reader (column))  then
            index = index +1
            dim current_value =convert_double(reader(column))
            dim time =DateTime.parse(reader("last"))
                If device = "heaters" Then
                    If current_value = 100 Then
                        last_value = 0
                    Else
                        last_value = 100
                    End If
                    output = output & "['" & time.AddMinutes(-1).ToString("s") & "'," & last_value & "],"
                End If
                output = output & "['" & time.ToString("s") & "'," & current_value & "]"
            End If

        End While
    output = output & "]"
    dbconn.close ()
    return output
end function
    
    
function get_weather() as string
    return CDbl(hs.DeviceValueEx("7975")).tostring() & "&deg;C / " & CDbl(hs.DeviceValueEx("9540")).tostring() & "%Rh / " & CDbl(hs.DeviceValueEx("1227")).tostring() & "m/s / " & get_direction()
end function    

function get_direction () as string
    dim text as string = hs.DeviceString("3898")
    if text.length >5 then
        return text
    else
        return text
    end if
end function



end class