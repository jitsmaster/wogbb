<%@ Language=JScript%>
<%
	var errNo = Request.QueryString("error").Item;
	var requestedFile = Request.QueryString("fileURL").Item;
	if (requestedFile != undefined)
		Response.Write(escape(requestedFile));

	if(errNo == 404)
	{
		Server.Transfer("404.htm");
	}
	else if(errNo == 500)
	{
		Server.Transfer("500.htm");
	}
	else
	{
		Response.Write('<HTML><HEAD><TITLE>Unknown Web Site Error</TITLE><META HTTP-EQUIV="Content-Type" Content="text/html; charset=Windows-1252">')
		Response.Write('<STYLE type="text/css"> BODY { font: 8pt/12pt verdana } H1 { font: 13pt/15pt verdana } H2 { font: 8pt/12pt verdana } A:link { color: red } A:visited { color: maroon }</STYLE>');
		Response.Write('</HEAD><BODY><H1>An unknown exception has occurred:</H1>');
		Response.Write(escape(errNo));
		Response.Write("<BR><BR>Please contact the administrator for this web site.</BODY></HTML>");
	}
%>
