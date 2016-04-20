using Ingeniux.Runtime.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Ingeniux.Runtime.Controllers
{
    public class BirdsModel
    {
        public HashSet<string> Birds = new HashSet<string>();
        public string OtherBirds = string.Empty;
        public bool IsTempUser = false;
    }

    public class MemberBirdListController : CMSPageDefaultController
    {
        internal override System.Web.Mvc.ActionResult handleStandardCMSPageRequest(CMSPageRequest pageRequest)
        {
            var auth = new AuthenticationLib();

            bool isTempUser = false;

            UserInfo user = Session["user"] as UserInfo;
            if (user == null)
            {
                string tempUserId = Session["tempIdForBirdList"] as string;
                if (!string.IsNullOrWhiteSpace(tempUserId))
                {
                    user = auth.User(tempUserId);
                    isTempUser = true;
                }
            }

            if (user != null && pageRequest.Form["posting"] == "true")
            {
                var selectedBirdIds = pageRequest.Form.GetValues("bird");
                user.Birds = selectedBirdIds
                    .Select(
                        b => new BirdInfo
                        {
                            XID = b
                        })
                    .ToList();

                user.OtherBirds = pageRequest.Form["otherBirds"];
                lock (auth.SyncRoot)
                    auth.Save();
            }

            HashSet<string> birdIds = user != null ? user.Birds
                    .Select(
                        b => b.XID)
                    .ToHashSet() : new HashSet<string>();

            pageRequest.Tag = new BirdsModel
            {
                Birds = birdIds,
                OtherBirds = user
                    .ToNullHelper()
                    .Propagate(
                        u => u.OtherBirds)
                    .Return(""),
                IsTempUser = isTempUser
            };
            
            return base.handleStandardCMSPageRequest(pageRequest);
        }
    }
}