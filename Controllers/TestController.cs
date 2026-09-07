using juggle_joy.Models;

using System;

using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

//using System.Threading.Tasks;

using System.Web.Mvc;

using RestSharp;

namespace juggle_joy.Controllers
{
    public class TestController : Controller
    {
        // GET: Test

        #region chat
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Assistant()
        {
            return View();
        }

        public ActionResult Handler()
        {
            return View();
        }

        public ActionResult chatlist(int taskid)
        {
            using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
            {
                var chatlists = ctx.tbl_chat.Where(x => x.TaskID == taskid).ToList();
                return View(chatlists);
            }

        }

        public ActionResult handlerchatlist(int taskid)
        {
            using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
            {
                var chatlists = ctx.tbl_chat.Where(x => x.TaskID == taskid).ToList();
                return View(chatlists);
            }

        }

        public ActionResult assistantchatlist(int taskid)
        {
            using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
            {
                var chatlists = ctx.tbl_chat.Where(x => x.TaskID == taskid).ToList();
                return View(chatlists);
            }

        }

        #endregion


        public ActionResult railmail()
        {
            return View();
        }


        private readonly string apiUrl = "https://api.royalmail.net/paf/postcode";  // Update with actual Royal Mail API URL
        private readonly string apiKey = "13d6dff71d4912a3eda025834f524705";  // Your Royal Mail API Key
        private readonly string apiSecret = "42e16cfd18b16ffb1d3333fa25ab643c";  // Your Royal Mail API Secret

        // Action method that will be called by JavaScript
        [HttpGet]
        public async Task<ActionResult> GetAddresses(string postcode)
        {
            using (HttpClient client = new HttpClient())
            {
                // Add authorization header
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

                // Create request URL with the postcode
                string requestUrl = $"{apiUrl}?postcode={postcode}";

                HttpResponseMessage response = await client.GetAsync(requestUrl);

                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadAsStringAsync();
                    return Json(data);  // Return the data as JSON
                }
                else
                {
                    return Content("Failed to fetch data from Royal Mail API.");
                }
            }
        }


        public ActionResult GetAdds()
        {
            return View();
        }


        [HttpPost]
        public async Task<ActionResult> address(string postcode)
        {
            string date = Convert.ToString(DateTime.Now);
            var options = new RestClientOptions("https://st.api.royalmail.net/addressfind/v1/address/" + postcode);
            var client = new RestClient(options);
            var request = new RestRequest("");
            request.AddHeader("X-IBM-Client-Id", "13d6dff71d4912a3eda025834f524705");
            request.AddHeader("X-IBM-Client-Secret", "42e16cfd18b16ffb1d3333fa25ab643c");
           //request.AddHeader("X-RMG-Language", "REPLACE_THIS_VALUE");
            request.AddHeader("X-RMG-Date-Time", date);
            //request.AddHeader("Accept", "REPLACE_THIS_VALUE");
            request.AddHeader("accept", "application/json");
            var response = await client.GetAsync(request);

            Console.WriteLine("{0}", response.Content);
            return RedirectToAction("GetAdds");
        }
    }
}