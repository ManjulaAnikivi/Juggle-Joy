using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace juggle_joy.Models
{
    public class PriceValidation
    {
        public int PriceID { get; set; }
        [Required(ErrorMessage = "Please enter price")]
        [RegularExpression("^[0-9]\\d{0,9}(\\.\\d{1,2})?%?$", ErrorMessage = "Price must be numeric and after decimal only two digits allowed.")]
        public decimal? ActualPrice { get; set; }
        [Required(ErrorMessage = "Please enter price")]
        [RegularExpression("^[0-9]\\d{0,9}(\\.\\d{1,2})?%?$", ErrorMessage = "Price must be numeric and after decimal only two digits allowed.")]
        public decimal? DiscountedPrice { get; set; }
    }
}