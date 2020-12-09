using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace prjITicket.Models
{
    public class COrderDetailByTicketGroup
    {
        public decimal Discount { get; set; }
        public int Quantity { get; set; }
        public List<TicketDesc> Tickets { get; set; }
    }
    public class TicketDesc
    {
        public int TicketCategoryId { get; set; }
        public int TicketTimeId { get; set; }
    }
}