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
        private IBurialRepository repo;
        private ApplicationDbContext identityContext { get; set; }
        public HomeController(IBurialRepository temp, ApplicationDbContext identity)
        {
            repo = temp;
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
                Burials = repo.Burials
                .Where(p => p.Area == area || area == null)
                .Skip((pageNum - 1) * pageSize)
                .Take(pageSize),

                PageInfo = new PageInfo
                {
                    TotalNumBurials = (area == null ? repo.Burials.Count() : repo.Burials.Where(x => x.Area == area).Count()),
                    BurialsPerPage = pageSize,
                    CurrentPage = pageNum
                }
            };
            return View(x);
        }
        [HttpGet]
        public IActionResult MoreInfo(long ellaid)
        {
            var info = repo.Burials.FirstOrDefault(x => x.Id == ellaid);
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
                Burials = repo.Burials
                .Skip((pageNum - 1) * pageSize)
                .Take(pageSize),

                PageInfo = new PageInfo
                {
                    TotalNumBurials = repo.Burials.Count(),
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
        }
    }
}