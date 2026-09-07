using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace juggle_joy.Models
{
    public class CategoryValidation
    {
        public int CategoryID { get; set; }


        [Required(ErrorMessage = "Please enter category name")]
        [Remote("IsCategoryNameExists", "category", AdditionalFields = "CategoryID", ErrorMessage = "Category name already exists")]
        public string Category { get; set; }

        [Required(ErrorMessage = "Please choose category image")]
        public string CategoryImage { get; set; }
    }
}