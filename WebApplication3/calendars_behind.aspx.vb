IMPORTs System.Web.UI.WebControls
Imports Scheduler
Imports System.Globalization
Imports MySql.Data.MySqlClient

Public class calendars_behind
    Inherits basic



sub scatter_plot ()
    Dim sensors as string
    if Trim(Request.QueryString("sensors"))="" then
        sensors="temp"
    else
        sensors=Request.QueryString("sensors")
    end if
	Dim strSQL As New System.Text.StringBuilder
    Dim names as HashTable= new hashTable
	Dim hs As Scheduler.hsapplication = Context.Items("Content")
    Dim nfi As System.Globalization.NumberFormatInfo = New System.Globalization.CultureInfo("en-US", False).NumberFormat
	strSQL.Append("SELECT lastchange as last, number, devicename FROM readings WHERE  type = '" & sensors & "' and lastchange >= DATE_SUB(NOW(),INTERVAL 4 DAY) ORDER BY lastchange ASC")
    Dim distinct as string = "select distinct(devicename)  FROM readings WHERE  type = '" & sensors & "' and lastchange >= DATE_SUB(NOW(),INTERVAL 10 DAY) ORDER BY lastchange ASC"
    Dim district_command as MySqlCommand = new MySqlCommand (distinct)
        Dim dbconn As MySqlConnection = New MySqlConnection("Database=homeseer;Data Source=localhost;User Id=homeseer;Password=homeseer")
	dbconn.Open()
	Dim sqlCmd As MySqlCommand = New MySqlCommand(strSQL.ToString)
    district_command.Connection =dbconn
	sqlCmd.Connection=dbconn
    
	' Read the data into the DBTable object
    Dim reader As MySqlDataReader
    reader = district_command.ExecuteReader()
    Dim  index as integer
    index = 1
    while reader.read()
        names.add(reader("devicename"), "[")
        index = index +1
    end while
    
     reader=sqlCmd.ExecuteReader()
    While reader.Read()
       names.add (reader("devicename"),names(reader("devicename")) & "[" & reader("last") & "," & reader("number") & "],")
    End While
        
end sub    


Sub set_query_string()
    Dim sensors as string
    if Trim(Request.QueryString("sensors"))="" then
        sensors="temp"
    else
        sensors=Request.QueryString("sensors")
    end if
	Dim strSQL As New System.Text.StringBuilder
    Dim names as HashTable= new hashTable
	Dim hs As Scheduler.hsapplication = Context.Items("Content")
    Dim nfi As System.Globalization.NumberFormatInfo = New System.Globalization.CultureInfo("en-US", False).NumberFormat
	strSQL.Append("SELECT unix_timestamp (lastchange) as last, number, devicename FROM readings WHERE  type = '" & sensors & "' and lastchange >= DATE_SUB(NOW(),INTERVAL 4 DAY) ORDER BY lastchange ASC")
    Dim distinct as string = "select distinct(devicename)  FROM readings WHERE  type = '" & sensors & "' and lastchange >= DATE_SUB(NOW(),INTERVAL 4 DAY) ORDER BY lastchange ASC"
    Dim district_command as MySqlCommand = new MySqlCommand (distinct)
        Dim dbconn As MySqlConnection = New MySqlConnection("Database=homeseer;Data Source=localhost;User Id=homeseer;Password=homeseer")
	dbconn.Open()
	Dim sqlCmd As MySqlCommand = New MySqlCommand(strSQL.ToString)
    district_command.Connection =dbconn
	sqlCmd.Connection=dbconn

	' Read the data into the DBTable object
    Dim reader As MySqlDataReader
    reader = district_command.ExecuteReader()
    Dim  index as integer
    index = 1
    while reader.read()
        names.add(reader("devicename"), index)
        index = index +1
        response.write("data.addColumn('number','" &reader ("devicename" ) & "');")
    end while
    reader.close ()
    reader=sqlCmd.ExecuteReader()
    While reader.Read()
       response.write ("var points = new Array(); points [0] = new Date (" &reader ("last")& "000); for (i=1;i<" & index & ";i++) { points [i] =null;}")
       response.write ("points [" & names(reader("devicename")) & "]="& Convert.ToDouble((reader ("number")), nfi).tostring(nfi) & ";")
       
       response.write ("data.addRow (points);")
    End While
	
	dbconn.Close()
End Sub

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

Function canvas () As String
    Dim room as string=ViewState ("room")
    If room.length >0 then
        return ""
    else
        return "class ='collapse'"
    end if
end function

Function convert_double (Byval d as Double) as String
    Dim nfi As System.Globalization.NumberFormatInfo = New System.Globalization.CultureInfo("en-US", False).NumberFormat
    return Convert.ToDouble(d, nfi).tostring(nfi)
end function






end class