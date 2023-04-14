using INTEXll.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace INTEXll.Components
{
    public class TypesViewComponents : ViewComponent
    {
		// bring in instance of the context file
		private burialContext context { get; set; }
		public TypesViewComponents (burialContext temp)
		{
			context = temp;
		}
		public IViewComponentResult Invoke()
		{
			ViewBag.SelectedType = RouteData?.Values["area"];
			var types = context.Burialmain
				.Select(x => x.Area)
				.Distinct()
				.OrderBy(x => x);

			return View(types);
		}
	}
}
