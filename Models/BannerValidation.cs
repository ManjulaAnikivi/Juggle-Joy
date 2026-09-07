using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace juggle_joy.Models
{
    public class BannerValidation
    {
        public int BannerId { get; set; }

        [Required(ErrorMessage = "Please add video")]

        public string BannerVideo { get; set; }

        [Required(ErrorMessage = "Please enter banner text")]
        public string BannerText { get; set; }

        [Required(ErrorMessage = "Please enter banner description")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Please enter url link")]
        public string UrlLink { get; set; }
    }
}