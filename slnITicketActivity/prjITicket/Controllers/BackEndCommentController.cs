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
    public class BackEndCommentController : Controller, IDisposable
    {
        TicketSysEntities db = new TicketSysEntities();

        public ActionResult CommentList()
        {
            if (Session[CDictionary.SK_Logined_Member] == null ||
                (Session[CDictionary.SK_Logined_Member] as Member).MemberRoleId != 4)
            {
                return RedirectToAction("Index", "Home");
            }
            return View(db.Categories);
        }

        [HttpPost]
        public JsonResult GetSubCateOption(int id)
        {
            IEnumerable<SubCateOption> jsons = db.SubCategories.Where(x => id == 0 || x.CategoryId == id)
                .Select(x => new SubCateOption { subCategories = x });
            return Json(jsons);
        }

        [HttpPost]
        public JsonResult CommentList(CommentAjax m)
        {
            int skip = m.PageSize * (m.PageCurrent - 1);
            int take = m.PageSize;
            IEnumerable<Comment> query = CommentCRUD.CommentQuery(m.AuthorSearch, m.Cate, m.SubCate, m.Date, m.Report,
                m.ShowBan, m.Keyword);
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
            IEnumerable<Comment> comments = query.SkipTake(skip, take);
            IEnumerable<CommentJson> jsons = comments.Select(comment => new CommentJson
            {
                Count = count,
                ChangePage = changepage,
                comment = comment,
                reports = db.CommentReport
            });
            return Json(jsons);
        }

        [HttpPost]
        public JsonResult CommentDetail(int id)
        {
            List<object> jsons = new List<object>();
            jsons.Add(new
            {
                ActivityId = db.Comment.FirstOrDefault(x => x.CommentID == id).CommentActivityId,
                ActivityTitle = db.Comment.FirstOrDefault(x => x.CommentID == id).Activity.ActivityName,
                Content = db.Comment.FirstOrDefault(x => x.CommentID == id).CommentContent,
                MemberId = db.Comment.FirstOrDefault(x => x.CommentID == id).CommentMemberID,
                MemberEmail = db.Comment.FirstOrDefault(x => x.CommentID == id).Member.Email,
                CommentId = id,
                IsBaned = db.Comment.FirstOrDefault(x => x.CommentID == id).IsBaned
            });
            foreach (var item in db.CommentReport.Where(x => x.CommentId == id).OrderByDescending(x => x.CommentReportId))
            {
                jsons.Add(new
                {
                    Member = item.Member.NickName,
                    Reason = item.ReportReason
                });
            }
            return Json(jsons);
        }

        [HttpPost]
        public async Task<EmptyResult> CommentHide(int id, string message, int cid)
        {
            int adminId = (Session[CDictionary.SK_Logined_Member] as Member).MemberID;
            await CommentCRUD.HideWithMessage(id, message, cid, adminId);
            return new EmptyResult();
        }

        [HttpPost]
        public async Task<EmptyResult> CommentHideBan(int id, string message, int cid, string reason, DateTime endtime)
        {
            int adminId = (Session[CDictionary.SK_Logined_Member] as Member).MemberID;
            await CommentCRUD.HideWithMessage(id, message, cid, adminId, reason, endtime);
            return new EmptyResult();
        }

        [HttpPost]
        public async Task<EmptyResult> CommentShow(int id, int cid)
        {
            await CommentCRUD.ShowWithMessage(id, cid);
            return new EmptyResult();
        }
    }
}