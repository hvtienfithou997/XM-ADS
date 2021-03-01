using IdGen;
using JobokoAdsModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Claims;

namespace JobokoAdsAPI.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Authorize(Policy = "SameIpPolicy")]
    public class APIBase : ControllerBase, IActionFilter
    {
        protected IdGenerator id_gen = new IdGenerator(0);
        protected string user = "";
        protected int page = 1;
        protected int page_size = 5;
        protected long total_recs = 0;
        private bool _is_admin;
        private bool _is_editor;
        protected DateTimeFormatInfo dtfi = new DateTimeFormatInfo() { ShortDatePattern="dd/MM/yyyy", DateSeparator="/" };
        public bool is_admin
        {
            get
            {
                try
                {
                    if (User != null && User.Identity.IsAuthenticated)
                        _is_admin = User.IsInRole(Role.ADMIN.ToString());
                }
                catch (Exception)
                {
                }
                return _is_admin;
            }
        }

        public Dictionary<string, bool> dic_sort = new Dictionary<string, bool> { { "ngay_sua", true } };

        public bool is_user
        {
            get
            {
                try
                {
                    if (User != null && User.Identity.IsAuthenticated)
                        _is_editor = User.IsInRole(Role.EDITOR.ToString());
                }
                catch (Exception)
                {
                }
                return _is_editor;
            }
        }

        protected void SetMetaData(dynamic obj, bool is_update)
        {
            if (is_update)
            {
                obj.ngay_sua = XMedia.XUtil.TimeInEpoch(DateTime.Now);
                obj.nguoi_sua = user;
            }
            else
            {
                obj.ngay_tao = XMedia.XUtil.TimeInEpoch(DateTime.Now);
                obj.nguoi_tao = user;
                obj.ngay_sua = XMedia.XUtil.TimeInEpoch(DateTime.Now);
                obj.nguoi_sua = user;
            }
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (User != null && User.Identity.IsAuthenticated)
            {
                user = User.FindFirst(x => x.Type == ClaimTypes.NameIdentifier).Value;
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            // do something after the action executes
        }

        public string listProvinceCountry()
        {
            string json_province_vn = @"[{'country':'Việt Nam','code':'VN','provinces':[{'name':'Toàn quốc'},{'name':'Hồ Chí Minh'},{'name':'Hà Nội'},{'name':'Bình Dương'},{'name':'Hải Phòng'},{'name':'Bắc Ninh'},{'name':'Quảng Ninh'},{'name':'Nam Định'},{'name':'Lào Cai'},{'name':'Điện Biên'},{'name':'Lai Châu'},{'name':'Sơn La'},{'name':'Yên Bái'},{'name':'Hoà Bình'},{'name':'Thái Nguyên'},{'name':'Lạng Sơn'},{'name':'Bắc Giang'},{'name':'Phú Thọ'},{'name':'Vĩnh Phúc'},{'name':'Hải Dương'},{'name':'Hưng Yên'},{'name':'Thái Bình'},{'name':'Hà Nam'},{'name':'Ninh Bình'},{'name':'Thanh Hóa'},{'name':'Nghệ An'},{'name':'Hà Tĩnh'},{'name':'Quảng Bình'},{'name':'Quảng Trị'},{'name':'Thừa Thiên Huế'},{'name':'Đà Nẵng'},{'name':'Quảng Nam'},{'name':'Quảng Ngãi'},{'name':'Bình Định'},{'name':'Phú Yên'},{'name':'Khánh Hòa'},{'name':'Ninh Thuận'},{'name':'Bình Thuận'},{'name':'Kon Tum'},{'name':'Gia Lai'},{'name':'Đắk Lắk'},{'name':'Đắk Nông'},{'name':'Lâm Đồng'},{'name':'Bình Phước'},{'name':'Tây Ninh'},{'name':'Đồng Nai'},{'name':'Bà Rịa - Vũng Tàu'},{'name':'Long An'},{'name':'Tiền Giang'},{'name':'Bến Tre'},{'name':'Trà Vinh'},{'name':'Vĩnh Long'},{'name':'Đồng Tháp'},{'name':'An Giang'},{'name':'Kiên Giang'},{'name':'Cần Thơ'},{'name':'Hậu Giang'},{'name':'Sóc Trăng'},{'name':'Bạc Liêu'},{'name':'Cà Mau'},{'name':'Cao Bằng'},{'name':'Hà Giang'},{'name':'Tuyên Quang'},{'name':'Bắc Kạn'}]},{'country':'United State','code':'US','provinces':[{'name':'Los Angeles'},{'name':'Chicago'},{'name':'NewYork City'},{'name':'California'}]}]";
            //string json_province_us = @"[{'country':'United State','code':'US','provinces':[{'name':'Los Angeles'},{'name':'Chicago'},{'name':'NewYork City'},{'name':'California'}]}]";
            
            return json_province_vn;
        }
    }
}