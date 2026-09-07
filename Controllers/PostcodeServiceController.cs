using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using RestSharp;

namespace juggle_joy.Controllers
{

public class PostcodeService
    {
        private readonly string clientId = "13d6dff71d4912a3eda025834f524705";
        private readonly string clientSecret = "42e16cfd18b16ffb1d3333fa25ab643c";
        private readonly string apiUrl = "https://st.api.royalmail.net/addressfind/v1/address/";

        public async Task<string> GetAddressAsync(string addressId, string language, string dateTime, string acceptValue)
        {
            var options = new RestClientOptions($"{apiUrl}{addressId}");
            var client = new RestClient(options);
            var request = new RestRequest();

            request.AddHeader("X-IBM-Client-Id", clientId);
            request.AddHeader("X-IBM-Client-Secret", clientSecret);
            //request.AddHeader("X-RMG-Language", language);
            request.AddHeader("X-RMG-Date-Time", dateTime);
            //request.AddHeader("Accept", acceptValue);
            request.AddHeader("accept", "application/json");

            var response = await client.GetAsync(request);

            if (response.IsSuccessful)
            {
                return response.Content;
            }
            else
            {
                throw new Exception($"Error retrieving address: {response.StatusCode} - {response.Content}");
            }
        }
    }

}
