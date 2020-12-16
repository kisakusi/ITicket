using prjITicket.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace prjITicket.ViewModel.BackEnd
{
    public class CBackEndMain
    {
        public int YesterdayOrderCount { get; set; }
        public int BeforeYesterdayOrderCount { get; set; }

        public int YesterDayTotalPrice { get; set; }
        public int BeforeYesterDayTotalPrice { get; set; }

        public int YesterdayMember { get; set; }

    }
}