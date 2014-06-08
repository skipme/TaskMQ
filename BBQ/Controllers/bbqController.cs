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
        //
        // GET: /bbq/

        public ActionResult Index()
        {
            return View();
        }
       
    }
}
