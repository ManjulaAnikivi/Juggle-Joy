using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace juggle_joy.Models
{
    public class UserLoginValidation
    {
        [Required(ErrorMessage = "Email is required")]
        public string EmailID { get; set; }

        [Required(ErrorMessage = "Password is required")]
        public string PasswordHash { get; set; }

        //public bool remember_me { get; set; } 

    }
}