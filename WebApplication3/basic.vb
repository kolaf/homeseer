IMPORTs System.Web.UI.WebControls
Imports Scheduler
Imports System.Globalization
Imports System.XML
Imports System.XML.Linq

Public class basic
    Inherits System.Web.UI.Page

Public Dim hs As Scheduler.hsapplication
protected outdoor as System.Web.UI.webcontrols.imagebutton
protected save_time as System.Web.UI.HtmlControls.HtmlInputSubmit
protected temperatures_box as System.Web.UI.HtmlControls.HtmlInputHidden
protected return_time_box as System.Web.UI.HtmlControls.HtmlInputText

Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
	hs  = Context.Items("Content")
    Dim return_time as string
            if Trim (Request.QueryString("return_time")) = "" then
            return_time = ""
        else
            return_time = Request.QueryString("return_time")
        end if
        
    Dim clear_cold as string
            if Trim (Request.QueryString("clear_cold")) = "" then
            clear_cold = ""
        else
            clear_cold = Request.QueryString("clear_cold")
            reset_heater (clear_cold)
        End If

        Dim light_control As String
        If Trim(Request.QueryString("light_control")) = "" Then
            light_control = ""
        Else
            light_control = Request.QueryString("light_control")
            toggle_lights(light_control)
        End If

    If Not Page.IsPostBack Then
        Dim day as string
            Dim room as string
            
        if Trim (Request.QueryString("day")) = "" then
            Dim dateTimeFormats As DateTimeFormatInfo
            dateTimeFormats = New CultureInfo("en-US").DateTimeFormat
            Dim time As DateTime = DateTime.Now
            day = time.ToString("ddd", dateTimeFormats)
        else
            day = Request.QueryString("day")
        end if
        
        if Trim (Request.QueryString("room")) = "" then
            room = ""
        else
            room = Request.QueryString("room")
        end if
        
        ViewState("day") = day
        ViewState("room") = room
    end if
    If lcase(Request.ServerVariables("AUTH_USER")) = "guest" Then Response.Redirect("/unauthorized.asp")
    If lcase(hs.WebLoggedInUser) = "guest" Then Response.Redirect("/unauthorized.asp")

   
    If return_time.length >0 then
        record_returntime (return_time)
        
    else
        'return_time_box.value=return_time
    end if
