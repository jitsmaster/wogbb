using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Ingeniux.Runtime.Controllers;

namespace Ingeniux.Runtime.Models
{
	/// <summary>
	/// Please note that OutputCache Attribute have completely stopped working on assets since certain system upgrade. Now ASP.NET takes over date comparison instead of
	/// letting iis handling it. Also, output cache attribute cannot set cachability and last modified date anymore. 
	/// We have stopped using this attribute and instead doing the cache setting and checking within the handling method.
	/// </summary>
	[Obsolete("OutputCache stopped working since certain .NET upgrade for assets. ASP.NET taking over IIS time stamp checking, and this attribute cannot customize cachibility and modify date. Do not use it.")]
	public class AssetOutputCacheAttribute : OutputCacheAttribute
	{
		public AssetOutputCacheAttribute()
			: base()
		{
		}

		public override void OnActionExecuting(ActionExecutingContext filterContext)
		{
			//if asset is in protected folder or forbidden folder, do not cache it
			var controller = filterContext.Controller;
			if (controller is AssetController)
			{
				var authman = (controller as AssetController).Authman;

				string assetPath = filterContext.HttpContext.Request.GetRelativePath();

				if (authman.IsForbiddenAsset(assetPath) || authman.IsProtectedAsset(assetPath))
					Duration = 0;
			}

			base.OnActionExecuting(filterContext);
		}
	}
}