using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web;

namespace RealEstate.ApiCallHelper
{
    public class ApiCallHelper
    {
        public string CallToAPI(string address, string output, string methodType, bool IsTokenRequired = true, string token = null, bool ContentTypeRequired = false)
        {
            // New code:
            HttpClient client = new HttpClient();
            string responseData = string.Empty;
            client.BaseAddress = new Uri(System.Configuration.ConfigurationManager.AppSettings["ApiBaseAddress"]);
            client.DefaultRequestHeaders.Accept.Clear();
            if (ContentTypeRequired)
                client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/x-www-form-urlencoded");

            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (IsTokenRequired)
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            if (methodType == "GET")
            {
                HttpResponseMessage response = client.GetAsync(address).Result;  // Blocking call!
                if (response.IsSuccessStatusCode)
                {
                    // Parse the response body. Blocking!
                    responseData = response.Content.ReadAsStringAsync().Result;

                }
            }
            else if (methodType == "PUT")
            {
                HttpResponseMessage response = client.PutAsync(address, new StringContent(output, Encoding.UTF8, "text/json")).Result;  // Blocking call!
                if (response.IsSuccessStatusCode)
                {
                    // Parse the response body. Blocking!
                    responseData = response.Content.ReadAsStringAsync().Result;
                    if (string.IsNullOrEmpty(responseData))
                    {
                        responseData = "success";
                    }
                }
            }
            else
            {
                HttpResponseMessage response = client.PostAsync(address, new StringContent(output, Encoding.UTF8, "text/json")).Result;  // Blocking call!
                if (response.IsSuccessStatusCode)
                {
                    // Parse the response body. Blocking!
                    responseData = response.Content.ReadAsStringAsync().Result;
                    if (string.IsNullOrEmpty(responseData))
                    {
                        responseData = "success";
                    }

                }
            }
            return responseData;
        }

        public string GetToken(string username, string password)
        {
            var pairs = new List<KeyValuePair<string, string>>
                        {
                            new KeyValuePair<string, string>( "grant_type", "password" ),
                            new KeyValuePair<string, string>( "username", username ),
                            new KeyValuePair<string, string> ( "Password", password )
                        };
            var content = new FormUrlEncodedContent(pairs);
            using (var client = new HttpClient())
            {
                var baseAddress = System.Configuration.ConfigurationManager.AppSettings["ApiBaseAddress"];
                var response =
                    client.PostAsync(baseAddress + "Token", content).Result;
                var token = response.Content.ReadAsStringAsync().Result;
                if (!string.IsNullOrEmpty(token))
                {
                    var tokenData = JObject.Parse(token);
                    if (tokenData != null)
                    {
                        HttpContext.Current.Session["Token"] = tokenData["access_token"];
                        //HttpContext.Current.Session["LoggedInUserName"] = username;
                        //HttpContext.Current.Session["LoggedInPassword"] = password;
                    }
                    else
                        return null;
                }
                return Convert.ToString(HttpContext.Current.Session["Token"]);
            }

        }

    }

}