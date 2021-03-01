using JobokoAdsAPI.Models;
using JobokoAdsES;
using JobokoAdsModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace JobokoAdsAPI.Controllers
{
    public class QuangCaoController : APIBase
    {
        [HttpGet]
        [Route("all")]
        public IActionResult GetAll(string term, string id_chien_dich, string id_quang_cao, int trang_thai, long ngay_tao_from, long ngay_tao_to, string field_sort, int sort_order, int page = 1, int page_size = 50)
        {
            DataResponsePaging res = new DataResponsePaging();
            try
            {
                dic_sort = !string.IsNullOrEmpty(field_sort) ? new Dictionary<string, bool> { { field_sort, sort_order == 1 } } : new Dictionary<string, bool> { { "ngay_sua", sort_order == 1 } };
                var quang_cao = QuangCaoRepository.Instance.GetAll(term, new List<string>() { user }, false, new string[] { "*" }, dic_sort, trang_thai, id_chien_dich, id_quang_cao, page, page_size, out total_recs);
                res.success = quang_cao != null;
                if (res.success)
                {
                    var lst_chien_dich = ChienDichRepository.Instance.GetMany(quang_cao.Select(x => x.id_chien_dich), new string[] { "id", "ten" });
                    res.data = quang_cao.Select(x => new QuangCaoChienDichMap(x, lst_chien_dich));
                    res.total = total_recs;
                    //res.data_sum = QuangCaoRepository.Instance.FilterAggregations(term, new string[] { "ten_hien_thi", "link_dich", "link_hien_thi", "tieu_de_1", "tieu_de_2" }, trang_thai, new List<string>{user},id_chien_dich, is_admin );
                }
            }
            catch (Exception ex)
            {
                res.msg = ex.Message; res.success = false;
            }

            return Ok(res);
        }

        [HttpGet]
        [Route("getsum")]
        public IActionResult DicTinhTong(string term, int trang_thai, string id_chien_dich, string id_quang_cao)
        {
            DataResponse res = new DataResponse();
            try
            {
                var dic = QuangCaoRepository.Instance.FilterAggregations(term, new string[] { "ten_hien_thi", "link_dich", "link_hien_thi", "tieu_de_1", "tieu_de_2" }, trang_thai, new List<string> { user }, id_chien_dich, id_quang_cao, is_admin);
                res.success = dic != null;
                if (res.success)
                {
                    res.data = dic;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            return Ok(res);
        }

        [HttpGet]
        [Route("getall")]
        public IActionResult GetAll(string fields)
        {
            string[] view_fields = fields.Split(",");
            DataResponsePaging res = new DataResponsePaging();
            try
            {
                var quang_cao = QuangCaoRepository.Instance.GetAll(new List<string>() { user }, view_fields);
                res.success = quang_cao != null;
                if (res.success)
                {
                    res.data = quang_cao;
                    res.total = total_recs;
                }
            }
            catch (Exception ex)
            {
                res.msg = ex.Message; res.success = false;
            }

            return Ok(res);
        }

        [HttpGet]
        [Route("get/{id}")]
        public IActionResult Get(string id)
        {
           
            DataResponse res = new DataResponse();
            try
            {
                if (QuangCaoRepository.Instance.IsOwner<QuangCao>(id, user))
                {
                    var quang_cao = QuangCaoRepository.Instance.GetById(id, null);
                    
                    res.success = quang_cao != null;
                    if (res.success)
                    {
                        res.data = quang_cao;
                    }
                }
                else
                {
                    res.msg = "NOT FOUND";
                }
            }
            catch (Exception ex)
            {
                res.msg = ex.Message; res.success = false;
            }

            return Ok(res);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("view/{id}/{id_tk}")]
        public IActionResult ViewAd(string id, string id_tk)
        {
            DataResponse res = new DataResponse();
            try
            {
                var quang_cao = QuangCaoRepository.Instance.GetById(id, new string[] { "ten_hien_thi", "link_dich", "link_hien_thi", "tieu_de_1", "tieu_de_2", "mo_ta_1", "mo_ta_2" });
                res.success = quang_cao != null;
                if (res.success)
                {
                    res.data = quang_cao;
                    //TEST Tăng view của quảng cáo và của từ khóa
                    /*QuangCaoRepository.Instance.IncreaseView(id);
                    TuKhoaRepository.Instance.IncreaseView(id_tk);*/
                }
            }
            catch (Exception ex)
            {
                res.msg = ex.Message; res.success = false;
            }

            return Ok(res);
        }

        // POST api/<QuangCaoController>
        [HttpPost]
        [Route("add")]
        public IActionResult Post([FromBody] QuangCao quang_cao)
        {
            DataResponse res = new DataResponse();
            try
            {
                res.success = true;
                //validate
                if (string.IsNullOrEmpty(quang_cao.ten_hien_thi))
                {
                    res.success = false;
                    res.msg = "Đặt tên cho quảng cáo";
                }
                if (string.IsNullOrEmpty(quang_cao.id_chien_dich))
                {
                    res.success = false;
                    res.msg = "Chưa có chiến dịch";
                }
                if (string.IsNullOrEmpty(quang_cao.tieu_de_1))
                {
                    res.success = false;
                    res.msg = "Đặt tiêu đề cho quảng cáo";
                }
                if (string.IsNullOrEmpty(quang_cao.link_dich))
                {
                    res.success = false;
                    res.msg = "Đặt link đích cho quảng cáo";
                }

                if (!IsValidUri(quang_cao.link_dich))
                {
                    res.success = false;
                    res.msg = "Link đích chưa đúng định dạng";
                }

                if (!res.success)
                {
                    return Ok(res);
                }

                SetMetaData(quang_cao, false);

                string ret_id = QuangCaoRepository.Instance.IndexRetId(quang_cao);

                res.success = !string.IsNullOrEmpty(ret_id);
                if (res.success)
                {
                    var id_chien_dich = QuangCaoRepository.Instance.GetById(ret_id, new string[] { "*" })?.id_chien_dich;
                    var trang_thai_chien_dich =
                        ChienDichRepository.Instance.GetById(id_chien_dich, new[] { "*" })?.trang_thai;
                    QuangCaoRepository.Instance.UpdateTrangThaiQcTheoChienDich(ret_id, (int)trang_thai_chien_dich);

                    res.data = ret_id;
                }
            }
            catch (Exception ex)
            {
                res.msg = ex.Message; res.success = false;
            }

            return Ok(res);
        }

        private bool IsValidUri(String uri)
        {
            return Uri.IsWellFormedUriString(uri, UriKind.Absolute);
        }

        // PUT api/<QuangCaoController>/5
        [HttpPut]
        [Route("update/{id}")]
        public IActionResult Put(string id, [FromBody] QuangCao quang_cao)
        {
            DataResponse res = new DataResponse();
            try
            {
                res.success = true;
                //validate
                if (string.IsNullOrEmpty(quang_cao.ten_hien_thi))
                {
                    res.success = false;
                    res.msg = "Đặt tên cho quảng cáo";
                }
                //if (string.IsNullOrEmpty(quang_cao.id_chien_dich))
                //{
                //    res.success = false;
                //    res.msg = "Chưa có chiến dịch";
                //}
                if (string.IsNullOrEmpty(quang_cao.tieu_de_1))
                {
                    res.success = false;
                    res.msg = "Đặt tiêu đề cho quảng cáo";
                }
                if (string.IsNullOrEmpty(quang_cao.link_dich))
                {
                    res.success = false;
                    res.msg = "Đặt link đích cho quảng cáo";
                }
                if (!IsValidUri(quang_cao.link_dich))
                {
                    res.success = false;
                    res.msg = "Link đích chưa đúng định dạng";
                }

                if (!res.success)
                {
                    return Ok(res);
                }
                quang_cao.id = id;
                SetMetaData(quang_cao, true);

                res.success = QuangCaoRepository.Instance.PartiallyUpdated(quang_cao.id, quang_cao.ten_hien_thi, (int)quang_cao.trang_thai, (int)quang_cao.loai_quang_cao, quang_cao.link_hien_thi, quang_cao.tieu_de_1, quang_cao.tieu_de_2, quang_cao.mo_ta_1, quang_cao.mo_ta_2);
                if (res.success)
                {
                    res.data = "";
                }
            }
            catch (Exception ex)
            {
                res.msg = ex.Message; res.success = false;
            }

            return Ok(res);
        }

        // DELETE api/<QuangCaoController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("tukhoa")]
        public IActionResult GetAdsKeyword(string tu_khoa, int so_luong, string dia_diem)

        {
            DataResponsePaging res = new DataResponsePaging();
            //Tìm tất cả từ khóa đang được chạy quảng cáo
            //Tìm kiểu đối sánh xem có phù hợp với từ khóa hiện tại hay không
            var lst = TuKhoaRepository.Instance.DauGiaQuangCaoTheoTuKhoa(tu_khoa, so_luong <= 0 ? 1 : so_luong, dia_diem);
            res.data = lst;
            /*var lst = TuKhoaRepository.Instance.GetTuKhoaHienThi(tu_khoa, so_luong <= 0 ? 1 : so_luong, dia_diem);

            var lst_id_quang_cao = lst.Select(x => x.id_quang_cao);
            var lst_ads = QuangCaoRepository.Instance.GetMany(lst_id_quang_cao, lst,
                new string[] { "id", "id_chien_dich", "ten_hien_thi", "link_dich", "link_hien_thi", "tieu_de_1", "tieu_de_2", "mo_ta_1", "mo_ta_2", "ids_tu_khoa" });

            res.data = lst_ads.Select(x => new QuangCaoMap(x));
            */
            /*
            foreach (var item in lst_ads)
            {
                QuangCaoRepository.Instance.IncreaseView(item.id);
            }*/
            res.success = true;
            return Ok(res);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("uview/{id}/{id_tk}")]
        public IActionResult UpdateView(string id, string id_tk)
        {
            DataResponsePaging res = new DataResponsePaging();

            res.success = QuangCaoRepository.Instance.IncreaseView(id, id_tk);
            return Ok(res);
        }

        //[AllowAnonymous]
        //[HttpGet]
        //[Route("uclick/{id}/{id_tk}")]
        //public IActionResult UpdateClick(string id, string id_tk)
        //{
        //    DataResponsePaging res = new DataResponsePaging();

        //    res.success = QuangCaoRepository.Instance.IncreaseClick(id, id_tk);
        //    return Ok(res);
        //}

        [AllowAnonymous]
        [HttpGet]
        [Route("go/{id}/{id_tk}/{co}")]
        public IActionResult GoAd(string id, string id_tk, double co)
        {
            var quang_cao = QuangCaoRepository.Instance.GetById(id, new string[] { "link_dich" });
            if (quang_cao != null)
            {
                var tu_khoa = TuKhoaRepository.Instance.GetById(id_tk, new string[] { "url_cuoi" });
                QuangCaoRepository.Instance.IncreaseClick(id, id_tk, co);
                if (tu_khoa != null && !string.IsNullOrEmpty(tu_khoa.url_cuoi))
                    quang_cao.link_dich = tu_khoa.url_cuoi;
                return Redirect(quang_cao.link_dich);
            }
            string referer = Request.Headers["Referer"].ToString();

            return Redirect(referer);
        }

        [HttpGet]
        [Route("getbychiendich")]
        public IActionResult GetByIdChienDich(string id)
        {
            DataResponse res = new DataResponse();
            try
            {
                var quang_cao = QuangCaoRepository.Instance.GetByIdChienDich(id, new[] { "id", "ten_hien_thi" });
                res.success = quang_cao != null;
                if (res.success)
                {
                    res.data = quang_cao;
                }
            }
            catch (Exception ex)
            {
                res.msg = ex.Message; res.success = false;
            }

            return Ok(res);
        }

        [HttpPut]
        [Route("updatetrangthai")]
        public IActionResult UpdateTrangThai(string id, int trang_thai)
        {
            DataResponse res = new DataResponse();
            try
            {
                //var lst_tu_khoa = TuKhoaRepository.Instance.GetByIdQuangCao(id, new[] { "id" })
                //    .Select(x => x.id).ToList();
                //if (lst_tu_khoa.Count > 0)
                //{
                //    foreach (var id_tk in lst_tu_khoa)
                //    {
                //        TuKhoaRepository.Instance.UpdateTrangThaiTkTheoQuangCao(id_tk, trang_thai);
                //    }
                //}

                res.success = QuangCaoRepository.Instance.UpdateTrangThai(id, trang_thai);
                if (res.success)
                {
                    TuKhoaRepository.Instance.UpdateTrangThaiByQuangCao(id, trang_thai);
                }

                res.msg = res.success ? "Cập nhật thành công" : "Cập nhật thất bại";
            }
            catch (Exception ex)
            {
                res.msg = ex.StackTrace; res.success = false;
            }
            return Ok(res);
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("uvcc")]
        public IActionResult UpViewClickChiPhi(string adid, string action, int count, string date, string pro, string prox)
        {
            if (!string.IsNullOrEmpty(prox))
                pro = prox;
            switch (action)
            {
                case "s":
                    QuangCaoRepository.Instance.IncreaseView(adid, pro, count);
                    break;

                case "v":
                    QuangCaoRepository.Instance.IncreaseView(adid, pro, count);
                    break;

                case "c":
                    QuangCaoRepository.Instance.IncreaseClick(adid, pro);
                    break;
            }
            return Ok();
        }
    }
}