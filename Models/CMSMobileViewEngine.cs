using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;

namespace Ingeniux.Runtime.Models
{
	public enum TabletTreatment
	{
		AsDesktop,
		AsMobile,
		AsTablet
	}

	internal class MobileViewFinder
	{
		public MobileViewFinder(ControllerContext controllerContext, String viewName, TabletTreatment tabletTreatment)
		{
			ControllerName = controllerContext.RouteData.Values["controller"].ToString();
			ViewBasePath = viewName.SubstringBefore("/", true, true).ToNullOrEmptyHelper()
				.Propagate(
					s => "/" + s)
				.Return(string.Empty);
			ViewName = viewName.SubstringAfter("/", true);

			MobileDeviceInformation = controllerContext.HttpContext.Request.Browser != null && controllerContext.HttpContext.Request.Browser.IsMobileDevice ?
				new Mobile.MobileDevice(controllerContext.HttpContext.Request.Browser) :
				null;

			if (MobileDeviceInformation != null && MobileDeviceInformation.IsTablet)
			{
				if (tabletTreatment == TabletTreatment.AsDesktop)
					MobileDeviceInformation = null;
				else if (tabletTreatment == TabletTreatment.AsMobile)
					(MobileDeviceInformation as Mobile.MobileDevice).IsTablet = false;
			}
		}

		public string ControllerName { get; set; }
		public string ViewBasePath { get; set; }
		public string ViewName { get; set; }
		public Mobile.IMobileDevice MobileDeviceInformation { get; set; }

		static string[] MobileViewLocations = new string[]
		{
			"{0}{2}/Mobile/{3}/{4}/{5}/{1}", //[Controller Name][/basepath]/Mobile/[Device Model]/[Major Version]/[Minor Version]/[ViewName]
			"{0}{2}/Mobile/{3}/{4}/{1}", //[Controller Name][/basepath]/Mobile/[Device Model]/[Major Version]/[ViewName]
			"{0}{2}/Mobile/{3}/{1}", //[Controller Name][/basepath]/Mobile/[Device Model]/[ViewName]
			"{0}{2}/Mobile/{1}" //[Controller Name][/basepath]/Mobile/[ViewName]
		};

		static string[] TabletViewLocations = new string[]
		{
			"{0}{2}/Tablet/{3}/{4}/{5}/{1}", //[Controller Name][/basepath]/Tablet/[Device Model]/[Major Version]/[Minor Version]/[ViewName]
			"{0}{2}/Tablet/{3}/{4}/{1}", //[Controller Name][/basepath]/Tablet/[Device Model]/[Major Version]/[ViewName]
			"{0}{2}/Tablet/{3}/{1}", //[Controller Name][/basepath]/Tablet/[Device Model]/[ViewName]
			"{0}{2}/Tablet/{1}" //[Controller Name][/basepath]/Mobile/[ViewName]
		};

		public string[] GenerateExtendedViewPaths()
		{
			if (MobileDeviceInformation == null)
				return new string[] { };

			string[] basePaths = MobileDeviceInformation.IsTablet ? TabletViewLocations : MobileViewLocations;

			string model = MobileDeviceInformation.Model.Trim();
			int version = MobileDeviceInformation.MajorVersion;
			double minorVersion = MobileDeviceInformation.MinorVersion;
			//string model = "Kindle Fire HD中文";
			//int version = 2;
			//double minorVersion = 0.8;

			var rawPaths = basePaths.SelectMany(
				s => new string[]{
					string.Format(s, ControllerName, ViewName, ViewBasePath, model,version, minorVersion),
					string.Format(s, "Shared", ViewName, ViewBasePath, model, version, minorVersion)
				});

			return rawPaths.Select(
					s => "~/Views/" + s)
				.SelectMany(
					s => new string[] {
						s + ".cshtml",
						s + ".vbhtml" })
				.ToArray();
		}
	}

	public class CMSMobileRazorViewEngine : RazorViewEngine
	{
		string[] _OriginalViewLocationFormats;
		string[] _OriginalPartialViewLocationFormats;

		public static TabletTreatment? _TabletTreatment;
		public string MobileBypassCookieName = "Bypass_Mobile_View";

		public CMSMobileRazorViewEngine()
			: base()
		{
			_OriginalViewLocationFormats = ViewLocationFormats;
			_OriginalPartialViewLocationFormats = PartialViewLocationFormats;

			//read from web.config on mobile bypass cookie name.
			//if doesn't exist, use default name
			string mobileBypassCookieNameSetting = ConfigurationManager.AppSettings["MobileViewBypassCookieName"];

			if (!string.IsNullOrWhiteSpace(mobileBypassCookieNameSetting))
				MobileBypassCookieName = mobileBypassCookieNameSetting;

			if (_TabletTreatment == null)
			{
				//only load the setting once
				string tabletTreatment = ConfigurationManager.AppSettings["TabletHandling"]
					.ToNullHelper()
					.Propagate(
						s => s.Trim())
					.Return(null);

				if (tabletTreatment == null)
					_TabletTreatment = TabletTreatment.AsTablet;
				else
				{
					_TabletTreatment = ExceptionWrapper.Get()
						.Branch(
							() => (TabletTreatment)Enum.Parse(typeof(TabletTreatment), tabletTreatment),
							e => TabletTreatment.AsTablet);
				}
			}
		}

		public override ViewEngineResult FindView(ControllerContext controllerContext, string viewName, string masterName, bool useCache)
		{
			bool bypassMobileView = checkMobilePreviewBypass(controllerContext);

			//bypass mobile views if cookie is set
			if (!bypassMobileView)
			{
				ViewLocationFormats = (new MobileViewFinder(controllerContext, viewName, _TabletTreatment.Value))
					.GenerateExtendedViewPaths()
					.Union(_OriginalViewLocationFormats)
					.ToArray();
			}
			else
			{
				ViewLocationFormats = _OriginalViewLocationFormats;
			}

			return base.FindView(controllerContext, viewName, masterName, useCache);
		}

		public override ViewEngineResult FindPartialView(ControllerContext controllerContext, string partialViewName, bool useCache)
		{
			bool bypassMobileView = checkMobilePreviewBypass(controllerContext);

			//bypass mobile views if cookie is set
			if (!bypassMobileView)
			{
				PartialViewLocationFormats = (new MobileViewFinder(controllerContext, partialViewName, _TabletTreatment.Value))
					.GenerateExtendedViewPaths()
					.Union(_OriginalPartialViewLocationFormats)
					.ToArray();
			}
			else
			{
				PartialViewLocationFormats = _OriginalPartialViewLocationFormats;
			}

			return base.FindPartialView(controllerContext, partialViewName, useCache);
		}

		private bool checkMobilePreviewBypass(ControllerContext controllerContext)
		{
			var cookie = controllerContext.HttpContext.Request.Cookies[MobileBypassCookieName];
			bool bypassMobileView = cookie != null && cookie.Value != null && cookie.Value.Trim().ToLowerInvariant() == "true";

			if (bypassMobileView)
			{
				//make sure it is not on design time, if on design-time, no bypass
				string xmlPath = ConfigurationManager.AppSettings["PageFilesLocation"]
					.ToNullHelper()
					.Propagate(
						s => s.Trim())
					.Return(string.Empty);

				bypassMobileView = !Reference.Reference.IsDesignTime(xmlPath);
			}
			return bypassMobileView;
		}
	}
}