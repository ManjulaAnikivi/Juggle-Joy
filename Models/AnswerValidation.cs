using Microsoft.Ajax.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace juggle_joy.Models
{
    public class AnswerValidation
    {
        public int AnswerID { get; set; }
        public int TaskID { get; set; }
        public int QuestionID { get; set; }
        public string Answer { get; set; }
        public int? MinitaskID { get; set; }

    }
}