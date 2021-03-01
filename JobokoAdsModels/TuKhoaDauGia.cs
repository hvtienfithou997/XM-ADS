using System;
using System.Collections.Generic;
using System.Text;

namespace JobokoAdsModels
{
    public class TuKhoaDauGia
    {
        public string id_tu_khoa { get; set; }
        public string id_quang_cao { get; set; }
        public string id_chien_dich { get; set; }
        public double gia_thau { get; set; }
        public string tu_khoa { get; set; }
        public string link_dich { get; set; }
        public string link_hien_thi { get; set; }
        public string tieu_de_1 { get; set; }
        public string tieu_de_2 { get; set; }
        public string mo_ta_1 { get; set; }
        public string mo_ta_2 { get; set; }
        public double diem_sap_xep { get; set; }
        /// <summary>
        /// điểm tối ưu của chiến dịch: 0 -> 1
        /// </summary>
        public double diem_toi_uu { get; set; }
    }
}
