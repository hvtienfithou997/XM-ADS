using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace JobokoWeb.Utils
{
    public class APIUtils
    {
        private static readonly string API_URL = XMedia.XUtil.ConfigurationManager.AppSetting["API_URL"];
        public static string CallAPI(string end_point, string json, string token, out bool success, out string msg, string method = "GET")
        {
            msg = "";
            success = false;
            string res = "";
            try
            {
                msg = $"{token} -> json = {json}";
                using (WebClient wc = new WebClient())
                {
                    wc.Headers[HttpRequestHeader.Authorization] = $"Bearer {token}";

                    wc.Headers[HttpRequestHeader.ContentType] = "application/json";
                    switch (method)
                    {
                        case "POST":
                            res = wc.UploadString($"{API_URL}/{end_point}", method, json);
                            break;
                        case "PUT":
                            break;
                        default:
                            res = wc.DownloadString($"{API_URL}/{end_point}");
                            break;
                    }

                    success = true;
                }
            }
            catch (Exception ex)
            {
                msg = $"{ex.Message} => {ex.StackTrace}";
                success = false;
            }
            return res;
        }
        public static async Task<string> CallAPIAsync(string end_point, string json, string token, string method = "GET")
        {
            string res = "";
            try
            {
                using (WebClient wc = new WebClient())
                {
                    wc.Headers[HttpRequestHeader.Authorization] = $"Bearer {token}";

                    wc.Headers[HttpRequestHeader.ContentType] = "application/json";
                    switch (method)
                    {
                        case "POST":
                            res = await wc.UploadStringTaskAsync(new Uri($"{API_URL}/{end_point}"), method, json);
                            break;
                        case "PUT":
                            break;
                        default:
                            res = await wc.DownloadStringTaskAsync(new Uri($"{API_URL}/{end_point}"));
                            break;
                    }

                }
            }
            catch (Exception ex)
            {
            }
            return res;
        }
    }
}
