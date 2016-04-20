using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;
using Ingeniux.Runtime;

namespace Ingeniux.Runtime.Models
{
	public class XmlResult : ActionResult
	{
		public XDocument ResultDocument
		{
			get;
			set;
		}

		public string ContentType
		{
			get;
			private set;
		}

		public XmlResult(XDocument resultDoc, string overrideContentType = "text/xml")
		{
			ResultDocument = resultDoc;
			ContentType = overrideContentType.ToNullOrEmptyHelper()
				.Return("text/xml");
		}

		public XmlResult(XElement resultEle, string overrideContentType = "text/xml")
		{
			ResultDocument = new XDocument(resultEle);
			ContentType = overrideContentType.ToNullOrEmptyHelper()
				.Return("text/xml");
		}

		public override void ExecuteResult(ControllerContext context)
		{
			context.HttpContext.Response.ContentType = ContentType;
			if (ResultDocument == null || ResultDocument.Root == null)
				context.HttpContext.Response.Write(string.Empty);
			else
				ResultDocument.ToNullHelper().Branch(
					doc => doc.Save(context.HttpContext.Response.Output, SaveOptions.None),
					() => { /*if doc doesn't exist, does nothing*/ });
		}
	}
}