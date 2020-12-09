using BackEnd.ViewModel;
using prjITicket.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BackEnd.Controllers
{
    public class BackEndActivityController : Controller
    {
        // GET: BackEndActivity
        public ActionResult ActivityMaintain()
        {
           
            return View("ActivityMaintain", "_BackEndLayoutPage");
        }

    

        public ActionResult ActivityDetail(int id)
        {
            CBackEndActivity backEndActivities = null;


            TicketSysEntities ticket = new TicketSysEntities();
            backEndActivities = (
             from t in ticket.Activity
             from s in ticket.Seller
             from Status in ticket.ActivityStatus

           where t.SellerID == s.SellerID && t.ActivityStatusID == Status.ActivityStatusID
           && t.ActivityID == id
             select new CBackEndActivity { ActivityEntity = t, Seller = s, ActivityStatus = Status }
           ).FirstOrDefault();
            return View(backEndActivities);
        }
    }
}