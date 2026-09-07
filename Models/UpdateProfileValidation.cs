using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace juggle_joy.Models
{
    public class UpdateProfileValidation
    {
        public int AdminID { get; set; }

        [Required(ErrorMessage = "Please enter fullname")]
        public string FullName { get; set; }
        public string UserName { get; set; }

        [Required(ErrorMessage = "Please enter email ID")]
        [RegularExpression(@"^([\w\!\#$\%\&\'\*\+\-\/\=\?\^\`{\|\}\~]+\.)*[\w\!\#$\%\&\'\*\+\-\/\=\?\^\`{\|\}\~]+@((((([a-zA-Z0-9]{1}[a-zA-Z0-9\-]{0,62}[a-zA-Z0-9]{1})|[a-zA-Z])\.)+[a-zA-Z]{2,6})|(\d{1,3}\.){3}\d{1,3}(\:\d{1,5})?)$", ErrorMessage = "Please enter valid E-mail")]
        public string EmailID { get; set; }

        [Required(ErrorMessage = "Please enter phone no")]
        [RegularExpression(@"^[0-9]{8,15}$", ErrorMessage = "Phone number must be between 8 to 15 digits")]
        public string PhoneNo { get; set; }
  
    }
}