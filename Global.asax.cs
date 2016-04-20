using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Ingeniux.Runtime;
using System.Web.Configuration;
using FiftyOne.Foundation.Mobile.Detection;
using Ingeniux.Runtime.Models;
using Ingeniux.Runtime.Mobile;
using Ingeniux.Runtime.RuntimeAuth;
using System.Configuration;
using System.IO;

namespace CMS_RT_MVC_SAMPLE
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.IgnoreRoute("{*favicon}", new { favicon = @"(.*/)?favicon.ico(/.*)?" });

            routes.MapRoute(
                "RTA Session Retrieving Handler",
                "Session.ashx",
                new
                {
                    controller = "Authentication",
                    action = "GetSession"
                });

            routes.MapRoute(
                "RTALoginHandler",
                "Login.ashx",
                new
                {
                    controller = "Authentication",
                    action = "Login"
                });

            routes.MapRoute(
                "RTA Log out Handler",
                "Logout.ashx",
                new
                {
                    controller = "Authentication",
                    action = "Logout"
                });

            routes.MapRoute(
                "Dynamic Xml Preview",
                "IGXDynamicXmlPreview",
                new
                {
                    controller = "CMSPageDefault",
                    action = "DynamicXmlPreview"
                });

            routes.MapRoute(
                "ICEUpdate",
                "IGXDTICEUpdate",
                new
                {
                    controller = "CMSPageDefault",
                    action = "IceUpdate"
                });

            routes.MapRoute(
                "Stylesheets",
                "stylesheets/{*path}",
                new
                {
                    controller = "Asset",
                    action = "Get"
                });

            routes.MapRoute(
                "Media",
                "media/{*path}",
                new
                {
                    controller = "Asset",
                    action = "Get"
                });

            routes.MapRoute(
                "SSImages",
                "swapandsellimgs/{*path}",
                new
                {
                    controller = "Asset",
                    action = "Get"
                });

            routes.MapRoute(
                "Images",
                "images/{*path}",
                new
                {
                    controller = "Asset",
                    action = "Get"
                });

            routes.MapRoute(
                "Documents",
                "documents/{*path}",
                new
                {
                    controller = "Asset",
                    action = "Get"
                });

            routes.MapRoute(
                "Prebuilt",
                "prebuilt/{*path}",
                new
                {
                    controller = "Asset",
                    action = "Get"
                });

            routes.MapRoute(
                "Assets",
                "assets/{*path}",
                new
                {
                    controller = "Asset",
                    action = "Get"
                });

            routes.MapRoute(
                "WOGBB Change password",
                "member/changepassword",
                new
                {
                    controller = "Login",
                    action = "ChangePassword"
                });


            routes.MapRoute(
                "WOGBB Login",
                "login",
                new
                {
                    controller = "Login",
                    action = "Login"
                });

            routes.MapRoute(
            "WOGBB logout",
            "member/logout",
            new
            {
                controller = "Login",
                action = "Logout"
            });

            routes.MapRoute(
                "WOGBB User Update",
                "member/signup",
                new
                {
                    controller = "Login",
                    action = "Signup"
                });

            routes.Add(new CmsRoute());
        }

        protected void Application_Start()
        {
            HttpCapabilitiesBase.BrowserCapabilitiesProvider = new IGXMobileCapabilitiesProvider();

            ViewEngines.Engines.Clear();
            ViewEngines.Engines.Add(new CMSMobileRazorViewEngine());

            AreaRegistration.RegisterAllAreas();

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);

            //delete the temp xslt folder, this will make the start up slower though
            string dssTempFolder = Server.MapPath("~/App_Data/_dss_temp_stylesheets_");
            //bypass if delete can't happen, make the site start anyway
            try
            {
                foreach (string f in Directory.GetFiles(dssTempFolder))
                {
                    File.SetAttributes(f, FileAttributes.Normal);
                    File.Delete(f);
                }

                Directory.Delete(dssTempFolder, true);
            }
            catch { }
        }

        public override string GetVaryByCustomString(HttpContext context, string custom)
        {
            if (custom == "RTA")
            {
                UserData rtaSession = AuthenticationManager.GetRuntimeAuthenticationUserData(context.Request);

                string varyStr = rtaSession != null ?
                    rtaSession.ToJson() : string.Empty;

                //get cookie settings
                string cookies = ConfigurationManager.AppSettings["CacheVariationCookieNames"] ?? string.Empty;

                if (!string.IsNullOrWhiteSpace(cookies))
                {
                    varyStr += string.Join("|",
                        cookies.Split(';')
                            .Select(
                                cName => cName.Trim())
                            .Where(
                                cName => !string.IsNullOrWhiteSpace(cName))
                            .Select(
                                cName => context.Request.Cookies[cName]
                                    .ToNullHelper()
                                    .Propagate(
                                        c => c.Value)
                                    .Return(string.Empty)));
                }

                return varyStr;
            }

            return base.GetVaryByCustomString(context, custom);
        }
    }
}