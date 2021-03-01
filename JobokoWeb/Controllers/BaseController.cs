using JobokoAdsModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JobokoWeb.Controllers
{
    [Authorize]
    public class BaseController : Controller
    {
        private static readonly string API_URL = XMedia.XUtil.ConfigurationManager.AppSetting["API_URL"];

        public int page_size = 8;
        private bool _is_admin;
        private string _token;
        public string Token
        {
            get
            {
                if (User != null && User.Identity.IsAuthenticated)
                {
                    var claim_token = User.Claims.FirstOrDefault(x => x.Type == "token");
                    if (claim_token != null)
                    {
                        _token = claim_token.Value;
                    }
                }
                return _token;
            }
            set => _token = value;
        }
        public bool Is_Admin
        {
            get
            {
                try
                {
                    var ro = (int)Role.ADMIN;
                    _is_admin = User.IsInRole(ro.ToString());
                }
                catch (Exception)
                {
                }
                return _is_admin;
            }
        }

        public BaseController()
        {
        }
        public override ViewResult View()
        {
            ViewBag.api_url = API_URL;
            ViewBag.token = Token;
            return base.View();
        }

        public override ViewResult View(string viewName)
        {
            ViewBag.api_url = API_URL;
            ViewBag.token = Token;
            return base.View(viewName);
        }

        public override ViewResult View(object model)
        {
            ViewBag.api_url = API_URL;
            ViewBag.token = Token;
            return base.View(model);
        }

        public override ViewResult View(string viewName, object model)
        {
            ViewBag.api_url = API_URL;
            ViewBag.token = Token;
            return base.View(viewName, model);
        }

        protected void SetAlert(string msg, string type)
        {
            TempData["msg"] = msg;
            if (type == "success")
            {
                TempData["msg_type"] = "alert-success";
            }
            else if (type == "warning")
            {
                TempData["msg_type"] = "alert-warning";
            }
            else if (type == "error")
            {
                TempData["msg_type"] = "alert-danger";
            }
        }

        protected void SetMetaData(dynamic obj, bool is_update)
        {
            if (is_update)
            {
                obj.ngay_sua = XMedia.XUtil.TimeInEpoch(DateTime.Now);
                obj.nguoi_sua = HttpContext.User.Identity.Name;
            }
            else
            {
                obj.ngay_tao = XMedia.XUtil.TimeInEpoch(DateTime.Now);
                obj.nguoi_tao = HttpContext.User.Identity.Name;
                obj.ngay_sua = XMedia.XUtil.TimeInEpoch(DateTime.Now);
                obj.nguoi_sua = HttpContext.User.Identity.Name;
            }
        }
    }
}
