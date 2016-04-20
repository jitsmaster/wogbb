using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using System.IO;
using Ingeniux.Runtime.RuntimeAuth;
using Ingeniux.Runtime.Models;

namespace Ingeniux.Runtime.Controllers
{
	[SessionState(System.Web.SessionState.SessionStateBehavior.ReadOnly)]
	public class AssetController : Controller
	{
		public AuthenticationManager Authman;
		bool isPreviewRequest = false;
		public string AssetBasePath { get; protected set; }
		protected override void Initialize(System.Web.Routing.RequestContext requestContext)
		{
			base.Initialize(requestContext);

			string sitePath = ConfigurationManager.AppSettings["PageFilesLocation"];

            if (string.IsNullOrWhiteSpace(sitePath))
            {
                sitePath = requestContext.HttpContext.Server.MapPath("~/App_Data/site");
            }

			isPreviewRequest = requestContext.HttpContext.Request.QueryString["_previewAsset_"] == "true";

			//check if the asset followed with querystring "previewAsset"
			AssetBasePath = (isPreviewRequest) ?
				ConfigurationManager.AppSettings["DesignTimeAssetsLocation"] :
				sitePath;

			if (string.IsNullOrEmpty(AssetBasePath))
				AssetBasePath = sitePath;

			Authman = AuthenticationManager.Get(sitePath);
		}

		private bool changed(DateTime modificationDate)
		{
			var headerValue = Request.Headers["If-Modified-Since"];
			if (headerValue == null)
				return true;
			var modifiedSince = DateTime.Parse(headerValue).ToLocalTime();
			return modifiedSince < modificationDate;
		}

