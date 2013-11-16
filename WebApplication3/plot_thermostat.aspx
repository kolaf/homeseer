
<%@ Page Language="vb" AutoEventWireup="false"
Inherits="fallokken.plot_thermostat" debug="true" %>
<!--  Temperature_Plot.ASPX by Steve Anderson (aka Snevl) -->
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html>
  <head>
    <script src="RGraph/libraries/RGraph.common.core.js" ></script>
    <script src="RGraph/libraries/RGraph.scatter.js" ></script>
    <meta charset="utf-8">
    <meta http-equiv="X-UA-Compatible" content="IE=edge">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <meta name="description" content="">
    <meta name="author" content="">
    <meta http-equiv="refresh" content="300" /> 
    <link rel="shortcut icon" href="../../docs-assets/ico/favicon.png">

    <title>Fallokken control</title>
      <!-- Bootstrap core CSS -->
    <link href="dist/css/bootstrap.css" rel="stylesheet">

    <!-- Custom styles for this template -->
    <link href="css/navbar.css" rel="stylesheet">

    <!-- HTML5 Shim and Respond.js IE8 support of HTML5 elements and media queries -->
    <!-- WARNING: Respond.js doesn't work if you view the page via file:// -->
    <!--[if lt IE 9]>
      <script src="https://oss.maxcdn.com/libs/html5shiv/3.7.0/html5shiv.js"></script>
      <script src="https://oss.maxcdn.com/libs/respond.js/1.3.0/respond.min.js"></script>
    <![endif]-->
  </head>
  <body>

<%
Dim sensors as string  =Request.QueryString("sensors")
Dim hs As Scheduler.hsapplication
hs = Context.Items("Content")

If lcase(Request.ServerVariables("AUTH_USER")) = "guest" Then Response.Redirect("/unauthorized.asp")
If lcase(hs.WebLoggedInUser) = "guest" Then Response.Redirect("/unauthorized.asp")


%>

<%= get_header () %>

    <div class="container">
    <h2><%=viewstate("room") %></h2>
<script>
    window.onload = function ()
    {
        // The datasets as shown on the chart. Each point is an array, described below.
        var temperature = <%=get_room_data("number", "thermometers")  %>;
        var target = <%=get_room_data("target", "thermometers")  %>;
        var heater = <%=get_room_data("number", "heaters")  %>;
        var labels = [];
        for (var i = 0; i < heater.length; i++ ) {
            if (heater[i][1] > 0) {
            heater[i][1]= 14;
            }
            if (heater[i][1] == 0) {
                heater[i][1]= 10;
            }
        }
        var interval = <%= plot_interval %> ;
        var start = new Date(temperature[0][0]).getHours();
        for (var i = 0; i < interval; i++ ) {
            if (i % Math.floor(interval/24) == 0 || interval <24){
                labels.push((start +i) % 24);
            }
        }
        
        // Create the Scatter chart. The arguments are: the canvas ID and the data to be represented on the chart.
        // You can have multiple sets of data if you wish
        var sg = new RGraph.Scatter('thermostat', temperature, target, heater)
        
            // Configure the chart to look as you want it to.
            .Set('chart.background.barcolor1','white')
            .Set('chart.background.barcolor2', 'white')
            .Set('chart.grid.color', 'rgba(238,238,238,1)')
            .Set('chart.gutter.left', 30)
            .Set('chart.line', true)
            .Set('chart.tickmarks', null)
            .Set('chart.labels', labels)
            .Set('chart.ymax', 30 ) 
            .Set('chart.ymin', 10 ) 
            .Set('chart.background.grid.autofit.numhlines', 20)
            .Set('chart.background.grid.autofit.numvlines', <%=plot_interval%>)
            .Set('chart.xmax', temperature[temperature.length-1][0] ) // Important!
            .Set('chart.xmin', temperature[0][0] ) // Important!
        
        
            // Now call the .Draw() method to draw the chart.
            .Draw();
    }
</script>
<canvas id='thermostat' width='700' height='500' style="margin: 0 auto;">[No canvas support]</canvas>
<a href="plot_thermostat.aspx?room=<%=ViewState("room")%>&plot_interval=<%=plot_interval - 24%>">shorter</a> | <a href="plot_thermostat.aspx?room=<%=ViewState("room")%>&plot_interval=<%=plot_interval+24%>">longer</a>
    </div>
  <script src="jquery.min.js"></script>
    <!-- Include all compiled plugins (below), or include individual files as needed -->
    <script src="dist/js/bootstrap.js"></script>
    
  </body>
  
</html>

