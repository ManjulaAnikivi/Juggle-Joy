using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace juggle_joy.Models
{
    public class RecommendationsValidation
    {

        public int RecID { get; set; }
        public string TaskID { get; set; }
        public string AssistantID { get; set; }
        public string Name { get; set; }
        public string Rating { get; set; }
        public string Note { get; set; }
        public string Price { get; set; }
        public string Availability { get; set; }
        public string NextStep { get; set; }
        public string MiniTaskID { get; set; }
    }


}