using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace JobokoWeb.Controllers
{
    public class LogController : BaseController
    {
        public IActionResult Index(int page = 1)
        {
            ViewBag.page = page;
            return View();
        }
        public IActionResult TuKhoaTimKiem(string tu_khoa, string ngay_bat_dau, string ngay_ket_thuc, int page = 1)
        {
            ViewBag.page = page;
            ViewBag.tu_khoa = tu_khoa;
            ViewBag.ngay_bat_dau = ngay_bat_dau;
            ViewBag.ngay_ket_thuc = ngay_ket_thuc;
            return View();
        }
        public IActionResult TrangTuKhoaTimKiem(string tu_khoa, string ngay_bat_dau, string ngay_ket_thuc, int page = 1)
        {
            ViewBag.page = page;
            ViewBag.tu_khoa = tu_khoa;
            ViewBag.ngay_bat_dau = ngay_bat_dau;
            ViewBag.ngay_ket_thuc = ngay_ket_thuc;
            return View();
        }
    }
}
