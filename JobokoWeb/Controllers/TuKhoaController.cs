using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JobokoWeb.Controllers
{
    public class TuKhoaController : BaseController
    {
        // GET: ChienDichontroller
        public ActionResult Index(string id_chien_dich,string id_quang_cao, string term, int trang_thai, string field_sort, int sort_order = 1, int page = 1)
        {
            ViewBag.id_chien_dich = id_chien_dich;
            ViewBag.id_quang_cao = id_quang_cao;
            ViewBag.term = term;
            ViewBag.trang_thai = trang_thai;
            ViewBag.page = page;
            ViewBag.field_sort = field_sort;
            ViewBag.sort_order = sort_order;
            return View();
        }
        public ActionResult Create(string id_quang_cao, string id_chien_dich)
        {
            ViewBag.id_quang_cao = id_quang_cao;
            ViewBag.id_chien_dich = id_chien_dich;
            return View();
        }
        // GET: ChienDichontroller/Details/5
        public ActionResult Details(string id)
        {
            return View();
        }


        // GET: ChienDichontroller/Edit/5
        public ActionResult Edit(string id, string id_chien_dich, string id_quang_cao)
        {
            ViewData["id"] = id;
            ViewData["id_chien_dich"] = id_chien_dich;
            ViewData["id_quang_cao"] = id_quang_cao;
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
    }
}
