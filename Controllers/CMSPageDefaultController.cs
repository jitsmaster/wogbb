using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Web;
using System.Web.Mvc;
using Ingeniux.Runtime;
using Ingeniux.Runtime.Models;
using System.Configuration;
using System.IO;
using System.Globalization;
using System.Web.Mvc.Html;
using Ingeniux.Runtime.XSLT.ICE;

namespace Ingeniux.Runtime.Controllers
{
	/// <summary>
	/// This is the default controller for CMS pages
	/// It uses the route map file to locate page id with route
	/// then retrieve the Xml Document for given id and pass it down as model for view
	/// It will use the root element name as view name
	/// </summary>
	public class CMSPageDefaultController : Controller
	{
		protected ICMSRoutingRequest _CMSPageRoutingRequest;
		private static ControllerSummary[] _ControllerSummaries;
		internal string _SitePath;
		internal string _DTSitePath;
		private const string PAGEFACTORY_APPSTATE_NAME = "PageFactory";

		internal CMSPageFactory _PageFactory;
		protected bool _AllowTfrm = true;
		internal bool _LegacyTransformation = true;
		private bool _IsDTAuthenticated;
		private bool _IsDesignTime = false;
		private bool _UseTempStylesheetsLocation;

		public bool IsDTAuthenticated
		{
			get { return _IsDTAuthenticated; }
		}

		public bool IsDesignTime
		{
			get { return _IsDesignTime; }
		}

		public bool XsltTransformAtTempLocation
		{
			get
			{
				return _UseTempStylesheetsLocation;
			}
		}

		protected override void OnActionExecuting(ActionExecutingContext filterContext)
		{
			base.OnActionExecuting(filterContext);
			if (_IsDesignTime && !_IsDTAuthenticated)
				filterContext.Result = new DesignTimeNotAuthenticatedResult();
		}

		internal void init(System.Web.Routing.RequestContext requestContext)
		{
			this.Initialize(requestContext);
		}

		protected override void Initialize(System.Web.Routing.RequestContext requestContext)
		{
			base.Initialize(requestContext);

			_SitePath = ConfigurationManager.AppSettings["PageFilesLocation"];
            if (string.IsNullOrWhiteSpace(_SitePath))
            {
                _SitePath = requestContext.HttpContext.Server.MapPath("~/App_Data/site");
            }

			_DTSitePath = ConfigurationManager.AppSettings["DesignTimeAssetsLocation"]
				.ToNullOrEmptyHelper()
				.Return(_SitePath);
			_AllowTfrm = ConfigurationManager.AppSettings["EnableTFRMParameter"].ToNullHelper().Branch(
				str => str.Trim().ToLowerInvariant() == "true",
				() => true);
			_LegacyTransformation = ConfigurationManager.AppSettings["LegacyRendering"].ToNullHelper()
				.Branch(
					str => str.Trim().ToLowerInvariant() != "false",
				() => true);

			_UseTempStylesheetsLocation = ConfigurationManager.AppSettings["UseTempStylesheetsLocation"].ToNullHelper().Branch(str => str.Trim().ToLowerInvariant() != "false", () => true);

			_IsDesignTime = Reference.Reference.IsDesignTime(_SitePath);

			if (_IsDesignTime)
			{
				//if design time, check if authenticated already.
				//we will use the parent dt site cookie
				string authCookie = requestContext.HttpContext.Request.Cookies["IGXAuth"]
					.ToNullHelper()
					.Propagate(
						c => c.Value)
					.Return(null);

				_IsDTAuthenticated = !string.IsNullOrWhiteSpace(authCookie);
			}

			//page factory is a cheap object to construct, since both surl map and ssmap are cached most time
			_PageFactory = new CMSPageFactory(_SitePath, !_LegacyTransformation);

			_PageFactory.CacheSiteControls = ConfigurationManager.AppSettings["CacheSiteControls"].ToNullHelper().Branch<bool>(
				str => str.Trim().ToLowerInvariant() == "true",
				() => true);

			if (_PageFactory.CacheSiteControls)
			{
				// only set site control schemas when list is not empty
				string[] siteControlSchemas = ConfigurationManager.AppSettings["SiteControlSchemas"].ToNullHelper().Branch<string[]>(
					str => str.Split(';').Select(
						s => s.Trim()).Where(
						s => s != "").ToArray(),
					() => new string[] { });

				if (siteControlSchemas.Length > 0)
					_PageFactory.SiteControlSchemas = siteControlSchemas;
			}

			_PageFactory.LocalExportsIncludeLinks = ConfigurationManager.AppSettings["LocalExportsIncludeLinks"].ToNullHelper().Branch<bool>(
				str => str.Trim().ToLowerInvariant() == "true",
				() => true);
		}

