using Nest;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace JobokoAdsModels
{
    public class QuangCao : BaseModel
    {
        [DisplayName("Tên quảng cáo")]
        public string ten_hien_thi { get; set; }
        [DisplayName("Link đích")]
        public string link_dich { get; set; }
        [DisplayName("Loại quảng cáo")]
        public LoaiQuangcao loai_quang_cao { get; set; }
        [DisplayName("Link hiển thị")]
        public string link_hien_thi { get; set; }
        [DisplayName("Tiêu đề 1")]
        public string tieu_de_1 { get; set; }
        [DisplayName("Tiêu đề 2")]
        public string tieu_de_2 { get; set; }
        [DisplayName("Mô tả quảng cáo 1")]
        public string mo_ta_1 { get; set; }
        [DisplayName("Mô tả quảng cáo 2")]
        public string mo_ta_2 { get; set; }
        [DisplayName("Chiến dịch")]
        [Keyword]
        public string id_chien_dich { get; set; }
        [Keyword]
        public List<string> ids_tu_khoa { get; set; }
        [DisplayName("Trạng thái")]
        public TrangThaiQuangCao trang_thai { get; set; }
        [DisplayName("Lượt click")]
        public long luot_click { get; set; }
        [DisplayName("Lượt hiển thị")]
        public long luot_hien_thi { get; set; }
        [DisplayName("CPC T/b")]
        public double cpc_trung_binh { get; set; }
        [DisplayName("Tỷ lệ tương tác")]
        public double ty_le_tuong_tac { get; set; }
        [DisplayName("Chi phí")]
        public double chi_phi { get; set; }
        [DisplayName("Trạng thái chiến dịch")]
        public TrangThaiChienDich trang_thai_chien_dich { get; set; }
        [DisplayName("Vị trí hiển thị trung bình")]
        public double vi_tri_trung_binh { get; set; }
    }
}
