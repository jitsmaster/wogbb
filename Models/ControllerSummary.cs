using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Ingeniux.Runtime
{
	public class ControllerSummary
	{
		public string Name {get; private set; }
		public string[] ActionNames { get; private set; }

		public ControllerSummary(Type controllerType)
		{
			Name = controllerType.Name;

			//get all methods that are publish and returns ActionResult
			ActionNames = controllerType.GetMethods(/*System.Reflection.BindingFlags.Public*/)
				.Where(
					m => typeof(ActionResult).IsAssignableFrom(m.ReturnType))
				.Select(
					m => m.Name)
				.ToArray();
		}
	}
}