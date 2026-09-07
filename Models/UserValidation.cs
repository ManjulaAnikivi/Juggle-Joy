using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace juggle_joy.Models
{
    public class UserValidation
    {
        public int UserID { get; set; }

        [Required(ErrorMessage = "First name is required")]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Only letters are allowed")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [RegularExpression(@"^([\w\!\#$\%\&\'\*\+\-\/\=\?\^\`{\|\}\~]+\.)*[\w\!\#$\%\&\'\*\+\-\/\=\?\^\`{\|\}\~]+@((((([a-zA-Z0-9]{1}[a-zA-Z0-9\-]{0,62}[a-zA-Z0-9]{1})|[a-zA-Z])\.)+[a-zA-Z]{2,6})|(\d{1,3}\.){3}\d{1,3}(\:\d{1,5})?)$", ErrorMessage = "Please enter valid E-mail")]
        [System.Web.Mvc.Remote("IsEmailIdExist", "Home", ErrorMessage = "Email ID already exists")]

        public string EmailID { get; set; }
        [Required(ErrorMessage = "Mobile number is required")]
        [RegularExpression(@"^[0-9]{8,15}$", ErrorMessage = "Phone number must be between 8 to 15 digits")]
        public string PhoneNo { get; set; }

        [Required(ErrorMessage = "Address is required")]
        public string Address { get; set; }
        [Required(ErrorMessage = "City is required")]
        public string City { get; set; }
        [Required(ErrorMessage = "Postal code is required")]
        //[RegularExpression(@"^[A-Za-z0-9\s]{6,10}$", ErrorMessage = "Must contain 6 to 10 Alphanumerics are allowed")]
        public string ZipPostalCode { get; set; }
        [Required(ErrorMessage = "Select your country")]
        public int CountryID { get; set; }
        [Required(ErrorMessage = "Select your State")]
        public int State { get; set; }

        [Required(ErrorMessage = "User name is required")]
        public string UserName { get; set; }
        [Required(ErrorMessage = "Password is required")]
        [RegularExpression(@"^(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9]).{8,}$", ErrorMessage = " ")]
        public string PasswordHash { get; set; }

        
        [Required(ErrorMessage = "Please select above")]
        public int HeardFrom { get; set; }
        //[Required(ErrorMessage = "Email is required")]
        [RegularExpression(@"^([\w\!\#$\%\&\'\*\+\-\/\=\?\^\`{\|\}\~]+\.)*[\w\!\#$\%\&\'\*\+\-\/\=\?\^\`{\|\}\~]+@((((([a-zA-Z0-9]{1}[a-zA-Z0-9\-]{0,62}[a-zA-Z0-9]{1})|[a-zA-Z])\.)+[a-zA-Z]{2,6})|(\d{1,3}\.){3}\d{1,3}(\:\d{1,5})?)$", ErrorMessage = "Please enter valid E-mail")]
        [System.Web.Mvc.Remote("IsPartnerEmailIdExist", "Home", ErrorMessage = "Email ID already exists")]
        public string PartnerEmail { get; set; }

        public string UserGoogleCaptchaToken { get; set; }

       
        [Required(ErrorMessage = "Country is required")]
        public string CountryName { get; set; }

    }

    public class AssistantValidation
    {
        public int UserID { get; set; }

        [Required(ErrorMessage = "Handler is required")]
        public int HandlerID { get; set; }


        [Required(ErrorMessage = "First name is required")]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Only letters are allowed")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required")]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Only letters are allowed")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [RegularExpression(@"^([\w\!\#$\%\&\'\*\+\-\/\=\?\^\`{\|\}\~]+\.)*[\w\!\#$\%\&\'\*\+\-\/\=\?\^\`{\|\}\~]+@((((([a-zA-Z0-9]{1}[a-zA-Z0-9\-]{0,62}[a-zA-Z0-9]{1})|[a-zA-Z])\.)+[a-zA-Z]{2,6})|(\d{1,3}\.){3}\d{1,3}(\:\d{1,5})?)$", ErrorMessage = "Please enter valid E-mail")]
        [System.Web.Mvc.Remote("IsEmailIdExist", "assistant", AdditionalFields = "UserID", ErrorMessage = "Email ID already exists")]
        public string EmailID { get; set; }


        public string PhoneNo { get; set; }

        [Required(ErrorMessage = "Address is required")]
        public string Address { get; set; }
        [Required(ErrorMessage = "City is required")]
        public string City { get; set; }
        [Required(ErrorMessage = "Postal code is required")]
        public string ZipPostalCode { get; set; }
        [Required(ErrorMessage = "Select your country")]
        public int CountryID { get; set; }
        [Required(ErrorMessage = "Select your State")]
        public int State { get; set; }

        [Required(ErrorMessage = "User name is required")]
        [System.Web.Mvc.Remote("IsUserNameExixt", "assistant", AdditionalFields = "UserID", ErrorMessage = "User name already exists")]
        public string UserName { get; set; }
        [Required(ErrorMessage = "Password is required")]
        [RegularExpression(@"^(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9])(?=.*?[#?!@$%^&*-]).{8,}$", ErrorMessage = " ")]


        public string PasswordHash { get; set; }

        [Required(ErrorMessage = "Confirm your password")]
        [Compare("PasswordHash", ErrorMessage = "Password and confirm password should be same.")]
        public string ConfirmPassword { get; set; }

        //[Display(Name = "Privacypolicy")]
        //[Range(typeof(bool), "true", "true", ErrorMessage = "Please read and agree to the privacy policy")]
        //public bool PrivacyPolicy { get; set; }

        // public string UserType { get; set; }

        public string ContactAssistant { get; set; }
        public int HeardFrom { get; set; }
    }

    public class HandlerValidation
    {
        public int UserID { get; set; }

        [Required(ErrorMessage = "First name is required")]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Only letters are allowed")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required")]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Only letters are allowed")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [RegularExpression(@"^([\w\!\#$\%\&\'\*\+\-\/\=\?\^\`{\|\}\~]+\.)*[\w\!\#$\%\&\'\*\+\-\/\=\?\^\`{\|\}\~]+@((((([a-zA-Z0-9]{1}[a-zA-Z0-9\-]{0,62}[a-zA-Z0-9]{1})|[a-zA-Z])\.)+[a-zA-Z]{2,6})|(\d{1,3}\.){3}\d{1,3}(\:\d{1,5})?)$", ErrorMessage = "Please enter valid E-mail")]
        [System.Web.Mvc.Remote("IsEmailIdExist", "handler", AdditionalFields = "UserID", ErrorMessage = "Email ID already exists")]
        public string EmailID { get; set; }


        public string PhoneNo { get; set; }

        [Required(ErrorMessage = "Address is required")]
        public string Address { get; set; }
        [Required(ErrorMessage = "City is required")]
        public string City { get; set; }
        [Required(ErrorMessage = "Postal code is required")]
        public string ZipPostalCode { get; set; }
        [Required(ErrorMessage = "Select your country")]
        public int CountryID { get; set; }
        [Required(ErrorMessage = "Select your State")]
        public int State { get; set; }

        [Required(ErrorMessage = "User name is required")]
        [System.Web.Mvc.Remote("IsUserNameExixt", "Handler", AdditionalFields = "UserID", ErrorMessage = "User name already exists")]
        public string UserName { get; set; }
        [Required(ErrorMessage = "Password is required")]
        [RegularExpression(@"^(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9])(?=.*?[#?!@$%^&*-]).{8,}$", ErrorMessage = " ")]


        public string PasswordHash { get; set; }

        [Required(ErrorMessage = "Confirm your password")]
        [Compare("PasswordHash", ErrorMessage = "Password and confirm password should be same.")]
        public string ConfirmPassword { get; set; }

        //[Display(Name = "Privacypolicy")]
        //[Range(typeof(bool), "true", "true", ErrorMessage = "Please read and agree to the privacy policy")]
        //public bool PrivacyPolicy { get; set; }

        // public string UserType { get; set; }

        public string ContactAssistant { get; set; }
        public int HeardFrom { get; set; }
    }
}