		/// <summary>
		/// The action to channel all CMS requests
		/// </summary>
		/// <returns></returns>
		[IGXRuntimeCache(VaryByHeader = "User-Agent", VaryByCustom = "RTA", Duration = 5)]

		public virtual ActionResult Index()
		{
			return handleRequest(handleNoneCmsPageRoute, handleCmsPageRequestWithRemainingPath, handleStandardCMSPageRequest);
		}

		protected ActionResult handleRequest(Func<ICMSRoutingRequest, ActionResult> nonCmsRouteHandle, Func<CMSPageRequest, string, ActionResult> remainingPathHandle, Func<CMSPageRequest, ActionResult> standardCmsPageHandle)
		{
			if (_AllowTfrm)
			{
				//exception for settings.xml and urlmap.xml, allow them to display only if 
				string path = Request.Url.AbsolutePath.ToLower();
				if (path.EndsWith("settings/settings.xml") || path.EndsWith("settings/urlmap.xml"))
				{
					var relativePath = path.Substring(Request.ApplicationPath.Length);
					string filePath = Path.Combine(_SitePath, relativePath.TrimStart('/').Replace("/", @"\"));
					if (System.IO.File.Exists(filePath))
					{
						return new XmlResult(XDocument.Load(filePath));
					}
				}
			}

			try
			{
				_CMSPageRoutingRequest = _PageFactory.GetPage(Request, false);
			}
			catch (DesignTimeInvalidPageIDException exp)
			{
				//this message is exclusive to design time and need to present a special page
				return new XmlResult(new XElement("DynamicPreviewError",
					exp.Message));
			}

			if (_CMSPageRoutingRequest.CMSRequest == null || !_CMSPageRoutingRequest.CMSRequest.Exists)
			{
				return nonCmsRouteHandle(_CMSPageRoutingRequest);
			}

			ICMSRequest cmsRequest = _CMSPageRoutingRequest.CMSRequest;

			//if the request is for redirection, do redirect
			if (cmsRequest is CMSPageRedirectRequest)
			{
				var cmsRedir = cmsRequest as CMSPageRedirectRequest;
				//string absoluteUrl = cmsRedir.FinalUrl.ToAbsoluteUrl();
				string absoluteUrl = cmsRedir.FinalUrl;
				if (!absoluteUrl.StartsWith("http://") && !absoluteUrl.StartsWith("https://"))
					absoluteUrl = VirtualPathUtility.ToAbsolute("~/" + absoluteUrl.TrimStart('/'));

				if (cmsRedir.NoRedirectCache)
				{
					Response.Expires = -1;
					Response.Cache.SetCacheability(HttpCacheability.NoCache);
				}

				if (cmsRedir.IsPermanent)
				{
					Response.RedirectPermanent(absoluteUrl);
				}
				else
				{
					Response.Redirect(absoluteUrl);
				}
				return null;
			}
			else if (cmsRequest is CMSPageRequest)
			{
				CMSPageRequest pageRequest = cmsRequest as CMSPageRequest;

				//first handle requests with remaining path, if the method doesn't return any result, proceed
				//to treat it as a standard CMS page
				string remainigPath = _CMSPageRoutingRequest.RemaingPath;
				if (!string.IsNullOrEmpty(remainigPath))
				{
					ActionResult cmsWithRemainingPathResult = remainingPathHandle(pageRequest, remainigPath);
					if (cmsWithRemainingPathResult != null)
						return cmsWithRemainingPathResult;
				}

				if (pageRequest.RestrictedAccess)
				{
					disableClientSideCaching();
				}

				return standardCmsPageHandle(pageRequest);
			}
			else
			{
				throw new HttpException(501, Ingeniux.Runtime.Properties.Resources.RequestTypeNotRecognized);
			}
		}

		private void disableClientSideCaching()
		{
			Response.Expires = 0;
			Response.Cache.SetCacheability(HttpCacheability.NoCache);
			Response.Cache.SetNoStore();
		}

		/// <summary>
		/// The method that handles standard CMS page request. Subclass can choose to override to change its behavior.
		/// </summary>
		/// <param name="pageRequest"></param>
		/// <returns></returns>
		internal virtual ActionResult handleStandardCMSPageRequest(CMSPageRequest pageRequest)
		{
			StructureUrlMap urlMap = _PageFactory.UrlMap;

			string triggerFile = System.IO.Path.Combine(urlMap.SitePath, urlMap.Settings.GetSetting<string>("RuntimeCache", "TriggerFile"));

			//if the transform option is 4 or 5, return xml directly, also, bypass caching
			if (pageRequest.IsComponent || _AllowTfrm && (pageRequest.TransformOption == TransformOptions.ExpansionOnly || pageRequest.TransformOption == TransformOptions.Raw))
				return new XmlResult(pageRequest.ContentDocument);
			else
			{
				IGXPageLevelCache pageLevelCache = IGXPageLevelCache.Get(triggerFile);

				//add page level caching information, first request to the same page will always executing. 2nd request will set the cachability
				pageLevelCache.CheckPageCache(Request.Path.ToLowerInvariant(), pageRequest.Content, pageRequest.NavigationUsingCacheContent);

				//locate the view based on root element name, and pass in the XElement of page as model
				return viewOrXsltFallback(pageRequest);
			}
		}

		/// <summary>
		/// Get called when routingRequest not pointing to CMS page
		/// Default behavior is to introduce 404 error. 
		/// </summary>
		/// <param name="routingRequest"></param>
		protected virtual ActionResult handleNoneCmsPageRoute(ICMSRoutingRequest routingRequest)
		{
			//throw 404 error, and let the 404 page handle it
			throw new HttpException(404, Ingeniux.Runtime.Properties.Resources.CannotLocateIngeniuxCMSPage);
		}

		/// <summary>
		/// This is the method to process request with remaining path.
		/// Requests with remaining path are routes that will live under a CMS page, and need to use the CMS parent page content
		/// for part of rendering.
		/// By default, it is considered as non-existing route and will be throwing 404. Implementer can indeed customize this method directly
		/// or have sub class to override it.
		/// </summary>
		/// <param name="pageRequest"></param>
		/// <param name="remainingPath"></param>
		/// <remarks>If remainging path request is to be handled for specific subclass controller, suggest to call "handleStandardCMSPageRequest"
		/// and add additional information on the ActionResult (view data)</remarks>
		/// <returns></returns>
		protected virtual ActionResult handleCmsPageRequestWithRemainingPath(CMSPageRequest pageRequest, string remainingPath)
		{
			//throw 404 error, and let the 404 page handle it
			throw new HttpException(404, Ingeniux.Runtime.Properties.Resources.CannotLocateIngeniuxCMSPage);
		}

		/// <summary>
		/// The standard CMS page handling. If a view with the root element name exists, use it. Otherwise, try to transform
		/// the page xml content with XSLT, via traditional MSXML 4 DOM, like the old CMS Runtime engine.
		/// This ensures an existing site can start using the new MVC runtime without any work, and slowly convert to full MVC site page type by page type.
		/// 
		/// </summary>
		/// <param name="pageRequest"></param>
		/// <remarks>This method generally doesn't need overriding at all. Only override it is additional data need to be passed down to view. (e.g Cartella data).e</remarks>
		/// <returns></returns>
		internal virtual ActionResult viewOrXsltFallback(CMSPageRequest pageRequest)
		{
			string thisControllerName = this.GetType().Name.SubstringBefore("Controller", true, true);

			string controllerName = thisControllerName;
			locateControllerByRequest(pageRequest, ref controllerName);

			string viewName = controllerName != thisControllerName ? "../" + controllerName + "/" + pageRequest.ViewName : pageRequest.ViewName;

			ViewResult viewResult = View(viewName, pageRequest);
			ViewEngineResult viewEngineResult = viewResult.ViewEngineCollection.FindView(ControllerContext, viewResult.ViewName, viewResult.MasterName);
			if (viewEngineResult != null && viewEngineResult.View != null && viewEngineResult.ViewEngine != null)
				return viewResult;
			else
			{
				//create xslt result, if preview, use the preview site path for xslt
				string sitePath = pageRequest is CMSPagePreviewRequest ? _DTSitePath : _SitePath;
				return new XsltTransformResult(pageRequest, sitePath, _UseTempStylesheetsLocation, _SitePath, _LegacyTransformation);
			}
		}

		internal void exception(ExceptionContext filterContext)
		{
			this.OnException(filterContext);
		}

		/// <summary>
		/// Handle 404 exception and redirect it to the 404 error page, and fallback to NotFoundError view
		/// </summary>
		/// <param name="filterContext"></param>
		protected override void OnException(ExceptionContext filterContext)
		{
			//handle 404 exception
			if (filterContext.Exception is HttpException)
			{
				HttpException httpException = filterContext.Exception as HttpException;
				if (httpException.GetHttpCode() == 404)
				{
					filterContext.ExceptionHandled = true;
					filterContext.HttpContext.Response.StatusCode = 404;
					//add 404 page handling specified by url map

					string error404PageId = _PageFactory.UrlMap.Error404PageId;
					if (!string.IsNullOrEmpty(error404PageId))
					{
						CMSPageRequest page404 = _PageFactory.GetPage(Request, error404PageId) as CMSPageRequest;
						if (page404 != null && page404.Exists)
						{
							ActionResult result404 = viewOrXsltFallback(page404);
							result404.ExecuteResult(ControllerContext);
							return;
						}
					}

					//go to provided Error view, structured url map 404 page is not used, only if the view exists
					//otherwise, use base procedure
					ViewResult viewResult = View("NotFoundError", new Error404()
					{
						Title = Ingeniux.Runtime.Properties.Resources.Error404Title,
						BodyCopy = Ingeniux.Runtime.Properties.Resources.Error404BodyCopy,
						Factory = _PageFactory
					});

					ViewEngineResult viewEngineResult = viewResult.ViewEngineCollection.FindView(ControllerContext, viewResult.ViewName, viewResult.MasterName);
					if (viewEngineResult != null && viewEngineResult.View != null && viewEngineResult.ViewEngine != null)
					{
						viewResult.ExecuteResult(ControllerContext);
						return;
					}
				}
			}

			base.OnException(filterContext);
		}


		/// <summary>
		/// The action to handle page preview with post in xml content and override request parameters
		/// Recommand to use base functionalities in subclass or not overriding it.
		/// </summary>
		/// <returns></returns>
		[AcceptVerbs(HttpVerbs.Post), ValidateInput(false)]
		public virtual ActionResult Preview()
		{
			CMSPagePreviewRequest previewRequest = _PageFactory.GetPreviewPage(Request, false) as CMSPagePreviewRequest;
			return viewOrXsltFallback(previewRequest);
		}

		static readonly object dynamicPreviewLoc = new object();

		/// <summary>
		/// The action to handle page dynamic preview. information passed in via querystrings
		/// </summary>
		/// <returns></returns>
		[ValidateInput(false)]
		public virtual ActionResult DynamicPreview()
		{
			CMSPageDynamicPreviewRequest dynamicPreviewRequest;
			//dynamic preview can only be done one at a time, since they share the same navigation builder, which needs the pub target context
			//multi-thread it will cause mix up of urls from different pub targets
			dynamicPreviewRequest = _PageFactory.GetDynamicPreviewPage(Request, false) as CMSPageDynamicPreviewRequest;

			//no caching for preview
			disableClientSideCaching();

			if (dynamicPreviewRequest.IsComponent ||
				_AllowTfrm && (dynamicPreviewRequest.TransformOption == TransformOptions.ExpansionOnly ||
				dynamicPreviewRequest.TransformOption == TransformOptions.Raw))
			{
				return new XmlResult(dynamicPreviewRequest.ContentDocument);
			}
			else if (dynamicPreviewRequest.TransformOption == TransformOptions.Default)
			{
				return viewOrXsltFallback(dynamicPreviewRequest);
			}
			else
			{
				return new PreviewXsltTransformResult(dynamicPreviewRequest, _SitePath, false);
			}
		}

		/// <summary>
		/// The action to handle xml preview in dynamic preview mode. It actually returned html
		/// </summary>
		/// <returns></returns>
		[AcceptVerbs(HttpVerbs.Get)]
		public virtual ActionResult DynamicXmlPreview()
		{
			try
			{
				CMSPageDynamicPreviewRequest dynamicPreviewRequest;

				dynamicPreviewRequest = _PageFactory.GetDynamicPreviewPage(Request, false) as CMSPageDynamicPreviewRequest;

				disableClientSideCaching();

				if (Request.QueryString["viewSource"] == "true")
					return new XmlResult(dynamicPreviewRequest.ContentDocument, "text/plain"); //view source will set content to plain text to prevent browser formatting
				else
					return new PreviewXsltTransformResult(dynamicPreviewRequest, _SitePath);
			}
			catch (Exception e)
			{
				return new XmlResult(new XElement("DynamicXmlPreviewError", "Error performing XML preview:" + e.Message));
			}
		}

		/// <summary>
		/// get updated content for a given field on the page.
		/// This method doesn't allow override
		/// </summary>
		/// <returns></returns>
		[AcceptVerbs(HttpVerbs.Post), ValidateInput(false)]
		public CommentFilteredJsonResult IceUpdate()
		{
			CommentFilteredJsonResult result = new CommentFilteredJsonResult();

			disableClientSideCaching();

			try
			{
				CMSIceFieldMarkupUpdateRequest iceUpdateRequest = _PageFactory.GetIceFieldMarkupUpdater(Request);

				string fieldViewName = ControllerContext.Controller.GetAvailableView(iceUpdateRequest);

				string contentStr;

				//when not able to locate the template in both editable and normal path, try to use the default fallback templates for text and html
				if (ControllerContext.Controller.ViewExists(fieldViewName))
				{
					using (StringWriter sw = new StringWriter(CultureInfo.InvariantCulture))
					{
						HtmlHelper html = new HtmlHelper(new ViewContext(ControllerContext, new WebFormView(ControllerContext, "Shared/Title"), new ViewDataDictionary() { },
							new TempDataDictionary() { }, sw), new ViewPage());

						contentStr = Server.HtmlDecode(html.Partial(fieldViewName, iceUpdateRequest).ToHtmlString());
					}
				}
				else
				{
					DynamicPreviewICEProcessor iceUpdateProc = new DynamicPreviewICEProcessor(_SitePath);
					InContextEditUpdateInfo iceInfo = iceUpdateProc.IceUpdatePreparation(iceUpdateRequest);

					if (iceInfo == null)
						throw new ArgumentException("Cannot locate either MVC view or Xslt template for element \"" + iceUpdateRequest.FieldName + "\". This field cannot be edited. This is an error of site implementation.");

					//use classic transformation engine, even though slower, make sure work with all preview cases
					LegacyTransformationEngine transformer = new LegacyTransformationEngine(_SitePath, string.Empty, true, _UseTempStylesheetsLocation);

					//during ice update preparation, updated xslts werer already processed, we don't need to process it again.
					bool ssUpdated = false;
					contentStr = transformer.Transform(iceInfo.Field, iceInfo.MainStylesheet.FilePath, iceInfo.MainStylesheet.Doc, ssUpdated);
				}

				result.Data = new XHRResponse(
					new Dictionary<string, object>() {
					{"newMarkup", contentStr}
				});
			}
			catch (Exception e)
			{
				result.Data = new XHRResponse(
					e.Message, XHRResponseType.PROCESSING_ERROR);
			}

			return result;
		}

		private static ControllerSummary locateControllerByRequest(CMSPageRequest pageRequest, ref string controller)
		{
			var schema = pageRequest.RootElementName;

			if (_ControllerSummaries == null)
			{
				_ControllerSummaries = (from c in TypeFinder.GetDerivedTypesFromAppDomain(typeof(CMSPageDefaultController))
										where !c.IsAbstract && !c.IsInterface
										select new ControllerSummary(c)).ToArray();
			}

			var matchingController = _ControllerSummaries.Where(
					c => !string.IsNullOrEmpty(schema) && c.Name == schema + "Controller")
				.FirstOrDefault();

			if (matchingController != null)
			{
				controller = schema;
			}
			return matchingController;
		}
	}
}
