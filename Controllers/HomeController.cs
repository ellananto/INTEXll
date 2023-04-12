using INTEXll.Models;
using INTEXll.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using INTEXll.Data;
using Microsoft.ML.Transforms;
using Microsoft.ML.Transforms.Onnx;
using Microsoft.ML;
using System.IO;
using Microsoft.ML.Data;
using System.Net.Http;
using Newtonsoft.Json;
using System.Text;

namespace INTEXll.Controllers
{
    public class HomeController : Controller
    {
        private burialContext context { get; set; }
        private ApplicationDbContext identityContext { get; set; }
        public HomeController(burialContext temp, ApplicationDbContext identity)
        {
            context = temp;
            identityContext = identity;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }
        [HttpGet]

        public IActionResult Burials(string area, string filter, int pageNum = 1)
        {
            int pageSize = 100;
            ViewBag.SelectedType = area;
            ViewBag.SelectedFilter = filter;
            var x = new BurialsViewModel
            {
                Burials = context.Burialmain
                .Skip((pageNum - 1) * pageSize)
                .Take(pageSize),
                Textiles = context.Textile
                .Skip((pageNum - 1) * pageSize)
                .Take(pageSize),
                Bodyanalysischarts = context.Bodyanalysischart
                .Skip((pageNum - 1) * pageSize)
                .Take(pageSize),
                Structure = context.Structure
                .Skip((pageNum - 1) * pageSize)
                .Take(pageSize),
                TextileColors = context.Color
                .Skip((pageNum - 1) * pageSize)
                .Take(pageSize),
                TextileFunctions = context.Textilefunction
                .Skip((pageNum - 1) * pageSize)
                .Take(pageSize),

                PageInfo = new PageInfo
                {
                    TotalNumBurials = (area == null ? context.Burialmain.Count() : context.Burialmain.Where(x => x.Area == area).Count()),
                    BurialsPerPage = pageSize,
                    CurrentPage = pageNum
                }
            };
            return View(x);
        }

        [HttpPost]
        public IActionResult Burials(TFForm tf, BFiltersForm bf, TSForm ts, TextileForm t, TCForm tc, BAC bac, string table, int pageNum = 1)
        {
            ViewBag.SelectedType = table;
            int pageSize = 100;
            var Burialquery = context.Burialmain.AsQueryable();

            if (!string.IsNullOrEmpty(bf.SquareNS)) { Burialquery = Burialquery.Where(x => x.Squarenorthsouth == bf.SquareNS); }

            if (!string.IsNullOrEmpty(bf.NS)) { Burialquery = Burialquery.Where(x => x.Northsouth == bf.NS); }

            if (!string.IsNullOrEmpty(bf.SquareEW)) { Burialquery = Burialquery.Where(x => x.Squareeastwest == bf.SquareEW); }

            if (!string.IsNullOrEmpty(bf.EW)) { Burialquery = Burialquery.Where(x => x.Eastwest == bf.EW); }

            if (!string.IsNullOrEmpty(bf.Area)) { Burialquery = Burialquery.Where(x => x.Area == bf.Area); }

            if (bf.BurialNumber.HasValue) { Burialquery = Burialquery.Where(x => x.Burialnumber == bf.BurialNumber.Value.ToString()); }

            if (!string.IsNullOrEmpty(bf.HeadDirection)) { Burialquery = Burialquery.Where(x => x.Headdirection == bf.HeadDirection); }

            if (!string.IsNullOrEmpty(bf.AgeAtDeath)) { Burialquery = Burialquery.Where(x => x.Ageatdeath == bf.AgeAtDeath); }

            if (!string.IsNullOrEmpty(bf.Sex)) { Burialquery = Burialquery.Where(x => x.Sex == bf.Sex); }

            if (!string.IsNullOrEmpty(bf.HairColor)) { Burialquery = Burialquery.Where(x => x.Haircolor == bf.HairColor); }


            var viewModel = new BurialsViewModel
            {
                Burials = Burialquery.OrderBy(x => x.Id),
                PageInfo = new PageInfo
                {
                    TotalNumBurials = context.Burialmain.Count() == null ? 0 : context.Burialmain.Count(),
                    BurialsPerPage = pageSize,
                    CurrentPage = pageNum
                }
            };

            return View("Burials", viewModel);
        }

        [HttpGet]
        public IActionResult Supervised()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Supervised(HousingData data)
        {
            using (var client = new HttpClient())
            {
                var uri = new Uri("https://localhost:44339/score");

                var json = JsonConvert.SerializeObject(data);

                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(uri, content);

                var result = await response.Content.ReadAsStringAsync();

                var prediction = JsonConvert.DeserializeObject<Prediction>(result);

                ViewBag.Prediction = prediction;

                return View();

            }
        }

        [Authorize(Roles = "Admin")]
        public IActionResult BurialsAdmin(int pageNum = 1)
        {
            int pageSize = 100;

            var x = new BurialsViewModel
            {
                Burials = context.Burialmain
                .Skip((pageNum - 1) * pageSize)
                .Take(pageSize),

                PageInfo = new PageInfo
                {
                    TotalNumBurials = context.Burialmain.Count(),
                    BurialsPerPage = pageSize,
                    CurrentPage = pageNum
                }
            };
            return View(x);
        }



        public IActionResult Admin()
        {
            var userInfo = identityContext.Users.OrderBy(x => x.Id).ToList();
            return View(userInfo);
        }

        [HttpGet]
        public IActionResult EditRecordForm(long recordid)
        {
            ViewBag.Burialmain = context.Burialmain.ToList();
            // variable with sql statement to get the right record with passed applicationid 
            var app = context.Burialmain.Single(x => x.Id == recordid);
            // the ", application" is what will fill the form with values
            return View(app);
        }
        [HttpPost]
        public IActionResult EditRecordForm(Burialmain burialmain)
        {
            context.Update(burialmain);
            context.SaveChanges();
            return RedirectToAction("BurialsAdmin");
        }

        [HttpGet]
        public IActionResult DeleteRecordForm(long deleteid)
        {
            var app = context.Burialmain.Single(x => x.Id == deleteid);
            return View(app);
        }

        [HttpPost]
        public IActionResult DeleteRecordForm(Burialmain burialmain)
        {
            context.Burialmain.Remove(burialmain);
            context.SaveChanges();
            return RedirectToAction("Burials");

        }

        [HttpGet]
        public IActionResult MoreInfo(long id)
        {
            Burialmain burialinfo = context.Burialmain.Where(b => b.Id == id).FirstOrDefault();
            return PartialView("_SeeMore", burialinfo);
        }

        [HttpGet]
        public IActionResult AddRecord()
        {
            return View();
        }
        [HttpPost]
        public IActionResult AddRecord(Burialmain burialmain)
        {
            context.Burialmain.Add(burialmain);
            context.SaveChanges();
            return RedirectToAction("BurialsAdmin");
        }
        //ViewBag.Burialmain = context.Burialmain.ToList();
        // variable with sql statement to get the right record with passed applicationid 
        //var app = context.Burialmain.Single(x => x.Id == recordid);
        // the ", application" is what will fill the form with values
        public IActionResult Unsupervised()
        {
            return View();
        }
    }
}