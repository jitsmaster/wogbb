using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace Ingeniux.Runtime.Models
{
    public class UserExistException :Exception
    {
        public UserExistException(string msg)
            : base(msg)
        {

        }
    }


	public class UserInfo
	{
        [Required]
		public string UserId;
        [Required]
        [Display(Name="Name")]
		public string Name;
		public string Password;
        
        [Display(Name="Co-Members")]
        public string CoMembers;

        [Required]
        [Display(Name="Address")]
        public string Address;

        [Required]
        [Display(Name = "City")]
        public string City;

        [Required]
        [Display(Name = "State")]
        public string State;

        [Required]
        [Display(Name = "Zip")]
        public string Zip;

        [Required]
        [Display(Name = "Phone")]
        public string Phone;
        public string Email;
        public string Occupation;
        public bool Paid = false;
        public List<BirdInfo> Birds = new List<BirdInfo>();
        public string OtherBirds = string.Empty;
	}

    public class BirdInfo
    {
        public string XID;
        public string Species;
        public string Family;
        public string Name;
    }

	public class LoginModel
	{
		public string RedirectUrl;
		public string Error;
		public string UserName;
	}

	public class AuthenticationLib
	{
		static Dictionary<string, UserInfo> _Accounts = null;
		public static readonly object loc = new object();

		public AuthenticationLib()
		{
			if (_Accounts == null)
			{
				lock (loc)
				{
					if (_Accounts == null)
					{
						string filePath = HttpContext.Current.Server.MapPath("~/App_Data/accounts.xml");
						if (!File.Exists(filePath))
							_Accounts = new Dictionary<string, UserInfo>();
						else
						{
							XElement doc = Xtensions.SafeLoad(filePath);
							_Accounts = doc.Elements("a")
								.Select(
									a => new UserInfo
									{
										Name = a.GetAttributeValue("fname", string.Empty),
										UserId = a.GetAttributeValue("name", string.Empty).ToLowerInvariant(),
										Password = a.GetAttributeValue("password", string.Empty),
                                        CoMembers = a.GetAttributeValue("coMembers", string.Empty),
                                        Address = a.GetAttributeValue("addr", ""),
                                        City = a.GetAttributeValue("city", ""),
                                        State = a.GetAttributeValue("state", ""),
                                        Zip = a.GetAttributeValue("zip", ""),
                                        Phone = a.GetAttributeValue("phone", ""),
                                        Email = a.GetAttributeValue("email", ""),
                                        Occupation = a.GetAttributeValue("job", ""),
                                        Paid = a.GetAttributeValue("paid", "false") == "true",
                                        OtherBirds = a.GetAttributeValue("otherbirds", ""),
                                        Birds = a.Element("Birds")
                                            .ToNullHelper()
                                            .Propagate(
                                                bs => bs.Elements("bird")
                                                    .Select(
                                                        b => new BirdInfo{
                                                            Name = b.GetAttributeValue("name", ""),
                                                            Species = b.GetAttributeValue("specie", ""),
                                                            Family = b.GetAttributeValue("family", ""),
                                                            XID = b.GetAttributeValue("xid", "")
                                                        })
                                                    .ToList())
                                            .Return(new List<BirdInfo>())
									})
								.Where(
									a => !string.IsNullOrWhiteSpace(a.UserId))
								.Distinct(
									new GenericEqualityComparer<UserInfo, string>(
										u => u.UserId))
								.ToDictionary(
									u => u.UserId);
						}
					}
				}
			}
		}

        public object SyncRoot { get { return loc; } }

		public UserInfo Authenticate(string userName, string password)
		{
			userName = userName.ToLowerInvariant().Trim();
			if (!_Accounts.ContainsKey(userName))
				return null;

			UserInfo user = _Accounts[userName];
			if (user.Password != password)
				return null;

			return user;
		}

        public UserInfo User(string userId)
        {
            return _Accounts.ContainsKey(userId) ?
                _Accounts[userId] : null;
        }

        public IEnumerable<UserInfo> Users()
        {
            return _Accounts.Values;
        }

        public void SignupOrUpdate(UserInfo user, bool create)
        {
            string userId = user.UserId;

            if (create)
            {
               if (_Accounts.ContainsKey(userId))
                   throw new UserExistException(string.Format("Account for user '{0}' already exists. Please login", user.Name));
            }

            userId = user.UserId
                .ToNullOrEmptyHelper()
                .Return(user.Email);

            user.UserId = userId;

            if (_Accounts.ContainsKey(userId))
            {
                user.Password = _Accounts[userId].Password;
            }

            lock (loc)
            {
                _Accounts[userId] = user;
                Save();
            }
        }

        public void Save()
        {
            //save
            string filePath = HttpContext.Current.Server.MapPath("~/App_Data/accounts.xml");
            XElement accountDoc = new XElement("Accounts",
                _Accounts.Values
                    .Select(
                        a => new XElement("a",
                            new XAttribute("fname", a.Name ?? ""),
                            new XAttribute("name", a.UserId ?? ""),
                            new XAttribute("password", a.Password ?? "letmein"),
                            new XAttribute("coMembers", a.CoMembers ?? ""),
                            new XAttribute("addr", a.Address ?? ""),
                            new XAttribute("city", a.City ?? ""),
                            new XAttribute("state", a.State ?? ""),
                            new XAttribute("zip", a.Zip ?? ""),
                            new XAttribute("phone", a.Phone ?? ""),
                            new XAttribute("email", a.Email ?? ""),
                            new XAttribute("job", a.Occupation ?? ""),
                            new XAttribute("paid", a.Paid ? "true" : "false"),
                            new XAttribute("otherbirds", a.OtherBirds ?? ""),
                            new XElement("Birds",
                            a.Birds.Select(
                                b => new XElement("bird", new XAttribute("xid", b.XID))
                            )))));
            accountDoc.Save(filePath, SaveOptions.None);
        }
	}
}