using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace juggle_joy.Models
{

    public class option
    {
        public int OptionID { get; set; }
        public int QuestionID { get; set; }
        public string Options { get; set; }

    }
    public class QuestionValidation
    {
        public int QuestionID { get; set; }

        [Required(ErrorMessage = "Question is required")]
        [System.Web.Mvc.Remote("IsquestionExist", "question", AdditionalFields = "QuestionID", ErrorMessage = "Question already exists")]
        public string Question { get; set; }

        public int OptionID { get; set; }

        //[Required(ErrorMessage = "Option is required")]
        public List<option> options { get; set; }

        public string[] SubCategoryID { get; set; }

        public string OptionType { get; set; }

    }



}