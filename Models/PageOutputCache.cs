using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace Ingeniux.Runtime.Models
{
	/// <summary>
	/// The place to implement page level caching control
	/// </summary>
	public class PageOutputCache
	{
		/// <summary>
		/// The logic of page level caching control is inside this method. 
		/// The content of the page is passed in for integrator to determine if page needs to be cached and 
		/// how long to cache it
		/// </summary>
		/// <param name="pageContent">Page Content Document Root Element</param>
		/// <param name="cacheTime">Output, how long to cache the page by seconds</param>
		/// <returns>True if to enabling caching to the page</returns>
		public static bool CheckPageCache(XElement pageContent, out int cacheTime)
		{
			//by default, allow page cache
			bool allowPageCache = true;

			//by default, 0 means to use system settings
			cacheTime = 0;

			//here is where custom logic can occur, deciding if page is cachable, and how long it should be cached
			//below are sample code for sample site pages
			
			return allowPageCache;
		}
	}
}