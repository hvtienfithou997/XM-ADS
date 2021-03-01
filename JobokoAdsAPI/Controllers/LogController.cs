using JobokoAdsAPI.Models;
using JobokoAdsES;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Globalization;
using System.Linq;

namespace JobokoAdsAPI.Controllers
{
    public class LogController : APIBase
    {
        [HttpGet]
        [Route("getlog")]
        public IActionResult GetLog(string term, string ai)
        {
            DataResponse res = new DataResponse();
            try
            {
                if (string.IsNullOrEmpty(term))
                {
                    res.success = false;
                    res.data = string.Empty;
                    return Ok(res);
                }
                var date_start = DateTime.Now.AddDays(-7).Ticks;
                var date_end = DateTime.Now.Ticks;
                var log = LogRepository.Instance.GetMany(ai, term, date_start, date_end);
                res.success = !string.IsNullOrEmpty(log);
                res.data = res.success ? log : "";
                return Ok(res);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            return Ok();
        }

        /// <summary>
        /// Tính trung bình lượt show và lượt click
        /// </summary>
        /// <param name="ai">mã định danh</param>
        /// <param name="term">từ khóa</param>
        /// <param name="date_start">ngày bắt đầu</param>
        /// <param name="date_end">ngày kết thúc</param>
        /// <returns></returns>
        [HttpGet]
        [Route("sumlog")]
        public IActionResult DicTinhTong(string ai, string term)
        {
            DataResponse res = new DataResponse();
            try
            {
                var date_start = DateTime.Now.AddDays(-7).Ticks;
                var date_end = DateTime.Now.Ticks;
                var dic = LogRepository.Instance.TinhTongLog(ai, term, date_start, date_end);
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
        [Route("tralog")]
        public IActionResult TraCuuLog(string tu_khoa, string site_id, string ngay_bat_dau, string ngay_ket_thuc, int page = 1, int page_size = 50)
        {
            DataResponsePaging res = new DataResponsePaging();
            try
            {
                //ngay_bat_dau = ngay_bat_dau <= 0 ? DateTime.Now.AddDays(-7).Ticks : XMedia.XUtil.EpochToTime(ngay_bat_dau).Ticks;
                //ngay_ket_thuc = ngay_ket_thuc <= 0 ? DateTime.Now.Ticks : XMedia.XUtil.EpochToTime(ngay_ket_thuc).Ticks;
                long ngay_bd = parseStringToTicks(ngay_bat_dau), ngay_kt = parseStringToTicks(ngay_ket_thuc);
                page = page <= 0 ? 1 : page;
                var log = LogRepository.Instance.TraCuuLog(tu_khoa, site_id, ngay_bd, ngay_kt, page, out total_recs, page_size);
                res.success = log.Count > 0;
                res.data = res.success ? Newtonsoft.Json.JsonConvert.SerializeObject(log.Select(x => new { k = x.Key, v = x.Value })) : "";
                res.total = total_recs;
                return Ok(res);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            return Ok();
        }

        private DateTime parseStringToDateTime(string str)
        {
            if (!string.IsNullOrEmpty(str))
            {
                if (DateTime.TryParse(str, dtfi, DateTimeStyles.AssumeLocal, out DateTime dt))
                {
                    return dt;
                }
            }
            return new DateTime(1, 1, 1);
        }

        private long parseStringToTicks(string str)
        {
            return parseStringToDateTime(str).Ticks;
        }

        [HttpGet]
        [Route("getsumall")]
        public IActionResult DicTinhTongAll(string tu_khoa, long date_start, long date_end)
        {
            DataResponse res = new DataResponse();
            try
            {
                date_start = DateTime.Now.AddDays(-7).Ticks;
                date_end = DateTime.Now.Ticks;
                var dic = LogRepository.Instance.SumTraCuuLog(tu_khoa, date_start, date_end);
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
        [Route("tukhoatimkiem")]
        public IActionResult ChiTietTuKhoaTimKiem(string tu_khoa, string site_id, string ngay_bat_dau, string ngay_ket_thuc, int page, int page_size = 50)
        {
            DataResponsePaging res = new DataResponsePaging();
            try
            {
                page = page <= 0 ? 1 : page;
                res.success = true;
                long ngay_bd = parseStringToTicks(ngay_bat_dau), ngay_kt = parseStringToTicks(ngay_ket_thuc);
                res.data = LogRepository.Instance.ChiTietTuKhoaTimKiem(tu_khoa, site_id, ngay_bd, ngay_kt, page, out total_recs, page_size);
                res.total = total_recs;
            }
            catch (Exception e)
            {
                res.success = false;
                res.msg = e.Message;
            }

            return Ok(res);
        }

        [HttpGet]
        [Route("trangtukhoatimkiem")]
        public IActionResult TrangHienThiTuKhoaTimKiem(string tu_khoa, string site_id, string ngay_bat_dau, string ngay_ket_thuc, int page, int page_size = 50)
        {
            DataResponsePaging res = new DataResponsePaging();
            try
            {
                page = page <= 0 ? 1 : page;
                res.success = true;
                long ngay_bd = parseStringToTicks(ngay_bat_dau), ngay_kt = parseStringToTicks(ngay_ket_thuc);
                res.data = LogRepository.Instance.TrangHienThiTuKhoaTimKiem(tu_khoa, site_id, ngay_bd, ngay_kt, page, out total_recs, page_size);
                res.total = total_recs;
            }
            catch (Exception e)
            {
                res.success = false;
                res.msg = e.Message;
            }

            return Ok(res);
        }
    }
}