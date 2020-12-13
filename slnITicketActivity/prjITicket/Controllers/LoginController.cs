using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using PagedList;
using prjITicket.Models;
using prjITicket.Service;
using prjITicket.ViewModel;



namespace prjITicket.Controllers
{
    public class LoginController : Controller
    {
        // GET: Login
        TicketSysEntities db = new TicketSysEntities();
        //實體化MailService，以引用內部方法進行Email驗證
        MailService mailService = new MailService();

        //第三方登入=======
        public string FBLogin(string returnUrl)
        {
            facebook ms = JsonConvert.DeserializeObject<facebook>(returnUrl);

            var member = db.Member.Where(x => x.Email == ms.email).FirstOrDefault();
            if (member == null)
            {
                Member m = new Member();
                m.Email = ms.email;
                m.Name = ms.name;
                m.NickName = ms.name;
                m.MemberRoleId = 2;
                m.Point = 0;
                m.providerFB = true;
                db.Member.Add(m);
                db.SaveChanges();
               var Nmember = db.Member.Where(x => x.Email == ms.email).FirstOrDefault();
                Session[CDictionary.SK_Logined_Member] = Nmember;
                
                (Session[CDictionary.SK_Logined_Member] as Member).provider = "facebook";
               
            }
            else
            {
                if (member.providerFB !=true)
                {
                    member.providerFB = true;
                    db.SaveChanges();
                    Session[CDictionary.SK_Logined_Member] = member;
                    (Session[CDictionary.SK_Logined_Member] as Member).provider = "facebook";
                }
                else 
                {
                    Session[CDictionary.SK_Logined_Member] = member;
                    (Session[CDictionary.SK_Logined_Member] as Member).provider = "facebook";
                }
               
            }


            return "驗證成功";//RedirectToAction("ActivityList", "Activity");

        }

        public string GoLogin(string returnUrl)
        {
            google ms = JsonConvert.DeserializeObject<google>(returnUrl);

            var member = db.Member.Where(x => x.Email == ms.du).FirstOrDefault();
            if (member == null)
            {
                Member m = new Member();
                m.Email = ms.du;
                m.Name = ms.Ad;
                m.NickName = ms.Ad;
                m.MemberRoleId = 2;
                m.Point = 0;
                m.providerGO = true;
                db.Member.Add(m);
                db.SaveChanges();
                member = db.Member.Where(x => x.Email == ms.du).FirstOrDefault();
                Session[CDictionary.SK_Logined_Member] = member;
                
                (Session[CDictionary.SK_Logined_Member] as Member).provider = "google";
            }
            else
            {
                if (member.providerGO != true)
                {
                    member.providerGO = true;
                    db.SaveChanges();
                    Session[CDictionary.SK_Logined_Member] = member;
                    (Session[CDictionary.SK_Logined_Member] as Member).provider = "google";
                }
                else
                {
                    Session[CDictionary.SK_Logined_Member] = member;
                    (Session[CDictionary.SK_Logined_Member] as Member).provider = "google";
                }

            }


            return "驗證成功";//RedirectToAction("ActivityList", "Activity");

        }
        //第三方登入End=======

        //登入
        public ActionResult Login()
        {
            return View();
        }
        
