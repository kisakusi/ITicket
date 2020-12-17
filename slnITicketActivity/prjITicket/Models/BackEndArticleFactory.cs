using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;

namespace prjITicket.Models
{
    public class ArticleAjax
    {
        public int AuthorSearch { get; set; }
        public int Cate { get; set; }
        public int Date { get; set; }
        public int Report { get; set; }
        public string Type { get; set; }
        public int PageCurrent { get; set; }
        public int PageSize { get; set; }
        public string Keyword { get; set; }
    }

    public class ArticleJson
    {
        public int Count { get; set; }
        public int ChangePage { get; set; }

        public Article article { private get; set; }
        public int ARID => article.ArticleID;
        public string Picture => article.Picture;
        public string Title => article.ArticleTitle;
        public string Author => article.Member.Email;
        public string CategoryName => article.ArticleCategories.ArticleCategoryName;
        public string Date => article.Date.ToString("yyyy-MM-dd HH:mm:ss");

        public IEnumerable<Article_Report> reports { private get; set; }
        public int ReportCount => reports.Count(x => x.ArticleId == article.ArticleID);
    }

    public class ReplyJson
    {
        public int Count { get; set; }
        public int ChangePage { get; set; }

        public Reply reply { private get; set; }
        public int ARID => reply.ReplyID;
        public string Picture => reply.Article.Picture;
        public string Title => $"Re: {reply.Article.ArticleTitle}";
        public string Author => reply.Member.Email;
        public string CategoryName => reply.Article.ArticleCategories.ArticleCategoryName;
        public string Date => reply.ReplyDate.ToString("yyyy-MM-dd HH:mm:ss");

        public IEnumerable<Reply_Report> reports { private get; set; }
        public int ReportCount => reports.Count(x => x.ReplyId == reply.ReplyID);
    }

    public static class ArticleCRUD
    {
        public static IEnumerable<Article> ArticleQuery(int author, int cate, int date, int report, string keyword)
        {
            TicketSysEntities db = new TicketSysEntities();
            var q = db.Article_Report.GroupBy(x => x.ArticleId).Where(g => g.Count() >= report).AsEnumerable()
                .OrderByDescending(g => g.Count()).ThenByDescending(g => g.First().ArticleId).Select(g => g.First().Article);
            if (report == 0)
            {
                int[] vs = db.Article_Report.Select(x => x.ArticleId).Distinct().ToArray();
                q = q.Concat(db.Article.Where(x => !vs.Contains(x.ArticleID)).OrderByDescending(x => x.ArticleID));
            }
            return q.Where(x => author == 0 || x.MemberID == author)
                .Where(x => cate == 0 || x.ArticleCategoryID == cate)
                .Where(x => date == 0 || (DateTime.Now - x.Date).TotalDays <= date)
                .Where(x =>
                    string.IsNullOrEmpty(keyword) ||
                    x.ArticleTitle.ToLower().Contains(keyword) ||
                    x.Member.Email.Split('@')[0].ToLower().Contains(keyword)
                );
        }

        public static IEnumerable<Reply> ReplyQuery(int author, int cate, int date, int report, string keyword)
        {
            TicketSysEntities db = new TicketSysEntities();
            var q = db.Reply_Report.GroupBy(x => x.ReplyId).Where(g => g.Count() >= report).AsEnumerable()
                .OrderByDescending(g => g.Count()).ThenByDescending(g => g.First().ReplyId).Select(g => g.First().Reply);
            if (report == 0)
            {
                int[] vs = db.Reply_Report.Select(x => x.ReplyId).Distinct().ToArray();
                q = q.Concat(db.Reply.Where(x => !vs.Contains(x.ReplyID)).OrderByDescending(x => x.ReplyID));
            }
            return q.Where(x => author == 0 || x.MemberID == author)
                .Where(x => cate == 0 || x.Article.ArticleCategoryID == cate)
                .Where(x => date == 0 || (DateTime.Now - x.ReplyDate).TotalDays <= date)
                .Where(x =>
                     string.IsNullOrEmpty(keyword) ||
                     x.Article.ArticleTitle.ToLower().Contains(keyword) ||
                     x.Member.Email.Split('@')[0].ToLower().Contains(keyword)
                );
        }

