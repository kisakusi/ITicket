using prjITicket.Models;
using prjITicket.ViewModel.BackEnd;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace prjITicket.Controllers
{
    public class BackEndMainController : Controller
    {

        // GET: BackEndMain
        public ActionResult BackEndIndex()
        {
            TicketSysEntities ticket = new TicketSysEntities();

            if (Session[CDictionary.SK_Logined_Member] == null ||
              (Session[CDictionary.SK_Logined_Member] as Member).MemberRoleId != 4)
            {
                return RedirectToAction("Index", "Home");
            }

            CBackEndMain cBackEndMain = new CBackEndMain();
            DateTime t = DateTime.Today;
            DateTime yesterday = DateTime.Today.AddDays(-1);
            DateTime Beforeyesterday = DateTime.Today.AddDays(-2);


            int y = ticket.Orders.Count(o => yesterday < o.OrderDate && o.OrderDate < t);
            cBackEndMain.YesterdayOrderCount = y;

            int by = ticket.Orders.Count(o => Beforeyesterday < o.OrderDate && o.OrderDate < yesterday);
            cBackEndMain.BeforeYesterdayOrderCount = by;

            List<Order_Detail> YesterdayOrderdetail = ticket.Order_Detail.Where(o => yesterday < o.Orders.OrderDate && o.Orders.OrderDate < t).ToList();
            if (YesterdayOrderdetail.Count > 0)
            {
                int yTotalPrice = YesterdayOrderdetail.Sum(o => o.Quantity * o.Tickets.Price);
                cBackEndMain.YesterDayTotalPrice = yTotalPrice;

            }
            else
            {
                cBackEndMain.YesterDayTotalPrice = 0;
            }

            List<Order_Detail> BeforeYesterdayOrderdetail = ticket.Order_Detail.Where(o => Beforeyesterday < o.Orders.OrderDate && o.Orders.OrderDate < yesterday).ToList();
            if (BeforeYesterdayOrderdetail.Count > 0)
            {
                int byTotalPrice = BeforeYesterdayOrderdetail.Sum(o => o.Quantity * o.Tickets.Price);
                cBackEndMain.BeforeYesterDayTotalPrice = byTotalPrice;

            }
            else
            {
                cBackEndMain.BeforeYesterDayTotalPrice = 0;
            }

            int yMember = ticket.Member.Count(m => yesterday < m.fRegister_time && m.fRegister_time < t);
            cBackEndMain.YesterdayMember = yMember;


            return View(cBackEndMain);
        }
    }
}