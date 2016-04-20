using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;
using System.IO;

namespace Ingeniux.Runtime.Models
{
	public class XsltTransformResult : ActionResult
	{
		public CMSPageRequest PageRequest { get; protected set; }
		public string SitePath {get; protected set;}
		public string RtSitePath { get; protected set; }

		public bool UseTempXsltLocation = true;
		public bool LegacyRenderingEngine = true;
		public bool InContextEditMode = false;

		public XsltTransformResult(CMSPageRequest pageRequest, string sitePath, bool useTempXsltLocation, string rtSitePath = "", bool legacyRenderingEngine = true)
		{
			PageRequest = pageRequest;
			SitePath = sitePath;
			RtSitePath = rtSitePath.ToNullHelper().Return(sitePath);
			LegacyRenderingEngine = legacyRenderingEngine;
			InContextEditMode = pageRequest.EditMode;
			UseTempXsltLocation = useTempXsltLocation;
		}

		public override void ExecuteResult(ControllerContext context)
		{
			//use the old msxml4 dom to load up the expanded content
			DynamicTransformationEngine transformer = LegacyRenderingEngine ?
				new LegacyTransformationEngine(SitePath, RtSitePath, InContextEditMode, UseTempXsltLocation) :
				new DynamicTransformationEngine(SitePath, RtSitePath, InContextEditMode, UseTempXsltLocation);

			var response = context.HttpContext.Response;

			string contentType;
			try
			{
				string transformed = transformer.Transform(PageRequest, out contentType);
				response.ContentType = contentType;

				response.Write(transformed);
			}
			catch (Exception e)
			{
				throw new InvalidOperationException("Error occurred during XSLT transformation: " + e.Message, e);
			}
		}
	}
}