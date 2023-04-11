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

namespace INTEXll.Controllers
{
    public class HomeController : Controller
    {
        private burialContext context {get;set;}
        private ApplicationDbContext identityContext { get; set; }
        public HomeController(burialContext temp, ApplicationDbContext identity)
        {
            context
                = temp;
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

        public IActionResult Burials(string area, int pageNum = 1)
        {
            int pageSize = 100;

            var x = new BurialsViewModel
            {
                Burials = context.Burialmain
                .Where(p => p.Area == area || area == null)
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
        [HttpGet]
        public IActionResult MoreInfo(long ellaid)
        {
            var info = context.Burialmain.FirstOrDefault(x => x.Id == ellaid);
            return View(info);
        }

        public IActionResult Supervised()
        {
            return View();
        }

        [Authorize]
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
        public IActionResult Users()
        {
            var userInfo = identityContext.Users.OrderBy(x => x.Id).ToList();
            return View(userInfo);
        }


        //public IActionResult EditRecordForm(long recordid)
        //{
        //    var x = new BurialsViewModel
        //    {
        //        Burials = repo.Burials
        //       .Where(x => x.Id == recordid),
        //    };
        //    return View(x);
        //}

        public class SupervisedController : Controller
        {
            private readonly MLContext _mlContext;
            private readonly ITransformer _model;
            private readonly PredictionEngine<SupervisedData, SupervisedPrediction> _engine;

            public SupervisedController()
            {
                _mlContext = new MLContext();

                // Load the ONNX model
                var modelPath = Path.Combine(Directory.GetCurrentDirectory(), "Models", "model.onnx");
                _model = _mlContext.Model.Load(modelPath, out var schema);

                // Create a prediction engine
                _engine = _mlContext.Model.CreatePredictionEngine<SupervisedData, SupervisedPrediction>(_model);
            }

            [HttpGet]
            public IActionResult Index()
            {
                return View();
            }

            [HttpPost]
            public IActionResult Index(SupervisedData input)
            {
                // Make a prediction using the ONNX model
                var output = _engine.Predict(input);

                // Pass the prediction result to the view
                ViewData["output"] = output.Prediction;

                return View();
            }
        }

        public class SupervisedData
        {
            [ColumnName("input")]
            public float Input { get; set; }
        }

        public class SupervisedPrediction
        {
            [ColumnName("output")]
            public float Prediction { get; set; }

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
    }
}