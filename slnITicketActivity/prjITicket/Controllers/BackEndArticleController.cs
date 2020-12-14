using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using prjITicket.Models;

namespace prjITicket.Controllers
{
    public class BackEndArticleController : Controller, IDisposable
    {
        TicketSysEntities db = new TicketSysEntities();

        public ActionResult ArticleList()
        {
            if (Session[CDictionary.SK_Logined_Member] == null ||
                (Session[CDictionary.SK_Logined_Member] as Member).MemberRoleId != 4)
            {
                return RedirectToAction("Index", "Home");
            }
            return View(db.ArticleCategories);
        }

        [HttpPost]
        public JsonResult ArticleList(ArticleAjax m)
        {
            int skip = m.PageSize * (m.PageCurrent - 1);
            int take = m.PageSize;
            IEnumerable<Article> query = ArticleCRUD.ArticleQuery(m.AuthorSearch, m.Cate, m.Date, m.Report, m.Keyword);
            int count = query.Count();
            if (count == 0)
            {
                return Json(new { });
            }
            int changepage = 0;
            if (count <= skip)
            {
                changepage = (int)Math.Ceiling((decimal)count / take);
                skip = take * (changepage - 1);
            }
            IEnumerable<Article> articles = query.SkipTake(skip, take);
            IEnumerable<ArticleJson> jsons = articles.Select(article => new ArticleJson
            {
                Count = count,
                ChangePage = changepage,
                article = article,
                reports = db.Article_Report
            });
            return Json(jsons);
        }

        [HttpPost]
        public JsonResult ReplyList(ArticleAjax m)
        {
            int skip = m.PageSize * (m.PageCurrent - 1);
            int take = m.PageSize;
            IEnumerable<Reply> query = ArticleCRUD.ReplyQuery(m.AuthorSearch, m.Cate, m.Date, m.Report, m.Keyword);
            int count = query.Count();
            if (count == 0)
            {
                return Json(new { });
            }
            int changepage = 0;
            if (count <= skip)
            {
                changepage = (int)Math.Ceiling((decimal)count / take);
                skip = take * (changepage - 1);
            }
            IEnumerable<Reply> replies = query.SkipTake(skip, take);
            IEnumerable<ReplyJson> jsons = replies.Select(reply => new ReplyJson
            {
                Count = count,
                ChangePage = changepage,
                reply = reply,
                reports = db.Reply_Report
            });
            return Json(jsons);
        }

        [HttpPost]
        public JsonResult ArticleDetail(int id)
        {
            List<object> jsons = new List<object>();
            jsons.Add(new
            {
                ArticleId = id,
                ArticleTitle = db.Article.FirstOrDefault(x => x.ArticleID == id).ArticleTitle,
                Content = db.Article.FirstOrDefault(x => x.ArticleID == id).ArticleContent.Replace('\"', '\''),
                MemberId = db.Article.FirstOrDefault(x => x.ArticleID == id).MemberID,
                MemberEmail = db.Article.FirstOrDefault(x => x.ArticleID == id).Member.Email,
                ARID = id
            });
            foreach (var item in db.Article_Report.Where(x => x.ArticleId == id).OrderByDescending(x => x.Article_ReportId))
            {
                jsons.Add(new
                {
                    Member = item.Member.NickName,
                    Reason = item.Report.ReportReason
                });
            }
            return Json(jsons);
        }

        [HttpPost]
        public JsonResult ReplyDetail(int id)
        {
            List<object> jsons = new List<object>();
            jsons.Add(new
            {
                ArticleId = db.Reply.FirstOrDefault(x => x.ReplyID == id).ArticleID,
                ArticleTitle = db.Reply.FirstOrDefault(x => x.ReplyID == id).Article.ArticleTitle,
                Content = db.Reply.FirstOrDefault(x => x.ReplyID == id).ReplyContent.Replace('\"', '\''),
                MemberId = db.Reply.FirstOrDefault(x => x.ReplyID == id).MemberID,
                MemberEmail = db.Reply.FirstOrDefault(x => x.ReplyID == id).Member.Email,
                ARID = id
            });
            foreach (var item in db.Reply_Report.Where(x => x.ReplyId == id).OrderByDescending(x => x.Reply_ReportId))
            {
                jsons.Add(new
                {
                    Member = item.Member.NickName,
                    Reason = item.Report.ReportReason
                });
            }
            return Json(jsons);
        }

        [HttpPost]
        public bool ArticleReplyConfirm(int id)
        {
            return ArticleCRUD.ArticleReplyConfirmIsReport(id);
        }

        [HttpPost]
        public async Task<EmptyResult> ArticleReplyDelete(int id, string message, string type, int arid)
        {
            int adminId = (Session[CDictionary.SK_Admin_Logined_Member] as Member).MemberID;
            await ArticleCRUD.DeleteWithMessage(id, message, type, arid, adminId);
            return new EmptyResult();
        }

        [HttpPost]
        public async Task<EmptyResult> ArticleReplyDeleteBan(int id, string message, string type, int arid, 
            string reason, DateTime endtime)
        {
            int adminId = (Session[CDictionary.SK_Admin_Logined_Member] as Member).MemberID;
            await ArticleCRUD.DeleteWithMessage(id, message, type, arid, adminId, reason, endtime);
            return new EmptyResult();
        }
    }
}