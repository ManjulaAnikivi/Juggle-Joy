using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace juggle_joy.Models
{
    public class Faq_CategoryValidation
    {
        public int Faq_Cat_id { get; set; }

        [Required(ErrorMessage = "Please enter category name")]
        [Remote("IsFAQCategoryNameExists", "cms", AdditionalFields = "Faq_Cat_id", ErrorMessage = "Category name already exists")]
        public string Category { get; set; }
    }
}