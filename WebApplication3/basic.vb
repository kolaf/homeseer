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
        end if

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

    try
	outdoor.ImageURL =hs.DeviceVGP_GetGraphic(8967,hs.DeviceValue(8967))
    catch ex As Exception 
        'hs.writelog ("wwebsite", "null pointer")
    end try
    If return_time.length >0 then
        record_returntime (return_time)
        
    else
        'return_time_box.value=return_time
    end if
End Sub

public sub record_returntime (Byval return_hours as string)
        dim time = datetime.now
        dim return_time = time.addhours (return_hours)
        hs.SaveIniSetting("Global","return_time", return_time.tostring(), "thermostat.ini")
        Response.Redirect("/lights.aspx")
    end sub

    
public sub reset_heater (byval room as string)
    hs.saveIniSetting(room,"failed","0","thermostat.ini")
    hs.WriteLog("website","Resetting cooling for " & room)
    Dim message_base as String = hs.GetIniSetting("Global","aradmin", "", "thermostat.ini") & "&ttl=300&message=note=:="
    Dim webClient As New System.Net.WebClient
    Dim result As String = webClient.DownloadString(message_base & datetime.now.ToString() & ": Resetting cooling for " & room)
    Response.Redirect("/lights.aspx")
end sub
    
Public function return_time_input ()
    dim current_value as string
    dim button_text as string = "Home in..."
    dim Time =datetime.now
    dim out = hs.GetIniSetting("Global", "return_time","0","thermostat.ini")
    if out <> "0" then
        dim return_time = datetime.parse(out)
        if DateTime.Compare(time, return_time) < 0 then
            current_value = ((return_time - time).totalseconds/3600).tostring("F2")
        end if
    else
        current_value = "56"
    end if
    return current_value
end function

public function get_header ()
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
            
            <li><a href="calendars.aspx">Calendars</a></li>
            <li><a href="deviceutility">Homeseer</a></li>
            
                        <li class="dropdown">
              <a href="#" class="dropdown-toggle" data-toggle="dropdown">Measurements<b class="caret"></b></a>
              <ul class="dropdown-menu">
                <li><a href="plots.aspx?sensors=temp">Temperature</a></li>
                <li><a href="plots.aspx?sensors=humidity">Humidity</a></li>
                <li><a href="plots.aspx?sensors=windspeed">Windspeed</a></li>
                <li><a href="plots.aspx?sensors=winddir">Wind direction</a></li>
                <li><a href="plots.aspx?sensors=heater">Heating</a></li>
                <li><a href="plots.aspx?sensors=light">Lights</a></li>
                <li><a href="plot_thermostat.aspx?room=Livingroom">Thermostat living room</a></li>
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
             
         s=s & DateTime.now.tostring() & <![CDATA[</a><br><a href="current_weather.aspx">
           ]]>.Value 
           s=s & get_weather() & <![CDATA[
            
                
              </a><br></div>
          <div class="form-group">
              <input type='submit' class="btn btn-success" id='save_time' value='Home in...' role = "away"/> 
            </div>
            <div class="form-group">
              <input type='text' id='return_time' name ='return_time' size='4' value =']]>.Value 

                
                
                     return_time_input ()
s=s & return_time_input () & <![CDATA['/> hours</div>
          </ul>
         </form>
        </div><!--/.nav-collapse -->
      </div>
      </div>
    </div>
]]>.Value
return s
end function


public function get_direction () as string
    dim text as string = hs.DeviceString("3898")
    if text.length >10 then
        return text
    else
        return text
    end if
end function

public function get_weather() as string
    return CDbl(hs.DeviceValueEx("7975")).tostring() & "&deg;C / " & CDbl(hs.DeviceValueEx("9540")).tostring() & "%Rh / " & CDbl(hs.DeviceValueEx("1227")).tostring() & "m/s / " & get_direction()
end function    

protected Function double_from_string (Byval de as String) as Double
    Dim culture As CultureInfo
    culture = CultureInfo.CreateSpecificCulture("en-US")
    Dim style As NumberStyles
    style = NumberStyles.AllowDecimalPoint
    Dim d as Double
    Double.TryParse(de, style, culture,d)
    return d
end function   

protected Function convert_double (Byval d as Double) as String
    Dim nfi As System.Globalization.NumberFormatInfo = New System.Globalization.CultureInfo("en-US", False).NumberFormat
    return Convert.ToDouble(d, nfi).tostring(nfi)
end function

End class
