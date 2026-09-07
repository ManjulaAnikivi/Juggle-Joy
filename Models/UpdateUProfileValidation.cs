using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace juggle_joy.Models
{
    public class UpdateUProfileValidation
    {
        public int UserID { get; set; }

        //[Required(ErrorMessage = "First name is required")]
        //[RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Only letters are allowed")]
        public string FirstName { get; set; }

        //[Required(ErrorMessage = "Last name is required")]
        //[RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Only letters are allowed")]
        public string LastName { get; set; }

        public string FullName { get; set; }

        [Required(ErrorMessage = "Email id is required")]
        public string EmailID { get; set; }


        [Required(ErrorMessage = "Phone number is required")]
        public string PhoneNo { get; set; }

        [Required(ErrorMessage = "Postal code is required")]
        public string ZipPostalCode { get; set; }

        [Required(ErrorMessage = "This field is required")]
        public int HeardFrom { get; set; }

        //[Required(ErrorMessage = "Partners email id is required")]
        [RegularExpression(@"[a-z0-9._%+-]+@[a-z0-9.-]+\.[a-z]{2,4}", ErrorMessage = "Please enter valid Email ID")]
        [System.Web.Mvc.Remote("PartnerEmailIdExist", "Home", AdditionalFields = "UserID",ErrorMessage = "Email ID already exists")]
        public string Partner_EmailID { get; set; }

        [Required(ErrorMessage = "Address is required")]
        public string Address { get; set; }

        [Required(ErrorMessage = "City is required")]
        public string City { get; set; }

        [Required(ErrorMessage = "State is required")]
        public string StateID { get; set; }

        [Required(ErrorMessage = "Country is required")]
        public string CountryID { get; set; }

        public int otp { get; set; }

    }
}