<?xml version="1.0" encoding="UTF-8"?>

<xsl:stylesheet xmlns="http://www.w3.org/1999/xhtml"
	xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
	xmlns:msxsl="urn:schemas-microsoft-com:xslt" 
	xmlns:igxlib="urn:igxlibns" 
	exclude-result-prefixes="msxsl igxlib"
	version="1.0">
	
	<xsl:output method="xml" 
		version="1.0" 
		encoding="UTF-8" 
		indent="no" 
		omit-xml-declaration="yes" 
		doctype-public="-//W3C//DTD XHTML 1.0 Transitional//EN" 
		doctype-system="http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd" />
	
	<xsl:variable name="isPreview" select="contains(/*/IGX_Info/REQUEST_INFO/URL, '/xml/')"/>
	
	<!-- Variables needed for structured urls -->
	<xsl:variable name="base" select="substring-before(normalize-space(/*/IGX_Info/REQUEST_INFO/URL),/*/@ID)"/>
	<!-- Remove the port if it is present. -->
	<xsl:variable name="shortbase" select="concat(substring-before($base, ':80'),substring-after($base, ':80'))"/>	
	<xsl:variable name="siteURL">
		<xsl:choose>
			<xsl:when test="string($shortbase)">
				<xsl:value-of select="$shortbase"/>
			</xsl:when>
			<xsl:when test="string($base)">
				<xsl:value-of select="$base"/>
			</xsl:when>
			<xsl:otherwise>http://www.ingeniux.com/</xsl:otherwise>
		</xsl:choose>
	</xsl:variable>
	
	<xsl:variable name="lowercase" select="'abcdefghijklmnopqrstuvwxyz'"/>
	<xsl:variable name="uppercase" select="'ABCDEFGHIJKLMNOPQRSTUVWXYZ'"/>
    
    <!-- Validation template -->
	<xsl:template match="/">
		<html xmlns="http://www.w3.org/1999/xhtml">
			<head>
				<title>
					<xsl:choose>
						<xsl:when test="normalize-space(/*/BrowserBarTitle)">
							<xsl:value-of select="/*/BrowserBarTitle"/>
						</xsl:when>
						<!-- TODO: change to the appropriate organization name -->
						<xsl:otherwise><xsl:value-of select="/*/Title"/> | Organization Name</xsl:otherwise>
					</xsl:choose>
					<xsl:value-of select="/*/BrowserBarTitle[string(.)] | /*/Title[not(string(/*/BrowserBarTitle))]" disable-output-escaping="yes"/>
				</title>
				<xsl:if test="not($isPreview)">
					<!-- Base tag before javascript, dojo calls, and css -->
					<xsl:text disable-output-escaping="yes"><![CDATA[<base href="]]></xsl:text><xsl:value-of select="$siteURL"/><xsl:text disable-output-escaping="yes"><![CDATA["></base>]]></xsl:text>
				</xsl:if>
				<meta http-equiv="Content-Type" content="text/html; charset=UTF-8"/>
				<meta name="description" content="{/*/MetaDescription[string(.)] | /*/Abstract[not(string(/*/MetaDescription))]}"/>
				<meta name="keywords" content="{/*/MetaKeywords}"/>
				<meta http-equiv="imagetoolbar" content="no"/>
				<link rel="icon" type="image/png" href="prebuilt/images/image.png"/>
				<meta name="robots">
					<xsl:attribute name="content">
						<xsl:for-each select="(/*/NoFollow | /*/NoIndex | /*/NoODP)[.='true']">
							<xsl:value-of select="translate(local-name(), $uppercase, $lowercase)"/>
							<xsl:if test="position() != last()">,</xsl:if>
						</xsl:for-each>
					</xsl:attribute>
				</meta>
			</head>
			<body>
				<xsl:comment>Transformed from <xsl:value-of select="/*/@ID"/>, with page type <xsl:value-of select="local-name(/*)"/></xsl:comment>
			</body>
		</html>
	</xsl:template>
	
</xsl:stylesheet>
