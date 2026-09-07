using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace juggle_joy.Models
{
    public class CMSValidation
    {
        [Required(ErrorMessage = "Please select page name")]
        public int? CmsID { get; set; }

        //[Required(ErrorMessage = "Please enter CMS Text")]
        public string CmsText { get; set; }
    }
}