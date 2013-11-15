IMPORTs System.Web.UI.WebControls
Imports Scheduler
Imports System.Globalization

Public class lights_behind
    Inherits basic





Function get_heading() As String
    Dim binding as HashTable= new hashTable
    binding.add("Mon","Monday")
    binding.add("Tue","Tuesday")
    binding.add("Wed","Wednesday")
    binding.add("Thu","Thursday")
    binding.add("Fri","Friday")
    binding.add("Sat","Saturday")
    binding.add("Sun","Sunday")
    Dim day as string=binding(ViewState ("day"))
    
    Dim room as string=ViewState ("room")
    return hs.GetIniSetting(room, "Name", "unknown", "thermostat.ini") & " on a " & day 
end Function

    Function get_values() As String
        Dim keys as List (of Integer)
        Dim bottom as Integer = 15
        Dim format As String = "00"
        Dim horizontal as String
        Dim output as String= ""
        Dim nfi As System.Globalization.NumberFormatInfo = New System.Globalization.CultureInfo("en-US", False).NumberFormat
        Dim vertical as String
        Dim values as SortedList = new SortedList 
        Dim day as string=ViewState ("day")
        
        Dim room as string=ViewState ("room")
        Dim hs As Scheduler.hsapplication = Context.Items("Content")
        Dim dateTimeFormats As DateTimeFormatInfo
        dateTimeFormats = New CultureInfo("en-US").DateTimeFormat
        Dim time As DateTime = DateTime.Now
        Dim i = 0
        for i = 0 to 23
            Dim current_target As Double = Convert.ToDouble(hs.GetIniSetting(room,"temperature_" & day & "_" & i.tostring (format), bottom.tostring (nfi),"thermostat.ini"),nfi)
            values.add (i,(current_target- bottom).tostring(nfi))
        next i
        i = 0
        horizontal = "["
        vertical = "["
        For Each k In values.keys
            if i > 0
                horizontal = horizontal & "," & k
                vertical = vertical & "," & values (k)
            else
                horizontal = horizontal & k
                vertical = vertical & values (k)
            end if
            i = i +1
        Next k
        horizontal = horizontal & "]"
        vertical = vertical & "]"
        if room.length >0 then
        
        output = output & "bar = new RGraph.Bar('cvs'," & vertical & ").Set('chart.labels', " & horizontal & ")"
        output = output & ".Set('chart.adjustable', RGraph.isOld() ? false : true)"
        output = output & ".Set('chart.margin', 1)"
        output = output & ".Set('chart.tickmarks.inner', true)"
        output = output & ".Set('chart.label.inner', true)"
        output = output & ".Set('chart.ylabels.specific', ['25','24','23','22','21','20','19','18','17','16','15'])"
        output = output & ".Set('chart.ymax', '10')"
        output = output & ".Set('chart.background.grid.autofit', true)"
        output = output & ".Set('chart.background.grid.autofit.align', true)"
        output = output & ".Set('chart.gutter.left', 25)"
        output = output & ".Set('chart.title', '" & get_heading() & "');"
        output = output & "bar.Draw();"
        output = output & "function readvars(obj){"
        output = output & "document.forms['myform'].elements['temperatures_box'].value =(RGraph.Registry.Get('chart.adjusting.shape')[0].data);"
        output = output & "document.forms['myform'].elements['message_box'].value = 'Not saved';"
        output = output & "}"
        output = output & "RGraph.AddCustomEventListener(bar, 'onadjustend', readvars);"
        
        end if
        
        return output
    End Function
    
    
    Function get_temperature_target(Byval room as String, ByVal time as DateTime) as Double
    Dim current_target_integer as Double
    Dim dateTimeFormats As DateTimeFormatInfo
    dim out = hs.GetIniSetting("Global", "return_time","0","thermostat.ini")
    if out <> "0" then
        dim return_time = datetime.parse(out)
        if DateTime.Compare(time, return_time) <0 then
            ' time is before return_time
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
    
    
    sub save_clicked(ByVal sender As Object, e as EventArgs)
        Dim bottom as Integer = 15
        Dim format As String = "00"
        Dim nfi As System.Globalization.NumberFormatInfo = New System.Globalization.CultureInfo("en-US", False).NumberFormat
        Dim hs As Scheduler.hsapplication = Context.Items("Content")
        Dim day as string=ViewState ("day")
        
        Dim room as string=ViewState ("room")
        
        dim temperatures as String = temperatures_box.value
        if temperatures.length >0 then
            If lcase(Request.ServerVariables("AUTH_USER")) = "guest" Then Response.Redirect("/unauthorized.asp")
            If lcase(hs.WebLoggedInUser) = "guest" Then Response.Redirect("/unauthorized.asp")

            dim values () as String =   Split (temperatures, ",")
            Dim i as integer = 0
            for each v in values
                Dim temp as double = Convert.todouble(v,nfi) + bottom
                hs.SaveIniSetting(room,"temperature_" & day & "_" & i.tostring(format),temp.tostring(nfi),"thermostat.ini")            
                i=i +1
            next v
         end if
        ''message_box.text = "Saved"
        ViewState("room") = ""
     End sub

