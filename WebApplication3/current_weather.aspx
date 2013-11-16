
<%@ Page Language="vb" AutoEventWireup="false"
Inherits="fallokken.lights_behind" debug="true" %>
<!--  Temperature_Plot.ASPX by Steve Anderson (aka Snevl) -->
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<!DOCTYPE html>
<html>
  <head>
  <meta http-equiv="refresh" content="300" /> 
  <script src="RGraph/libraries/RGraph.common.core.js" ></script>
    <script src="RGraph/libraries/RGraph.common.dynamic.js" ></script>
    <script src="RGraph/libraries/RGraph.bar.js" ></script>
    <script src="RGraph/libraries/RGraph.thermometer.js" ></script>
    <script src="RGraph/libraries/RGraph.gauge.js" ></script>
    <script src="RGraph/libraries/RGraph.odo.js" ></script>
     
    
    

    <meta name="description" content="A range Line chart which is adjustable - so you can drag the data points up and down" />
    <meta name="robots" content="noindex,nofollow" />
    
    <meta charset="utf-8">
    <meta http-equiv="X-UA-Compatible" content="IE=edge">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <meta name="description" content="">
    <meta name="author" content="">
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
<%= get_header () %>
<script>
    var bar;
    window.onload = function () {
    
    
    var thermometer = new RGraph.Thermometer('temperature_canvas',-30,30,<%=convert_double(CDbl(hs.DeviceValueEx("7975")).tostring())%>)
        
            // Configure the thermometer chart to look as you want.
            .Set('chart.gutter.left', 45)
            .Set('chart.gutter.right', 45)
            .Set('chart.scale.visible', true)
            .Set('chart.units.post', "C" )
            .Set('chart.title', 'Temperature');
            // Now call the .Draw() method to draw the chart.
            
        var gradient = thermometer.context.createLinearGradient(0,0,0,255);
        gradient.addColorStop(0, 'red');
        gradient.addColorStop(1, 'blue');
        

        thermometer.Set('chart.colors', [gradient]);
        thermometer.Draw();
        var meter = new RGraph.Gauge('humidity_canvas', 0,100,<%=convert_double(CDbl(hs.DeviceValueEx("9540")).tostring())%>)
            .Set('chart.title.top', 'Humidity %')
            
            .Draw();

            
    var gauge = new RGraph.Gauge('windspeed_canvas', 0, 15, <%=convert_double(CDbl(hs.DeviceValueEx("1227")).tostring())%>)
        
            // Configure the chart to appear as wished
            .Set('chart.title.top', 'Wind speed m/s')
            
            // Now call the .Draw() method to draw the chart.
            .Draw();
            
    var odo = new RGraph.Odometer('winddirection_canvas', 0, 360, <%=convert_double(CDbl(hs.DeviceValueEx("3898")).tostring())%>)

            // Configure the Odometer to appear as you want.
            .Set('chart.needle.thickness', 3)
            .Set('chart.title', 'Wind direction')
            .Set('chart.green.max', 360)
            .Set('chart.red.min', 360)
            .Set('chart.green.color', 'grey')
            
            // Now call the .Draw() method to draw the chart.
            .Draw();
    }
    
            
</script>
    <div class="container">

<div class='row' id = "gauges">
<h2>Current weather </h2>
<div class='col-xs-12 col-sm-6 col-md-3' align='center'>
<canvas id='temperature_canvas' width='130' height='250' style="margin: 0 auto;">[No canvas support]</canvas>
</div>
<div class='col-xs-12 col-sm-6 col-md-3' align='center'>
<canvas id='humidity_canvas' width='250' height='250' style="margin: 0 auto;">[No canvas support]</canvas>
</div>
<div class='col-xs-12 col-sm-6 col-md-3' align='center'>
<canvas id='windspeed_canvas' width='250' height='250' style="margin: 0 auto;">[No canvas support]</canvas>
</div>
<div class='col-xs-12 col-sm-6 col-md-3' align='center'>
<canvas id='winddirection_canvas' width='250' height='250' style="margin: 0 auto;">[No canvas support]</canvas>
</div>

</div>




    </div><!-- /.container -->
  <!-- jQuery (necessary for Bootstrap's JavaScript plugins) -->
    <script src="jquery.min.js"></script>
    <!-- Include all compiled plugins (below), or include individual files as needed -->
    <script src="dist/js/bootstrap.js"></script>

  </body>
</html>