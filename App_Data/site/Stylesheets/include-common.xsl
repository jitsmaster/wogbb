<?xml version="1.0" encoding="ISO-8859-1"?>

<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" 
				xmlns:msxsl="urn:schemas-microsoft-com:xslt" 
				xmlns:igxlib="urn:igxlibns" 
				exclude-result-prefixes="msxsl igxlib"
				version="1.0">

				
				
			<!-- 
					* This stylesheet is meant to hold template matches for common elements, 
			        such as Title, Abstract, BodyCopy, Image, etc. 
			-->	

				
    <xsl:template match="Title">
        <h2><xsl:value-of select="." /></h2>
    </xsl:template>
    
    <xsl:template match="SubHeadline">
        <b><xsl:value-of select="." /></b><p />
    </xsl:template>
    
    <xsl:template match="Byline">
        <xsl:if test="string(.)">
            <i>Written by <xsl:value-of select="." /></i><p />
        </xsl:if>
    </xsl:template>
    
    <xsl:template match="Page[@Name = 'TopStory']">
        <b>TOP STORY:</b><br />
        <a href="{@URL}">
            <xsl:value-of select="@Title" />
        </a>
        <p />
    </xsl:template>
    
    <xsl:template match="Page[@Name = 'HomePageLink']">
        <a href="{@URL}">
            <b>RETURN HOME</b>
        </a>
        <p />
    </xsl:template>		
    
    <xsl:template match="Image[@Name = 'Image'] | Image[@Name = 'StoryImage']">
        <xsl:if test="string(@File)">
            <img src="images/{@File}" align="{@Alignment}" border="{@Border}" alt="{@AlternateText}" />
        </xsl:if>
    </xsl:template>			
    
    <xsl:template match="BodyCopy">
        <xsl:choose>
            <xsl:when test="contains(., '&lt;/p&gt;')">
                <xsl:value-of select="." disable-output-escaping="yes" />
            </xsl:when>
            <xsl:otherwise>
                <p>
                    <xsl:value-of select="." disable-output-escaping="yes" />
                </p>
            </xsl:otherwise>
        </xsl:choose>	    
    </xsl:template>
    
    <xsl:template match="RelatedLinkComponent">
        <xsl:variable name="URL">
            <xsl:choose>
                <!-- If DocumentURL is being used -->
                <xsl:when test="string(DocumentURL)">
                    <xsl:value-of select="DocumentURL"/>
                </xsl:when>
                <!-- If DocumentURL is NOT being used -->
                <xsl:otherwise>
                    <xsl:value-of select="LinkURL"/>
                </xsl:otherwise>
            </xsl:choose>
        </xsl:variable>
        <a href="{$URL}">
            <xsl:if test="NewWindow = 'true'">
                <xsl:attribute name="target">_blank</xsl:attribute>
            </xsl:if>
            <xsl:value-of select="LinkText" />
        </a>
        <br />
    </xsl:template>	

</xsl:stylesheet>