End Sub

    Public Sub toggle_lights(ByVal light_control As String)
        If hs.DeviceValue(light_control) = 0 Then
            hs.SetDeviceValueByRef(light_control, 100, True)
            hs.WriteLog("Lights", "switching on")
        Else
            hs.SetDeviceValueByRef(light_control, 0, True)
            hs.WriteLog("Lights", "switching off")
        End If
        Response.Redirect("/lights.aspx")
    End Sub

    Public Sub record_returntime(ByVal return_hours As String)
        Dim time = datetime.now
        Dim return_time = time.addhours(return_hours)
        hs.SaveIniSetting("Global", "return_time", return_time.tostring(), "thermostat.ini")
        Response.Redirect("/lights.aspx")
    End Sub


    Public Sub reset_heater(ByVal room As String)
        hs.SaveINISetting(room, "failed", "0", "thermostat.ini")
        hs.WriteLog("website", "Resetting cooling for " & room)
        Dim message_base As String = hs.GetINISetting("Global", "aradmin", "", "thermostat.ini") & "&ttl=300&message=note=:="
        Dim webClient As New System.Net.WebClient
        Dim result As String = webClient.DownloadString(message_base & DateTime.Now.ToString() & ": Resetting cooling for " & room)
        Response.Redirect("/lights.aspx")
    End Sub

    Public Function return_time_input()
        Dim current_value As String
        Dim button_text As String = "Home in..."
        Dim Time = DateTime.Now
        Dim out = hs.GetINISetting("Global", "return_time", "0", "thermostat.ini")
        If out <> "0" Then
            Dim return_time = DateTime.Parse(out)
            If DateTime.Compare(Time, return_time) < 0 Then
                current_value = ((return_time - Time).TotalSeconds / 3600).ToString("F2")
            End If
        Else
            current_value = "56"
        End If
        Return current_value
    End Function

    Public Function get_header()
        Dim s As String = <![CDATA[

        <div class="navbar navbar-default navbar-static-top">
          <div class="container">
            <div class="navbar navbar-default">
            <div class="navbar-header">
              <button type="button" class="navbar-toggle" data-toggle="collapse" data-target=".navbar-collapse">
                <span class="icon-bar"></span>
                <span class="icon-bar"></span>
                <span class="icon-bar"></span>
              </button>
              <a class="navbar-brand active" href="lights.aspx">Fall&oslash;kken</a>
            </div>
            <div class="navbar-collapse collapse">
              <ul class="nav navbar-nav">
                <li class="dropdown">
                  <a href="#" class="dropdown-toggle" data-toggle="dropdown">Pages<b class="caret"></b></a>
                  <ul class="dropdown-menu">
                <li><a href="calendars.aspx">Calendars</a></li>
                <li><a href="deviceutility">Homeseer</a></li>
                </ul>
                </li>
                            <li class="dropdown">
                  <a href="#" class="dropdown-toggle" data-toggle="dropdown">Measurements<b class="caret"></b></a>
                  <ul class="dropdown-menu">
                    <li><a href="plots.aspx?sensors=temp">Temperature</a></li>
                    <li><a href="plots.aspx?sensors=humidity">Humidity</a></li>
                    <li><a href="plots.aspx?sensors=windspeed">Windspeed</a></li>
                    <li><a href="plots.aspx?sensors=winddir">Wind direction</a></li>
                    <li><a href="plots.aspx?sensors=heater">Heating</a></li>
                    <li><a href="plots.aspx?sensors=light">Lights</a></li>
                    <li><a href="plot_thermostat.aspx?room=Livingroom">Thermostat livingroom</a></li>
                    <li><a href="plot_thermostat.aspx?room=BedroomN">Thermostat bedroomN</a></li>
                    <li><a href="plot_thermostat.aspx?room=BedroomSE">Thermostat bedroomSE</a></li>
                  </ul>
                </li>

            </ul>
            <ul class="nav navbar-nav navbar-right">
            <form class="navbar-form navbar-right" action = "lights.aspx" method = "get" role="away">
               <div class="form-group">
              

                
                <a href="#">
                 ]]>.Value

        s = s & DateTime.Now.ToString("H:mm:ss dd-MM-yyyy") & <![CDATA[</a><br><a href="current_weather.aspx">
           ]]>.Value
        s = s & get_weather() & <![CDATA[
            
                
              </a><br></div>
          <div class="form-group">
              <input type='submit' class="btn btn-success" id='save_time' value='Home in...' role = "away"/> 
            </div>
            <div class="form-group">
              <input type='text' id='return_time' name ='return_time' size='4' value =']]>.Value



        return_time_input()
        s = s & return_time_input() & <![CDATA['/> hours</div>
                  </ul>
                 </form>
                </div><!--/.nav-collapse -->
              </div>
              </div>
            </div>
        ]]>.Value
        Return s
    End Function


    Public Function get_direction() As String
        Dim text As String = hs.DeviceString("3898")
        If text.Length > 15 Then
            Return text
        Else
            Return text
        End If
    End Function

    Public Function get_weather() As String
        Return CDbl(hs.DeviceValueEx("7975")).ToString() & "&deg;C / " & CDbl(hs.DeviceValueEx("9540")).ToString() & "%Rh / " & CDbl(hs.DeviceValueEx("1227")).ToString() & "m/s" ' / " & get_direction()
    End Function

    Protected Function double_from_string(ByVal de As String) As Double
        Dim culture As CultureInfo
        culture = CultureInfo.CreateSpecificCulture("en-US")
        Dim style As NumberStyles
        style = NumberStyles.AllowDecimalPoint
        Dim d As Double
        Double.TryParse(de, style, culture, d)
        Return d
    End Function

    Protected Function convert_double(ByVal d As Double) As String
        Dim nfi As System.Globalization.NumberFormatInfo = New System.Globalization.CultureInfo("en-US", False).NumberFormat
        Return Convert.ToDouble(d, nfi).ToString(nfi)
    End Function

End Class
