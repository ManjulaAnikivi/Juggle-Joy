using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace juggle_joy.Models
{
    public class LoginValidation
    {
        [Required(ErrorMessage = "Please enter username")]
        public string username { get; set; }

        [Required(ErrorMessage = "Please enter password")]
        public string password { get; set; }
    }
}