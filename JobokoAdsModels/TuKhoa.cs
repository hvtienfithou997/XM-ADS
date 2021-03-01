using Nest;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace JobokoAdsModels
{
    public class TuKhoa : BaseModel
    {
        [DisplayName("Từ khóa")]
        public string tu_khoa { get; set; }
        [DisplayName("Kiểu đối sánh")]
        public KieuDoiSanh kieu_doi_sanh { get; set; }
        [DisplayName("URL cuối")]
        public string url_cuoi { get; set; }
        [DisplayName("Lượt click")]
        public long luot_click { get; set; }
        [DisplayName("Lượt hiển thị")]
        public long luot_hien_thi { get; set; }
        [DisplayName("Chi phí")]
        public double chi_phi { get; set; }
        [DisplayName("Lượt chuyển đổi")]
        public long luot_chuyen_doi { get; set; }
        [DisplayName("Lượt click/ngày")]
        public long luot_click_ngay { get; set; }
        [DisplayName("Lượt hiển thị/ngày")]
        public long luot_hien_thi_ngay { get; set; }
        [DisplayName("CPC T/b")]
        public double cpc_trung_binh { get; set; }
        [DisplayName("Tỷ lệ tương tác")]
        public double ty_le_tuong_tac { get; set; }
        [DisplayName("Chi phí/ngày")]
        public double chi_phi_ngay { get; set; }
        [DisplayName("Lượt chuyển đổi/ngày")]
        public long luot_chuyen_doi_ngay { get; set; }
        [DisplayName("Chiến dịch")]
        
        public string id_chien_dich { get; set; }
        [DisplayName("Quảng cáo")]
        
        public string id_quang_cao { get; set; }
        [DisplayName("Trạng thái")]
        public TrangThaiTuKhoa trang_thai { get; set; }
        [DisplayName("Trạng thái quảng cáo")]
        public TrangThaiQuangCao trang_thai_quang_cao { get; set; }
        [DisplayName("Trạng thái chiến dịch")]
        public TrangThaiChienDich trang_thai_chien_dich { get; set; }
        [DisplayName("Địa điểm mục tiêu")]
        public List<string> dia_diem_muc_tieu { get; set; }
        [DisplayName("Địa điểm loại trừ")]
        public List<string> dia_diem_loai_tru { get; set; }
        [DisplayName("Giá thầu")]
        public double gia_thau { get; set; }
        [Percolator()]
        public QueryContainer query { get; set; }


    }
}
