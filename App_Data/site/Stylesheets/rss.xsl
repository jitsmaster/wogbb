<?xml version="1.0" encoding="UTF-8"?>

<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" 
	xmlns:msxsl="urn:schemas-microsoft-com:xslt" 
	xmlns:rsslib="urn:rsslibns" xmlns:atom="http://www.w3.org/2005/Atom"
	exclude-result-prefixes="rsslib msxsl #default xsl"
	version="1.0">
	
	<xsl:output method="xml" 
				version="1.0" 
				encoding="UTF-8" 
				indent="no" 
				omit-xml-declaration="no" 
				 />
	
	<!-- Included Stylesheets -->
	<xsl:include href="include-rss.xsl"/>
	
	<xsl:variable name="get" select="/*/IGX_Info/GET"/>
	<xsl:variable name="anav" select="/*/Navigation[@Name='AncestorNavigation']" />
	
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
			<xsl:otherwise><xsl:value-of select="/*/BaseURL"/></xsl:otherwise>
		</xsl:choose>
	</xsl:variable>
	
	<!-- *Begin* Root Template -->
	<xsl:template match="/">
		<rss version="2.0">
			<channel>
				<title><xsl:value-of select="/*/Title"/></title>
				<link><xsl:value-of select="$siteURL"/></link>		
				<atom:link rel="self" type="application/rss+xml" href="{$siteURL}{rsslib:removeQS(string($anav/Page[last()]/@URL))}"/>
				<description><xsl:value-of select="Abstract"/></description>
				<language>en-us</language>
				<pubDate><xsl:value-of select="rsslib:formatRSSDate('')"/></pubDate>
				<lastBuildDate><xsl:value-of select="rsslib:formatRSSDate('')"/></lastBuildDate>
				<xsl:apply-templates select="/*/Navigation[@Name!='AncestorNavigation']//Page
					[@Schema!='Folder' and not(contains(@Schema, 'Component'))]" mode="rss">
					<xsl:sort select="@Date | @EventStartDate" order="descending"/>
				</xsl:apply-templates>	            
			</channel>
		</rss>
	</xsl:template>
	
	<xsl:template match="Page" mode="rss">
		<item>
			<title><xsl:value-of select="@Title"/></title>
			<link><xsl:if test="not(contains(@URL, 'http:'))"><xsl:value-of select="$siteURL"/></xsl:if><xsl:value-of select="rsslib:removeQS(string(@URL))"/></link>
			<description><xsl:value-of select="rsslib:absoluteHref(string(@Abstract), string($siteURL))"/></description>
			<pubDate><xsl:value-of select="rsslib:formatRSSDate(string((@Date | @EventStartDate)[string(.)]))"/></pubDate>
			<guid><xsl:if test="not(contains(@URL, 'http:'))"><xsl:value-of select="$siteURL"/></xsl:if><xsl:value-of select="rsslib:removeQS(string(@URL))"/></guid>
		</item>
	</xsl:template>
</xsl:stylesheet>