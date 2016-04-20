<?xml version="1.0" encoding="ISO-8859-1"?>
<!DOCTYPE xsl:stylesheet [<!ENTITY js_include SYSTEM "extension_functions.js">]>

<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" 
	xmlns:msxsl="urn:schemas-microsoft-com:xslt" 
	xmlns:xalan="http://xml.apache.org/xalan"
	xmlns:calendar="urn:calendarlib"
	xmlns:igxlib="urn:igxlibns" 
	extension-element-prefixes="igxlib"
	
	version="1.0">
	
	
	<!--This stylesheet include the following out-of-box functions. These functions are used to provide functionalities XSLT can not provide:
		
		It is designed to work with both MSXML and XALAN.
		
		The actual functions are located in extension_functions.js in the same stylesheet folder.
		
		The following functions are included:
		+++++++++++++++++++++++++++++++
		IsNewMonth: 
		This function deterimines whether or not the StartDate's month currently beingprocessed by the template is the 
		same or different from the last page that wasprocessed. This allows the script to add row breaks or other delineations 
		betweenmonths.
		
		+++++++++++++++++++++++++++++++
		GenerateMonth: 
		retrieves the Month value from the ISO Date string
		
		+++++++++++++++++++++++++++++++
		GenerateDay: 
		retrieves the day number of the date
		
		+++++++++++++++++++++++++++++++
		GenerateYear: 
		retrieves the year number of the date
		
		+++++++++++++++++++++++++++++++
		formatBaseURL: 
		prepares the url for querystring parameters
		
		+++++++++++++++++++++++++++++++
		RandomIndex:
		create a single random number that is below a count
		
		+++++++++++++++++++++++++++++++
		RandomSequence: 
		generate a random list of indexes below a count that are not repeating
		
		+++++++++++++++++++++++++++++++
		IndexedItemFromList:
		return the n'th item from the list of  seperated items
		zero based index
		
		+++++++++++++++++++++++++++++++
		toLower:
		convert a string to all lower case
		
		+++++++++++++++++++++++++++++++
		UTCtoLocalTime: 
		convert UTC time to local time based on the server timezone, 
		The 2nd parameter is optional for compensating timezone difference, if the server is not at the same zone of the 
		target audiences
		
		+++++++++++++++++++++++++++++++
		fitSizeWidth: 
		function will make sure a image fit into a given fixed boundry
		
		+++++++++++++++++++++++++++++++
		replaceStr: 
		perform string replacement
		
		+++++++++++++++++++++++++++++++
		matchCatMultiMulti
		match 2 multi-select values to find out if there is any matching value
		
		+++++++++++++++++++++++++++++++
		hasIntersection
		match 2 lists to find out if there are any matching elements.
		
		
	-->
	<msxsl:script language="JavaScript" implements-prefix="igxlib">
		&js_include;
	</msxsl:script>
	
	<!-- if you are to add news functions in extension_functions.js, you must add the function name to the functions attributes
		otherwise your added function will not work on JAVA server
	-->
	<xalan:component prefix="igxlib" elements="" 
	    functions="IsNewMonth GenerateMonth GenerateDay GenerateYear formatBaseURL RandomIndex RandomSequence IndexedItemFromList toLower UTCtoLocalTime fitSizeWidth replaceStr hasIntersection matchCatMultiMulti" >
		<xalan:script lang="javascript">
			&js_include;
		</xalan:script>
	</xalan:component>


	
</xsl:stylesheet>