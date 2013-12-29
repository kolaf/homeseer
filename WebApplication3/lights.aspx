
<%@ Page Language="vb" AutoEventWireup="false"
Inherits="fallokken.lights_behind" debug="true" %>

<!--  Temperature_Plot.ASPX by Steve Anderson (aka Snevl) -->
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<!DOCTYPE html>
<html>
  <head>
  <meta http-equiv="refresh" content="300;URL='lights.aspx'" />   
    <script src="RGraph/libraries/RGraph.common.core.js" ></script>
    <script src="RGraph/libraries/RGraph.common.dynamic.js" ></script>
    <script src="RGraph/libraries/RGraph.bar.js" ></script>

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
    <%=get_values() %>
    }
</script>
    <div class="container">


<form runat="server" id = "myform">



    <div class="row">
    
    <%=get_all_lights()%>
   
   
</div>   
 


 <hr>

  <div class=row collapse-group>
   Thermostats<br />
    <p <%=canvas() %> id="viewdetails" align ='center'>
        <input type='hidden' id='temperatures_box' runat='server'/> 
        <canvas id='cvs' width='600' height='250'>[No canvas support]</canvas><br>
        <asp:textbox id='message_box' runat='server' maxlength='12' readonly='false' visible='true' />
        <asp:Button id='Button1' Text='Save' runat='server' OnClick='save_clicked' />
        </p>
    </div>
 <%=thermostat_links()%>
 
</form>      

    </div><!-- /.container -->
  <!-- jQuery (necessary for Bootstrap's JavaScript plugins) -->
    <script src="jquery.min.js"></script>
    <!-- Include all compiled plugins (below), or include individual files as needed -->
    <script src="dist/js/bootstrap.js"></script>
    
  </body>
</html>