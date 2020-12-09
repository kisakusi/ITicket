using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BackEnd.Controllers
{
    public class BackEndOrderController : Controller
    {
        // GET: BackEndOrder
        public ActionResult OrderQuery()
        {
            return View("OrderQuery", "_BackEndLayoutPage");
        }
        public ActionResult OrderList()
        {
            return View("OrderList", "_BackEndLayoutPage");
        }
    }
}