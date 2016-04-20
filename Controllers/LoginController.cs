using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Ingeniux.Runtime.Models;
using Ingeniux.Runtime;

namespace Ingeniux.Runtime.Controllers
{
    public class LoginController : CMSPageDefaultController
    {
        public ActionResult Logout()
        {
            Session["user"] = null;
            return Redirect("~/");
        }

        public ActionResult ChangePassword(string OPassword, string Password, string PasswordC)
        {
            CMSPageRequest page = _PageFactory.GetPage(Request, "x7") as CMSPageRequest;
            if (Request.HttpMethod.ToLowerInvariant() == "post")
            {
                OPassword = Request.Form["OPassword"];
                Password = Request.Form["Password"];
                PasswordC = Request.Form["PasswordC"];

                var user = Session["user"] as Ingeniux.Runtime.Models.UserInfo;
                if (user == null)
                    page.Tag = "Please login first in order to change password.";
                else
                {
                    if (user.Password != OPassword)
                    {
                        page.Tag = "Current password is incorrect.";
                    }
                    else if (Password != PasswordC)
                    {
                        page.Tag = "Password confirmation doesn't match.";
                    }
                    else
                    {
                        if (Password.Length < 6)
                            page.Tag = "Password must be at least 6 characters.";

                        user.Password = Password;
                        AuthenticationLib auth = new AuthenticationLib();
                        auth.SignupOrUpdate(user, false);
                    }
                }

                if (page.Tag != null)
                    return View("ChangePassword", page);
                else
                    return Redirect("~/");
            }

            return View("ChangePassword", page);
        }

        public ActionResult Login(string redir, string user, string pass)
        {
            if (Request.HttpMethod.ToLowerInvariant() == "post")
            {
                AuthenticationLib authLib = new AuthenticationLib();
                UserInfo userInfo = authLib.Authenticate(user, pass);
                if (userInfo == null)
                {
                    CMSPageRequest page = _PageFactory.GetPage(Request, "x7") as CMSPageRequest;
                    page.Tag = new LoginModel
                    {
                        RedirectUrl = redir,
                        UserName = user,
                        Error = "Invalid User name or Password"
                    };

                    return View("Login", page);
                }
                else if (!userInfo.Paid)
                {
                    CMSPageRequest page = _PageFactory.GetPage(Request, "x7") as CMSPageRequest;
                    page.Tag = new LoginModel
                    {
                        RedirectUrl = redir,
                        UserName = user,
                        Error = "Payment not processed yet. You will be able to login as soon as payment is received."
                    };

                    return View("Login", page);
                }
                else
                {
                    //save as session
                    Session["user"] = userInfo;

                    if (string.IsNullOrWhiteSpace(redir))
                        return Redirect("~/");
                    else
                        //redirect to redir url
                        return Redirect(redir);
                }
            }
            else
            {
                CMSPageRequest page = _PageFactory.GetPage(Request, "x7") as CMSPageRequest;
                page.Tag = new LoginModel
                {
                    RedirectUrl = redir
                };
                return View("Login", page);
            }
        }
    }
}
