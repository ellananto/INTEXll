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

        public IActionResult Burials(string area, int pageNum = 1)
        {
            int pageSize = 100;
            ViewBag.SelectedType = area;
            ViewBag.SelectedPage = pageNum;
            int totalNumBurials;
            if (area == "BurialMain")
            {
                totalNumBurials = context.Burialmain.Count();
            }
            else if (area == "BodyAnalysisChart")
            {
                totalNumBurials = context.Bodyanalysischart.Count();
            }
            else if (area == "Textile")
            {
                totalNumBurials = context.Textile.Count();
            }
            else if (area == "Structure")
            {
                totalNumBurials = context.StructureTextile.Count();
            }
            else if (area == "TextileFunction")
            {
                totalNumBurials = context.Textilefunction.Count();
            }
            else if (area == "TextileColor")
            {
                totalNumBurials = context.Color.Count();
            }
            else
            {
                totalNumBurials = context.Burialmain.Count();
            }

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
                    TotalNumBurials = context.Burialmain.Count(),
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
            ViewBag.SelectedPage = pageNum;
            int pageSize = 100;
            IQueryable<dynamic> bigQuery = null;

            switch (table)
            {
                case "BodyAnalysisChart":
                    var bodyAnalysisQuery = context.Bodyanalysischart.AsQueryable();

                    if (bac.EstimateStature.HasValue) { bodyAnalysisQuery = bodyAnalysisQuery.Where(x => x.Estimatestature == bac.EstimateStature); }

                    bigQuery = bodyAnalysisQuery;

                    break;

                case "Textile":
                    var textileQuery = context.Textile.AsQueryable();

                    if (!string.IsNullOrEmpty(t.Locale)) { textileQuery = textileQuery.Where(x => x.Locale == t.Locale); }

                    bigQuery = textileQuery;

                    break;

                case "Structure":
                    var structureQuery = context.Structure.AsQueryable();

                    if (!string.IsNullOrEmpty(ts.TSValue)) { structureQuery = structureQuery.Where(x => x.TSValue == ts.TSValue); }

                    bigQuery = structureQuery;

                    break;

                case "TextileFunction":
                    var functionQuery = context.Textilefunction.AsQueryable();

                    if (!string.IsNullOrEmpty(tf.Value)) { functionQuery = functionQuery.Where(x => x.Value == tf.Value); }

                    bigQuery = functionQuery;

                    break;

                case "TextileColor":
                    var colorsQuery = context.Color.AsQueryable();

                    if (!string.IsNullOrEmpty(tc.TValue)) { colorsQuery = colorsQuery.Where(x => x.Value == tc.TValue); }

                    bigQuery = colorsQuery;

                    break;


                default:

                    var burialQuery = context.Burialmain.AsQueryable();

                    if (!string.IsNullOrEmpty(bf.SquareNS)) { burialQuery = burialQuery.Where(x => x.Squarenorthsouth == bf.SquareNS); }

                    if (!string.IsNullOrEmpty(bf.NS)) { burialQuery = burialQuery.Where(x => x.Northsouth == bf.NS); }

                    if (!string.IsNullOrEmpty(bf.SquareEW)) { burialQuery = burialQuery.Where(x => x.Squareeastwest == bf.SquareEW); }

                    if (!string.IsNullOrEmpty(bf.EW)) { burialQuery = burialQuery.Where(x => x.Eastwest == bf.EW); }

                    if (!string.IsNullOrEmpty(bf.Area)) { burialQuery = burialQuery.Where(x => x.Area == bf.Area); }

                    if (bf.BurialNumber.HasValue) { burialQuery = burialQuery.Where(x => x.Burialnumber == bf.BurialNumber.Value.ToString()); }

                    if (!string.IsNullOrEmpty(bf.HeadDirection)) { burialQuery = burialQuery.Where(x => x.Headdirection == bf.HeadDirection); }

                    if (!string.IsNullOrEmpty(bf.AgeAtDeath)) { burialQuery = burialQuery.Where(x => x.Ageatdeath == bf.AgeAtDeath); }

                    if (!string.IsNullOrEmpty(bf.Sex)) { burialQuery = burialQuery.Where(x => x.Sex == bf.Sex); }

                    if (!string.IsNullOrEmpty(bf.HairColor)) { burialQuery = burialQuery.Where(x => x.Haircolor == bf.HairColor); }

                    bigQuery = burialQuery;

                    break;
            }

            var viewModel = new BurialsViewModel
            {
                Burials = bigQuery.OfType<Burialmain>().OrderBy(x => x.Id).Skip((pageNum - 1) * pageSize).Take(pageSize),
                Bodyanalysischarts = bigQuery.OfType<Bodyanalysischart>().OrderBy(x => x.Id),
                Textiles = bigQuery.OfType<Textile>().OrderBy(x => x.Id),
                Structure = bigQuery.OfType<Structure>().OrderBy(x => x.Id),
                TextileColors = bigQuery.OfType<Color>().OrderBy(x => x.Id),
                TextileFunctions = bigQuery.OfType<Textilefunction>().OrderBy(x => x.Id),
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
            if (data.HeadDirection_E == 1) { data.HeadDirection_W = 0; }
            if (data.HeadDirection_W == 2) { data.HeadDirection_W = 1; data.HeadDirection_E = 0; }
            if (data.NorthSouth_N == 2) { data.NorthSouth_N = 0; }
            if (data.EastWest_E == 1) { data.EastWest_W = 0; }
            if (data.EastWest_E == 2) { data.EastWest_W = 1; data.EastWest_E = 0; }
            if (data.AdultSubAdult_A == 1) { data.AdultSubAdult_C = 0; }
            if (data.AdultSubAdult_C == 2) { data.AdultSubAdult_C = 1; data.AdultSubAdult_A = 0; }
            if (data.SamplesCollected_True == 1) { data.SamplesCollected_False = 0; data.SamplesCollected_Other = 0; }
            if (data.SamplesCollected_True == 2) { data.SamplesCollected_True = 0; data.SamplesCollected_False = 1; data.SamplesCollected_Other = 0; }
            if (data.SamplesCollected_True == 3) { data.SamplesCollected_True = 0; data.SamplesCollected_False = 0; data.SamplesCollected_Other = 1; }
            if (data.Area_NE == 1) { data.Area_NW = 0; data.Area_SE = 0; data.Area_SW = 0; }
            if (data.Area_NE == 2) { data.Area_NE = 0; data.Area_NW = 1; data.Area_SE = 0; data.Area_SW = 0; }
            if (data.Area_NE == 3) { data.Area_NE = 0; data.Area_NW = 0; data.Area_SE = 1; data.Area_SW = 0; }
            if (data.Area_NE == 4) { data.Area_NE = 0; data.Area_NW = 0; data.Area_SE = 0; data.Area_SW = 1; }
            if (data.AgeAtDeath_A == 1) { data.AgeAtDeath_C = 0; data.AgeAtDeath_I = 0; data.AgeAtDeath_N = 0; } 
            if (data.AgeAtDeath_A == 2) { data.AgeAtDeath_A = 0; data.AgeAtDeath_C = 1; data.AgeAtDeath_I = 0; data.AgeAtDeath_N = 0; }
            if (data.AgeAtDeath_A == 3) { data.AgeAtDeath_A = 0; data.AgeAtDeath_C = 0; data.AgeAtDeath_I = 1; data.AgeAtDeath_N = 0; }
            if (data.AgeAtDeath_A == 4) { data.AgeAtDeath_A = 0; data.AgeAtDeath_C = 0; data.AgeAtDeath_I = 0; data.AgeAtDeath_N = 0; }
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
                .OrderBy(x => x.Id)
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
            var app = context.Burialmain.Single(x => x.Id == recordid);
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