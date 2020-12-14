using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using prjITicket.Models;

namespace prjITicket.Controllers
{
    public class BackEndMemberController : Controller, IDisposable
    {
        TicketSysEntities db = new TicketSysEntities();

        public ActionResult MemberList()
        {
            if (Session[CDictionary.SK_Logined_Member] == null||
                (Session[CDictionary.SK_Logined_Member] as Member).MemberRoleId!=4)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        public async Task<string> SendMessageAsync(string state, string members, string message)
        {
            try
            {
                List<int> collection = null;
                switch (state)
                {
                    case "All":
                    case "會員":
                        collection = db.Member.Select(x => x.MemberID).ToList();
                        break;
                    case "商家":
                        collection = db.Member.Where(x => x.MemberRoleId == 3).Select(x => x.MemberID).ToList();
                        break;
                    case "普通會員":
                        collection = db.Member.Where(x => x.MemberRoleId == 2).Select(x => x.MemberID).ToList();
                        break;
                    case "未驗證會員":
                        collection = db.Member.Where(x => x.MemberRoleId == 1).Select(x => x.MemberID).ToList();
                        break;
                    case "停權會員":
                        collection = db.BanLIst.Where(x => x.EndTime > DateTime.Now)
                            .Select(x => x.BanMemberId).Distinct().ToList();
                        break;
                    case "指定會員":
                        collection = members.Split(',').Select(x => int.Parse(x)).ToList();
                        break;
                }
                List<Task> tasks = new List<Task>();
                foreach (int id in collection)
                {
                    tasks.Add(Task.Run(() => MemberCRUD.SendMessageToMember(id, message)));
                }
                await Task.WhenAll(tasks);
                return "系統通知發送完畢!";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        [HttpPost]
        public JsonResult MerchantList(MemberAjax m)
        {
            int skip = m.PageSize * (m.PageCurrent - 1);
            int take = m.PageSize;
            IEnumerable<Seller> query = MemberCRUD.SellerQuery(m.Keyword, m.NonVerify);
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
            IEnumerable<Seller> sellers = query.SellerSort(m.SortRule, skip, take);
            IEnumerable<SellerJson> jsons = sellers.Select(seller => new SellerJson
            {
                Count = count,
                ChangePage = changepage,
                seller = seller,
                banlists = db.BanLIst
            });
            return Json(jsons);
        }

        [HttpPost]
        public JsonResult GeneralList(MemberAjax m)
        {
            int skip = m.PageSize * (m.PageCurrent - 1);
            int take = m.PageSize;
            IEnumerable<Member> query = MemberCRUD.MemberQuery(m.RoleId, m.Keyword);
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
            IEnumerable<Member> members = query.MemberSort(m.SortRule, skip, take);
            IEnumerable<MemberJson> jsons = members.Select(member => new MemberJson
            {
                Count = count,
                ChangePage = changepage,
                member = member,
                banlists = db.BanLIst
            });
            return Json(jsons);
        }

        [HttpPost]
        public JsonResult BanMemberList(MemberAjax m)
        {
            int skip = m.PageSize * (m.PageCurrent - 1);
            int take = m.PageSize;
            IEnumerable<Member> query = MemberCRUD.BanMemberQuery(m.Keyword);
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
            IEnumerable<Member> members = query.MemberSort(m.SortRule, skip, take);
            IEnumerable<MemberJson> jsons = members.Select(member => new MemberJson
            {
                Count = count,
                ChangePage = changepage,
                member = member,
                banlists = db.BanLIst
            });
            return Json(jsons);
        }

        [HttpPost]
        public JsonResult MemberDetail(int id)
        {
            Member member = db.Member.FirstOrDefault(x => x.MemberID == id);
            if (member.MemberRoleId == 3)
            {
                SellerDetail json = new SellerDetail
                {
                    member = member,
                    seller = db.Seller.FirstOrDefault(x => x.MemberId == id)
                };
                return Json(json);
            }
            else
            {
                MemberDetail json = new MemberDetail
                {
                    member = member
                };
                return Json(json);
            }
        }

        [HttpPost]
        public async Task<EmptyResult> BanMember(int id, string reason, DateTime endtime)
        {
            int adminId = (Session[CDictionary.SK_Admin_Logined_Member] as Member).MemberID;
            await MemberCRUD.BanMemberWithMessage(id, adminId, reason, endtime);
            return new EmptyResult();
        }

        [HttpPost]
        public async Task<EmptyResult> UnBanMember(int id)
        {
            await MemberCRUD.UnBanMemberWithMessage(id);
            return new EmptyResult();
        }

        [HttpPost]
        public async Task<EmptyResult> MerchantVerification(int id, bool fPass)
        {
            await MemberCRUD.MerchantVerificationWithMessage(id, fPass);
            return new EmptyResult();
        }

        [HttpPost]
        public string DataDownloadCheck(int id)
        {
            try
            {
                string filename = db.Seller.FirstOrDefault(x => x.MemberId == id).fFileName;
                string filepath = Server.MapPath($"~/Content/Login/SellerImage/{filename}");
                FileStream stream = new FileStream(filepath, FileMode.Open, FileAccess.Read, FileShare.Read);
                return "Success";
            }
            catch
            {
                return "Failure";
            }
        }

        [HttpGet]
        public ActionResult DataDownload(int id)
        {
            if (Session[CDictionary.SK_Logined_Member] == null||
                (Session[CDictionary.SK_Logined_Member] as Member).MemberRoleId!=4)
            {
                return Content($"<script>window.close()</script>");
            }
            try
            {
                string filename = db.Seller.FirstOrDefault(x => x.MemberId == id).fFileName;
                string filepath = Server.MapPath($"~/Content/Login/SellerImage/{filename}");
                FileStream stream = new FileStream(filepath, FileMode.Open, FileAccess.Read, FileShare.Read);
                return File(stream, "application/msword", filename);
            }
            catch
            {
                return Content($"<script>window.close()</script>");
            }
        }
    }
}