using JobokoAdsModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JobokoAdsAPI.Models
{
    public class QuangCaoMap
    {
        public string id { get; set; }
        public string id_chien_dich { get; set; }
        public string ten_hien_thi { get; set; }
        public string link_dich { get; set; }
        public string link_hien_thi { get; set; }
        public string tieu_de_1 { get; set; }
        public string tieu_de_2 { get; set; }
        public string mo_ta_1 { get; set; }
        public string mo_ta_2 { get; set; }
        public List<string> ids_tu_khoa { get; set; }
        public double gia_thau { get; set; }
        public QuangCaoMap() { }
        public QuangCaoMap(QuangCao qc)
        {
            this.id = qc.id;
            this.id_chien_dich = qc.id_chien_dich;
            this.ten_hien_thi = qc.ten_hien_thi;
            this.link_dich = qc.link_dich;
            this.link_hien_thi = qc.link_hien_thi;
            this.tieu_de_1 = qc.tieu_de_1;
            this.tieu_de_2 = qc.tieu_de_2;
            this.mo_ta_1 = qc.mo_ta_1;
            this.mo_ta_2 = qc.mo_ta_2;
            this.ids_tu_khoa = qc.ids_tu_khoa;
        }
    }
}
