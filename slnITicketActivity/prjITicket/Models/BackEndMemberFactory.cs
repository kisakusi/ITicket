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
    public class MemberAjax
    {
        public int PageCurrent { get; set; }
        public int PageSize { get; set; }
        public string Keyword { get; set; }
        public string SortRule { get; set; }
        public string MemberRoleInfo { get; set; }
        public bool MemberRoleIsBan { get; set; }
    }

    public class MemberJson
    {
        public int Count { get; set; }
        public Member member { private get; set; }
        public int MemberID => member.MemberID;
        public string Email => member.Email;
        public string NickName => member.NickName;
        public string Name => member.Name;
        public string Phone => member.Phone;
        public string MemberRoleName => member.MemberRole.MemberRoleName;
        public IEnumerable<BanLIst> banlists { private get; set; }
        public string Reason => banlists.OrderByDescending(x => x.BanId)
            .FirstOrDefault(x => x.BanMemberId == member.MemberID && x.EndTime > DateTime.Now)?.Reason;
        public string EndTime => banlists.OrderByDescending(x => x.BanId)
            .FirstOrDefault(x => x.BanMemberId == member.MemberID && x.EndTime > DateTime.Now)?.EndTime.ToString("yyyy-MM-dd");
    }

    public class MerchantDetail
    {
        public Member member { private get; set; }
        public string Email => member.Email;
        public string Name => member.Name;
        public string IDentityNumber => member.IDentityNumber;
        public string Passport => member.Passport;
        public string NickName => member.NickName;
        public string BirthDate => member.BirthDate?.ToString("yyyy-MM-dd");
        public string Phone => member.Phone;
        public Nullable<int> Point => member.Point;
        public string Address => member.Address;
        public string MemberRoleName => member.MemberRole.MemberRoleName;
        public string Sex => member.Sex == null ? null : member.Sex.Value ? "男性" : "女性";
        public string District => member.DistrictId == null ?
            null : $"{member.Districts.DistrictName} (${member.Districts.Cities.CityName})";
        public bool SplitLine => true;
        public Seller seller { private get; set; }
        public string CompanyName => seller?.CompanyName;
        public string TaxIDNumber => seller?.TaxIDNumber;
        public string SellerHomePage => seller?.SellerHomePage;
        public string SellerPhone => seller?.SellerPhone;
        public string SellerDiscription => seller?.SellerDeccription;
    }

    public class GeneralDetail
    {
        public Member member { private get; set; }
        public string Email => member.Email;
        public string Name => member.Name;
        public string IDentityNumber => member.IDentityNumber;
        public string Passport => member.Passport;
        public string NickName => member.NickName;
        public string BirthDate => member.BirthDate?.ToString("yyyy-MM-dd");
        public string Phone => member.Phone;
        public Nullable<int> Point => member.Point;
        public string Address => member.Address;
        public string MemberRoleName => member.MemberRole.MemberRoleName;
        public string Sex => member.Sex == null ? null : member.Sex.Value ? "男性" : "女性";
        public string District => member.DistrictId == null ?
            null : $"{member.Districts.DistrictName} (${member.Districts.Cities.CityName})";
    }

    public static class MemberCRUD
    {
        public static IEnumerable<Member> Query(string keyword, int[] role, bool isban)
        {
            TicketSysEntities db = new TicketSysEntities();
            List<int> banlist = db.BanLIst.Where(x => x.EndTime > DateTime.Now)
                .Select(x => x.BanMemberId).Distinct().OrderBy(x => x).ToList();
            return db.Member.AsEnumerable()
                .Where(x => string.IsNullOrEmpty(keyword) ||
                            x.Email.Split('@')[0].ToLower().Contains(keyword) ||
                            x.NickName.ToLower().Contains(keyword) ||
                            x.Name.ToLower().Contains(keyword) ||
                            (x.Phone != null && x.Phone.Contains(keyword)))
                .Where(x => role.Contains(0) || Array.BinarySearch(role, x.MemberRoleId) >= 0)
                .Where(x => !isban || banlist.BinarySearch(x.MemberID) >= 0);
        }

        public static IEnumerable<Member> QuerySort(this IEnumerable<Member> query, string sortrule, int skip, int take)
        {
            IEnumerable<Member> members;
            switch (sortrule)
            {
                case "1d":
                    members = query.OrderByDescending(x => x.Email).ThenByDescending(x => x.MemberID)
                        .Skip(skip).Take(take).ToList();
                    break;
                case "1a":
                    members = query.OrderBy(x => x.Email).ThenByDescending(x => x.MemberID)
                        .Skip(skip).Take(take).ToList();
                    break;
                case "2d":
                    members = query.OrderByDescending(x => x.NickName).ThenByDescending(x => x.MemberID)
                        .Skip(skip).Take(take).ToList();
                    break;
                case "2a":
                    members = query.OrderBy(x => x.NickName).ThenByDescending(x => x.MemberID)
                        .Skip(skip).Take(take).ToList();
                    break;
                case "3d":
                    members = query.OrderByDescending(x => x.Name).ThenByDescending(x => x.MemberID)
                        .Skip(skip).Take(take).ToList();
                    break;
                case "3a":
                    members = query.OrderBy(x => x.Name).ThenByDescending(x => x.MemberID)
                        .Skip(skip).Take(take).ToList();
                    break;
                case "4d":
                    members = query.OrderByDescending(x => x.Phone).ThenByDescending(x => x.MemberID)
                        .Skip(skip).Take(take).ToList();
                    break;
                case "4a":
                    members = query.OrderBy(x => x.Phone).ThenByDescending(x => x.MemberID)
                        .Skip(skip).Take(take).ToList();
                    break;
                case "5d":
                    members = query.OrderBy(x => x.MemberRoleId).ThenByDescending(x => x.MemberID)
                        .Skip(skip).Take(take).ToList();
                    break;
                case "5a":
                    members = query.OrderByDescending(x => x.MemberRoleId).ThenByDescending(x => x.MemberID)
                        .Skip(skip).Take(take).ToList();
                    break;
                default:
                    members = query.OrderByDescending(x => x.MemberID).Skip(skip).Take(take).ToList();
                    break;
            }
            return members;
        }

        public static async Task SendMessageToMember(int id, string message)
        {
            try
            {
                TicketSysEntities db = new TicketSysEntities();
                ShortMessage shortmessage = new ShortMessage
                {
                    MemberID = id,
                    MessageContent = message
                };
                db.Entry(shortmessage).State = EntityState.Added;
                db.SaveChanges();
                MailMessage mail = new MailMessage
                {
                    From = new MailAddress("iticket128@gmail.com"),
                    Subject = "iTicket 發送了一則系統通知給您",
                    Body = $@"<h3>iTicket 發送了一則系統通知給您</h3><p>{message}</p>",
                    IsBodyHtml = true
                };
                mail.To.Add(db.Member.FirstOrDefault(x => x.MemberID == id).Email);
                using (SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com"))
                {
                    SmtpServer.Port = 587;
                    SmtpServer.Credentials = new NetworkCredential("iticket128@gmail.com", "!@#qweasd");
                    SmtpServer.EnableSsl = true;
                    await SmtpServer.SendMailAsync(mail);
                }
            } catch { }
        }

        public static async Task BanMemberWithMessage(int id, string reason, DateTime endtime)
        {
            try
            {
                TicketSysEntities db = new TicketSysEntities();
                BanLIst banlist = new BanLIst
                {
                    BanMemberId = id,
                    AdminMemberId = 1,
                    Reason = reason,
                    EndTime = endtime
                };
                db.Entry(banlist).State = EntityState.Added;
                db.SaveChanges();
                await SendMessageToMember(id, $"iTicket 管理員以「{reason}」為由將您停權到 {endtime:yyyy-MM-dd}");
            }
            catch { }
        }
    }
}