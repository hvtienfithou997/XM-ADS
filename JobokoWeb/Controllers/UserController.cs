using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace JobokoWeb.Controllers
{
    public class UserController : BaseController
    {
        public IActionResult Index()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Login([FromForm] object value)
        {
            try
            {
                string msg = "";
                string json = Newtonsoft.Json.JsonConvert.SerializeObject(new
                {
                    user = HttpContext.Request.Form["username"].ToString(),
                    pass = HttpContext.Request.Form["password"].ToString()
                });
                try
                {
                    var res = Utils.APIUtils.CallAPI("user/login", json, string.Empty, out bool success, out msg, "POST");

                    var obj = JToken.Parse(res);
                    if (obj != null)
                    {
                        if (obj["success"].ToObject<bool>())
                        {
                            var user = obj["data"].ToObject<JobokoAdsModels.User>();

                            var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
                            identity.AddClaim(new Claim(ClaimTypes.Name, user.user_name));
                            identity.AddClaim(new Claim(ClaimTypes.GivenName, !string.IsNullOrEmpty(user.full_name) ? user.full_name : ""));
                            identity.AddClaim(new Claim(ClaimTypes.Email, !string.IsNullOrEmpty(user.email) ? user.email : ""));
                            identity.AddClaim(new Claim("token", obj["token"].ToString()));

                            if (obj["data"]["roles"] != null)
                                foreach (var role in obj["data"]["roles"].ToObject<List<string>>())
                                {
                                    identity.AddClaim(new Claim(ClaimsIdentity.DefaultRoleClaimType, role));
                                }
                            var principal = new ClaimsPrincipal(identity);
                            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, new AuthenticationProperties()
                            {
                                IsPersistent = true,
                                ExpiresUtc = DateTime.UtcNow.AddYears(1),
                                AllowRefresh = true
                            });
                            string ret = HttpContext.Request.Form["ReturnUrl"].ToString();
                            if (!string.IsNullOrEmpty(ret))
                                return Redirect(ret);
                            else
                                return Redirect("/");
                        }
                        else
                        {
                            ViewBag.error = obj["msg"].ToString();
                        }
                    }
                }
                catch (Exception ex)
                {
                    ViewBag.error = $"{ex.Message} detail: {msg}";
                }

                return View();
            }
            catch (Exception ex)
            {
                ViewBag.error = $"{ex.Message}";
            }

            return View();
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(Login));
        }

        public IActionResult Add()
        {
            return View();
        }

        public IActionResult Edit(string id)
        {
            ViewBag.id = id;
            return View();
        }

        public IActionResult UserInfo(string id)
        {
            ViewBag.id = id;
            return View();
        }

       
    }
}