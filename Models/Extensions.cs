using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.WebPages;
using System.Web.Mvc;
using System.Web.Mvc.Html;

namespace Ingeniux.Runtime
{
	public static class RazorViewExtensions
	{
		/// <summary>
		/// Extension method to find out if a section is defined in view page
		/// if not, call the defaultContent callback to render default content
		/// </summary>
		/// <param name="page"></param>
		/// <param name="sectionName"></param>
		/// <param name="sectionDefaultContent"></param>
		/// <returns></returns>
		public static HelperResult RenderSection(this WebPageBase page,
			string sectionName, Func<dynamic, HelperResult> sectionDefaultContent)
		{
			if (page.IsSectionDefined(sectionName))
			{
				return page.RenderSection(sectionName);
			}
			return sectionDefaultContent(null);
		}
	}

	public static class HtmlHelperExtensions
	{
		public static bool ViewExists(this ControllerBase controller, string viewName)
		{
			ViewEngineResult viewEngineResult = ViewEngines.Engines.FindPartialView(controller.ControllerContext, viewName);

			return (viewEngineResult != null && viewEngineResult.View != null && viewEngineResult.ViewEngine != null);
		}

		/// <summary>
		/// default fallback views to display text or html element.
		/// if the element is text or html element. Otherwise, return empty string
		/// </summary>
		/// <param name="helper"></param>
		/// <param name="elementToDisplay"></param>
		/// <returns></returns>
		public static MvcHtmlString Display(this HtmlHelper helper, ICMSElement elementToDisplay)
		{
			string viewName = GetAvailableView(helper.ViewContext.Controller, elementToDisplay);

			if (!string.IsNullOrEmpty(viewName))
				return helper.Partial(viewName, elementToDisplay);
			else
				return MvcHtmlString.Empty;
		}

		public static void RenderDisplay(this HtmlHelper helper, ICMSElement elementToDisplay)
		{
			string viewName = GetAvailableView(helper.ViewContext.Controller, elementToDisplay);

			if (!string.IsNullOrEmpty(viewName))
				helper.RenderPartial(viewName, elementToDisplay);
		}

		public static string GetAvailableView(this ControllerBase controller, ICMSElement elementToDisplay)
		{
			string fieldName = elementToDisplay.ViewName ?? elementToDisplay.Content.Name.LocalName;

			//check editable version first
			string viewName = "Editable/" + fieldName;
			if (!ViewExists(controller, viewName))
				viewName = fieldName;

			if (!ViewExists(controller, viewName))
			{
				string fieldType = elementToDisplay.Type;
				if (fieldType == "dhtml")
					viewName = "Editable/_DefaultHtml";
				else if (fieldType == "string" || fieldType == string.Empty)
					viewName = "Editable/_DefaultText";
			}
			return viewName;
		}
	}

	public static class UrlHelperExtensions
	{
		public static string ProcessUrl(this UrlHelper helper, string url)
		{
			url = url ?? "";
			if (!url.StartsWith("http://") && !url.StartsWith("https://"))
			{
				if (url.Contains("www."))
				{
					string prefix = helper.RequestContext.HttpContext.Request.IsSecureConnection ? "https://" : "http://";
					url = prefix + url.TrimStart('/');
				}
				else
				{
					url = helper.Content("~/" + url);
				}
			}

			return url;
		}

		public static string Asset(this UrlHelper helper, string path, ICMSEnvironment model)
		{
			path = path.TrimStart('/');

			if (model != null && model.IsPreview)
			{
				path += (path.Contains("?")) ? "&" : "?";
				path += "_previewAsset_=true";
			}

			string fullPath = "~/assets/" + path;
			fullPath = helper.Content(fullPath);

			return fullPath;
		}
	}
}