using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using System.Xml.Linq;

namespace Ingeniux.Runtime.Models
{
	public class PreviewXsltTransformResult : XsltTransformResult
	{
		public Boolean InlinePreview
		{
			get;
			private set;
		}

		public PreviewXsltTransformResult(CMSPageDynamicPreviewRequest pageRequest, string sitePath, bool inlinePreview = true)
			:base (pageRequest, sitePath, false, "", false)
		{
			InlinePreview = inlinePreview;
		}

		public override void ExecuteResult(ControllerContext context)
		{
			//use the old msxml4 dom to load up the expanded content, also, don't use temp transform location for preview
			LegacyTransformationEngine transformer = new LegacyTransformationEngine(SitePath, RtSitePath, false, false);

			var response = context.HttpContext.Response;

			response.ContentType = InlinePreview ? "text/html" : "text/xml";

			string contentType;
			try
			{
				string previewXmlStylesheetPath = Path.Combine(context.HttpContext.Request.PhysicalApplicationPath,
					InlinePreview ?
                    "Content/getXmlPreview.xsl"
					:
					"Content/getPlainXmlPreview.xsl");

				string transformed = transformer.Transform(PageRequest, out contentType, previewXmlStylesheetPath);
				response.Write(transformed);
			}
			catch (Exception e)
			{
				//return formatted content with 500 error
				string errorMsg = e.Message;
# if DEBUG
				errorMsg += "\r\n" + e.StackTrace;
#endif

				XElement errorResult = new XElement("DynamicPreviewError",
					e.Message);

				response.Write(errorResult.ToString());
			}
		}
	}
}