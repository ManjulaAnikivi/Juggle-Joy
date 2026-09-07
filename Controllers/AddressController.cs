using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Net.Http;


namespace juggle_joy.Controllers
{
    public class AddressController : Controller
    {
        // GET: Address


            private readonly string apiKey = "13d6dff71d4912a3eda025834f524705";  // Royal Mail API Key
            private readonly string apiSecret = "42e16cfd18b16ffb1d3333fa25ab643c";  // Royal Mail API Secret
                                                                                     //private readonly string apiUrl = "https://api.royalmail.net/addressfinder/autocomplete";  // Adjust as per Royal Mail documentation
        private readonly string apiUrl = "https://st.api.royalmail.net/addressfind/v1/address";

      
        private readonly string apiUrlAutocomplete = "https://api.royalmail.net/addressfinder/autocomplete";
        private readonly string apiUrlAddressDetails = "https://st.api.royalmail.net/addressfind/v1/address/";

        // GET: Address Autocomplete
        public async Task<ActionResult> GetAddressSuggestions(string query)
        {
            if (string.IsNullOrEmpty(query))
            {
                return Json(new { success = false, message = "Query cannot be empty." }, JsonRequestBehavior.AllowGet);
            }

            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiSecret}");

                    //var response = await client.GetAsync($"{apiUrlAutocomplete}?query={query}&maxresults=5&apikey={apiKey}");
                    var response = await client.GetAsync($"{apiUrlAddressDetails}/{query}?apikey={apiKey}");
                    if (response.IsSuccessStatusCode)
                    {
                        var result = await response.Content.ReadAsStringAsync();
                        return Json(new { success = true, data = result }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(new { success = false, message = "Failed to retrieve addresses." }, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // GET: Address Details by Address ID
        public async Task<ActionResult> GetAddressDetails(string addressId)
        {
            if (string.IsNullOrEmpty(addressId))
            {
                return Json(new { success = false, message = "Address ID cannot be empty." }, JsonRequestBehavior.AllowGet);
            }

            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiSecret}");

                    var response = await client.GetAsync($"{apiUrlAddressDetails}{addressId}?apikey={apiKey}");

                    if (response.IsSuccessStatusCode)
                    {
                        var result = await response.Content.ReadAsStringAsync();
                        return Json(new { success = true, data = result }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(new { success = false, message = "Failed to retrieve address details." }, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

    }
    }

