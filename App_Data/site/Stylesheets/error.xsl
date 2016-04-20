<?xml version="1.0" encoding="ISO-8859-1"?>
<!-- 
This template is used by the Ingeniux RunTime Server to transform server errors into client pages.
For the Java RunTime Server, this is configurable on a per-site basis using entries in xpower.properties:
	xpower.server.useErrorStylesheet = true
	xpower.server.errorStylesheet = error.xsl
-->
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" 
				xmlns:msxsl="urn:schemas-microsoft-com:xslt" 
				xmlns:igxlib="urn:igxlibns" 
				version="1.0">


	<xsl:template match="/">
		<html>
			<head>
				<style type="text/css">
					body { font-face: MS Trebuchet,Helvetica; };
					. { font-face: MS Trebuchet,Helvetica; };
					td { font-face: MS Trebuchet,Helvetica; font-size: 12px; };
					h4 { font-face: MS Trebuchet,Helvetica; font-size: 16px; font-weight: bold; };
					p { font-face: MS Trebuchet,Helvetica; };
				</style>
				<title>Error occured</title>
			</head>
			<body>
				<p>An error occured while trying to display the requested page. </p>
				<p>
					<h4>Error Information</h4>
					<table>
						<tr><td valign="top">Status:</td><td><xsl:value-of select="/ServerError/Status" /></td></tr>
						<tr><td valign="top">File:</td><td><xsl:value-of select="/ServerError/File" /></td></tr>
						<tr><td valign="top">Cause:</td><td><xsl:value-of select="/ServerError/Cause" /></td></tr>
						<tr><td valign="top">Reason:</td><td><xsl:value-of select="/ServerError/Reason" /></td></tr>
					</table>
				</p>
			</body>
		</html>
	</xsl:template>



</xsl:stylesheet>