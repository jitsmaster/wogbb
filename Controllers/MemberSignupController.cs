using Ingeniux.Runtime.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Ingeniux.Runtime.Controllers
{
    public class MemberSignupController : CMSPageDefaultController
    {
        internal override System.Web.Mvc.ActionResult handleStandardCMSPageRequest(CMSPageRequest pageRequest)
        {
            //handle form post
            var isSignupPost = pageRequest.Form["signupPost"] == "1";

            if (isSignupPost)
            {
                var forms = pageRequest.Form;
                //handle sign up first
                UserInfo user = new UserInfo
                {
                    Name = forms["Name"],
                    CoMembers = forms["CoMembers"],
                    Address = forms["Address"],
                    City = forms["City"],
                    State = forms["State"],
                    Zip = forms["Zip"],
                    Phone = forms["Phone"],
                    Email = forms["Email"],
                    Occupation = forms["Occupation"],
                };

                bool isNewUser = forms["IsNewUser"] == "true";

                if (isNewUser)
                {
                    user.UserId = forms["UserId"];
                    user.Password = forms["Password"];
                }

                AuthenticationLib auth = new AuthenticationLib();

                try
                {
                    auth.SignupOrUpdate(user, isNewUser);
                    pageRequest.Tag = isSignupPost;
                    //issue a temp id just allow birds list update
                    Session["tempIdForBirdList"] = user.UserId;
                }
                catch (UserExistException e)
                {
                    pageRequest.Tag = user.Name;
                }
                catch (Exception e)
                {
                    pageRequest.Tag = "Signup Error: " + e.Message;
                }
            }            

            return base.handleStandardCMSPageRequest(pageRequest);
        }
    }
}