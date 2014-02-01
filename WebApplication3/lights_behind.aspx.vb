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

    Function get_all_lights() As String

        Dim enumerator
        Dim device
        Dim output As String
        Dim lights As Hashtable
        lights = New Hashtable
        enumerator = hs.GetDeviceEnumerator
        If Not enumerator Is Nothing Then
        Else
            hs.WriteLog("Numerator", "invalid object")
        End If

        While Not enumerator.Finished()
            'If enumerator.CountChanged Then
            '    hs.WriteLog("Numerator", "The device count has changed")
            'End If
            device = enumerator.GetNext()
            If Not device Is Nothing And device.Device_Type_String(hs) = "AC_MODULE" Then
                Dim list_for_room As ArrayList
                Try
                    list_for_room = lights(device.Location(hs))
                Catch ex As NullReferenceException
                    list_for_room = New ArrayList

                End Try
                If list_for_room Is Nothing Then
                    list_for_room = New ArrayList
                End If
                list_for_room.Add(device)
                Dim location = device.Location(hs)
                Try
                    lights.Add(device.Location(hs), list_for_room)
                Catch exception As ArgumentException
                End Try
            End If
        End While
        enumerator = lights.GetEnumerator()
        While enumerator.MoveNext()
            output = output & "<div class='col-xs-12 col-sm-4 col-md-3'>" & enumerator.Key & "<p align ='center'>"
            For i As Integer = 0 To enumerator.value.Count - 1
                Dim light_switch = enumerator.value(i)
                'output = output & light_switch.Ref(hs) & " _ " & light_switch.Name(hs) & ": " & light_switch.Device_Type_String(hs) & "<br>"
                output = output & "<a href ='lights.aspx?light_control=" & light_switch.Ref(hs) & "'><img src='" & hs.DeviceVGP_GetGraphic(light_switch.Ref(hs), hs.DeviceValue(light_switch.Ref(hs))) & "'><br>" & light_switch.Name(hs) & "</a><br>"

            Next i
            output = output & "</p></div>"
        End While
        Return output
    End Function


    Function get_temperature(ByVal room As String) As Double
        Dim thermometer As String = hs.GetINISetting(room, "Thermometer", "0", "thermostat.ini")
        If thermometer = "0" Then
            hs.WriteLog("Thermostat", "Unknown thermometer for room " & room)
            Return 100.0
        End If
        Return CDbl(hs.DeviceValueEx(thermometer))
    End Function

    Function get_temperature_target(ByVal room As String, ByVal hour As String, ByVal time As DateTime) As Double
        Dim current_target_integer As Double
        Dim dateTimeFormats As DateTimeFormatInfo
        Dim nfi As System.Globalization.NumberFormatInfo = New System.Globalization.CultureInfo("en-US", False).NumberFormat
        dateTimeFormats = New CultureInfo("en-US").DateTimeFormat
        Dim culture As CultureInfo
        culture = CultureInfo.CreateSpecificCulture("en-US")
        Dim style As NumberStyles
        style = NumberStyles.AllowDecimalPoint
        If hour = -1 Then
            Dim format As String = "HH"
            hour = time.ToString(format)
        End If
        Dim current_target As Double = Convert.ToDouble(hs.GetIniSetting(room, "temperature_" & time.ToString("ddd", dateTimeFormats) & "_" & hour, "0.0", "thermostat.ini"), nfi)
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
            Dim heat As String = hs.GetINISetting(room, "Heater", "0", "thermostat.ini")
            Dim heat_graphics As String = hs.DeviceVGP_GetGraphic(heat, hs.DeviceValue(heat))
            If hs.GetINISetting(room, "failed", "0", "thermostat.ini") <> "0" Then
                heat_graphics = "images/RFXCOM/ico_warn.gif"
            End If
            output = output & "<div class='col-xs-12 col-sm-4 col-md-4'><p align ='center'><a href='lights.aspx?room=" & room & "'>" & hs.GetINISetting(room, "Name", "unknown", "thermostat.ini") & "</a><a href ='lights.aspx?clear_cold=" & room & "'><img src= " & heat_graphics & "></a><br>"
            output = output & "Temperature: " & get_temperature(room) & " -> Target: " & get_temperature_target(room, -1, DateTime.Now) & "<br>"
            For Each day In days
                output = output & "<a href='lights.aspx?room=" & room & "&day=" & day & "'>" & day & "</a> "
            Next day

            output = output & "</p></div>"
            If boxes Mod 3 = 2 Then
                output = output & "</div>"
            End If

            boxes = boxes + 1
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