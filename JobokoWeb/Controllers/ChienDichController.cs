using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JobokoWeb.Controllers
{
    public class ChienDichController : BaseController
    {
        // GET: ChienDichontroller
        public ActionResult Index(string term, string id_chien_dich, int trang_thai, string field_sort, int sort_order = 1, int page = 1)
        {
            ViewBag.term = term;
            ViewBag.trang_thai = trang_thai;
            ViewBag.page = page;
            ViewBag.id_chien_dich = id_chien_dich;
            ViewBag.field_sort = field_sort;
            ViewBag.sort_order = sort_order;
            return View();
        }
        public ActionResult Create()
        {
            return View();
        }
        // GET: ChienDichontroller/Details/5
        public ActionResult Details(string id)
        {
            return View();
        }


        // GET: ChienDichontroller/Edit/5
        public ActionResult Edit(string id)
        {
            ViewData["id"] = id;
            return View();
        }

        public ActionResult EditLocation(string id)
        {
            ViewData["id"] = id;
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

        public IActionResult Report()
        {
            return View();
        }
    }
}
