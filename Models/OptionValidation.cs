using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace juggle_joy.Models
{
    public class options
    {
        public int OptionID { get; set; }
        public int QuestionID { get; set; }
        public string Question { get; set; }

    }
    public class OptionValidation
    {
        public int OptionID { get; set; }

        [Required(ErrorMessage = "Question is required")]
        public int QuestionID { get; set; }

        [Required(ErrorMessage = "Option is required")]
        public List<options> Question { get; set; }


    }
}