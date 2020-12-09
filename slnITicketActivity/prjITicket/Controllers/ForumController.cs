using prjITicket.Models;
using prjITicket.Models.Forum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using System.Drawing;
using System.Web.UI;
using prjITicket.ViewModel;

namespace 期末專題_討論版.Controllers
{
    
    public class ForumController : Controller
    {
        //...結果他是從資料庫撈圖片...所以可以用Json，如果要用Json傳圖片，還要先把圖片轉檔成base64？
        //所以我要先把完整的圖片傳到後端(包含名字)，再用下面的程式碼傳來裁減座標、進行裁減？
        [HttpPost]
        public JsonResult CropImage(string id, int? x1, int? x2, int? y1, int? y2,int? imgWidth)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            if (string.IsNullOrWhiteSpace(id))
            {
                result.Add("result", "error");
                result.Add("msg", "沒有輸入資料編號");
                return Json(result);
            }
            if (!x1.HasValue || !x2.HasValue || !y1.HasValue || !y2.HasValue)
            {
                result.Add("result", "error");
                result.Add("msg", "裁剪圖片區域值有缺少");
                return Json(result);
            }
            string id2 = id.Remove(0, 23);
            byte[] arr = Convert.FromBase64String(id2);
            MemoryStream instance = new MemoryStream(arr);
            Bitmap oBitmap = new Bitmap(instance);
            CropImageUtility cropUtils = new CropImageUtility();
            //不可以唯讀啊！
            int X1 = x1.Value;
            int X2 = x2.Value;
            int Y1 = y1.Value;
            int Y2 = y2.Value;
            if (imgWidth > 700)
            {
                X1 = X1 * imgWidth.Value / 700;
                X2 = X2 * imgWidth.Value / 700;
                Y1 = Y1 * imgWidth.Value / 700;
                Y2 = Y2 * imgWidth.Value / 700;
            }
            Dictionary<string, string> processResult = cropUtils.ProcessImageCrop
            (
                oBitmap,
                new int[] { X1, X2, Y1, Y2 }
            );
            if (processResult["result"].Equals("Success", StringComparison.OrdinalIgnoreCase))
            {
                result.Add("result", "OK");
                result.Add("msg", "");
                result.Add("CropImage", processResult["CropImage"].Remove(0, 1));
            }
            else
            {
                result.Add("result", processResult["result"]);
                result.Add("msg", processResult["msg"]);
            }
            return Json(result);

        }
         
        //文章要不要有刪除功能呢？
        //水桶文底下的留言是可以被接受的？
        //水桶作者，文章屏蔽？


