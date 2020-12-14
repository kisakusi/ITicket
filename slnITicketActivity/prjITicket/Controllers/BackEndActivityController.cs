﻿using BackEnd.ViewModel;
using prjITicket.Models;
using prjITicket.ViewModel;
using prjITicket.ViewModel.BackEnd;
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
            CBackEndActivityDetailModel cBackEndActivityDetailModel = new CBackEndActivityDetailModel();

            TicketSysEntities ticket = new TicketSysEntities();

            cBackEndActivityDetailModel.Detail = (
             from t in ticket.Activity
             from s in ticket.Seller
             from Status in ticket.ActivityStatus
             join tickettime in ticket.TicketTimes on t.ActivityID equals tickettime.ActivityId into tickettimeNull
             from tickettime in tickettimeNull.DefaultIfEmpty()

             where t.SellerID == s.SellerID
             && t.ActivityStatusID == Status.ActivityStatusID
             && t.ActivityID == id
             select new CBackEndActivityDetail
             {
                 ActivityEntity = t,
                 Seller = s,
                 ActivityStatus = Status,
                 TicketTimes = tickettime
             }
           ).FirstOrDefault();


            cBackEndActivityDetailModel.FailedReason =
                ticket.ActivityFailedReason
                .Select(f => new CBackEndActivityReason
                {
                    failedReason = f
                }).ToList();


            cBackEndActivityDetailModel.ActivityTimes =
                ticket.TicketTimes
                .Where(t => t.ActivityId.Equals(id))
                .Select(t => new CBackEndActivityTimes
                {
                    TicketTimes = t
                })
                .ToList();

            return View(cBackEndActivityDetailModel);
        }
    }
}