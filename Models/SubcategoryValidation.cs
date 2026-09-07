using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace juggle_joy.Models
{
    public class SubcategoryValidation
    {
        public int SubCategoryID { get; set; }

        [Required(ErrorMessage = "Please select category")]
        public int CategoryID { get; set; }


        [Required(ErrorMessage = "Please enter sub category name")]
        [System.Web.Mvc.Remote("IsSubCategoryNameExists", "category", AdditionalFields = "SubCategoryID,CategoryID", ErrorMessage = "Sub category name already exists")]

        public string SubCategory { get; set; }
    }
}