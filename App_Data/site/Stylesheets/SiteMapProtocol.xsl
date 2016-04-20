<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" 
	xmlns:msxsl="urn:schemas-microsoft-com:xslt" 
	exclude-result-prefixes="xsl #default msxsl sitemaplib" version="1.0" 
	xmlns:sitemaplib="urn:sitemaplibns" >
    
    <xsl:output media-type="text/xml" method="xml" omit-xml-declaration="no" encoding="UTF-8" indent="yes"/>
	
	  <msxsl:script language="JavaScript" implements-prefix="sitemaplib">
		<![CDATA[
			function frequency(dateStr) {
				var pageDate = UTCtoLocalDateObj(dateStr, false, false, false);
				if (pageDate != null) {
					var today = new Date();
					var diff = today.valueOf() - pageDate.valueOf();  //time since last changed, in milliseconds
					var days = Math.floor(diff / 1000 / 60 / 60 / 24);
					if (days <= 1)
						return "daily";
					if (days <= 7)
						return "weekly";
					if (days <= 60) //give it two months, so we have some monthly items
						return "monthly";
					return "yearly";
				}	
				return "never";
			}
			
			function UTCtoLocalDateObj( utcTimeStr, isUTC, dateOnly, endOfDay )
			{
				//parse the XML time passed in
				try
				{
					var localDate = new Date();
					var year = utcTimeStr.substr(0, 4);
					var month = parseInt(utcTimeStr.substr(4, 2), 10) - 1; // The second parameter to parseInt is the radix, which is required when the input might start with a "0", as in "08"
					var day = utcTimeStr.substr(6,2);
					var hour = utcTimeStr.substr(9,2);
					var minute = utcTimeStr.substr(12, 2);
					var second = utcTimeStr.substr(15,2);
					
                    if (isUTC == 'false') {
                        localDate.setFullYear(year, month, day);
                        if (dateOnly != 'true')
                            localDate.setHours(hour, minute, second);
                        else if (endOfDay == 'true')
                            localDate.setHours(23, 59, 59);
                        else
                            localDate.setHours(0, 0, 0);         
                    }
                    else
                    {
                        localDate.setUTCFullYear(year, month, day);
                        localDate.setUTCHours(hour, minute, second);                            

                        if (dateOnly != 'true')
                            localDate.setUTCHours(hour, minute, second);
                        else if (endOfDay == 'true')
                            localDate.setUTCHours(23, 59, 59);
                        else
                            localDate.setUTCHours(0, 0, 0); 
                    }
				}
				catch (e)
				{
					return "";
				}					
				
				return localDate;			
			}
		]]>
	  </msxsl:script>
    
    <xsl:variable name="nav" select="//Navigation[@Name='URLNavigation']"/>
    
    <xsl:variable name="base" select="substring-before(normalize-space(/*/IGX_Info/REQUEST_INFO/URL),/*/@ID)"/>
    <!-- Remove the port if it is present. -->
    <xsl:variable name="shortbase" select="concat(substring-before($base, ':80'),substring-after($base, ':80'))"/>	
    <xsl:variable name="siteURL">
        <xsl:choose>
            <xsl:when test="normalize-space($shortbase)">
                <xsl:value-of select="$shortbase"/>
            </xsl:when>
            <xsl:when test="normalize-space($base)">
                <xsl:value-of select="$base"/>
            </xsl:when>
        </xsl:choose>
    </xsl:variable>
    
    <!-- XSL processing starts here -->
    <xsl:template match="/">
        <!-- use xsl text here to prevent having to exlude the sitemap namespace -->
        <xsl:text disable-output-escaping="yes"><![CDATA[<urlset xmlns="http://www.sitemaps.org/schemas/sitemap/0.9">]]></xsl:text>
            
            <!-- Generate a <url> node with sub items for each page in the site in a flat list -->
            <xsl:apply-templates select="$nav//Page[@Schema!='Folder' and not(contains(@Schema, 'Component'))]"/>
        <xsl:text disable-output-escaping="yes"><![CDATA[</urlset>]]></xsl:text>
    </xsl:template>
    
    <xsl:template match="Page">
        <url>
            <loc><xsl:value-of select="$siteURL"/><xsl:apply-templates select="@URL"/></loc>
            <lastmod><xsl:apply-templates select="@Changed"/></lastmod>
        	  <priority><xsl:apply-templates select="@Schema"/></priority>
            <changefreq><xsl:apply-templates select="@Changed" mode="change"/></changefreq>
        </url>
    </xsl:template>
    
    <xsl:template match="@URL">
        <xsl:choose>
            <xsl:when test="contains(., '?')"><xsl:value-of select="substring-before(., '?')"/></xsl:when>
            <xsl:otherwise><xsl:value-of select="."/></xsl:otherwise>
        </xsl:choose>
    </xsl:template>
    
    <xsl:template match="@Changed">
        <xsl:if test="normalize-space(.)">
            <xsl:value-of select="concat(substring(., 1, 4), '-', substring(.,5,2), '-', substring(.,7),'Z')"/>
        </xsl:if>
    </xsl:template>
	
	  <xsl:template match="@Changed" mode="change">
		  <xsl:if test="normalize-space(.)">
  			<xsl:value-of select="sitemaplib:frequency(string(.))"/>
	  	</xsl:if>
	  </xsl:template>
    
    <xsl:template match="@Schema">
      <xsl:choose>
        <!-- Override schema based priority setting with value from the specific page if it is supplied/set -->
        <xsl:when test="string(../@Priority)"><xsl:value-of select="../@Priority"/></xsl:when>
        <!-- Customize this part for your site's schema names & business logic -->
        <xsl:when test=".='Home'">1.0</xsl:when>
        <xsl:when test=".='SectionFront'">.9</xsl:when>           
        <xsl:when test=".='Detail'">.5</xsl:when>            
        <xsl:when test=".='SearchResults' or .='FormProcessorPage'">.0</xsl:when>
        <!-- General default value -->
        <xsl:otherwise>.3</xsl:otherwise>
      </xsl:choose>
    </xsl:template>
</xsl:stylesheet>
