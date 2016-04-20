<?xml version="1.0" encoding="ISO-8859-1"?>

<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" 
				xmlns:msxsl="urn:schemas-microsoft-com:xslt" 
				xmlns:igxlib="urn:igxlibns" 
				exclude-result-prefixes="msxsl igxlib"
				version="1.0">

	
	
			<!-- 
					* This stylesheet is meant to hold template matches for navigation elements.
					  Examples are given below.
			-->		
	
	
    <xsl:template match="Navigation[@Name = 'ChildNavigation' or @Name = 'SiblingNavigation']">
        <table width="100%">
            <tr>		
                <xsl:apply-templates select="Page" />
            </tr>
        </table>
    </xsl:template>		
    
    <xsl:template match="Navigation[@Name = 'ChildNavigation' or @Name = 'SiblingNavigation']/Page">
        <td align="center" bgcolor="#DDDDDD">
            <a href="{@URL}"><xsl:value-of select="@Title" /></a>
        </td>
    </xsl:template>
    
    <xsl:template match="Navigation[@Name = 'AncestorNavigation']/Page">
        <a href="{@URL}"><xsl:value-of select="@Name" /></a>
        
        <xsl:if test="position () !=last ()">
            <xsl:text> ~ </xsl:text>
        </xsl:if>
    </xsl:template>

</xsl:stylesheet>
