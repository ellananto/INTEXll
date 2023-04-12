using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using INTEXll.Models;

namespace INTEXll.Controllers
{
    [ApiController]
    [Route("/score")]
    public class PredController : Controller
    {
        private InferenceSession _session;

        public PredController(InferenceSession session)
        {
            _session = session;
        }

        [HttpPost]
        public ActionResult Score(HousingData data)
        {
            var result = _session.Run(new List<NamedOnnxValue>
            {
                NamedOnnxValue.CreateFromTensor("float_input", data.AsTensor())
            });
            Tensor<string> score = result.First().AsTensor<string>();
            var prediction = new Prediction { PredictedValue = score.First().ToString() };
            result.Dispose();
            return Ok(prediction);
        }
    }
}
