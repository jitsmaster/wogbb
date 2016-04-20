using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web;
using System.Web.Script.Serialization;

namespace Ingeniux.Runtime
{
	public class CommentFilteredJsonResult : JsonResult
	{
		public override void ExecuteResult(ControllerContext context)
		{
			if (context == null)
			{
				throw new ArgumentNullException("context");
			}

			HttpResponseBase response = context.HttpContext.Response;

			if (!String.IsNullOrEmpty(ContentType))
			{
				response.ContentType = ContentType;
			}
			else
			{
				response.ContentType = "text/json";
			}
			if (ContentEncoding != null)
			{
				response.ContentEncoding = ContentEncoding;
			}
			if (Data != null)
			{
				JavaScriptSerializer serializer = new JavaScriptSerializer();
				response.Write("/*" + serializer.Serialize(Data) + "*/");
			}
		}
	}
}
