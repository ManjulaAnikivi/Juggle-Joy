using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace juggle_joy.Models
{
    public class CancelReasonValidationcs
    {
        [Required(ErrorMessage = "Please select reason for unsubscribing")]
        public int CancelID { get; set; }
        [Required(ErrorMessage = "Please select reason for leaving")]
        public int CancelCatID { get; set; }
        public string OtherReason { get; set; }

        public string subscriptionId { get; set; }
    }
}