		public ActionResult Get()
		{
			string path = HttpUtility.UrlDecode(Request.GetRelativePath());

			//if it is going to ICE images, use a different system, get image content from resource
			if (path.ToLowerInvariant().EndsWith("images/_ice_/play.png"))
			{
				
				string mimeType = MIMEAssistant.GetMIMEType("png");

				var image = Properties.Resources.play;

				MemoryStream ms = new MemoryStream();
				image.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
				ms.Position = 0;

				return File(ms, mimeType);
			}
            else if (path.Contains("swapandsellimgs/"))
            {
                string ext = Path.GetExtension(path);
                string mimeType = MIMEAssistant.GetMIMEType(ext);

                string filePath = Path.Combine(Server.MapPath("~/App_Data/"), path.TrimStart('/'));
                if (!System.IO.File.Exists(filePath))
                    throw new HttpException(404, Ingeniux.Runtime.Properties.Resources.AssetDoesnTExist);

                return File(filePath, mimeType);
            }
			else
			{
				path = path.Contains("assets/") ? path.SubstringAfter("assets/") : path;
				string assetPath = Path.Combine(AssetBasePath, path.TrimStart('/').Replace("/", @"\"));

				if (!System.IO.File.Exists(assetPath))
					throw new HttpException(404, Ingeniux.Runtime.Properties.Resources.AssetDoesnTExist);

				DateTime lastWriteTime = System.IO.File.GetLastWriteTime(assetPath);

				if (!changed(lastWriteTime))
					return new HttpStatusCodeResult(304);

				string ext = Path.GetExtension(assetPath).TrimStart('.');

				string mimeType = MIMEAssistant.GetMIMEType(ext);

				if (path.StartsWith("documents/") || mimeType == MIMEAssistant.DEFAULT_MIME_TYPE)
					Response.AddHeader("Content-Disposition", "attachment");

				string thisUrl = Request.Url.AbsoluteUri;

				if (!isPreviewRequest)
				{
					if (Authman.IsForbiddenAsset(path) || Authman.IsProtectedAsset(path))
					{
						setNoCache();
					}
					else
					{
						Response.Cache.SetCacheability(HttpCacheability.Public);
						Response.Cache.SetExpires(DateTime.MaxValue);
						Response.CacheControl = "public";
						Response.Cache.SetLastModified(System.IO.File.GetLastWriteTime(assetPath));
					}

					//when runtime request, check protected and forbidden folder
					AssetRequestState stateCheck = Authman.CheckAssetAccessiblility(path, Request);
					if (stateCheck == AssetRequestState.Forbidden)
					{
						string forbiddenResponsePagePath = AuthenticationManager.Settings.ForbiddenFoldersResponsePage;

						//path is blocked, go to forbidden response page
						if (!string.IsNullOrWhiteSpace(forbiddenResponsePagePath))
						{
							if (!forbiddenResponsePagePath.EndsWith(".xml") || !forbiddenResponsePagePath.SubstringBefore(".", false, true).IsXId())
							{

								string fullRedirPath = forbiddenResponsePagePath.ToAbsoluteUrl();
								fullRedirPath += fullRedirPath.Contains("?") ? "&" : "?";
								fullRedirPath += "blockedPath=" + HttpUtility.UrlEncode(HttpUtility.UrlEncode(thisUrl));

								return Redirect(fullRedirPath);
							}
							else
							{
								//if the setting is standar xid.xml, then use more friendly path rewrite
								return rewriteToCmsPath(
									forbiddenResponsePagePath.SubstringBefore(".", false, true),
									new Dictionary<string, string> {
										{"blockedPath", thisUrl}});
							}
						}
						else
							throw new HttpException((int)stateCheck, Ingeniux.Runtime.Properties.Resources.AccessToAssetIsForbidden);
					}

					if (stateCheck == AssetRequestState.Unauthorized)
					{
						string loginPagePath = Authman.LoginPath;
						if (!string.IsNullOrWhiteSpace(loginPagePath))
						{
							string loginPathUrl = loginPagePath.ToAbsoluteUrl();
							loginPathUrl += loginPathUrl.Contains("?") ? "&" : "?";
							loginPathUrl += AuthenticationManager.Settings.RedirectionQueryStringName + "=" + Uri.EscapeDataString(Uri.EscapeDataString(thisUrl));
							return RedirectPermanent(loginPathUrl);
						}
						else
							throw new HttpException((int)stateCheck, Ingeniux.Runtime.Properties.Resources.AccessToAssetIsNotAuthorized);
					}

					//use download manager if is protected asset, this way we can use the download page
					if (!string.IsNullOrWhiteSpace(AuthenticationManager.Settings.BinaryDownloadPage) && Authman.IsProtectedAsset(path))
					{
						DownloadManager downloadsMan = Authman.DownloadsManager;
						string downloadPageId;
						Dictionary<string, string> queryStrings = new Dictionary<string, string>();
						bool presentDownloadPage = downloadsMan.ProcessProtectedDownload(Request.RequestContext.HttpContext,
							out queryStrings, out downloadPageId);

						if (!presentDownloadPage)
						{
							Response.AddHeader("Content-Disposition", "attachment");
							return File(assetPath, mimeType);
						}

						return rewriteToCmsPath(downloadPageId, queryStrings);
					}
				}

				return File(assetPath, mimeType);
			}
		}

		private ActionResult rewriteToCmsPath(string pageId,
			Dictionary<string, string> queryStrings)
		{
			//present download page that hosts this download
			CMSPageDefaultController pageController = initPageController();

			//get page by id
			ICMSRequest interceptPage = pageController._PageFactory.GetPage(Request, pageId);
			//add 3 query strings

			foreach (var pair in queryStrings)
				(interceptPage as ICMSEnvironment).QueryString.Add(pair.Key, pair.Value);

			//redo page content to reserialize the querystrings
			(interceptPage as CMSPageRequest).GetPageContent((interceptPage as CMSPageRequest).PageFilePath);

			setNoCache();

			return pageController.viewOrXsltFallback(interceptPage as CMSPageRequest);
		}

		private void setNoCache()
		{
			//make sure no caching of this page
			Response.Cache.SetCacheability(HttpCacheability.NoCache);
			Response.Cache.SetNoStore();
			Response.Expires = 0;
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
					CMSPageDefaultController pageController = initPageController();
					pageController.exception(filterContext);
				}
			}
			base.OnException(filterContext);
		}

