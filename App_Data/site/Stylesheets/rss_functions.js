<![CDATA[
	var days = new Array( "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" );
	var longmonths = new Array( "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" );
	var dateToday = new Date();
	
	function formatRSSDate(dateStr) {
        var dateObj = new Date();
        if (dateStr != '')
            dateObj = UTCtoLocalDateObj(dateStr);
        return GetRFC822Date(dateObj);
    }
    
    function absoluteHref(abstract, siteURL) {
        //If there's a link that doesn't start with http, add the site url.
        return abstract.replace(/<a href="(?!http)/g, "<a href=\"" + siteURL);
    }
    
   /*Accepts a Javascript Date object as the parameter;
   outputs an RFC822-formatted datetime string. */
   function GetRFC822Date(oDate)
   {
     var aMonths = new Array("Jan", "Feb", "Mar", "Apr", "May", "Jun", 
                  "Jul", "Aug", "Sep", "Oct", "Nov", "Dec");
     
     var aDays = new Array( "Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat");
     var dtm = new String();
     
     dtm = aDays[oDate.getDay()] + ", ";
     dtm += pad(oDate.getDate()) + " ";
     dtm += aMonths[oDate.getMonth()] + " ";
     dtm += oDate.getFullYear() + " ";
     dtm += pad(oDate.getHours()) + ":";
     dtm += pad(oDate.getMinutes()) + ":";
     dtm += pad(oDate.getSeconds()) + " " ;
     dtm += getTZOString(oDate.getTimezoneOffset());
     return dtm;
   }
   
   /* accepts the client's time zone offset from GMT in minutes as a parameter.
   returns the timezone offset in the format [+|-}DDDD */
   function getTZOString(timezoneOffset)
   {
   var hours = Math.floor(timezoneOffset/60);
   var modMin = Math.abs(timezoneOffset%60);
   var s = new String();
   s += (hours > 0) ? "-" : "+";
   var absHours = Math.abs(hours)
   s += (absHours < 10) ? "0" + absHours :absHours;
   s += ((modMin == 0) ? "00" : modMin);
   return(s);
   }

    
    function removeQS(url) {
        if (url.indexOf('?')!=-1) {
            return url.substring(0, url.indexOf('?'));
        }
        return url;
    }
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////		
	// This function is used only to pad 2 digit numbers
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////		
	
	function pad(number)	
	{
		if (number < 10)
			number = "0" + number;

		return number + "";
	}
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////		
	// this function will convert UTC time to local time based on the server timezone
	// it will return the locatime in date object
	//parameters:
	//	UTCTimeStr- ISO format time
	//	isUTC - string 'true' or 'false, do UTC to local conversion or not
	//	dateOnly - strip out time value or not
	//	endOfDay - only apply when dateOnly is 'true'
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	
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