        public void  Delete(int articleID)
        {
            Member member = Session[CDictionary.SK_Logined_Member] as Member;
            TicketSysEntities db = new TicketSysEntities();
            Article article = db.Article.Where(n => n.ArticleID == articleID).FirstOrDefault();
            if (Session[CDictionary.SK_Logined_Member] == null)
            {

                Page Page = new Page();
                Response.Write("<script>alert('您尚未登入！即將跳轉至登入頁面');" +
                    "window.location.href='http://localhost:49949/Login/Login';</script> ");
            }
            else if (article.MemberID != member.MemberID)
            {
                Response.Write("<script>alert('您非該文章作者，禁止刪除！即將跳轉至討論版首頁');" +
                    "window.location.href='http://localhost:49949/Forum/forum_mainblock';</script> ");
            }
            else
            {
                var q = db.Article.Where(n => n.ArticleID == articleID).FirstOrDefault();
                var reply = db.Reply.Where(n => n.ArticleID == articleID);
                foreach (var item in reply)
                {
                    db.Reply.Remove(item);
                }
                db.Article.Remove(q);
                db.SaveChanges();
                Response.Write("<script>" +                     
                    "alert('刪除成功！即將跳轉至討論版首頁');" +
                    "window.location.href='http://localhost:49949/Forum/forum_mainblock';" +
                    "</script> ");
            }
           
        }
        public ActionResult Reply_Delete(int replyID, int ArticleID)
        {
            TicketSysEntities db = new TicketSysEntities();
            var q = db.Reply.Where(n => n.ReplyID == replyID).FirstOrDefault();
            db.Reply.Remove(q);
            db.SaveChanges();
            return RedirectToAction("forum_content", "Forum", new { articleID = ArticleID });
        }
        public ActionResult forum_homepage()
        {
            return RedirectToAction("forum_mainblock");
        }
        //初次載入調用，之後都用不到惹
        public ActionResult forum_mainblock()
        {
            TicketSysEntities db = new TicketSysEntities();
            var q = (from n in db.Article
                     orderby n.Date descending
                     select n).ToList();
            var p = db.ArticleCategories.Select(n => n).ToList();
            int maxPage = q.Count / 4;
            var qq = new VMforum_mainblock { Article = q, ArticleCategories = p, page = maxPage };
            return View("forum_mainblock", "_ForumLayout", qq);
        }
        public ActionResult forum_content(int? articleID)
        {
            TicketSysEntities db = new TicketSysEntities();
            var q = from n in db.Article
                    where n.ArticleID == articleID
                    select n;
            Article article = q.FirstOrDefault();
            List<Report> report = db.Report.ToList();
            return View(new VMReport() { Article = article, Report = report });
        }
        //關閉保護html傳送
        [ValidateInput(false)]
        [HttpPost]
        public string forum_content(string content, int articleID)
        {
            Member member = Session[CDictionary.SK_Logined_Member] as Member;
            try
            {
                if (Session[CDictionary.SK_Logined_Member] == null)
                {
                    return "您尚未登入！即將跳轉至登入頁面";
                }
                else
                {
                TicketSysEntities db = new TicketSysEntities();
                Reply rp = new Reply();
                rp.MemberID = 1;
                rp.ReplyDate = DateTime.Now;
                rp.ArticleID = articleID;
                rp.ReplyContent = content;
                db.Reply.Add(rp);
                db.SaveChanges();
                return "成功";

                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        //新增文章、編輯文章、刪除文章(能不能別跳轉？)
        //before系列──各種按鈕判斷有沒有登入：新增留言、編輯留言、刪除留言......
        //改變編輯─標題圖示的顯示與編輯
        public void before_Add_article()
        {
            if (Session[CDictionary.SK_Logined_Member] == null)
            {

                Page Page = new Page();
                Response.Write("<script>alert('您尚未登入！即將跳轉至登入頁面');" +
                    "window.location.href='http://localhost:49949/Login/Login';</script> ");
            }
            else
            {
                Response.Write("<script>" +
                    "window.location.href='http://localhost:49949/Forum/Add_article';</script> ");
            }
        }
        
        public void before_Edit_article(int? articleID)
        {
            Member member = Session[CDictionary.SK_Logined_Member] as Member;
            TicketSysEntities db = new TicketSysEntities();
            Article article = db.Article.Where(n => n.ArticleID == articleID).FirstOrDefault();
            if (Session[CDictionary.SK_Logined_Member] == null)
            {

                Page Page = new Page();
                Response.Write("<script>alert('您尚未登入！即將跳轉至登入頁面');" +
                    "window.location.href='http://localhost:49949/Login/Login';</script> ");
            }
            else if(article.MemberID != member.MemberID)            
            {
                Response.Write("<script>alert('您非該文章作者，禁止編輯！即將跳轉至討論版首頁');" +
                    "window.location.href='http://localhost:49949/Forum/forum_mainblock';</script> ");
            }
            else
            {
                Response.Write("<script>" +
                    $"window.location.href='http://localhost:49949/Forum/Edit_article?articleID={articleID}';</script> ");
            }
        }
        //
        public ActionResult Add_article()
        {
            TicketSysEntities db = new TicketSysEntities();
            var q =db.ArticleCategories.Select(n => n).ToList();

            return View(q);
        }
        //關閉保護html傳送
        [ValidateInput(false)]
        [HttpPost]
        public string Add_article(string title, string content, string picPath)
        {
            if (Session[CDictionary.SK_Logined_Member] == null)
                return "Fail";

                try
            {

                //其他部分
                TicketSysEntities db = new TicketSysEntities();
                Member member = Session[CDictionary.SK_Logined_Member] as Member;//如果轉型失敗，回傳null;
                Article article = new Article();
                article.MemberID = member.MemberID;
                article.Date = DateTime.Now;
                article.ArticleCategoryID = 1;
                article.ArticleTitle = title;
                article.ArticleContent = content;
                article.Picture = picPath;
                db.Article.Add(article);
                db.SaveChanges();
                return "OK";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        public ActionResult Edit_article(int? articleID)
        {
            TicketSysEntities db = new TicketSysEntities();
            var q = from n in db.Article
                    where n.ArticleID == articleID
                    select n;
            Article article = q.FirstOrDefault();
            return View(article);
        }
        [ValidateInput(false)]
        [HttpPost]
        public string Edit_article(string title, string content,int ArticleID, string picPath)
        {
            TicketSysEntities db = new TicketSysEntities();
            Article article = db.Article.Where(n => n.ArticleID == ArticleID).FirstOrDefault();
            if (article != null)
            {
                article.ArticleTitle = title;
                article.ArticleContent = content;
                article.Date = DateTime.Now;
                article.Picture = picPath;
                db.SaveChanges();
            }
            return "OK";
        }
        public string Reply_Edit(string rpcontent, int replyID)
        {
            try
            {
                TicketSysEntities db = new TicketSysEntities();
                Reply reply = db.Reply.Where(n => n.ReplyID == replyID).FirstOrDefault();
                if (reply != null)
                {
                    reply.ReplyContent = rpcontent;
                    reply.ReplyDate = DateTime.Now;

                    db.SaveChanges();
                }


                return "OK";
            }
            catch (Exception ex)
            {

                return ex.Message;
            }
        }
        public ActionResult forum_reply(int? articleID)
        {
            TicketSysEntities db = new TicketSysEntities();
            var q = from n in db.Article
                    where n.ArticleID == articleID
                    select n;
            Article article = q.FirstOrDefault();
            List<Report> report = db.Report.ToList();

            return PartialView(new VMReport() { Article = article, Report = report });

        }

        public string Reply_report(int ReportID, int ReplyID)
        {
            try
            {
                if (Session[CDictionary.SK_Logined_Member] == null)
                    return "您尚未登入！即將跳轉至登入頁面";
                TicketSysEntities db = new TicketSysEntities();
                Reply_Report report = new Reply_Report();

                Member member = Session[CDictionary.SK_Logined_Member] as Member;
                report.MemberId = member.MemberID;
                report.ReplyId = ReplyID;
                report.ReportId = ReportID;
                db.Reply_Report.Add(report);
                db.SaveChanges();
                return "成功";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }


        public string Article_report(int ReportID, int ArticleID)
        {
            try
            {
                if (Session[CDictionary.SK_Logined_Member] == null)
                    return "您尚未登入！即將跳轉至登入頁面";
                TicketSysEntities db = new TicketSysEntities();
                Article_Report report = new Article_Report();

                Member member = Session[CDictionary.SK_Logined_Member] as Member;
                report.MemberId = member.MemberID;
                report.ArticleId = ArticleID;
                report.ReportId = ReportID;
                db.Article_Report.Add(report);
                db.SaveChanges();
                return "成功";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        //文章搜尋
        public ActionResult SearchArticle(string searchText = "", int Page = 0, int CategoryID = 0, int searchType = 30)
        {
            TicketSysEntities db = new TicketSysEntities();
            CForum_ArticleList_Factory al = new CForum_ArticleList_Factory();

            List<Article> articles = db.Article.ToList();

            //有選擇討論版
            if (CategoryID != 0)
                articles = al.Article_Category(articles, CategoryID);

            //搜尋有文字
            if (!string.IsNullOrEmpty(searchText))
            {
                //searchType =2...有勾作者
                //searchType =3...有勾標題
                //searchType =5...有勾內文  
                List<Article> q = new List<Article>();
                if (searchType % 2 == 0)
                    q = q.Union(al.Article_Search_Editor(articles, searchText)).ToList();
                if (searchType % 3 == 0)
                    q = q.Union(al.Article_Search_Title(articles, searchText)).ToList();
                if (searchType % 5 == 0)
                    q = q.Union(al.Article_Search_Content(articles, searchText)).ToList();
                articles = q;
            }
            //todo:按日期檢索
            ///
            int maxPage = (articles.Count() / 4);
            //第幾頁
            articles = articles.OrderByDescending(n => n.Date).Skip(Page * 4).ToList();//這個頁數不能一起算欸！放最後篩好了
            var p = db.ArticleCategories.Select(n => n).ToList();
            var qq = new VMforum_mainblock { Article = articles, ArticleCategories = p, searchWord = searchText, page = maxPage, ArticleCategoryID = CategoryID };
            return PartialView(qq);


        }

        //⬇⬇⬇⬇⬇⬇⬇⬇⬇⬇⬇⬇⬇⬇⬇⬇⬇⬇⬇⬇⬇貼文、留言Emotion⬇⬇⬇⬇⬇⬇⬇⬇⬇⬇⬇⬇⬇⬇⬇⬇⬇⬇⬇⬇//

        public string Reply_EmotionAction(int ReplyID, int ActionID)
        {
            try
            {
                if (Session[CDictionary.SK_Logined_Member] == null)
                    return "您尚未登入！即將跳轉至登入頁面";
                TicketSysEntities db = new TicketSysEntities();
                Reply_Emotion RE = new Reply_Emotion();
                Member member = Session[CDictionary.SK_Logined_Member] as Member;

                RE.MemberId = member.MemberID;
                RE.ReplyId = ReplyID;
                RE.ActionId = ActionID;
                db.Reply_Emotion.Add(RE);
                db.SaveChanges();
                return "成功";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public string Reply_EmotionAction_Cancel(int ReplyID)
        {
            try
            {
                if (Session[CDictionary.SK_Logined_Member] == null)
                    return "您尚未登入！即將跳轉至登入頁面";
                TicketSysEntities db = new TicketSysEntities();
                Member member = Session[CDictionary.SK_Logined_Member] as Member;
                var q = db.Reply_Emotion.Where(n => n.ReplyId == ReplyID && n.MemberId == member.MemberID).FirstOrDefault();
                db.Reply_Emotion.Remove(q);
                db.SaveChanges();
                return "成功";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public string Article_EmotionAction(int ArticleID, int ActionID)
        {
            try
            {
                if (Session[CDictionary.SK_Logined_Member] == null)
                    return "您尚未登入！即將跳轉至登入頁面";
                TicketSysEntities db = new TicketSysEntities();
                Article_Emotion AE = new Article_Emotion();
                Member member = Session[CDictionary.SK_Logined_Member] as Member;

                AE.MemberId = member.MemberID;
                AE.ArticleId = ArticleID;
                AE.ActionId = ActionID;
                db.Article_Emotion.Add(AE);
                db.SaveChanges();
                return "成功";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public string Article_EmotionAction_Cancel(int ArticleID)
        {
            try
            {
                if (Session[CDictionary.SK_Logined_Member] == null)
                    return "您尚未登入！即將跳轉至登入頁面";
                TicketSysEntities db = new TicketSysEntities();
                Member member = Session[CDictionary.SK_Logined_Member] as Member;
                var q = db.Article_Emotion.Where(n => n.ArticleId == ArticleID && n.MemberId == member.MemberID).FirstOrDefault();
                db.Article_Emotion.Remove(q);
                db.SaveChanges();
                return "成功";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
    }
}