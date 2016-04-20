using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Ingeniux.Runtime.Models
{
	public class Error404 : ICMSEnvironment
	{
		public string Title { get; set; }
		public string BodyCopy { get; set; }

		#region ICMSEnvironment Members

		public HttpCookieCollection Cookies
		{
			get { return new HttpCookieCollection(); }
		}

		public System.Collections.Specialized.NameValueCollection Form
		{
			get { return new System.Collections.Specialized.NameValueCollection(); }
		}

		public System.Collections.Specialized.NameValueCollection QueryString
		{
			get { return new System.Collections.Specialized.NameValueCollection(); }
		}

		public System.Collections.Specialized.NameValueCollection ServerVariables
		{
			get { return new System.Collections.Specialized.NameValueCollection(); }
		}

		public string RequestPath
		{
			get { return ""; }
		}

		public string URL
		{
			get { return ""; }
		}

		public Site Site
		{
			get { return null; }
		}

		public UserAgent UserAgent
		{
			get { return null; }
		}

		public bool EditMode
		{
			get { return false; }
		}

		public bool IsPreview
		{
			get { return false; }
		}

		public string ViewMode
		{
			get;
			set;
		}

		#endregion

		#region ICMSEnvironment Members


		public ICMSPageFactory Factory
		{
			get;
			internal set;
		}

		public bool IncludeAllPages
		{
			get { return true; }
		}

		public TransformOptions TransformOption
		{
			get { return TransformOptions.Default; }
		}

		public string CurrentPublishingTargetID
		{
			get { return string.Empty; }
		}

		#endregion
	}
}