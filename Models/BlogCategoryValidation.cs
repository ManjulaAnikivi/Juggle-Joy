using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace juggle_joy.Models
{
    public class BlogCategoryValidation
    {
        public int CategoryID { get; set; }


        [Required(ErrorMessage = "Please enter category name")]
        [Remote("IsCategoryNameExists", "Blogs", AdditionalFields = "CategoryID", ErrorMessage = "Category name already exists")]
        public string Category { get; set; }
    }
}