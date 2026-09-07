using System;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace juggle_joy.Models
{
    public class FaqValidation
    {
        public int FaqID { get; set; }

        [Required(ErrorMessage = "Category is required")]
        public String Faq_Cat_id { get; set; }

        [Required(ErrorMessage = "Please enter your question")]
        [Remote("Isfaqquestion_Exists", "cms", AdditionalFields = "FaqID", ErrorMessage = "Question already exists")]
        public string FaqQuestion { get; set; }

        [Required(ErrorMessage = "Please enter answer")]
        public string FaqAnswer { get; set; }
    }
}