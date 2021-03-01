using Nest;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobokoAdsModels
{
    [ElasticsearchType(IdProperty = nameof(id))]

    public class TuKhoaQuangCaoLog
    {
        [Keyword]
        public string id { get; set; }
        /// <summary>
        /// Ngày giờ tạo ra log này
        /// </summary>
        public long ngay_tao { get; set; }
        /// <summary>
        /// Ngày tạo ra log này (đầu ngày ko tính giờ) dùng để group theo 1 ngày
        /// </summary>
        public long ngay_tao_group { get; set; }
        public string id_tu_khoa { get; set; }
        public string id_quang_cao { get; set; }
        public string id_chien_dich { get; set; }
        public double chi_phi { get; set; }
        public HanhDongTuKhoa hanh_dong { get; set; }
    }
}