        [HttpPost]
        public ActionResult Login(string Email, string Password,FormCollection form)
        {
            //string admin = "admin";
            //int time = (DateTime.Now.Month * DateTime.Now.Day) + 2;
            //string adminPassword = "msit128" + time.ToString();

            //CAdmin ad = new CAdmin();            
            //ad.Admin = admin;
            //ad.AdminPassword = adminPassword;
            //List<CAdmin> AdMember = new List<CAdmin>();
            //AdMember.Add(ad);
            //if (Email==admin && Password==adminPassword)
            //{
            //    Session[CDictionary.SK_Admin_Logined_Member] = AdMember;
            //    return RedirectToAction("ActivityList", "Activity");
            //}
          
            var isVerify = new GoogleReCaptcha().GetCaptchaResponse(form["g-recaptcha-response"]);
            if (isVerify)
            {
                var member = db.Member
                    .Where(m => m.Email == Email && m.Password == Password)
                    .FirstOrDefault();

                //若member為null，表示會員未註冊
                if (member == null)
                {
                    ViewBag.Message = "帳密錯誤，登入失敗";
                    return View();
                }
                else if (member.MemberRoleId == 1)
                {
                    ViewBag.Message = "尚未進行信箱驗證，請至信箱進行驗證作業";
                    return View();
                }

                if (member.MemberRole.MemberRoleName == "管理者")
                {
                    Session[CDictionary.SK_Admin_Logined_Member] = member;
                }
                else
                {
                    Session[CDictionary.SK_Logined_Member] = member;
                }
            }
            else {
                ViewBag.Message = "請勾選驗證機器人";
                return View();
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public string ValidateReCAPTCHA(string response)
        {
            var SECRET_KEY = "6LdLEP8ZAAAAAINndsRWW6iVg5w6-XsLJN841xfk";
            var client = new WebClient();
            var reply =
                client.DownloadString(
                    string.Format("https://www.google.com/recaptcha/api/siteverify?secret={0}&response={1}", SECRET_KEY, response));
            return reply;
        }

        //接收驗證信連接密碼修改頁面
        //==================================================
        public ActionResult PasswordValidate(string UserName, string RegisterCheckCode)
        {
            Member member = db.Member.Where(m => m.Email == UserName).FirstOrDefault();
            //CMember s = new CMember() { entity = member };
            ViewBag.email = UserName;
            return View();
        }
        [HttpPost,ActionName("PasswordValidate")]
        public ActionResult PasswordValidatePost(string Password,string email)
        {
            
            Member member = db.Member.Where(m => m.Email == email).FirstOrDefault();
            member.Password = Password;
            db.SaveChanges();
            
            return RedirectToAction("Login","Login");
        }

        public ActionResult Forget()
        {
            return View();
        }
        [HttpPost]
        public string Forget1(QMember formData)
        {
           var mem= db.Member.Where(x => x.Email == formData.Email).FirstOrDefault();
            if (mem == null) 
            {
                return "無此帳號";
            }
            else 
            {
                //取得信箱驗證碼
                //======================================
                string RegisterCheckCode = mailService.GetValidateCode();
                //取得寫好的驗證範本內容
                string TempMail = System.IO.File.ReadAllText(
                    Server.MapPath("~/Views/Shared/ForgetEmail.html"));
                //宣告驗證Email驗證用的Url
                UriBuilder ValidateUrl = new UriBuilder(Request.Url)
                {
                    Path = Url.Action("PasswordValidate", "Login",
                    new { UserName = formData.Email, RegisterCheckCode = RegisterCheckCode })
                };
                //藉由Service將使用者資料填入驗證信範本中
                string MailBody = mailService.GetRegisterMailBody(TempMail,
                    formData.Email, ValidateUrl.ToString().Replace("%3F", "?"));
                //呼叫Service寄出驗證信
                mailService.SendRegisterMail(MailBody, formData.Email);
            }
  
            return "成功";//RedirectToAction("Login", "Login");
        }


        //登出
        public ActionResult Logout()
        {
            Session.Clear();  
            return RedirectToAction("ActivityList", "Activity");
        }


        //註冊會員
        public ActionResult Register()
        {            
            return View();
        }

        [HttpPost]
        public ActionResult Register(QMember formData)
        {

            //若模型沒有通過驗證則顯示目前的View
            if (ModelState.IsValid == false)
            {
                return View();
            }
            else if (formData.agreeterm == false)
            {

                ViewBag.Message = "請勾選";
                return View();
            }

            // 依帳號取得會員並指定給member
            var member = db.Member
                .Where(m => m.Email == formData.Email)
                .FirstOrDefault();

            //取得信箱驗證碼
            //======================================
            string RegisterCheckCode = mailService.GetValidateCode();

            //若member為null，表示會員未註冊
            //======================================
            if (member == null)
            {
                Member m = new Member();
                m.Email = formData.Email;
                m.Password = formData.Password;
                m.Name = "Guest";
                m.NickName = "Guest";
                m.MemberRoleId = 1;
                m.Point = 0;
                m.RegisterCheckCode = RegisterCheckCode;
                db.Member.Add(m);
                db.SaveChanges();
                //取得寫好的驗證範本內容
                string TempMail = System.IO.File.ReadAllText(
                    Server.MapPath("~/Views/Shared/RegisterEmailTemplate.html"));
                //宣告驗證Email驗證用的Url
                UriBuilder ValidateUrl = new UriBuilder(Request.Url)
                {
                    Path = Url.Action("EmailValidate", "Login",
                    new { UserName = formData.Email, RegisterCheckCode = RegisterCheckCode })
                };
                //藉由Service將使用者資料填入驗證信範本中
                string MailBody = mailService.GetRegisterMailBody(TempMail,
                    formData.Email, ValidateUrl.ToString().Replace("%3F", "?"));
                //呼叫Service寄出驗證信
                mailService.SendRegisterMail(MailBody, formData.Email);



                return RedirectToAction("Login");
            }
            ViewBag.Message = "此帳號己有人使用，註冊失敗";
            return View();


        }

        //接收驗證信連接傳進來的Action
        //==================================================
        public ActionResult EmailValidate(string UserName, string RegisterCheckCode)
        {
            Member member = db.Member.Where(m => m.Email == UserName).FirstOrDefault();
            UserName = member.Email;
            ViewBag.EmailValidate = mailService.EmailValidate(UserName, RegisterCheckCode);
            return View();
        }

        public ActionResult BussRegister()
        {
          
         return View();
          
        }
        [HttpPost]
        public ActionResult BussRegister(QBussMember formData, HttpPostedFileBase FileSave)
        {
            //若模型沒有通過驗證則顯示目前的View
            if (ModelState.IsValid == false)
            {
                return View();
            }
            else if (formData.agreeterm == false)
            {

                ViewBag.Message = "請勾選";
                return View();
            }
            else if (FileSave == null) 
            {
                ViewBag.Message = "請上傳檔案";
                return View();
            }

            // 依帳號取得會員並指定給member
            var bussmember = db.Member
                .Where(m => m.Email == formData.Email)
                .FirstOrDefault();

            //取得信箱驗證碼
            //======================================
            string RegisterCheckCode = mailService.GetValidateCode();


            //若member為null，表示會員未註冊
            if (bussmember == null)
            {
                //FileSave.SaveAs(@"C:\FileSave\" + FileSave.FileName);
                Member m = new Member();
                m.Email = formData.Email;
                m.Password = formData.Password;
                m.Name = "Guest";
                m.NickName = "Guest";
                m.RegisterCheckCode = RegisterCheckCode;
                m.MemberRoleId = 1;
                m.Point = 0;
                db.Member.Add(m);
                db.SaveChanges();

                Seller s = new Seller();
                s.MemberId = m.MemberID;
                s.SellerPhone = formData.SellerPhone;
                s.CompanyName = formData.CompanyName;
                s.TaxIDNumber = formData.TaxIDNumber;
                s.fPass = false;
                db.Seller.Add(s);
                db.SaveChanges();

                //取得寫好的驗證範本內容
                string TempMail = System.IO.File.ReadAllText(
                    Server.MapPath("~/Views/Shared/RegisterEmailTemplate.html"));
                //宣告驗證Email驗證用的Url
                UriBuilder ValidateUrl = new UriBuilder(Request.Url)
                {
                    Path = Url.Action("EmailValidate", "Login",
                    new { UserName = formData.Email, RegisterCheckCode = RegisterCheckCode })
                };
                //藉由Service將使用者資料填入驗證信範本中
                string MailBody = mailService.GetRegisterMailBody(TempMail,
                    formData.Email, ValidateUrl.ToString().Replace("%3F", "?"));
                //呼叫Service寄出驗證信
                mailService.SendRegisterMail(MailBody, formData.Email);

                return RedirectToAction("Login");
            }

            ViewBag.Message = "此帳號己有人使用，註冊失敗";
            return View();
        }

        //提供下載檔案
        public ActionResult DemoDownload()
        {
            //@"\DemoFile\企業合同確認書.docx"
            FileInfo fl = new FileInfo(@"D:\DemoFile\企業合同確認書.docx");
            var cd = new System.Net.Mime.ContentDisposition
            {
                FileName = fl.Name,
                Inline = false,
            };
            Response.AppendHeader("Content -Disposition", cd.ToString());

            var readStream = new FileStream(fl.FullName, FileMode.Open, FileAccess.Read);
            string contentType = MimeMapping.GetMimeMapping(fl.FullName);
            return File(readStream, contentType);
        }
        public ActionResult MemberEdit()
        {
            
            if (Session[CDictionary.SK_Logined_Member] != null)
            {
                string email = (Session[CDictionary.SK_Logined_Member] as Member).Email;
                var member = db.Member.Where(x => x.Email == email).FirstOrDefault();
                member.Password = "";
                CMember c = new CMember { entity = member };
                return View(c);
            }
            return View();
        }
       
        public ActionResult BussEdit()
        {
            if (Session[CDictionary.SK_Logined_Member] != null)
            {
                string email = (Session[CDictionary.SK_Logined_Member] as Member).Email;
                int id=(Session[CDictionary.SK_Logined_Member] as Member).MemberID;
                var member = db.Member.Where(x => x.Email == email).FirstOrDefault();
                var seller=db.Seller.Where(x => x.MemberId == id).FirstOrDefault();
                member.Password = "";
                CBussMember buss = new CBussMember() { entity=member, BussEntity=seller };               
                return View(buss); 
            }
            return View();

        }

        public string MemberSave(QMember b)
        {
            
            int id = (Session[CDictionary.SK_Logined_Member] as Member).MemberID;
            Member prod = db.Member.FirstOrDefault(t => t.MemberID == id);
            //if (ModelState.IsValid == false)
            //{
            //    return "修改失敗";
            //}
            if (prod != null)
            {
                prod.Name = b.Name;
                prod.NickName = b.NickName;
                prod.Address = b.Address==null?null:b.Address;
                prod.BirthDate = b.BirthDate == null ? null : b.BirthDate;
                prod.IDentityNumber = b.IDentityNumber == null ? null : b.IDentityNumber;
                prod.Passport = b.Passport == null ? null : b.Passport;
                prod.Phone = b.Phone == null ? null : b.Phone;
                
                prod.Sex = b.Sex;//== null ? null : b.Sex;
                db.SaveChanges();
                return "修改成功";
            }
            return "修改失敗";

        }

        public string BussMemberSave(QBussMember b)
        {
            int id = (Session[CDictionary.SK_Logined_Member] as Member).MemberID;
            
            Seller prod = db.Seller.FirstOrDefault(t => t.MemberId == id);
            if (prod != null)
            {
                prod.CompanyName = b.CompanyName;
                prod.TaxIDNumber = b.TaxIDNumber;
                prod.SellerHomePage = b.SellerHomePage;
                prod.SellerDeccription = b.SellerDeccription;
                prod.SellerPhone = b.SellerPhone;               
                db.SaveChanges();
                return "修改成功";
            }
            return "修改失敗";

        }

        public string MemberPassSave(QMember b)
        {
            int id = (Session[CDictionary.SK_Logined_Member] as Member).MemberID;
            string password=(Session[CDictionary.SK_Logined_Member] as Member).Password;
            Member prod = db.Member.FirstOrDefault(t => t.MemberID == id&&t.Password==b.Password);
            if (prod != null&&b.NPassword!=null)
            {
                prod.Password = b.NPassword;
                db.SaveChanges();
                password = "";
                return "修改成功";              
            }
            password = "";
            return "修改失敗";

        }

        public string IsHasMember(string Email)
        {
            var member = db.Member
                .Where(m => m.Email == Email)
                .FirstOrDefault();
            //[a-zA-Z0-9._%+-]+@[a-zA-z0-9.-]+\.[a-zA-Z]{2,}$
            if (!Regex.IsMatch(Email, @"^[a-zA-Z0-9._%+-]+@[a-zA-z0-9.-]+\.[a-zA-Z]{2,}$"))
            {
                return "請輸入email格式";
            }
            if (Email == "")
            {
                return "請輸入帳號";
            }
            else if (member == null)
            {
                return "\u2705 可以使用此帳號";
            }
            else
            {
                return "此帳號己有人使用";
            }

        }
        public string IsPassword(string password)
        {
      
            if (!Regex.IsMatch(password, @"^(?=.*[a-zA-Z])(?=.*\d).{8,12}$"))
            {
                return "請輸入8-12碼,至少一個英文及數字";
            }           
            else
            {
                return "\u2705 密碼格式正確";
            }

        }

        public string TaxResult()
        {
            //using (WebClient server = new WebClient())
            //{
            //    try
            //    {
            //        server.Encoding = Encoding.UTF8;
            //        server.Headers.Add(HttpRequestHeader.ContentType, "application/json");
            //        var json = server.DownloadString("http://localhost:62254/api/FFoodpanda");
            //        List<CProduct> list = JsonConvert.DeserializeObject<List<CProduct>>(json);
            //        dataGridView1.DataSource = list;
            //    }
            //    catch (WebException ex)
            //    {

            //    }
            //}
            return "此統編認證正確";
        }

        //上傳會員照片
        //=====================================================
        public void CreateImage(string input)
        {
            //已登入
            if (Session[CDictionary.SK_Logined_Member] != null)
            {
                string email = (Session[CDictionary.SK_Logined_Member] as Member).Email;
                var member = db.Member.Where(m => m.Email == email).FirstOrDefault();
                input = input.Substring(input.IndexOf(",") + 1);  //把"data:image/png;base64,"這部分捨棄,取逗號後面的值 
                byte[] imgData = Convert.FromBase64String(input); //轉成2進位
                MemoryStream ms = new MemoryStream(imgData); //儲存資料流
                Bitmap bitmap = new Bitmap(ms);
                string photoName = Guid.NewGuid().ToString() + ".png"; //產生亂數檔名及副檔名
                bitmap.Save(Server.MapPath("~/images/Login/Upload/" + photoName), System.Drawing.Imaging.ImageFormat.Png);
                member.Icon = photoName; //儲存Icon
                db.SaveChanges();
                (Session[CDictionary.SK_Logined_Member] as Member).Icon = photoName;
            }
        }
        //載入所有城市至Cities資料庫
        //====================================================
        public void loadAllCities()
        {
            string[] cities = { "台北市", "基隆市", "新北市", "宜蘭縣", "桃園市", "新竹市", "新竹縣", "苗栗縣", "台中市", "彰化縣", "南投縣", "嘉義市", "嘉義縣", "雲林縣", "台南市", "高雄市", "澎湖縣", "金門縣", "屏東縣", "台東縣", "花蓮縣", "連江縣" };
            Cities city = new Cities();
            for (int i = 0; i < cities.Length; i++)
            {
                city.CityName = cities[i];
                db.Cities.Add(city);
                db.SaveChanges();
            }
        }
        //載入所有地區至Districts資料庫
        public void loadAllDistricts()
        {
            string[] cities = { "台北市", "基隆市", "新北市", "宜蘭縣", "桃園市", "新竹市", "新竹縣", "苗栗縣", "台中市", "彰化縣", "南投縣", "嘉義市", "嘉義縣", "雲林縣", "台南市", "高雄市", "澎湖縣", "金門縣", "屏東縣", "台東縣", "花蓮縣", "連江縣" };
            string[,] districts = new string[,]{{"中正區", "大同區", "中山區", "松山區", "大安區", "萬華區", "信義區", "士林區", "北投區", "內湖區", "南港區", "文山區" },
        { "100", "103", "104", "105", "106", "108", "110", "111", "112", "114", "115", "116"} };
            Districts districts1 = new Districts();
            for (int i = 0; i < districts.GetUpperBound(0); i++)
            {

            }
        }

        //載入會員修改頁面抓取City欄位
        //====================================================
        public string getAllCities()
        {
            //取得Cities資料庫內的資料
            var city = db.Cities.Select(c => new { c.CityID, c.CityName }).ToList();

            return JsonConvert.SerializeObject(city);
        }

        //藉由CityId取得Districts
        //====================================================
        public string getDistrictsByCityId(int cityId)
        {
            //取得Districts資料庫內的資料
            var districts = db.Districts.Where(d => d.CityId == cityId).Select(d => new { d.DistrictId, d.DistrictName }).ToList();
            return JsonConvert.SerializeObject(districts);
        }


        //藉由districtId取得postCode
        //====================================================
        public string getPostCodeByDistrictId(int districtId)
        {
            var postCode = db.Districts.FirstOrDefault(d => d.DistrictId == districtId).PostCode;
            return postCode;
        }

        //會員訂單管理查詢
        public ActionResult getOrderbyEmail(string Email, int page = 1)
        {
            int pagesize = 5;
            int pagecurrent = page < 1 ? 1 : page;
            List<Orders> order = db.Orders.Where(o => o.Email == Email).ToList();
            IPagedList<Orders> pagelist = order.ToPagedList(pagecurrent, pagesize);
            ViewBag.Email = Email;
            return PartialView("getOrderbyEmail", pagelist);
        }

        //會員我的收藏查詢
        public ActionResult getActivityFavouriteByMemberId(int MemberId, int page = 1)
        {
            int pagesize = 6;
            int pagecurrent = page < 1 ? 1 : page;
            List<ActivityFavourite> favourite = db.ActivityFavourite.Where(a => a.MemberId == MemberId).ToList();
            IPagedList<ActivityFavourite> pagelist = favourite.ToPagedList(pagecurrent, pagesize);
            ViewBag.MemberId = MemberId;
            return PartialView("getActivityFavouriteByMemberId", pagelist);
        }

        //todo
        //取得後台系統訊息 ShoppingCartList.cshtml參考
        public ActionResult getShortMassageByMemberId(int MemberId)
        {
            List<ShortMessage> massage = db.ShortMessage.Where(s => s.MemberID == MemberId).ToList();
            return PartialView(massage);
        }

    }
}