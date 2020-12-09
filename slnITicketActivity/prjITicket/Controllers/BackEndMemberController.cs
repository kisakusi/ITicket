using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using prjITicket.Models;

namespace prjITicket.Controllers
{
    public class BackEndMemberController : Controller
    {
        TicketSysEntities db = new TicketSysEntities();

        public ActionResult MemberList()
        {
            return View();
        }

        [HttpPost]
        public JsonResult MemberList(MemberAjax f)
        {
            int skip = (f.PageCurrent - 1) * f.PageSize;
            int take = f.PageSize;
            int[] role = (f.MemberRoleInfo ?? "0").Select(x => int.Parse(x.ToString())).ToArray();
            IEnumerable<Member> query = MemberCRUD.Query(f.Keyword, role, f.MemberRoleIsBan);
            IEnumerable<Member> members = query.QuerySort(f.SortRule, skip, take);
            IEnumerable<MemberJson> jsons = members.Select(member => new MemberJson
            {
                Count = query.Count(),
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
                MerchantDetail json = new MerchantDetail
                {
                    member = member,
                    seller = db.Seller.FirstOrDefault(x => x.MemberId == id)
                };
                return Json(json);
            }
            else
            {
                GeneralDetail json = new GeneralDetail
                {
                    member = member
                };
                return Json(json);
            }
        }

        [HttpPost]
        public async Task<EmptyResult> SendMessageAsync(string members, string message)
        {
            try
            {
                IEnumerable<int> collection = string.IsNullOrEmpty(members) ?
                    db.Member.Select(x => x.MemberID).ToList() :
                    members.Split(',').Select(x => int.Parse(x));

                foreach (int id in collection)
                {
                    await MemberCRUD.SendMessageToMember(id, message);
                }
            }
            catch { }
            return new EmptyResult();
        }

        [HttpPost]
        public async Task<EmptyResult> BanMemberAsync(int id, string reason, DateTime endtime)
        {
            await MemberCRUD.BanMemberWithMessage(id, reason, endtime);
            return new EmptyResult();
        }

        [HttpPost]
        public EmptyResult UnBanMember()
        {
            return new EmptyResult();
        }

        [HttpPost]
        public async Task<EmptyResult> MultiBanMemberAsync(string members, string reason, DateTime endtime)
        {
            try
            {
                IEnumerable<int> collection = members.Split(',').Select(x => int.Parse(x));
                foreach (int id in collection)
                {
                    await MemberCRUD.BanMemberWithMessage(id, reason, endtime);
                }
            }
            catch { }
            return new EmptyResult();
        }
    }
}