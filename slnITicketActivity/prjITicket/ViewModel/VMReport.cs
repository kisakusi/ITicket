using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using prjITicket.Models;

namespace prjITicket.ViewModel
{
    public class VMReport
    {
        public Article Article { get; set; }
        public List<Report> Report { get; set; }
        
    }
    public class VMforum_mainblock
    {
        public List<ArticleCategories> ArticleCategories { get; set; }
        public List<Article> Article { get; set; }
        public string searchWord { get; set; }
        public int page { get; set; }
        public int ArticleCategoryID { get; set; }
    }

}