using ServiceStack.ServiceClient.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BBQ.Controllers
{
    public class bbqController : Controller
    {
        const string tmq_host = "http://localhost:82/";
        //
        // GET: /bbq/

        public ActionResult Index()
        {
            return View();
        }
        public JsonResult PxyGetBrokerConfig()
        {
            var client = new JsonServiceClient(tmq_host);
            string json = client.Get<string>("tmq/c?format=json");
            return new JsonResult() { Data = json };
        }
    }
}
