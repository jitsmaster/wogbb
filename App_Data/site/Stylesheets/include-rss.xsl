<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE xsl:stylesheet [<!ENTITY rss_include SYSTEM "rss_functions.js">]>

<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
	xmlns:msxsl="urn:schemas-microsoft-com:xslt"
	xmlns:xalan="http://xml.apache.org/xalan"
	xmlns:rsslib="urn:rsslibns"
	extension-element-prefixes="rsslib"
	version="1.0">
	
	<msxsl:script language="JavaScript" implements-prefix="rsslib">
		&rss_include;
	</msxsl:script>
	
	<xalan:component prefix="rsslib" elements="" 
		functions="formatRSSDate;absoluteHref;GetRFC822Date;getTZOString;removeQS;pad;UTCtoLocalDateObj" >
		<xalan:script lang="javascript">
			&rss_include;
		</xalan:script>
	</xalan:component>
	
</xsl:stylesheet>