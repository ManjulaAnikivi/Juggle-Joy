using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace juggle_joy.Models
{
    public class GiftValidation
    {
        public int GiftID { get; set; }
        public string GiftCode { get; set; }
        public int Duration { get; set; }
        public decimal ActualPrice { get; set; }
        public decimal DiscountedPrice { get; set; }
        public decimal DiscountPercent { get; set; }
        public string SenderEmail { get; set; }
        public string RecepientName { get; set; }
        public string RecepientEmail { get; set; }
        public System.DateTime SentDate { get; set; }
        public string Message { get; set; }
    }
}