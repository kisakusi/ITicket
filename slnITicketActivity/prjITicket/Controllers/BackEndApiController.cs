using BackEnd.ViewModel;
using LinqKit;
using prjITicket.Models;
using prjITicket.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace BackEnd.Controllers
{
    public class formDataClass
    {
        private string statusID0;
        private string statusID1;
        private string statusID2;
        private string companyName;
        private string activityName;

        public string StatusID0 { get => statusID0; set => statusID0 = value; }
        public string StatusID1 { get => statusID1; set => statusID1 = value; }
        public string StatusID2 { get => statusID2; set => statusID2 = value; }
        public string CompanyName { get => companyName; set => companyName = value; }
        public string ActivityName { get => activityName; set => activityName = value; }
    }
    public class updateActivtyStatus
    {
        private int statusID;
        private int activityID;

        public int StatusID { get => statusID; set => statusID = value; }
        public int ActivityID { get => activityID; set => activityID = value; }
    }
    public class OrderQuery
    {
        private string orderStatus0;//未付款
        private string orderStatus1; //已付款
        private string orderGuid;//訂單編號
        private string username;//訂購人姓名
        private string useremail;

        public string OrderGuid { get => orderGuid; set => orderGuid = value; }
        public string OrderStatus0 { get => orderStatus0; set => orderStatus0 = value; }
        public string OrderStatus1 { get => orderStatus1; set => orderStatus1 = value; }
        public string Username { get => username; set => username = value; }
        public string UserEmail { get => useremail; set => useremail = value; }
    }
    public class OrderDetail
    {
        private string orderID;

        public string OrderID { get => orderID; set => orderID = value; }
    }

    public class WebApiController : ApiController
    {

        TicketSysEntities ticket = new TicketSysEntities();

        //後臺產品查詢API
        [HttpPost]
        public List<CBackEndActivity> getActivity([FromBody]formDataClass formDataClass)
        {
            string StatusID0 = formDataClass.StatusID0;
            int AcStatusID0;
            int.TryParse(StatusID0, out AcStatusID0);

            string StatusID1 = formDataClass.StatusID1;
            int AcStatusID1;
            int.TryParse(StatusID1, out AcStatusID1);

            string StatusID2 = formDataClass.StatusID2;
            int AcStatusID2;
            int.TryParse(StatusID2, out AcStatusID2);

            string CompanyName = !string.IsNullOrEmpty(formDataClass.CompanyName) ? formDataClass.CompanyName.Trim() : null;
            string ActivityName = !string.IsNullOrEmpty(formDataClass.ActivityName) ? formDataClass.ActivityName.Trim() : null;

            IQueryable<CBackEndActivity> backEndActivities = null;


            Activity activity = new Activity();
            Seller seller = new Seller();

            var predicate_Activity = PredicateBuilder.New<Activity>(true);
            var predicate_seller = PredicateBuilder.New<Seller>(true);
            var predicate_Status = PredicateBuilder.New<ActivityStatus>(false);


            if (!string.IsNullOrEmpty(ActivityName))
                predicate_Activity = predicate_Activity.And(a => a.ActivityName.Contains(ActivityName));

            if (!string.IsNullOrEmpty(CompanyName))
                predicate_seller = predicate_seller.And(s => s.CompanyName.Contains(CompanyName));

            if (!string.IsNullOrEmpty(StatusID0))
                predicate_Status = predicate_Status.Or(st => st.ActivityStatusID.Equals(AcStatusID0));

            if (!string.IsNullOrEmpty(StatusID1))
                predicate_Status = predicate_Status.Or(st => st.ActivityStatusID.Equals(AcStatusID1));

            if (!string.IsNullOrEmpty(StatusID2))
                predicate_Status = predicate_Status.Or(st => st.ActivityStatusID.Equals(AcStatusID2));

            backEndActivities = from a in ticket.Activity.Where(predicate_Activity)
                                join s in ticket.Seller.Where(predicate_seller)
                                on a.SellerID equals s.SellerID
                                join status in ticket.ActivityStatus.Where(predicate_Status)
                                on a.ActivityStatusID equals status.ActivityStatusID
                                select new CBackEndActivity { ActivityEntity = a, Seller = s, ActivityStatus = status };

            return backEndActivities.ToList();
        }

        //後台產品審核API
        [HttpPut]
        public string updateActivtyStatus([FromBody]updateActivtyStatus updateActivtyStatus)
        {
            try
            {
                int ActivityID = updateActivtyStatus.ActivityID;
                int StatusID = updateActivtyStatus.StatusID;

                var Activity = ticket.Activity.Where(ac => ac.ActivityID == ActivityID).FirstOrDefault();
                Activity.ActivityStatusID = StatusID;
                int num = ticket.SaveChanges();
                if (num == 1)
                {
                    return "true";
                }
                else
                {
                    return "false";
                }
            }
            catch (Exception ex)
            {
                return (ex.Message);

            }

        }

        //後台訂單查詢API
        [HttpPost]
        public List<CBackEndOrders> getOrders([FromBody]OrderQuery orderQuery)
        {
            string OrderStatus0 = orderQuery.OrderStatus0;
            bool OrderStatus0_bool;
            bool.TryParse(OrderStatus0, out OrderStatus0_bool);

            string OrderStatus1 = orderQuery.OrderStatus1;
            bool OrderStatus1_bool;
            bool.TryParse(OrderStatus1, out OrderStatus1_bool);


            string OrderGuid = !string.IsNullOrEmpty(orderQuery.OrderGuid) ? orderQuery.OrderGuid.Trim() : null;
            string UserName = !string.IsNullOrEmpty(orderQuery.Username) ? orderQuery.Username.Trim() : null;
            string UserEmail = !string.IsNullOrEmpty(orderQuery.UserEmail) ? orderQuery.UserEmail.Trim() : null;

            IQueryable<CBackEndOrders> BackEndOrders = null;

            var predicate_OrdersStatus = PredicateBuilder.New<Orders>(false);
            var predicate_OrderGuid = PredicateBuilder.New<Orders>(true);
            var predicate_UserName = PredicateBuilder.New<Orders>(true);
            var predicate_UserEmail = PredicateBuilder.New<Orders>(true);


            if (!string.IsNullOrEmpty(OrderGuid))
                predicate_OrderGuid = predicate_OrderGuid.And(o => o.OrderGuid.Equals(OrderGuid));

            if (!string.IsNullOrEmpty(UserName))
                predicate_UserName = predicate_UserName.And(o => o.Name.Equals(UserName));

            if (!string.IsNullOrEmpty(UserEmail))
                predicate_UserEmail = predicate_UserEmail.And(o => o.Email.Equals(UserEmail));

            if (!string.IsNullOrEmpty(OrderStatus0))
                predicate_OrdersStatus = predicate_OrdersStatus.Or(o => o.OrderStatus.Equals(OrderStatus0_bool));

            if (!string.IsNullOrEmpty(OrderStatus1))
                predicate_OrdersStatus = predicate_OrdersStatus.Or(o => o.OrderStatus.Equals(OrderStatus1_bool));

            BackEndOrders = from o in ticket.Orders.Where(predicate_OrdersStatus).Where(predicate_OrderGuid).Where(predicate_UserName).Where(predicate_UserEmail)
                            select new CBackEndOrders
                            {
                                Orders = o,
                            };

            return BackEndOrders.ToList();
        }


        //後臺訂單明細API
        [HttpPost]
        public List<CBackEndOrderDetail> getOrderDetail([FromBody]OrderDetail orderDetail)
        {
            string OrderID = orderDetail.OrderID;        
            int Orderid;
            int.TryParse(OrderID, out Orderid);


            IQueryable<CBackEndOrderDetail> BackEndOrders = null;

            Orders Orders = new Orders();
            Order_Detail OrderDetail = new Order_Detail();
            Districts Districts = new Districts();
            Cities Cities = new Cities();
            Tickets Tickets = new Tickets();
            Activity Activity = new Activity();
            TicketTimes TicketTimes = new TicketTimes();
            TicketCategory TicketCategory = new TicketCategory();

            var predicate_Orders = PredicateBuilder.New<Orders>(true);
            var Predicate_OrderDetail = PredicateBuilder.New<Order_Detail>(true);
            var Predicate_Activity = PredicateBuilder.New<Activity>(true);
            var Predicate_Tickets = PredicateBuilder.New<Tickets>(true);
            var Predicate_TicketTimes = PredicateBuilder.New<TicketTimes>(true);
            var Predicate_TicletCategory = PredicateBuilder.New<TicketCategory>(true);
            var Predicate_Districts = PredicateBuilder.New<Districts>(true);
            var Predicate_Cities = PredicateBuilder.New<Cities>(true);

            if (!string.IsNullOrEmpty(OrderID))
                predicate_Orders = predicate_Orders.And(o => o.OrderID.Equals(Orderid));


            BackEndOrders = from o in ticket.Orders.Where(predicate_Orders)
                            join od in ticket.Order_Detail.Where(Predicate_OrderDetail)
                            on o.OrderID equals od.OrderID

                            join t in ticket.Tickets.Where(Predicate_Tickets)
                            on od.TicketId equals t.TicketID

                            join tt in ticket.TicketTimes.Where(Predicate_TicketTimes)
                            on t.TicketTimeId equals tt.TicketTimeId

                            join tc in ticket.TicketCategory.Where(Predicate_TicletCategory)
                            on t.TicketCategoryId equals tc.TicketCategoryId

                            join ac in ticket.Activity.Where(Predicate_Activity)
                            on tt.ActivityId equals ac.ActivityID

                            join d in ticket.Districts.Where(Predicate_Districts)
                            on o.DistrictId equals d.DistrictId

                            join c in ticket.Cities.Where(Predicate_Cities)
                            on d.CityId equals c.CityID

                            select new CBackEndOrderDetail
                            {
                                Orders = o,
                                OrderDetail = od,
                                Districts = d,
                                Cities = c,
                                Tickets = t,
                                TicketTimes = tt,
                                Activity = ac,
                                TicketCategory = tc
                            };

            return BackEndOrders.ToList();
        }
    }
}
