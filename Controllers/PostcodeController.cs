using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace juggle_joy.Controllers
{
    public class PostcodeController : Controller
    {
        // GET: Postcode
    
        private readonly PostcodeService _postcodeService;

        public PostcodeController()
        {
            _postcodeService = new PostcodeService();
        }

        [HttpGet]
        public ActionResult FindAddress()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> FindAddress(string addressId, string language, string dateTime, string acceptValue)
        {
            try
            {
                var address = await _postcodeService.GetAddressAsync(addressId, language, dateTime, acceptValue);
                ViewBag.AddressResult = address;
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = ex.Message;
            }

            return View();
        }
    }

}
