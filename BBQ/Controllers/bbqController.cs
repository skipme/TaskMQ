using ServiceStack.ServiceClient.Web;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
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
        public JsonResult PxySet(string data, string urlpostfix = null)
        {
            string urlbase = tmq_host + "tmq/c" + urlpostfix ?? "";

            var client = WebRequest.Create(urlbase + "?format=json");
            client.Method = "POST";
            client.ContentType = "application/json";
            using (var writer = new StreamWriter(client.GetRequestStream()))
            {
                writer.Write(data);
            }
            var json = new StreamReader(client.GetResponse().GetResponseStream()).ReadToEnd();
            return new JsonResult() { Data = json };
        }
    }
}
