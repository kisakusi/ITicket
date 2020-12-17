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
    public class SubCateOption
    {
        public SubCategories subCategories { private get; set; }
        public int SubCategoryId => subCategories.SubCategoryId;
        public string SubCategoryName => subCategories.SubCategoryName;
    }

    public class CommentAjax
    {
        public int AuthorSearch { get; set; }
        public int Cate { get; set; }
        public int SubCate { get; set; }
        public int Date { get; set; }
        public int Report { get; set; }
        public int ShowBan { get; set; }
        public int PageCurrent { get; set; }
        public int PageSize { get; set; }
        public string Keyword { get; set; }
    }

    public class CommentJson
    {
        public int Count { get; set; }
        public int ChangePage { get; set; }

        public Comment comment { private get; set; }
        public int CommentID => comment.CommentID;
        public string Author => comment.Member.Email;
        public string Title => comment.Activity.ActivityName;
        public int Score => comment.CommentScore;
        public string Date => comment.CommentDate.ToString("yyyy-MM-dd HH:mm:ss");
        public int IsBaned => comment.IsBaned ? 1 : 0;

        public IEnumerable<CommentReport> reports { private get; set; }
        public int ReportCount => reports.Count(x => x.CommentId == comment.CommentID);
    }

    public static class CommentCRUD
    {
        public static IEnumerable<Comment> CommentQuery(int author, int cate, int subcate, int date, int report,
            int showban, string keyword)
        {
            TicketSysEntities db = new TicketSysEntities();
            var q = db.CommentReport.GroupBy(x => x.CommentId).Where(g => g.Count() >= report).AsEnumerable()
                .OrderByDescending(g => g.Count()).ThenByDescending(g => g.First().CommentId).Select(g => g.First().Comment);
            if (report == 0)
            {
                int[] vs = db.CommentReport.Select(x => x.CommentId).Distinct().ToArray();
                q = q.Concat(db.Comment.Where(x => !vs.Contains(x.CommentID)).OrderByDescending(x => x.CommentID));
            }
            return q.Where(x => author == 0 || x.CommentMemberID == author)
                .Where(x => cate == 0 || x.Activity.SubCategories.CategoryId == cate)
                .Where(x => subcate == 0 || x.Activity.SubCategoryId == subcate)
                .Where(x => date == 0 || (DateTime.Now - x.CommentDate).TotalDays <= date)
                .Where(x => showban == 1 || x.IsBaned == false)
                .Where(x =>
                    string.IsNullOrEmpty(keyword) ||
                    x.Activity.ActivityName.ToLower().Contains(keyword) ||
                    x.Member.Email.Split('@')[0].ToLower().Contains(keyword)
                );
        }

        public static async Task HideWithMessage(int id, string message, int cid, int adminId, 
            string reason = "", DateTime? endtime = null)
        {
            TicketSysEntities db = new TicketSysEntities();
            try
            {
                Comment comment = db.Comment.FirstOrDefault(x => x.CommentID == cid);
                comment.IsBaned = true;
                db.SaveChanges();
                if (reason == "" || endtime == null)
                {
                    ShortMessage shortmessage = new ShortMessage
                    {
                        MemberID = id,
                        MessageContent = $"評論隱藏通知: {message}"
                    };
                    db.Entry(shortmessage).State = EntityState.Added;
                    db.SaveChanges();
                    MailMessage mail = new MailMessage
                    {
                        From = new MailAddress("iticket128@gmail.com"),
                        Subject = "iTicket 發送了一則系統通知給您",
                        Body = $"<h3>iTicket 發送了一則系統通知給您</h3><h4>評論隱藏通知</h4><p>{message}</p>",
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
                        MessageContent = $"評論隱藏通知: {message}"
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
                                      <h4>評論隱藏通知</h4>
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

        public static async Task ShowWithMessage(int id, int cid)
        {
            TicketSysEntities db = new TicketSysEntities();
            try
            {
                Comment comment = db.Comment.FirstOrDefault(x => x.CommentID == cid);
                comment.IsBaned = false;
                db.SaveChanges();
                await MemberCRUD.SendMessageToMember(id, "iTicket 管理員已解除隱藏您的評論");
            }
            catch { }
        }
    }
}