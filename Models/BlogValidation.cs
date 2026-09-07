using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace juggle_joy.Models
{
    public class BlogValidation
    {
        public int BlogID { get; set; }
        [Required(ErrorMessage = "Please select category")]
        public int? CategoryID { get; set; }

        [Required(ErrorMessage = "Please enter blog title")]
        [Remote("Isblogquestion_Exists", "Blogs", AdditionalFields = "BlogID", ErrorMessage = "Blog title already exists")]
        public string BlogTitle { get; set; }

        [Required(ErrorMessage = "Please choose blog image")]
        public string BlogImage { get; set; }

        [Required(ErrorMessage = "Please enter blog description")]
        public string BlogDescription { get; set; }


        public Nullable<System.DateTime> Date { get; set; }
    }
}