		private CMSPageDefaultController initPageController()
		{
			CMSPageDefaultController pageController = new CMSPageDefaultController();
			pageController.init(this.Request.RequestContext);
			//change route data, otherwise mvc 404 page will not have view applied, since it will not be able to load view
			pageController.ControllerContext.RouteData.Values["controller"] = "CMSPageDefault";
			pageController.ControllerContext.RouteData.Values["action"] = "Index";
			return pageController;
		}
	}


	/// <summary>
	/// Hard-coded mimetypes, TODO: try to read from IIS mime settings
	/// </summary>
	public static class MIMEAssistant
	{
		public const string DEFAULT_MIME_TYPE = "application/octet-stream";
		private static readonly Dictionary<string, string> MIMETypesDictionary = new Dictionary<string, string>
                                                                             {
                                                                                 {"ai", "application/postscript"},
                                                                                 {"aif", "audio/x-aiff"},
                                                                                 {"aifc", "audio/x-aiff"},
                                                                                 {"aiff", "audio/x-aiff"},
                                                                                 {"asc", "text/plain"},
                                                                                 {"atom", "application/atom+xml"},
                                                                                 {"au", "audio/basic"},
                                                                                 {"avi", "video/x-msvideo"},
                                                                                 {"bcpio", "application/x-bcpio"},
                                                                                 {"bin", "application/octet-stream"},
                                                                                 {"bmp", "image/bmp"},
                                                                                 {"cdf", "application/x-netcdf"},
                                                                                 {"cgm", "image/cgm"},
                                                                                 {"class", "application/octet-stream"},
                                                                                 {"cpio", "application/x-cpio"},
                                                                                 {"cpt", "application/mac-compactpro"},
                                                                                 {"csh", "application/x-csh"},
                                                                                 {"css", "text/css"},
                                                                                 {"dcr", "application/x-director"},
                                                                                 {"dif", "video/x-dv"},
                                                                                 {"dir", "application/x-director"},
                                                                                 {"djv", "image/vnd.djvu"},
                                                                                 {"djvu", "image/vnd.djvu"},
                                                                                 {"dll", "application/octet-stream"},
                                                                                 {"dmg", "application/octet-stream"},
                                                                                 {"dms", "application/octet-stream"},
                                                                                 {"doc", "application/msword"},
                                                                                 {"dtd", "application/xml-dtd"},
                                                                                 {"dv", "video/x-dv"},
                                                                                 {"dvi", "application/x-dvi"},
                                                                                 {"dxr", "application/x-director"},
                                                                                 {"eps", "application/postscript"},
                                                                                 {"etx", "text/x-setext"},
                                                                                 {"exe", "application/octet-stream"},
                                                                                 {"ez", "application/andrew-inset"},
																				 {"flv", "video/x-flv"},
                                                                                 {"gif", "image/gif"},
                                                                                 {"gram", "application/srgs"},
                                                                                 {"grxml", "application/srgs+xml"},
                                                                                 {"gtar", "application/x-gtar"},
                                                                                 {"hdf", "application/x-hdf"},
                                                                                 {"hqx", "application/mac-binhex40"},
                                                                                 {"htm", "text/html"},
                                                                                 {"html", "text/html"},
                                                                                 {"ice", "x-conference/x-cooltalk"},
                                                                                 {"ico", "image/x-icon"},
                                                                                 {"ics", "text/calendar"},
                                                                                 {"ief", "image/ief"},
                                                                                 {"ifb", "text/calendar"},
                                                                                 {"iges", "model/iges"},
                                                                                 {"igs", "model/iges"},
                                                                                 {
                                                                                     "jnlp", "application/x-java-jnlp-file"
                                                                                     },
                                                                                 {"jp2", "image/jp2"},
                                                                                 {"jpe", "image/jpeg"},
                                                                                 {"jpeg", "image/jpeg"},
                                                                                 {"jpg", "image/jpeg"},
                                                                                 {"js", "application/x-javascript"},
                                                                                 {"kar", "audio/midi"},
                                                                                 {"latex", "application/x-latex"},
                                                                                 {"lha", "application/octet-stream"},
                                                                                 {"lzh", "application/octet-stream"},
                                                                                 {"m3u", "audio/x-mpegurl"},
                                                                                 {"m4a", "audio/mp4a-latm"},
                                                                                 {"m4b", "audio/mp4a-latm"},
                                                                                 {"m4p", "audio/mp4a-latm"},
                                                                                 {"m4u", "video/vnd.mpegurl"},
                                                                                 {"m4v", "video/x-m4v"},
                                                                                 {"mac", "image/x-macpaint"},
                                                                                 {"man", "application/x-troff-man"},
                                                                                 {"mathml", "application/mathml+xml"},
                                                                                 {"me", "application/x-troff-me"},
                                                                                 {"mesh", "model/mesh"},
                                                                                 {"mid", "audio/midi"},
                                                                                 {"midi", "audio/midi"},
                                                                                 {"mif", "application/vnd.mif"},
                                                                                 {"mov", "video/quicktime"},
                                                                                 {"movie", "video/x-sgi-movie"},
                                                                                 {"mp2", "audio/mpeg"},
                                                                                 {"mp3", "audio/mpeg"},
                                                                                 {"mp4", "video/mp4"},
                                                                                 {"mpe", "video/mpeg"},
                                                                                 {"mpeg", "video/mpeg"},
                                                                                 {"mpg", "video/mpeg"},
                                                                                 {"mpga", "audio/mpeg"},
                                                                                 {"ms", "application/x-troff-ms"},
                                                                                 {"msh", "model/mesh"},
                                                                                 {"mxu", "video/vnd.mpegurl"},
                                                                                 {"nc", "application/x-netcdf"},
                                                                                 {"oda", "application/oda"},
                                                                                 {"ogg", "application/ogg"},
                                                                                 {"pbm", "image/x-portable-bitmap"},
                                                                                 {"pct", "image/pict"},
                                                                                 {"pdb", "chemical/x-pdb"},
                                                                                 {"pdf", "application/pdf"},
                                                                                 {"pgm", "image/x-portable-graymap"},
                                                                                 {"pgn", "application/x-chess-pgn"},
                                                                                 {"pic", "image/pict"},
                                                                                 {"pict", "image/pict"},
                                                                                 {"png", "image/png"},
                                                                                 {"pnm", "image/x-portable-anymap"},
                                                                                 {"pnt", "image/x-macpaint"},
                                                                                 {"pntg", "image/x-macpaint"},
                                                                                 {"ppm", "image/x-portable-pixmap"},
                                                                                 {
                                                                                     "ppt", "application/vnd.ms-powerpoint"
                                                                                     },
                                                                                 {"ps", "application/postscript"},
                                                                                 {"qt", "video/quicktime"},
                                                                                 {"qti", "image/x-quicktime"},
                                                                                 {"qtif", "image/x-quicktime"},
                                                                                 {"ra", "audio/x-pn-realaudio"},
                                                                                 {"ram", "audio/x-pn-realaudio"},
                                                                                 {"ras", "image/x-cmu-raster"},
                                                                                 {"rdf", "application/rdf+xml"},
                                                                                 {"rgb", "image/x-rgb"},
                                                                                 {"rm", "application/vnd.rn-realmedia"},
                                                                                 {"roff", "application/x-troff"},
                                                                                 {"rtf", "text/rtf"},
                                                                                 {"rtx", "text/richtext"},
                                                                                 {"sgm", "text/sgml"},
                                                                                 {"sgml", "text/sgml"},
                                                                                 {"sh", "application/x-sh"},
                                                                                 {"shar", "application/x-shar"},
                                                                                 {"silo", "model/mesh"},
                                                                                 {"sit", "application/x-stuffit"},
                                                                                 {"skd", "application/x-koan"},
                                                                                 {"skm", "application/x-koan"},
                                                                                 {"skp", "application/x-koan"},
                                                                                 {"skt", "application/x-koan"},
                                                                                 {"smi", "application/smil"},
                                                                                 {"smil", "application/smil"},
                                                                                 {"snd", "audio/basic"},
                                                                                 {"so", "application/octet-stream"},
                                                                                 {"spl", "application/x-futuresplash"},
                                                                                 {"src", "application/x-wais-source"},
                                                                                 {"sv4cpio", "application/x-sv4cpio"},
                                                                                 {"sv4crc", "application/x-sv4crc"},
                                                                                 {"svg", "image/svg+xml"},
                                                                                 {
                                                                                     "swf", "application/x-shockwave-flash"
                                                                                     },
                                                                                 {"t", "application/x-troff"},
                                                                                 {"tar", "application/x-tar"},
                                                                                 {"tcl", "application/x-tcl"},
                                                                                 {"tex", "application/x-tex"},
                                                                                 {"texi", "application/x-texinfo"},
                                                                                 {"texinfo", "application/x-texinfo"},
                                                                                 {"tif", "image/tiff"},
                                                                                 {"tiff", "image/tiff"},
                                                                                 {"tr", "application/x-troff"},
                                                                                 {"tsv", "text/tab-separated-values"},
                                                                                 {"txt", "text/plain"},
                                                                                 {"ustar", "application/x-ustar"},
                                                                                 {"vcd", "application/x-cdlink"},
                                                                                 {"vrml", "model/vrml"},
                                                                                 {"vxml", "application/voicexml+xml"},
                                                                                 {"wav", "audio/x-wav"},
                                                                                 {"wbmp", "image/vnd.wap.wbmp"},
                                                                                 {"wbmxl", "application/vnd.wap.wbxml"},
                                                                                 {"wml", "text/vnd.wap.wml"},
                                                                                 {"wmlc", "application/vnd.wap.wmlc"},
                                                                                 {"wmls", "text/vnd.wap.wmlscript"},
                                                                                 {
                                                                                     "wmlsc",
                                                                                     "application/vnd.wap.wmlscriptc"
                                                                                     },
                                                                                 {"wrl", "model/vrml"},
                                                                                 {"xbm", "image/x-xbitmap"},
                                                                                 {"xht", "application/xhtml+xml"},
                                                                                 {"xhtml", "application/xhtml+xml"},
                                                                                 {"xls", "application/vnd.ms-excel"},
                                                                                 {"xml", "application/xml"},
                                                                                 {"xpm", "image/x-xpixmap"},
                                                                                 {"xsl", "application/xml"},
                                                                                 {"xslt", "application/xslt+xml"},
                                                                                 {
                                                                                     "xul",
                                                                                     "application/vnd.mozilla.xul+xml"
                                                                                     },
                                                                                 {"xwd", "image/x-xwindowdump"},
                                                                                 {"xyz", "chemical/x-xyz"},
                                                                                 {"zip", "application/zip"},
																				 //added new Office X formats
																				 {"xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"},
																				{"xltx", "application/vnd.openxmlformats-officedocument.spreadsheetml.template"},
																				{"potx", "application/vnd.openxmlformats-officedocument.presentationml.template"},
																				{"ppsx", "application/vnd.openxmlformats-officedocument.presentationml.slideshow"},
																				{"pptx", "application/vnd.openxmlformats-officedocument.presentationml.presentation"},
																				{"sldx", "application/vnd.openxmlformats-officedocument.presentationml.slide"},
																				{"docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document"},
																				{"dotx", "application/vnd.openxmlformats-officedocument.wordprocessingml.template"},
																				{"xlam", "application/vnd.ms-excel.addin.macroEnabled.12"},
																				{"xlsb", "application/vnd.ms-excel.sheet.binary.macroEnabled.12"}
                                                                             };
		public static string GetMIMEType(string ext)
		{
			ext = ext.ToLowerInvariant().Trim();
			if (MIMETypesDictionary.ContainsKey(ext))
			{
				return MIMETypesDictionary[ext];
			}
			return DEFAULT_MIME_TYPE;
		}
	}
}