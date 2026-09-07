using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace juggle_joy.Models
{
    public class AssistantResetPasswordValidation
    {
        [Required]
        public string ResetCode { get; set; }

        [Required(ErrorMessage = "Please enter new password")]
        [RegularExpression(@"^(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9])(?=.*?[#?!@$%^&*-]).{8,}$", ErrorMessage = "Password must be minimum 8 characters and must contain lower case and upper case letters, numbers and 1 special character")]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "Please confirm new password")]
        [Compare("NewPassword", ErrorMessage = "Confirm password does not match")]
        public string ConfirmNewPassword { get; set; }
    }
}