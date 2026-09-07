using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace juggle_joy.Models
{
    public class TaskValidation
    {
        public int TaskID { get; set; }
        
        public string TaskTitle { get; set; }
        public int? CategoryID { get; set; }
        public int? subcategory { get; set; }
        public string address { get; set; }

        public string Deadline { get; set; }

        public string Description { get; set; }
      
        public string Repeating { get; set; }

        public string WorkTime { get; set; }

        public decimal? Budget { get; set; }

        public string DeadlineType { get; set; }

        public DateTime? specificDate { get; set; }

        public int? Status { get; set; }

        public string Image { get; set; }
    }
}