sub outdoor_clicked(ByVal sender as Object, e as EventArgs)
    hs  = Context.Items("Content")
    if hs.DeviceValue(8967) = 0 then
        hs.SetDeviceValueByRef(8967, 100, True)
    else
        hs.SetDeviceValueByRef(8967, 0, True)
    end if
    outdoor.ImageURL =hs.DeviceVGP_GetGraphic(8967,hs.DeviceValue(8967))
    
end sub

Function get_temperature(Byval room as String) as Double
    Dim thermometer as String =hs.GetIniSetting(room,"Thermometer","0","thermostat.ini")
    if thermometer = "0" then
        hs.WriteLog("Thermostat", "Unknown thermometer for room " & room)
        return 100.0
    End If
    return CDbl(hs.DeviceValueEx(thermometer))
End Function

Function get_temperature_target(Byval room as String, ByVal hour as String, ByVal time as DateTime) as Double
    Dim current_target_integer as Double
    Dim dateTimeFormats As DateTimeFormatInfo
    Dim nfi As System.Globalization.NumberFormatInfo = New System.Globalization.CultureInfo("en-US", False).NumberFormat
    dateTimeFormats = New CultureInfo("en-US").DateTimeFormat
    Dim culture As CultureInfo
    culture = CultureInfo.CreateSpecificCulture("en-US")
    Dim style As NumberStyles
    style = NumberStyles.AllowDecimalPoint
    if hour = -1 then
        Dim format As String = "HH"
        hour =time.ToString(format)
    End if
    Dim current_target As Double = Convert.ToDouble(hs.GetIniSetting(room,"temperature_" & time.ToString("ddd", dateTimeFormats) & "_" & hour,"0.0","thermostat.ini"),nfi)
    If current_target = 0.0 Then
        current_target_integer = 15.0
    End If
    return current_target
end Function

Function thermostat_links ()
    Dim hs As Scheduler.hsapplication
    Dim output as string= ""
    hs  = Context.Items("Content")
    Dim days() as String = {"Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun"}
    Dim rooms as String = hs.GetIniSetting("Global","rooms","0","thermostat.ini")
    if rooms = "0" then
        hs.SaveIniSetting("Global","rooms","BedroomSE","thermostat.ini")    
        rooms = "BedroomSE"        
    end if
    Dim room_list() as String = Split (rooms, ",")
    Dim room as String
    Dim day as  String
    Dim boxes as integer = 0
    for each room in room_list
        if boxes mod 3 = 0 then
            output = output & "<div class='row'>"
        end if
        Dim heat as String = hs.GetIniSetting(room,"Heater","0","thermostat.ini")
        output = output & "<div class='col-xs-12 col-sm-6 col-md-4'><p align ='center'><a href='lights.aspx?room=" & room & "'>" & hs.GetIniSetting(room, "Name", "unknown", "thermostat.ini") & "</a><a href ='lights.aspx?clear_cold=" & room & "'><img src= " & hs.DeviceVGP_GetGraphic(heat,hs.DeviceValue(heat)) & "></a><br>"
        output = output & "Temperature: " & get_temperature (room) & " -> Target: " & get_temperature_target(room, -1, Datetime.now) & "<br>"
        for each day in days
            output = output & "<a href='lights.aspx?room=" & room & "&day=" & day & "'>" & day & "</a> "
        next day
        
        output = output & "</p></div>"
        if boxes mod 3 = 2 then
            output = output & "</div>"
        end if
        
        boxes = boxes +1
    Next room
    if boxes mod 3 <> 2 then
            output = output & "</div>"
        end if
        
    Return output
end Function


Function canvas () As String
    Dim room as string=ViewState ("room")
    If room.length >0 then
        return ""
    else
        return "class ='collapse'"
    end if
end function

End class