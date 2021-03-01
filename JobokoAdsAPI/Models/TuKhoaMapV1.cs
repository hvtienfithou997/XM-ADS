using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JobokoAdsModels;

namespace JobokoAdsAPI.Models
{
    public class TuKhoaMapV1
    {
        public string tu_khoa { get; set; }
        public long luot_click { get; set; }
        public double chi_phi { get; set; }
        public KieuDoiSanh kieu_doi_sanh { get; set; }
        public TrangThaiTuKhoa trang_thai { get; set; }
        public long luot_hien_thi { get; set; }
    }
}
