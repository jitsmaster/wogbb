using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Xml.Linq;
using Ingeniux.Runtime.Controllers;

namespace Ingeniux.Runtime
{
	public class CmsRoute : RouteBase
	{
		#region Static Members
		private const string DEFAULT_CONTROLLER_NAME = "CMSPageDefault";
		private static readonly string defaultAction = "Index";
		private static string _SitePath = ConfigurationManager.AppSettings["PageFilesLocation"];
		private static CMSPageFactory _PageFactory;
		private static ControllerSummary[] controllerSummaries;
        static readonly object loc = new object();

		private static string _BaseController = null;

		static CmsRoute()
		{

			initControlleSummeries();
		}

		private static void initControlleSummeries()
		{
			controllerSummaries =
				(from c in TypeFinder.GetDerivedTypesFromAppDomain(typeof(CMSPageDefaultController))
				 where !c.IsAbstract && !c.IsInterface
				 select new ControllerSummary(c)).ToArray();
		}
		#endregion

		public override RouteData GetRouteData(HttpContextBase httpContext)
		{
            if (_PageFactory == null)
            {
                lock (loc)
                {
                    if (_PageFactory == null)
                    {
                            if (string.IsNullOrWhiteSpace(_SitePath))
                            {
                                _SitePath = httpContext.Server.MapPath("~/App_Data/site");
                            }

			                _PageFactory = new CMSPageFactory(_SitePath);
                    }
                }
            }

			if (string.IsNullOrWhiteSpace(_BaseController))
			{
				_BaseController = ConfigurationManager.AppSettings["BaseControllerName"]
					.ToNullOrEmptyHelper()
					.Return(DEFAULT_CONTROLLER_NAME);
			}

			ICMSRequest pageRequest = null;
			RouteData data = null;
			string action = defaultAction;
			string controller = _BaseController;
			
			string appPath = httpContext.Request.ApplicationPath;
			if (appPath == "/")
				appPath = string.Empty;

			string relativePath = httpContext.Request.Path.Substring(appPath.Length).ToLowerInvariant();

			if (relativePath.StartsWith("/igxdtpreview"))
			{
				pageRequest = _PageFactory.GetPreviewPage(httpContext.Request, true);
				// because the preview does not get a url, but rather an id and xml, 
				// it is not possible to look for other actions based on the URL
				action = "Preview";
				locateController(pageRequest, ref controller);
				data = createRouteData(action, controller);
			}
			if (relativePath.StartsWith("/igxdynamicpreview"))
			{
				pageRequest = _PageFactory.GetDynamicPreviewPage(httpContext.Request, true, true);
				// because the preview does not get a url, but rather an id and xml, 
				// it is not possible to look for other actions based on the URL
				action = "DynamicPreview";
				locateController(pageRequest, ref controller);
				data = createRouteData(action, controller);
			}
			else
			{
				//only need the schema data, not the whole page
				ICMSRoutingRequest routingRequest = _PageFactory.GetPage(httpContext.Request, true);
				if (routingRequest != null && routingRequest.CMSRequest != null && routingRequest.CMSRequest.Exists)
				{
					pageRequest = routingRequest.CMSRequest;
					if (pageRequest != null)
					{
						//use default controller for routing request
						if (pageRequest is CMSPageRedirectRequest)
							data = createRouteData(defaultAction, controller);
						else
						{
							var matchingController = locateController(pageRequest, ref controller);

							if (!string.IsNullOrEmpty(routingRequest.RemaingPath.Trim('/')))
							{
								//use the first section of the remaining path as action, this is arbitrary.
								string actionInPath = routingRequest.RemaingPath.Trim('/').SubstringBefore("/");

								//if the controller doesn't have this action, use default action
								action = matchingController.ToNullHelper()
									.Propagate(
										c => c.ActionNames
											.Where(an => an.ToLowerInvariant() == actionInPath.ToLowerInvariant())
											.FirstOrDefault())
									.Return(defaultAction);

								//get the remaining path in the controller action itself
							}
							else
							{
								action = defaultAction;
							}

							data = createRouteData(action, controller);
						}
					}
					else
					{
						data = createRouteData(defaultAction, controller);
					}
				}
				else
				{
					data = createRouteData(defaultAction, controller);
				}
			}

			return data;
		}

		private RouteData createRouteData(string action, string controller)
		{
			MvcRouteHandler mvcHandler = new MvcRouteHandler(new DefaultControllerFactory());
			var data = new RouteData(this, mvcHandler);
			data.Values["action"] = action;

			//this is the value that will get passed to the ControllerFactory by MVC
			data.Values["controller"] = controller;
			return data;
		}

		private static ControllerSummary locateController(ICMSRequest pageRequest, ref string controller)
		{
			var schema = (pageRequest as ICMSPageTypeRequest).Schema;

			var matchingController = controllerSummaries.Where(
					c => !string.IsNullOrEmpty(schema) && c.Name == schema + "Controller")
				.FirstOrDefault();

			if (matchingController != null)
			{
				controller = schema;
			}
			return matchingController;
		}

		public override VirtualPathData GetVirtualPath(RequestContext requestContext, RouteValueDictionary values)
		{
			return new VirtualPathData(this, string.Empty);
		}
	}
}