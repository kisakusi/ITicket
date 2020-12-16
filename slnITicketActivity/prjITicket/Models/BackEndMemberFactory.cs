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
        public int RoleId { get; set; }
        public int PageCurrent { get; set; }
        public int PageSize { get; set; }
        public string Keyword { get; set; }
        public string SortRule { get; set; }
        public int NonVerify { get; set; }
    }

    public class MemberJson
    {
        public int Count { get; set; }
        public int ChangePage { get; set; }

        public Member member { private get; set; }
        public int MemberID => member.MemberID;
        public string Email => member.Email;
        public string NickName => member.NickName;
        public string Name => member.Name;
        public string Phone => member.Phone;
        public int MemberRoleId => member.MemberRoleId;
        public string MemberRoleName => member.MemberRole.MemberRoleName;
        public string MerchantNull => "Unknown";

        public IEnumerable<BanLIst> banlists { private get; set; }
        public string Reason => banlists.OrderByDescending(x => x.BanId)
            .FirstOrDefault(x => x.BanMemberId == member.MemberID && x.EndTime > DateTime.Now)?.Reason;
        public string EndTime => banlists.OrderByDescending(x => x.BanId)
            .FirstOrDefault(x => x.BanMemberId == member.MemberID && x.EndTime > DateTime.Now)?.EndTime.ToString("yyyy-MM-dd");
    }

    public class SellerJson
    {
        public int Count { get; set; }
        public int ChangePage { get; set; }

        public Seller seller { private get; set; }
        public int MemberID => seller.MemberId;
        public string Email => seller.Member.Email;
        public string NickName => seller.Member.NickName;
        public string Name => seller.Member.Name;
        public string Phone => seller.Member.Phone;
        public int MemberRoleId => 3;
        public string MemberRoleName => "商家";
        public string MerchantNull => seller.fPass == null ? "Null" : "Unknown";

        public IEnumerable<BanLIst> banlists { private get; set; }
        public string Reason => banlists.OrderByDescending(x => x.BanId)
            .FirstOrDefault(x => x.BanMemberId == seller.MemberId && x.EndTime > DateTime.Now)?.Reason;
        public string EndTime => banlists.OrderByDescending(x => x.BanId)
            .FirstOrDefault(x => x.BanMemberId == seller.MemberId && x.EndTime > DateTime.Now)?.EndTime.ToString("yyyy-MM-dd");
    }

    public static class MemberCRUD
    {
        public static IEnumerable<Member> MemberQuery(int roleId, string keyword)
        {
            TicketSysEntities db = new TicketSysEntities();
            return db.Member.Where(x => roleId == 9 || x.MemberRoleId == roleId).AsEnumerable()
                .Where(x =>
                    string.IsNullOrEmpty(keyword) ||
                    x.Email.Split('@')[0].ToLower().Contains(keyword) ||
                    x.NickName.ToLower().Contains(keyword) ||
                    x.Name.ToLower().Contains(keyword) ||
                    (x.Phone != null && x.Phone.Contains(keyword))
                );
        }

        public static IEnumerable<Seller> SellerQuery(string keyword, int nonverify)
        {
            TicketSysEntities db = new TicketSysEntities();
            return db.Seller.Where(x => nonverify == 0 || x.fPass == null).AsEnumerable()
                .Where(x =>
                    string.IsNullOrEmpty(keyword) ||
                    x.Member.Email.Split('@')[0].ToLower().Contains(keyword) ||
                    x.Member.NickName.ToLower().Contains(keyword) ||
                    x.Member.Name.ToLower().Contains(keyword) ||
                    (x.Member.Phone != null && x.Member.Phone.Contains(keyword))
                );
        }

        public static IEnumerable<Member> BanMemberQuery(string keyword)
        {
            TicketSysEntities db = new TicketSysEntities();
            List<int> banlist = db.BanLIst.Where(x => x.EndTime > DateTime.Now)
                .Select(x => x.BanMemberId).Distinct().OrderBy(x => x).ToList();
            return db.Member.AsEnumerable()
                .Where(x =>
                    string.IsNullOrEmpty(keyword) ||
                    x.Email.Split('@')[0].ToLower().Contains(keyword) ||
                    x.NickName.ToLower().Contains(keyword) ||
                    x.Name.ToLower().Contains(keyword) ||
                    (x.Phone != null && x.Phone.Contains(keyword))
                ).Where(x => banlist.BinarySearch(x.MemberID) >= 0);
        }

        public static IEnumerable<Member> MemberSort(this IEnumerable<Member> query, string sortrule, int skip, int take)
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
                default:
                    members = query.OrderByDescending(x => x.MemberID).Skip(skip).Take(take).ToList();
                    break;
            }
            return members;
        }

        public static IEnumerable<Seller> SellerSort(this IEnumerable<Seller> query, string sortrule, int skip, int take)
        {
            IEnumerable<Seller> seller;
            switch (sortrule)
            {
                case "1d":
                    seller = query.OrderByDescending(x => x.Member.Email).ThenByDescending(x => x.SellerID)
                        .Skip(skip).Take(take).ToList();
                    break;
                case "1a":
                    seller = query.OrderBy(x => x.Member.Email).ThenByDescending(x => x.SellerID)
                        .Skip(skip).Take(take).ToList();
                    break;
                case "2d":
                    seller = query.OrderByDescending(x => x.Member.NickName).ThenByDescending(x => x.SellerID)
                        .Skip(skip).Take(take).ToList();
                    break;
                case "2a":
                    seller = query.OrderBy(x => x.Member.NickName).ThenByDescending(x => x.SellerID)
                        .Skip(skip).Take(take).ToList();
                    break;
                case "3d":
                    seller = query.OrderByDescending(x => x.Member.Name).ThenByDescending(x => x.SellerID)
                        .Skip(skip).Take(take).ToList();
                    break;
                case "3a":
                    seller = query.OrderBy(x => x.Member.Name).ThenByDescending(x => x.SellerID)
                        .Skip(skip).Take(take).ToList();
                    break;
                case "4d":
                    seller = query.OrderByDescending(x => x.Member.Phone).ThenByDescending(x => x.SellerID)
                        .Skip(skip).Take(take).ToList();
                    break;
                case "4a":
                    seller = query.OrderBy(x => x.Member.Phone).ThenByDescending(x => x.SellerID)
                        .Skip(skip).Take(take).ToList();
                    break;
                default:
                    seller = query.OrderByDescending(x => x.SellerID).Skip(skip).Take(take).ToList();
                    break;
            }
            return seller;
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
                    Body = $"<h3>iTicket 發送了一則系統通知給您</h3><p>{message}</p>",
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
            catch { }
        }

        public static async Task BanMemberWithMessage(int id, int adminId, string reason, DateTime endtime)
        {
            try
            {
                TicketSysEntities db = new TicketSysEntities();
                BanLIst banlist = new BanLIst
                {
                    BanMemberId = id,
                    AdminMemberId = adminId,
                    Reason = reason,
                    EndTime = endtime
                };
                db.Entry(banlist).State = EntityState.Added;
                db.SaveChanges();
                await SendMessageToMember(id, $"iTicket 管理員以「{reason}」為由將您停權到 {endtime:yyyy-MM-dd}");
            }
            catch { }
        }

        public static async Task UnBanMemberWithMessage(int id)
        {
            try
            {
                TicketSysEntities db = new TicketSysEntities();
                List<BanLIst> banlists = db.BanLIst.Where(x => x.BanMemberId == id && x.EndTime > DateTime.Now).ToList();
                foreach (BanLIst banlist in banlists)
                {
                    db.Entry(banlist).State = EntityState.Deleted;
                    db.SaveChanges();
                }
                await SendMessageToMember(id, "iTicket 管理員已解除您的停權處分");
            }
            catch { }
        }

        public static async Task MerchantVerificationWithMessage(int id, bool fPass, string reason)
        {
            try
            {
                TicketSysEntities db = new TicketSysEntities();
                Seller seller = db.Seller.FirstOrDefault(x => x.MemberId == id);
                seller.fPass = fPass;
                db.SaveChanges();
                await SendMessageToMember(id, 
                    $"iTicket 管理員已{(fPass ? "通過" : "駁回")}您的商家審核{(fPass ? "" : $", 原因是「{reason}」")}");
            }
            catch { }
        }
    }

    public class MemberDetail
    {
        public IEnumerable<BanLIst> banlist { private get; set; }
        public IEnumerable<string> Reasons => banlist.Select(x => x.Reason);
        public IEnumerable<string> EndTimes => banlist.Select(x => x.EndTime.ToString("yyyy-MM-dd"));
        public bool SplitLine1 => true;
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

    public class SellerDetail
    {
        public IEnumerable<BanLIst> banlist { private get; set; }
        public IEnumerable<string> Reasons => banlist.Select(x => x.Reason);
        public IEnumerable<string> EndTimes => banlist.Select(x => x.EndTime.ToString("yyyy-MM-dd"));
        public bool SplitLine1 => true;
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
        public bool SplitLine2 => true;
        public Seller seller { private get; set; }
        public string CompanyName => seller?.CompanyName;
        public string TaxIDNumber => seller?.TaxIDNumber;
        public string SellerHomePage => seller?.SellerHomePage;
        public string SellerPhone => seller?.SellerPhone;
        public string SellerDiscription => seller?.SellerDeccription;
        public string fPass => seller?.fPass == null ? "尚未審核" : seller?.fPass == true ? "審核通過" : "審核不通過";
    }
}