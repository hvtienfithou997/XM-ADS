using JobokoAdsModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JobokoAdsAPI.Controllers
{
    public class TokenController : APIBase
    {
        [AllowAnonymous]
        [HttpGet]
        public IActionResult Get()
        {
            return Ok("API OK");
        }
        [AllowAnonymous]
        [HttpPost]
        [Route("login")]
        public IActionResult Login([FromBody] object value)
        {
            return Ok();
            var obj = JToken.Parse(value.ToString());
            User u_info = new User();
            bool is_success = false;
            string msg = ""; string token = "";
            var ip_add = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv6().ToString();
            string browser = Request.Headers["User-Agent"];
            if (obj != null)
            {
                string user_name = obj["user"]?.ToString();
                string password = obj["pass"]?.ToString();
                password = XMedia.XUtil.Encode(password);
                u_info = new User() { user_name = user_name, password = "1", roles = new List<string>() { "ADMIN" }, full_name="System Admin" };
                //QLCUNL.BL.UserBL.Login(user_name, password, ip_add, browser);
                is_success = true;

                if (is_success)
                {
                    msg = $"Chào {u_info.full_name}!";
                    token = TokenManager.BuildToken(u_info.user_name, u_info.roles, u_info.full_name, ip_add);
                }
                else
                {
                    msg = "Đăng nhập không thành công";
                }
            }
            else
            {
                msg = "Yêu cầu tham số user và pass";
            }

            return Ok(new
            {
                data = !is_success ? new object() : new
                {
                    u_info.full_name,
                    u_info.user_name,
                    u_info.email,
                },
                success = is_success,
                msg,
                token
            });
        }
    }
}
