<%@ Assembly src="basic.vb" %>
<%@ Page Language="vb" AutoEventWireup="false"
Src="calendars_behind.aspx.vb" Inherits="calendars_behind" debug="true" %>
<!--  Temperature_Plot.ASPX by Steve Anderson (aka Snevl) -->
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<!DOCTYPE html>
<html>
  <head>
  <script src="RGraph/libraries/RGraph.common.core.js" ></script>
    <script src="RGraph/libraries/RGraph.common.dynamic.js" ></script>
    <script src="RGraph/libraries/RGraph.bar.js" ></script>
    <script src="RGraph/libraries/RGraph.thermometer.js" ></script>
    <script src="RGraph/libraries/RGraph.gauge.js" ></script>
    <script src="RGraph/libraries/RGraph.meter.js" ></script>
    <script src="RGraph/libraries/RGraph.odo.js" ></script>
     
    
    
    
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
    <div class="container">


<iframe src="https://www.google.com/calendar/embed?mode=AGENDA&amp;height=600&amp;wkst=1&amp;bgcolor=%23FFFFFF&amp;src=frankose%40gmail.com&amp;color=%232952A3&amp;src=cillemy%40gmail.com&amp;color=%235229A3&amp;src=siriusfe%40gmail.com&amp;color=%2328754E&amp;ctz=Europe%2FOslo" style=" border-width:0 " width="700" height="600" frameborder="0" scrolling="no"></iframe>

    </div><!-- /.container -->
  <!-- jQuery (necessary for Bootstrap's JavaScript plugins) -->
    <script src="jquery.min.js"></script>
    <!-- Include all compiled plugins (below), or include individual files as needed -->
    <script src="dist/js/bootstrap.js"></script>

  </body>
</html>