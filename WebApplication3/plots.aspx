<%@ Page Language="vb" AutoEventWireup="false"
Inherits="fallokken.calendars_behind" debug="true" %>
<!--  Temperature_Plot.ASPX by Steve Anderson (aka Snevl) -->
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html>
  <head>
    <meta http-equiv="refresh" content="300" /> 
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

<%
Dim sensors as string  =Request.QueryString("sensors")



%>



<%= get_header () %>

    <div class="container">

<script type='text/javascript' src='http://www.google.com/jsapi'></script>
    <script type='text/javascript'>
      google.load('visualization', '1', {'packages':['AnnotatedTimeLine']});
      google.setOnLoadCallback(drawChart);
      function drawChart() {

            var data = new google.visualization.DataTable();
            data.addColumn('datetime', 'Date');
            
 
            <%set_query_string() %>
 
            
        var chart = new google.visualization.AnnotatedTimeLine(document.getElementById('chart_div'));
        chart.draw(data, {displayAnnotations: true, legendPosition: 'newRow'});

        }
 
         
    </script>
    
<table border='0' cellpadding='6' cellspacing='1' width='1024px'>
	<asp:label id="label1" runat="server" />
	<td valign="top" align="center">
    <div id='chart_div' style='width: 700px; height: 500px;'></div>
   <br>
       
 
    <script src="http://www.yr.no/sted/Norge/Akershus/Eidsvoll/Minnesund/ekstern_boks_time_for_time.js"></script><noscript><a href="http://www.yr.no/sted/Norge/Akershus/Eidsvoll/Minnesund/">yr.no: Værvarsel for Bryne</a></noscript><br>
    <img src="http://aa004xmu0m4dtdqty.api.met.no/weatherapi/radar/1.3/?radarsite=southeast_norway;type=reflectivity;content=animation;width=800">
    
    </td>
</table>
</div>
<script src="jquery.min.js"></script>
    <!-- Include all compiled plugins (below), or include individual files as needed -->
    <script src="dist/js/bootstrap.js"></script>

  </body>
  
</html>
