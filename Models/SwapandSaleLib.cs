using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Xml.Linq;
using Facebook;

namespace Ingeniux.Runtime.Models
{
	public enum EnumSwapSale
	{
		Swap,
		Sale
	}

	public class SwaporSaleEntry
	{
		private const string STR_YyyyMMddTHHmm = "yyyyMMddTHH:mm";
		public EnumSwapSale SwapOrSale;
		public string Name;
		public string Title;
		public string Description;
		public string Phone;
		public string Email;
		public DateTime? Date;
		public string FlickrImage;
		public string Location;

		public SwaporSaleEntry()
		{
		}

		public SwaporSaleEntry(XElement ele)
		{
			SwapOrSale = ele.GetAttributeValue("Type", "swap") == "swap" ?
				EnumSwapSale.Swap : EnumSwapSale.Sale;
			Name = ele.GetAttributeValue("Name", "");
			Title = ele.GetAttributeValue("Title", "");
			Description = ele.GetAttributeValue("Desc", "");
			Phone = ele.GetAttributeValue("Phone", "");
			Email = ele.GetAttributeValue("Email", "");
			string dateStr = ele.GetAttributeValue("Date", "");
			Date = string.IsNullOrEmpty(dateStr) ?
				null : (DateTime?) DateTime.ParseExact(dateStr, STR_YyyyMMddTHHmm, CultureInfo.CurrentCulture);
			FlickrImage = ele.GetAttributeValue("FlickrImage", "");
			Location = ele.GetAttributeValue("Location", "");
		}

		public XElement Serialize()
		{
			return new XElement("Entry",
                new XAttribute("Title", Title),
				new XAttribute("Type", SwapOrSale == EnumSwapSale.Sale ? "sale" : "swap"),
				new XAttribute("Name", Name ?? ""),
				new XAttribute("Desc", Description ?? ""),
				new XAttribute("Location", Location ?? ""),
				new XAttribute("Email", Email ?? ""),
				new XAttribute("Phone", Phone ?? ""),
				new XAttribute("Date",
					Date == null ? DateTime.Now.ToString(STR_YyyyMMddTHHmm) : Date.Value.ToString(STR_YyyyMMddTHHmm)),
				new XAttribute("FlickrImage", FlickrImage ?? ""));
		}
	}

	public class SwapandSaleLib
	{
		private static List<SwaporSaleEntry> _Entries = null;
		private static string _FilePath = String.Empty;
		static readonly object loc = new object();

		public SwaporSaleEntry[] Entries
		{
			get
			{
				return _Entries.ToArray();
			}
		}

		public SwapandSaleLib()
		{
			if (_Entries == null)
			{
				lock (loc)
				{
					_FilePath = HttpContext.Current.Server.MapPath("~/App_Data/SwaporSales.xml");
					if (_Entries == null)
					{
						XElement doc = Xtensions.SafeLoad(_FilePath);
						_Entries = doc.Elements("Entry")
							.Select(
								ele => new SwaporSaleEntry(ele))
							.ToList();								
					}
				}
			}
		}

		public void Save()
		{
			XElement root = new XElement("Sales",
				_Entries
					.Select(
						e => e.Serialize()));

			lock (loc)
			{
				root.Save(_FilePath);
			}
		}

		public SwaporSaleEntry Add(string type, string name, string title, string desc, string email, string phone, string location, string flickrImgPath)
		{
			SwaporSaleEntry entry = new SwaporSaleEntry
			{
				SwapOrSale = type == "sale" ? EnumSwapSale.Sale : EnumSwapSale.Swap,
				Name = name,
				Title = title,
				Description = desc,
				Email = email,
				Phone = phone,
				Location = location,
				FlickrImage = flickrImgPath,
				Date = DateTime.Now
			};

			_Entries.Add(entry);

			DateTime sixMonthAgo = DateTime.Today.AddMonths(-6);

			//remove entries that are 6 month old
			var oldEntries = _Entries
				.Where(
					e => e.Date == null || e.Date.Value <= sixMonthAgo)
				.ToArray();

            foreach (var oe in oldEntries)
            {
                _Entries.Remove(oe);
                //todo: archive images
            }

			Save();

			return entry;
		}

		public void PostFacebook(string msg)
		{
			//post on facebook
			FacebookClient fbc = new FacebookClient();
			//todo: obtain access token
		}
	}
}