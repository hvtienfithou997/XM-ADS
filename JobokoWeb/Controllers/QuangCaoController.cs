using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace JobokoWeb.Controllers
{
    public class QuangCaoController : BaseController
    {
        // GET: ChienDichontroller
        public ActionResult Index(string term, string id_chien_dich, int trang_thai, string id_quang_cao, string field_sort, int sort_order = 1, int page = 1)
        {
            ViewBag.term = term;
            ViewBag.id_chien_dich = id_chien_dich;
            ViewBag.id_quang_cao = id_quang_cao;
            ViewBag.trang_thai = trang_thai;
            ViewBag.page = page;
            ViewBag.field_sort = field_sort;
            ViewBag.sort_order = sort_order;
            return View();
        }
        public ActionResult Create(string id_chien_dich)
        {
            ViewBag.id_chien_dich = id_chien_dich;
            return View();
        }
        // GET: ChienDichontroller/Details/5
        public ActionResult Details(string id)
        {
            return View();
        }
        //[Route("/viewad/{id}/{id_tk}")]
        public ActionResult ViewAd(string id, string id_tk)
        {
            ViewBag.id = id;
            ViewBag.id_tk = id_tk;
            var res = Utils.APIUtils.CallAPI($"quangcao/view/{id}/{id_tk}", string.Empty, Token, out bool success, out string msg, "GET");
            if (!string.IsNullOrEmpty(res))
            {
                var obj = JToken.Parse(res);
                if (obj != null)
                {
                    if (obj["success"].ToObject<bool>())
                    {
                        var quang_cao = obj["data"].ToObject<JobokoAdsModels.QuangCao>();
                        quang_cao.link_dich = $"{XMedia.XUtil.ConfigurationManager.AppSetting["API_URL"]}/quangcao/go/{id}/{id_tk}";
                        return View(quang_cao);
                    }
                }

            }
            return View(new JobokoAdsModels.QuangCao());

        }

        // GET: ChienDichontroller/Edit/5
        public ActionResult Edit(string id, string id_chien_dich)
        {
            ViewData["id"] = id;
            ViewData["id_chien_dich"] = id_chien_dich;
            return View();
        }

        // GET: ChienDichontroller/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: ChienDichontroller/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
        public ActionResult TuKhoa(string tu_khoa, int so_luong, string dia_diem)
        {
            
            var res = Utils.APIUtils.CallAPI($"quangcao/tukhoa?tu_khoa={tu_khoa}&so_luong={so_luong}&dia_diem={dia_diem}", string.Empty, Token, out bool success, out string msg, "GET");
            if (!string.IsNullOrEmpty(res))
            {
                var obj = JToken.Parse(res);
                if (obj != null)
                {
                    if (obj["success"].ToObject<bool>())
                    {
                        var lst_quang_cao = obj["data"].ToObject<List<JobokoAdsModels.TuKhoaDauGia>>();
                        foreach (var quang_cao in lst_quang_cao)
                        {
                            _ = Utils.APIUtils.CallAPIAsync($"quangcao/uview/{quang_cao.id_quang_cao}/{quang_cao.id_tu_khoa}", string.Empty, Token, "GET");
                            quang_cao.link_dich = $"{XMedia.XUtil.ConfigurationManager.AppSetting["API_URL"]}/quangcao/go/{quang_cao.id_quang_cao}/{quang_cao.id_tu_khoa}/{quang_cao.gia_thau}";
                        }
                        return View(lst_quang_cao);
                    }
                }

            }
            return View(new List<JobokoAdsModels.QuangCao>());
        }
    }
}
