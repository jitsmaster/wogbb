using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace Ingeniux.Runtime.Models
{
	public class DesignTimeNotAuthenticatedResult : XmlResult
	{
		public DesignTimeNotAuthenticatedResult()
			: base(new XElement("NotAuthenticated",
				"Cannot access DSS preview site without parent CMS site authenticated"))
		{
		}

		public override void ExecuteResult(System.Web.Mvc.ControllerContext context)
		{
			//present 401 status code for not authenticated
			context.HttpContext.Response.StatusCode = 403;
			base.ExecuteResult(context);
		}
	}
}