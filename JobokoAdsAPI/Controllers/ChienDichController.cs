using JobokoAdsAPI.Models;
using JobokoAdsES;
using JobokoAdsModels;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace JobokoAdsAPI.Controllers
{
    public class ChienDichController : APIBase
    {
        [HttpGet]
        [Route("all")]
        public IActionResult GetAll(string term, int trang_thai, string id_chien_dich, string field_sort, int sort_order, int page = 1, int page_size = 50)
        {
            DataResponsePaging res = new DataResponsePaging();
            try
            {
                LogRepository.Instance.TraCuuTuKhoa(new List<TuKhoa>(), new List<string>(), 0, 0);

                dic_sort = !string.IsNullOrEmpty(field_sort) ? new Dictionary<string, bool> { { field_sort, sort_order == 1 } } : new Dictionary<string, bool> { { "ngay_sua", sort_order == 1 } };
                var chien_dich = ChienDichRepository.Instance.GetAll(term, new List<string>() { user }, false, new string[] { "*" }, dic_sort, trang_thai, id_chien_dich, page, page_size, out total_recs);
                res.success = chien_dich != null;
                if (res.success)
                {
                    res.data = chien_dich;
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
        [Route("getsum")]
        public IActionResult DicTinhTong(string term, int trang_thai, string id_chien_dich)
        {
            DataResponse res = new DataResponse();
            try
            {
                var dic = ChienDichRepository.Instance.FilterAggregations(term, new string[] { "ten" }, trang_thai, id_chien_dich,
                    new List<string> { user }, is_admin);
                res.success = dic != null;
                res.data = res.success ? dic : null;
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
                var dic = ChienDichRepository.Instance.FilterAggregations("", new string[] { "ten" }, -1, "",
                    new List<string> { user }, is_admin);
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
                var chien_dich = ChienDichRepository.Instance.GetAll(new List<string>() { user }, view_fields);
                res.success = chien_dich != null;
                if (res.success)
                {
                    res.data = chien_dich;
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
                if (ChienDichRepository.Instance.IsOwner<ChienDich>(id, user))
                {
                    var chien_dich = ChienDichRepository.Instance.GetById(id, null);
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

        // POST api/<ChienDichController>
        [HttpPost]
        [Route("add")]
        public IActionResult Post([FromBody] ChienDich chien_dich)
        {
            DataResponse res = new DataResponse();
            try
            {
                res.success = true;
                //validate
                if (string.IsNullOrEmpty(chien_dich.ten))
                {
                    res.success = false;
                    res.msg = "Đặt tên cho chiến dịch";
                }
                if (chien_dich.ngan_sach <= 1000)
                {
                    res.success = false;
                    res.msg = "Ngân sách cho chiến dịch quá nhỏ";
                }
                if (chien_dich.ngay_bat_dau <= 0)
                {
                    res.success = false;
                    res.msg = "Chọn ngày bắt đầu cho chiến dịch";
                }
                if (!res.success)
                {
                    return Ok(res);
                }

                if (chien_dich.ngay_bat_dau > chien_dich.ngay_ket_thuc)
                {
                    res.success = false;
                    res.msg = "Ngày bắt đầu phải nhỏ hơn ngày kết thúc";
                }

                chien_dich.ngay_chay = chien_dich.trang_thai == 0 ? XMedia.XUtil.TimeInEpoch(DateTime.Now) : 0;

                SetMetaData(chien_dich, false);

                string ret_id = ChienDichRepository.Instance.IndexRetId(chien_dich);
                res.success = !string.IsNullOrEmpty(ret_id);
                if (res.success)
                {
                    res.data = ret_id;
                }
            }
            catch (Exception ex)
            {
                res.msg = ex.Message; res.success = false;
            }

            return Ok(res);
        }

        // PUT api/<ChienDichController>/5
        [HttpPut]
        [Route("update/{id}")]
        public IActionResult Put(string id, [FromBody] ChienDich chien_dich)
        {
            DataResponse res = new DataResponse();
            try
            {
                res.success = true;
                //validate
                if (string.IsNullOrEmpty(chien_dich.ten))
                {
                    res.success = false;
                    res.msg = "Đặt tên cho chiến dịch";
                }
                if (chien_dich.ngan_sach <= 1000)
                {
                    res.success = false;
                    res.msg = "Ngân sách cho chiến dịch quá nhỏ";
                }
                if (chien_dich.ngay_bat_dau <= 0)
                {
                    res.success = false;
                    res.msg = "Chọn ngày bắt đầu cho chiến dịch";
                }
                if (chien_dich.ngay_bat_dau > chien_dich.ngay_ket_thuc)
                {
                    res.success = false;
                    res.msg = "Ngày bắt đầu phải nhỏ hơn ngày kết thúc";
                }
                if (!res.success)
                {
                    return Ok(res);
                }
                chien_dich.id = id;

                chien_dich.ngay_chay = chien_dich.trang_thai == 0 ? XMedia.XUtil.TimeInEpoch(DateTime.Now) : 0;

                SetMetaData(chien_dich, true);

                //res.success = ChienDichRepository.Instance.Update(chien_dich);

                res.success = ChienDichRepository.Instance.PartiallyUpdated(chien_dich.id, chien_dich.ten,
                    (int)chien_dich.trang_thai, chien_dich.ngan_sach, chien_dich.gia_thau, chien_dich.ngay_bat_dau,
                    chien_dich.ngay_ket_thuc, chien_dich.ip_loai_tru);
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

        // DELETE api/<ChienDichController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="id">id chien chich</param>
        /// <param name="trang_thai"> trang thai muon update</param>
        /// <returns></returns>
        [HttpPut]
        [Route("updatetrangthai")]
        public IActionResult UpdateTrangThai(string id, int trang_thai)
        {
            DataResponse res = new DataResponse();
            try
            {
                // update trạng thái của quảng cáo - trang_thai_chien_dich

                // update trạng thái của từ khóa - trang_thai_chien_dich

                res.success = ChienDichRepository.Instance.UpdateTrangThai(id, trang_thai);
                if (res.success)
                {
                    ChienDich cd = new ChienDich
                    {
                        ngay_chay = trang_thai == 0 ? XMedia.XUtil.TimeInEpoch(DateTime.Now) : 0
                    };
                    ChienDichRepository.Instance.UpdateNgayChay(id, cd.ngay_chay);
                    QuangCaoRepository.Instance.UpdateTrangThaiQcTheoChienDich(id, trang_thai);
                    TuKhoaRepository.Instance.UpdateTrangThaiByChienDich(id, trang_thai);
                }
                res.msg = res.success ? "Cập nhật thành công" : "Cập nhật thất bại";
            }
            catch (Exception ex)
            {
                res.msg = ex.StackTrace; res.success = false;
            }
            return Ok(res);
        }

        [HttpPut]
        [Route("updatediadiem/{id}")]
        public IActionResult UpdateDiaDiem(string id, [FromBody] ChienDich chien_dich)
        {
            DataResponse res = new DataResponse();
            try
            {
                chien_dich.id = id;

                res.success = ChienDichRepository.Instance.UpdateDiaDiem(chien_dich.id, chien_dich.dia_diem_muc_tieu,
                    chien_dich.dia_diem_loai_tru);

                if (res.success)
                {
                    var tu_khoa = TuKhoaRepository.Instance.GetByIdChienDich(chien_dich.id, new[] { "id" });
                    foreach (var tk in tu_khoa)
                    {
                        TuKhoaRepository.Instance.UpdateDiaDiem(tk.id, chien_dich.dia_diem_muc_tieu,
                            chien_dich.dia_diem_loai_tru);
                    }
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
        [Route("overview")]
        public IActionResult Overview(string sort, int desc)
        {
            DataResponsePaging res = new DataResponsePaging();
            dic_sort = new Dictionary<string, bool>() { { sort, desc == 1 } };
            try
            {
                var chien_dich = ChienDichRepository.Instance.Overview(new List<string>() { user }, new string[] { "ten", "trang_thai", "chi_phi", "luot_click", "luot_hien_thi", "cpc_trung_binh" }, dic_sort);
                res.success = chien_dich != null;
                List<ChienDichMapV1> lst_map_v1 = new List<ChienDichMapV1>();
                foreach (var item in chien_dich)
                {
                    ChienDichMapV1 map_v1 = new ChienDichMapV1
                    {
                        ten = item.ten,
                        chi_phi = item.chi_phi,
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

        [HttpGet]
        [Route("province")]
        public IActionResult Province()
        {
            DataResponse res = new DataResponse();
            try
            {
                res.success = !string.IsNullOrEmpty(listProvinceCountry());
                if (res.success)
                {
                    res.data = listProvinceCountry();
                }
            }
            catch (Exception ex)
            {
                res.msg = ex.Message; res.success = false;
            }

            return Ok(res);
        }

        [HttpGet]
        [Route("report")]
        public IActionResult Report(string tu_khoa, string quang_cao, long ngay_bat_dau, long ngay_ket_thuc)
        {
            DataResponsePaging res = new DataResponsePaging();
            try
            {
                var chien_dich = ChienDichRepository.Instance.Report(tu_khoa, quang_cao, ngay_bat_dau, ngay_ket_thuc, new List<string>() { user }, is_admin);
                res.success = chien_dich.Count > 0;
                if (res.success)
                    res.data = chien_dich;
            }
            catch (Exception ex)
            {
                res.msg = ex.Message; res.success = false;
            }

            return Ok(res);
        }
    }
}