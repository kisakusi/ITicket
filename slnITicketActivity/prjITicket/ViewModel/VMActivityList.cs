using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PagedList;
using prjITicket.Models;

namespace prjITicket.ViewModel
{
    //Activity的ViewModel
    public class VMActivityList
    {
        public List<Categories> Categories { get; set; }
        public List<Activity> ScrollImgActivities { get; set; }
        public List<SubCategories> HotSubCategories { get; set; }
        public int MaxPriceAll { get; set; }
    }
}