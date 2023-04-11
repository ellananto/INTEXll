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
		private IBurialRepository repo { get; set; }
		public TypesViewComponents (IBurialRepository temp)
		{
			repo = temp;
		}
		public IViewComponentResult Invoke()
		{
			ViewBag.SelectedType = RouteData?.Values["area"];
			var types = repo.Burials
				.Select(x => x.Area)
				.Distinct()
				.OrderBy(x => x);

			return View(types);
		}
	}
}
