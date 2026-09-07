using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace juggle_joy.Models
{
    public class Contact_Us_Validation
    {
        public int ContactId { get; set; }

        [Required(ErrorMessage = "Please enter your name")]
        public string ContactName { get; set; }

        [RegularExpression(@"^([\w\!\#$\%\&\'\*\+\-\/\=\?\^\`{\|\}\~]+\.)*[\w\!\#$\%\&\'\*\+\-\/\=\?\^\`{\|\}\~]+@((((([a-zA-Z0-9]{1}[a-zA-Z0-9\-]{0,62}[a-zA-Z0-9]{1})|[a-zA-Z])\.)+[a-zA-Z]{2,6})|(\d{1,3}\.){3}\d{1,3}(\:\d{1,5})?)$", ErrorMessage = "Please enter valid email address")]
        [Required(ErrorMessage = "Please enter email address")]
        public string ContactEmail { get; set; }

        [Required(ErrorMessage = "Please enter contact number")]
        [RegularExpression("[0-9]{3}[0-9]{3}[0-9]{4}", ErrorMessage = "Please enter valid contact number")]
        public string ContactPhone { get; set; }

        [Required(ErrorMessage = "Please enter subject")]
        public string ContactSubject { get; set; }

        [Required(ErrorMessage = "Please enter message")]
        public string ContactMsg { get; set; }

        public DateTime ContactDate { get; set; }

        public string UserGoogleCaptchaToken { get; set; }
    }
}