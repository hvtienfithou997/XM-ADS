using System.ComponentModel.DataAnnotations;

namespace JobokoAdsModels
{
    public enum TrangThaiChienDich
    {
        [Display(Name = "Bật")]
        BAT,
        [Display(Name = "Tạm dừng")]
        TAM_DUNG,
        [Display(Name = "Dừng do hết ngân sách")]
        DUNG_DO_HET_NGAN_SACH
    }

    public enum TrangThaiQuangCao
    {
        [Display(Name = "Bật")]
        BAT,
        [Display(Name = "Tạm dừng")]
        TAM_DUNG,
        [Display(Name = "Xóa")]
        XOA
    }

    public enum TrangThaiTuKhoa
    {
        [Display(Name = "Bật")]
        BAT,
        [Display(Name = "Tạm dừng")]
        TAM_DUNG,
        [Display(Name = "Xóa")]
        XOA
    }

    public enum LoaiQuangcao
    {
        [Display(Name = "Văn bản")]
        VAN_BAN
    }

    public enum KieuDoiSanh
    {
        [Display(Name = "Đối sánh rộng")]
        DOI_SANH_RONG,
        [Display(Name = "Khớp cụm từ")]
        KHOP_CUM_TU,
        [Display(Name = "Khớp chính xác")]
        KHOP_CHINH_XAC
    }

    public enum Role
    {
        ADMIN, EDITOR, USER
    }
    public enum HanhDongTuKhoa
    {
        [Display(Name = "Nhấp chuột")]
        CLICK,
        [Display(Name = "Chuyển đổi")]
        CHUYEN_DOI
    }

}