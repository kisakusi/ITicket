using PagedList;
using prjITicket.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace prjITicket.Controllers
{
    
    public class SellerCenterController : Controller
    {
        TicketSysEntities db = new TicketSysEntities();
        // GET: SellerCenter
        public ActionResult ManagementCenter()
        {

            int memberid = (Session[CDictionary.SK_Logined_Member] as Member).MemberID;
            var sellerid=db.Seller.Where(x => x.MemberId == memberid).FirstOrDefault();

            if (sellerid.fPass != true)
            {
                return RedirectToAction("ActivityList", "Activity");
            }

            //var list = db.Activity.Where(s => s.SellerID == sellerid.SellerID);
            return View(/*list*/);
        }

        public ActionResult GetQueryResult(string txtQuery="",int page=1)
        {
            //-------
            int pagesize = 6;
            int pagecurrent = page < 1 ? 1 : page;
            //------------------
            ViewBag.Keyword = txtQuery;
            int memberid = (Session[CDictionary.SK_Logined_Member] as Member).MemberID;
            var sellerid = db.Seller.Where(x => x.MemberId == memberid).FirstOrDefault();
            if (sellerid.fPass != true)
            {
                return RedirectToAction("ActivityList", "Activity");
            }
            IQueryable<Activity> list = null;
            if (txtQuery != "")
            {
               list = db.Activity.Where(s => s.SellerID == sellerid.SellerID&&s.ActivityName.Contains(txtQuery));
            }
            else
            {
                list= db.Activity.Where(s => s.SellerID == sellerid.SellerID);
            }
            var pagedlist = list.ToList().ToPagedList(pagecurrent, pagesize);

            return PartialView(pagedlist);
        }
























        public ActionResult GetUploadPage()
        {
            return PartialView("GetUploadPage");
        }
        public ActionResult GetActivityListPage()
        {
            int memberid = (Session[CDictionary.SK_Logined_Member] as Member).MemberID;
            var sellerid = db.Seller.Where(x => x.MemberId == memberid).FirstOrDefault();

            if (sellerid.fPass != true)
            {
                return RedirectToAction("ActivityList", "Activity");
            }

            var list = db.Activity.Where(s => s.SellerID == sellerid.SellerID);
            return PartialView("ManagementCenter",list);
        }

    }
}