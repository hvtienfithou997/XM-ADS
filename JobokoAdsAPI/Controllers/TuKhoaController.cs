using JobokoAdsAPI.Models;
using JobokoAdsES;
using JobokoAdsModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace JobokoAdsAPI.Controllers
{
    public class TuKhoaController : APIBase
    {
        [HttpGet]
        [Route("all")]
        public IActionResult GetAll(string term, string id_chien_dich, string id_quang_cao, int page, int page_size, long ngay_tao_from, long ngay_tao_to, int trang_thai, string field_sort, int sort_order)
        {
            DataResponsePaging res = new DataResponsePaging();
            try
            {
                dic_sort = !string.IsNullOrEmpty(field_sort) ? new Dictionary<string, bool> { { field_sort, sort_order == 1 } } : new Dictionary<string, bool> { { "ngay_sua", sort_order == 1 } };

                var tu_khoa = TuKhoaRepository.Instance.GetAll(term, id_chien_dich, id_quang_cao, new List<string>() { user }, false, new string[] { "*" }, dic_sort, trang_thai, page, page_size, out total_recs);
                res.success = tu_khoa != null;
                if (res.success)
                {
                    var dic_quang_cao = new Dictionary<string, QuangCao>();
                    var dic_chien_dich = new Dictionary<string, ChienDich>();
                    if (tu_khoa.Count() > 0)
                    {
                        dic_quang_cao = QuangCaoRepository.Instance.GetMany(tu_khoa.Select(x => x.id_quang_cao).Distinct()).ToDictionary(x => x.id, y => y);
                        dic_chien_dich = ChienDichRepository.Instance.GetMany(tu_khoa.Select(x => x.id_chien_dich).Distinct()).ToDictionary(x => x.id, y => y);
                    }

                    res.data = new TuKhoaMap().Map(tu_khoa, dic_chien_dich, dic_quang_cao);
                    res.total = total_recs;
                    //res.data_sum = TuKhoaRepository.Instance.FilterAggregations(term, new string[] { "tu_khoa" }, trang_thai, new List<string>() { user }, id_chien_dich, id_quang_cao, is_admin);
                }
                else
                {
                    res.data = tu_khoa;
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
                var dic = TuKhoaRepository.Instance.FilterAggregations(term, new string[] { "tu_khoa" }, trang_thai, new List<string>() { user }, id_chien_dich, id_quang_cao, is_admin);
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
        [Route("getsumall")]
        public IActionResult DicTinhTongAll()
        {
            DataResponse res = new DataResponse();
            try
            {
                var dic = TuKhoaRepository.Instance.FilterAggregations(string.Empty, new string[] { "tu_khoa" }, -1, new List<string>() { user }, string.Empty, string.Empty, is_admin);
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
        [Route("getsumtrangthai")]
        public IActionResult DicTinhTong(int trang_thai)
        {
            DataResponse res = new DataResponse();
            try
            {
                var dic = TuKhoaRepository.Instance.FilterAggregations(string.Empty, new string[] { "tu_khoa" }, trang_thai, new List<string>() { user }, string.Empty, string.Empty, is_admin);
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
        [Route("get/{id}")]
        public IActionResult Get(string id)
        {
            DataResponse res = new DataResponse();
            try
            {
                if (TuKhoaRepository.Instance.IsOwner<TuKhoa>(id, user))
                {
                    var chien_dich = TuKhoaRepository.Instance.GetById(id, null);
                    res.success = chien_dich != null;
                    if (res.success)
                    {
                        res.data = chien_dich;
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

        // POST api/<TuKhoaController>
        [HttpPost]
        [Route("add")]
        public IActionResult Post([FromBody] TuKhoa tu_khoa)
        {
            DataResponse res = new DataResponse();
            try
            {
                res.success = true;
                //validate
                if (string.IsNullOrEmpty(tu_khoa.tu_khoa))
                {
                    res.success = false;
                    res.msg = "Chưa có từ khóa";
                }
                if (string.IsNullOrEmpty(tu_khoa.id_chien_dich))
                {
                    var id_chien_dich = QuangCaoRepository.Instance.GetById(tu_khoa.id_quang_cao, new[] { "id_chien_dich" }).id_chien_dich;
                    //res.success = false;
                    //res.msg = "Chưa có chiến dịch";

                    tu_khoa.id_chien_dich = id_chien_dich;
                }

                if (!string.IsNullOrEmpty(tu_khoa.id_chien_dich))
                {
                    var chien_dich = ChienDichRepository.Instance.GetById(tu_khoa.id_chien_dich, new[] { "*" });

                    tu_khoa.dia_diem_muc_tieu = chien_dich.dia_diem_muc_tieu;
                    tu_khoa.dia_diem_loai_tru = chien_dich.dia_diem_loai_tru;
                }

                if (string.IsNullOrEmpty(tu_khoa.id_quang_cao))
                {
                    res.success = false;
                    res.msg = "Chưa có quảng cáo";
                }
                if (!string.IsNullOrEmpty(tu_khoa.url_cuoi))
                {
                    if (!IsValidUri(tu_khoa.url_cuoi))
                    {
                        res.success = false;
                        res.msg = "Url cuối chưa đúng định dạng";
                    }
                }
                if (!res.success)
                {
                    return Ok(res);
                }

                SetMetaData(tu_khoa, false);

                string ret_id = TuKhoaRepository.Instance.IndexRetId(tu_khoa);
                res.success = !string.IsNullOrEmpty(ret_id);
                if (res.success)
                {
                    //add id_tu_khoa va item quang_cao
                    if (!QuangCaoRepository.Instance.UpdateIdTuKhoa(tu_khoa.id_quang_cao, ret_id, out string msg))
                    {
                        res.success = false;
                        res.msg = $"Có lỗi: {msg}";
                    }

                    var select_tu_khoa = TuKhoaRepository.Instance.GetById(ret_id, new[] { "*" });
                    if (select_tu_khoa != null)
                    {
                        var trang_thai_quang_cao = QuangCaoRepository.Instance.GetById(select_tu_khoa.id_quang_cao, new[] { "*" })?.trang_thai;
                        var trang_thai_chien_dich = ChienDichRepository.Instance.GetById(select_tu_khoa.id_chien_dich, new[] { "*" })?.trang_thai;
                        TuKhoaRepository.Instance.UpdateTrangThaiTkTheoQuangCao(ret_id, (int)trang_thai_quang_cao);
                        TuKhoaRepository.Instance.UpdateTrangThaiTkTheoChienDich(ret_id, (int)trang_thai_chien_dich);
                    }

                    res.data = ret_id;
                }
            }
            catch (Exception ex)
            {
                res.msg = ex.Message; res.success = false;
            }

            return Ok(res);
        }

        // PUT api/<TuKhoaController>/5
        [HttpPut]
        [Route("update/{id}")]
        public IActionResult Put(string id, [FromBody] TuKhoa tu_khoa)
        {
            DataResponse res = new DataResponse();
            try
            {
                res.success = true;
                //validate
                if (string.IsNullOrEmpty(tu_khoa.tu_khoa))
                {
                    res.success = false;
                    res.msg = "Chưa có từ khóa";
                }
                //if (string.IsNullOrEmpty(tu_khoa.id_chien_dich))
                //{
                //    res.success = false;
                //    res.msg = "Chưa có chiến dịch";
                //}
                //if (string.IsNullOrEmpty(tu_khoa.id_quang_cao))
                //{
                //    res.success = false;
                //    res.msg = "Chưa có quảng cáo";
                //}
                if (!string.IsNullOrEmpty(tu_khoa.url_cuoi))
                {
                    if (!IsValidUri(tu_khoa.url_cuoi))
                    {
                        res.success = false;
                        res.msg = "Url cuối chưa đúng định dạng";
                    }
                }
                if (!res.success)
                {
                    return Ok(res);
                }
                tu_khoa.id = id;
                SetMetaData(tu_khoa, true);

                res.success = TuKhoaRepository.Instance.PartiallyUpdated(tu_khoa.id, (int)tu_khoa.trang_thai, tu_khoa.tu_khoa, (int)tu_khoa.kieu_doi_sanh, tu_khoa.url_cuoi);
                if (res.success)
                {
                    res.msg = "Sửa thành công";
                    //add id_tu_khoa va item quang_cao
                    //if (!QuangCaoRepository.Instance.UpdateIdTuKhoa(tu_khoa.id_quang_cao, tu_khoa.id, out string msg))
                    //{
                    //    res.success = false;
                    //    res.msg = $"Có lỗi: {msg}";
                    //}
                    //res.data = "";
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

        // DELETE api/<TuKhoaController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("hienthi")]
        public IActionResult GetAdsKeyword(string tu_khoa, int so_luong, string dia_diem)
        {
            DataResponsePaging res = new DataResponsePaging();
            //Tìm tất cả từ khóa đang được chạy quảng cáo
            //Tìm kiểu đối sánh xem có phù hợp với từ khóa hiện tại hay không
            //var str = string.Format("/"{0}/, dia_diem);

            var lst = TuKhoaRepository.Instance.GetTuKhoaHienThi(tu_khoa, so_luong <= 0 ? 1 : so_luong, dia_diem);
            res.data = lst;
            return Ok(res);
        }

        //[AllowAnonymous]
        //[HttpGet]
        //[Route("uview/{id}/{no}")]
        //public IActionResult UpdateViewNo(string id, int no)
        //{
        //    DataResponsePaging res = new DataResponsePaging();

        //    res.success = TuKhoaRepository.Instance.UpdateView(id, no);
        //    return Ok(res);
        //}

        //[AllowAnonymous]
        //[HttpGet]
        //[Route("uview/{id}")]
        //public IActionResult UpdateView(string id)
        //{
        //    DataResponsePaging res = new DataResponsePaging();

        //    res.success = TuKhoaRepository.Instance.IncreaseView(id);
        //    return Ok(res);
        //}

        //[AllowAnonymous]
        //[HttpGet]
        //[Route("uclick/{id}")]
        //public IActionResult UpdateClick(string id)
        //{
        //    DataResponsePaging res = new DataResponsePaging();

        //    res.success = TuKhoaRepository.Instance.IncreaseClick(id);
        //    return Ok(res);
        //}

        //[AllowAnonymous]
        //[HttpGet]
        //[Route("uclick/{id}/{no}")]
        //public IActionResult UpdateClickNo(string id, int no)
        //{
        //    DataResponsePaging res = new DataResponsePaging();

        //    res.success = TuKhoaRepository.Instance.UpdateClick(id, no);
        //    return Ok(res);
        //}

        [HttpPut]
        [Route("updatetrangthai")]
        public IActionResult UpdateTrangThai(string id, int trang_thai)
        {
            DataResponse res = new DataResponse();
            try
            {
                res.success = TuKhoaRepository.Instance.UpdateTrangThai(id, trang_thai);
                res.msg = res.success ? "Cập nhật thành công" : "Cập nhật thất bại";
            }
            catch (Exception ex)
            {
                res.msg = ex.StackTrace; res.success = false;
            }
            return Ok(res);
        }

        [HttpGet]
        [Route("overview")]
        public IActionResult Overview(string sort, int desc)
        {
            DataResponsePaging res = new DataResponsePaging();
            dic_sort = new Dictionary<string, bool>() { { sort, desc == 1 } };
            try
            {
                var chien_dich = TuKhoaRepository.Instance.Overview(new List<string>() { user }, new string[] { "tu_khoa", "trang_thai", "chi_phi", "kieu_doi_sanh", "luot_click", "luot_hien_thi" }, dic_sort);
                res.success = chien_dich != null;

                List<TuKhoaMapV1> lst_map_v1 = new List<TuKhoaMapV1>();
                foreach (var item in chien_dich)
                {
                    var map_v1 = new TuKhoaMapV1
                    {
                        tu_khoa = item.tu_khoa,
                        chi_phi = item.chi_phi,
                        kieu_doi_sanh = item.kieu_doi_sanh,
                        luot_click = item.luot_click,
                        trang_thai = item.trang_thai,
                        luot_hien_thi = item.luot_hien_thi
                    };
                    lst_map_v1.Add(map_v1);
                }
                if (res.success)
                {
                    res.data = lst_map_v1;
                    res.total = total_recs;
                }
            }
            catch (Exception ex)
            {
                res.msg = ex.Message; res.success = false;
            }

            return Ok(res);
        }
    }
}