using Nest;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace JobokoAdsModels
{
    public class ChienDich : BaseModel
    {
        [DisplayName("Chiến dịch")]
        public string ten { get; set; }
        [DisplayName("Trạng thái")]
        public TrangThaiChienDich trang_thai { get; set; }
        [DisplayName("Địa điểm mục tiêu")]
        public List<string> dia_diem_muc_tieu { get; set; }
        [DisplayName("Địa điểm loại trừ")]
        public List<string> dia_diem_loai_tru { get; set; }
        [DisplayName("Ngôn ngữ")]
        public List<string> ngon_ngu { get; set; }
        [DisplayName("Ngân sách")]
        public double ngan_sach { get; set; }
        [DisplayName("Ngân sách tối đa")]
        public double ngan_sach_toi_da { get; set; }
        [DisplayName("Chi phí (số tiền đã tiêu)")]
        public double chi_phi { get; set; }
        [DisplayName("Giá thầu")]
        public GiaThau gia_thau { get; set; }
        [DisplayName("Ngày chạy chiến dịch")]
        public long ngay_chay { get; set; }
        [DisplayName("Ngày bắt đầu")]
        public long ngay_bat_dau { get; set; }
        [DisplayName("Ngày kết thúc")]
        public long ngay_ket_thuc { get; set; }
        [DisplayName("IP loại trừ")]

        public List<string> ip_loai_tru { get; set; }
        
        [Keyword]
        public List<string> ids_tu_khoa { get; set; }
        
        [Keyword]
        public List<string> ids_quang_cao { get; set; }
        [DisplayName("Lượt click")]
        public long luot_click { get; set; }
        [DisplayName("Lượt hiển thị")]
        public long luot_hien_thi { get; set; }
        [DisplayName("CPC T/b")]
        public double cpc_trung_binh { get; set; }
        [DisplayName("Tỷ lệ tương tác")]
        public double ty_le_tuong_tac { get; set; }
        [DisplayName("Chi phí ngày hiện tại")]
        public double chi_phi_ngay { get; set; }
        [DisplayName("Ngày tính chi phí (đầu ngày)")]
        public long ngay_hien_tai{ get; set; }
        public int do_uu_tien { get; set; }
        public double diem_toi_uu { get; set; }
    }
}
