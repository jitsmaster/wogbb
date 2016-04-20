		<![CDATA[
			var days = new Array( "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" );
			var longmonths = new Array( "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" );
			var dateToday = new Date();
			
			//This function deterimines whether or not the StartDate's month currently being
			//processed by the template is the same or different from the last page that was
			//processed. This allows the script to add row breaks or other delineations between
			//months.
			//
			//usage:
			//		igxlib:IsNewMonth( string(@DateValueAttrib) )
			//
			var currentMonth;
			function IsNewMonth( NodeStr )
			{
				//make sure we have a valid Date parameter
				if ( NodeStr != "" )
				{
					//get the month field from the ISO string
					var month = NodeStr.substr( 4, 2 );
					if ( currentMonth == month )
					{
						//this month is the same as the previous page processed
						return false;
					}
					else
					{
						//update the current month with the new value
						//and return a 'true' flag
						currentMonth = month;
						return true;
					}
				}
				else
				{
					return true;
				}

			}

			//This function retrieves the Month value from the ISO Date string
			//and, based on the value of NodeFormat, returns the month in either
			//short ["Apr"] or long ["April"] format.
			//
			//usage:
			//		igxlib:GenerateMonth( string(@DateValueAttrib, ["short"/"long"]) )
			//
			function GenerateMonth( NodeStr, NodeFormat )
			{
				if ( NodeStr!= "" )
				{
					var month = parseFloat( NodeStr.substr( 4, 2 ) ) - 1;
					if ( NodeFormat.toLowerCase() == "long" )
					{
						return longmonths[ month ];
					} 
					else 
					{
						return longmonths[ month ].substr( 0, 3 );
					}
				}
				return "";
			}

			//This function retrieves the day number of the date specified in the
			//ISO Date attribute passed in. Any leading zeros are stripped and the
			//value is returned as a string.
			//
			//usage:
			//		igxlib:GenerateDay( string(@DateValueAttrib) )
			//
			function GenerateDay( NodeStr)
			{
				if ( NodeStr!= "" )
				{
					if ( NodeStr != null )
					{
						var day = parseInt( NodeStr.substr( 6, 2 ), 10);
						return "" + day;
					}
					return "";
				}
				return "";
			}


			function formatBaseURL( NodeStr)
			{
				// this function prepares the url for querystring parameters.
				var node = NodeStr;
				if ( node == null )
					return "";

				var url = node.text;
				if ( url == null )
					return "";

				if ( url.indexOf( ".xml?" ) == -1 )
					return url += "?";

				else
					return url += "&";
			}
                              
                            
			/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////		
			// this function is to create a random number that is below a count
			/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////		
			
			function RandomIndex( count )
			{
				var index = Math.floor( Math.random() * ( count + 1 ) );
				if ( index == 0 ) 
					index = 1;
				if ( index > count )
					index = count;

				return index;
			}                              

			/////////////////////////////////////////////////////////////////////////////
			// This function will generate a random list of indexes not repeating
			// It is used for random rotation that need to bring back mulitple indexes
			//	Parameters:
			//		count:		total count of the number
			//		numOutput:	number of random indexes to capture
			//	Return: "|" delimited list of indexes, the string ends with "|" 
			/////////////////////////////////////////////////////////////////////////////
			function RandomSequence(count, numOutput )
			{
				var sA = new Array();
				var i = 0;
				while (i<count)
				{
					sA.push(i+1);
					i++;
				}
				
				var tA = new Array();
				
				i=0;
				while ((i<numOutput)&&(i<count))
				{
					var index = Math.round( (sA.length - 1) * Math.random());
						
					tA.push(sA[index]);
					sA.splice(index,1);
					i++;
				}	

				return tA.join("|") + "|";
			}	
			
			/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////					
			// return the n'th item from the list of  seperated items
			// zero based index
			/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////		
			
			function IndexedItemFromList(index, listString, seperator)
			{
			    var listArray = listString.split(seperator);
			    return listArray[index];
			}
			
			/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////		
			// this function is to convert a string to all lower case
			/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////		
						
			function toLower(str)
			{
				var retVal = str.toLowerCase();
				return retVal;
			}
			
			/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////		
			// this function will convert UTC time to local time based on the server timezone
			// it will return the same XML format date string
			// usage: using a variable to store the local time, convert from UTC if the element has UTC attribute
			// 	<xsl:variable name="localTime">
			//		<xsl:choose>
			//			<xsl:when test="DateElement/@UTC">
			//				<xsl:value-of select="igxlib:UTCtoLocalTime(string(DateElement))" />
			//			</xsl:when>
			//			<xsl:otherwise>
			//				<xsl:value-of select="DateElement" />
			//			</xsl:otherwise>
			//		</xsl:choose>	
			//	</xsl:variable>
			//	The offset hours parameter don't have to be passed in. However, if server and client not in the same time zone
			//	the offsetHours is needed to compensate the timezone differences
			/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
			
			function UTCtoLocalTime( UTCTimeStr, offsetHours)
			{
				var localTime = UTCtoLocalDateObj( UTCTimeStr, "true", "false", "false" );
				
				if (offsetHours != null)
				{
					offsetHours = parseInt(offsetHours, 10);	
					
					if (!isNaN(offsetHours))
					{
						localTime = new Date(localTime.getTime() + offsetHours*60*60*1000);
					}
				}
				
				//construct the local time into XML time format string			
				year = localTime.getFullYear();
				month = pad(localTime.getMonth() + 1);

				day = pad(localTime.getDate());

				hour = pad(localTime.getHours());
									
				minute = pad(localTime.getMinutes());
									
				second = pad(localTime.getSeconds());
					
				var localTimeStr;
				if (!isNaN(year)&&!isNaN(month)&&!isNaN(day)&&!isNaN(hour)&&!isNaN(minute)&&!isNaN(second))
					localTimeStr = "" + year + month + day + "T" + hour + ":" + minute + ":" + second;
				else
					localTimeStr = "";
				
				return localTimeStr;		
				
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


			/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
			//this function will make sure a image fit into a given fixed boundry
			//w - image width
			//h - image heigh
			//Uw - boundry width
			//Uh - boundry height
			//usage:
			// <xsl:variable name="fitWidth" select="aailib:fitSizeWidth(string(@Width), string(@Height), string($Upperwidth), string($Upperheight)" />
			/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
			
			function fitSizeWidth(w, h, Uw, Uh)
			{
				var nw;
				
				w = parseInt(w, 10);
				h = parseInt(h, 10);
				Uw = parseInt(Uw, 10);
				Uh = parseInt(Uh, 10);
				
				var hwRatio = h/w;
				
				if ((w <= Uw)&&(h<=Uh))
					nw = w;
				else if((w<=Uw)&&(h>Uh))
					nw = Uh/hwRatio;
				else if ((w> Uw)&&(h<=Uh))
					nw = Uw;
				else if ((w>Uw)&&(h>Uh))
				{
					if (Uw*hwRatio > Uh)
						nw = Uh/hwRatio;
					else
						nw = Uw;
				}
				
				return nw;			
			}

			/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
			// This function will perform string replacement
			// Parameters:
			//		str: the string to perform replacement on
			//		oldPart: the old portion to be replaced
			//		newPart: the new portion to replace with
			/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
			function replaceStr(str, oldPart, newPart)
			{
				while(str.indexOf(oldPart) != -1)
				{
					str = str.replace(oldPart, newPart);
				}
				return str;
			}
			
			
			/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
			// This function is used to match 2 multi-select values to find out if there is any matching value
			//    Parameters:
			//        filterCats:    Multi-select value on the filter(listing) page, the delimitor must be |
			//        pageCats:      Multi-Select value on the page to be filtered, the delimitor must be |
			//   return: "true" or "false"
			/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
			
			function matchCatMultiMulti(filterCats, pageCats)
			{
				//return true if the two are equal, so that the function returns true if both are blank
				if (filterCats == pageCats)
					return "true";
				return hasIntersection(filterCats, pageCats, "|");
			}
			
			
			/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
			// This function is used to match 2 lists of items to find out if there is any interesection.  
			// This function replaces matchCatMultiMulti
			//    Parameters:
			//        filterCats:    the first list of values
			//        pageCats:      the second list of values
			//        splitParam:    the delimitor to split by, which defaults to "|".
			//   return: "true" or "false"
			/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
			
			function hasIntersection(filterCats, pageCats, splitParam) {
				splitParam = splitParam || "|";  //default to "|" if not explicitly defined.
			    var fCats = filterCats.split(splitParam);
			    var pCats = pageCats.split(splitParam);
			    
			    var match = false;
			    var i=0;
			    var j=0;
			    
				for (i=0; i<fCats.length;i++)
			    {
			        var fCat = fCats[i]
			        if (fCat!='') {
						for (j=0; j<pCats.length;j++)
						{
							var pCat = pCats[j];
				            
							if (fCat == pCat)
							{
								match = true;
								break;
							}
						}
					}
				}
			    
			    return match + "";
			}
		

			
		]]>