        public static IEnumerable<T> SkipTake<T>(this IEnumerable<T> query, int skip, int take)
        {
            return query.Skip(skip).Take(take).ToList();
        }

        public static bool ArticleReplyConfirmIsReport(int ArticleId)
        {
            TicketSysEntities db = new TicketSysEntities();
            return db.Reply_Report.Where(x => x.Reply.ArticleID == ArticleId).Count() != 0;
        }

        public static async Task DeleteWithMessage(int id, string message, string type, int arid,
            int adminId, string reason = "", DateTime? endtime = null)
        {
            TicketSysEntities db = new TicketSysEntities();
            if (type == "A")
            {
                try
                {
                    List<Article_Report> reportsA = db.Article_Report.Where(x => x.ArticleId == arid).ToList();
                    foreach (Article_Report reportA in reportsA)
                    {
                        db.Entry(reportA).State = EntityState.Deleted;
                        db.SaveChanges();
                    }
                    int[] repliesIDs = db.Reply.Where(x => x.ArticleID == arid).Select(x => x.ReplyID).ToArray();
                    foreach (int rid in repliesIDs)
                    {
                        List<Reply_Report> reportsR = db.Reply_Report.Where(x => x.ReplyId == rid).ToList();
                        foreach (Reply_Report reportR in reportsR)
                        {
                            db.Entry(reportR).State = EntityState.Deleted;
                            db.SaveChanges();
                        }
                        Reply reply = db.Reply.FirstOrDefault(x => x.ReplyID == rid);
                        db.Entry(reply).State = EntityState.Deleted;
                        db.SaveChanges();
                    }
                    Article article = db.Article.FirstOrDefault(x => x.ArticleID == arid);
                    db.Entry(article).State = EntityState.Deleted;
                    db.SaveChanges();
                    if (reason == "" || endtime == null)
                    {
                        ShortMessage shortmessage = new ShortMessage
                        {
                            MemberID = id,
                            MessageContent = $"文章刪除通知: {message}"
                        };
                        db.Entry(shortmessage).State = EntityState.Added;
                        db.SaveChanges();
                        MailMessage mail = new MailMessage
                        {
                            From = new MailAddress("iticket128@gmail.com"),
                            Subject = "iTicket 發送了一則系統通知給您",
                            Body = $"<h3>iTicket 發送了一則系統通知給您</h3><h4>文章刪除通知</h4><p>{message}</p>",
                            IsBodyHtml = true
                        };
                        mail.To.Add(db.Member.FirstOrDefault(x => x.MemberID == id).Email);
                        using (SmtpClient smtpClient = new SmtpClient("smtp.gmail.com"))
                        {
                            smtpClient.Port = 587;
                            smtpClient.Credentials = new NetworkCredential("iticket128@gmail.com", "!@#qweasd");
                            smtpClient.EnableSsl = true;
                            await smtpClient.SendMailAsync(mail);
                        }
                    }
                    else
                    {
                        BanLIst banlist = new BanLIst
                        {
                            BanMemberId = id,
                            AdminMemberId = adminId,
                            Reason = reason,
                            EndTime = endtime.Value
                        };
                        db.Entry(banlist).State = EntityState.Added;
                        db.SaveChanges();
                        ShortMessage shortmessage = new ShortMessage
                        {
                            MemberID = id,
                            MessageContent = $"文章刪除通知: {message}"
                        };
                        db.Entry(shortmessage).State = EntityState.Added;
                        db.SaveChanges();
                        ShortMessage shortmessage2 = new ShortMessage
                        {
                            MemberID = id,
                            MessageContent = $"iTicket 管理員以「{reason}」為由將您停權到 {endtime:yyyy-MM-dd}"
                        };
                        db.Entry(shortmessage2).State = EntityState.Added;
                        db.SaveChanges();
                        MailMessage mail = new MailMessage
                        {
                            From = new MailAddress("iticket128@gmail.com"),
                            Subject = "iTicket 發送了一則系統通知給您",
                            Body = $@"<h3>iTicket 發送了一則系統通知給您</h3>
                                      <p>iTicket 管理員以「{reason}」為由將您停權到 {endtime:yyyy-MM-dd}</p>
                                      <h4>文章刪除通知</h4>
                                      <p>{message}</p>",
                            IsBodyHtml = true
                        };
                        mail.To.Add(db.Member.FirstOrDefault(x => x.MemberID == id).Email);
                        using (SmtpClient smtpClient = new SmtpClient("smtp.gmail.com"))
                        {
                            smtpClient.Port = 587;
                            smtpClient.Credentials = new NetworkCredential("iticket128@gmail.com", "!@#qweasd");
                            smtpClient.EnableSsl = true;
                            await smtpClient.SendMailAsync(mail);
                        }
                    }
                }
                catch { }
            } else
            {
                try
                {
                    List<Reply_Report> reports = db.Reply_Report.Where(x => x.ReplyId == arid).ToList();
                    foreach (Reply_Report report in reports)
                    {
                        db.Entry(report).State = EntityState.Deleted;
                        db.SaveChanges();
                    }
                    Reply reply = db.Reply.FirstOrDefault(x => x.ReplyID == arid);
                    db.Entry(reply).State = EntityState.Deleted;
                    db.SaveChanges();
                    if (reason == "" || endtime == null)
                    {
                        ShortMessage shortmessage = new ShortMessage
                        {
                            MemberID = id,
                            MessageContent = $"回覆刪除通知: {message}"
                        };
                        db.Entry(shortmessage).State = EntityState.Added;
                        db.SaveChanges();
                        MailMessage mail = new MailMessage
                        {
                            From = new MailAddress("iticket128@gmail.com"),
                            Subject = "iTicket 發送了一則系統通知給您",
                            Body = $"<h3>iTicket 發送了一則系統通知給您</h3><h4>回覆刪除通知</h4><p>{message}</p>",
                            IsBodyHtml = true
                        };
                        mail.To.Add(db.Member.FirstOrDefault(x => x.MemberID == id).Email);
                        using (SmtpClient smtpClient = new SmtpClient("smtp.gmail.com"))
                        {
                            smtpClient.Port = 587;
                            smtpClient.Credentials = new NetworkCredential("iticket128@gmail.com", "!@#qweasd");
                            smtpClient.EnableSsl = true;
                            await smtpClient.SendMailAsync(mail);
                        }
                    }
                    else
                    {
                        BanLIst banlist = new BanLIst
                        {
                            BanMemberId = id,
                            AdminMemberId = adminId,
                            Reason = reason,
                            EndTime = endtime.Value
                        };
                        db.Entry(banlist).State = EntityState.Added;
                        db.SaveChanges();
                        ShortMessage shortmessage = new ShortMessage
                        {
                            MemberID = id,
                            MessageContent = $"回覆刪除通知: {message}"
                        };
                        db.Entry(shortmessage).State = EntityState.Added;
                        db.SaveChanges();
                        ShortMessage shortmessage2 = new ShortMessage
                        {
                            MemberID = id,
                            MessageContent = $"iTicket 管理員以「{reason}」為由將您停權到 {endtime:yyyy-MM-dd}"
                        };
                        db.Entry(shortmessage2).State = EntityState.Added;
                        db.SaveChanges();
                        MailMessage mail = new MailMessage
                        {
                            From = new MailAddress("iticket128@gmail.com"),
                            Subject = "iTicket 發送了一則系統通知給您",
                            Body = $@"<h3>iTicket 發送了一則系統通知給您</h3>
                                      <p>iTicket 管理員以「{reason}」為由將您停權到 {endtime:yyyy-MM-dd}</p>
                                      <h4>回覆刪除通知</h4>
                                      <p>{message}</p>",
                            IsBodyHtml = true
                        };
                        mail.To.Add(db.Member.FirstOrDefault(x => x.MemberID == id).Email);
                        using (SmtpClient smtpClient = new SmtpClient("smtp.gmail.com"))
                        {
                            smtpClient.Port = 587;
                            smtpClient.Credentials = new NetworkCredential("iticket128@gmail.com", "!@#qweasd");
                            smtpClient.EnableSsl = true;
                            await smtpClient.SendMailAsync(mail);
                        }
                    }
                }
                catch { }
            }
        }
    }
}