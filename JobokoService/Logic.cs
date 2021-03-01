using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobokoService
{
    public static class Logic
    {
        public static void TuDongDonTienChienDich(string nguoi_dung)
        {
            var user = JobokoAdsES.UserRepository.Instance.GetById(nguoi_dung, new string[] { "*" });


            var tong_ngan_sach_nguoi_dung = user.ngan_sach;
            var lst_id_chien_dich = JobokoAdsES.ChienDichRepository.Instance.GetAll(new List<string>() { nguoi_dung }, new string[] { "id" }).Select(x => x.id);
            //1 dự đoán xem chiến dịch nào có khả năng tiêu được nhiều tiền
            //1.1 lấy các từ khóa của các chiến dịch
            var all_tu_khoa = JobokoAdsES.TuKhoaRepository.Instance.GetTuKhoaTheoChienDich(lst_id_chien_dich);
            //1.2 tìm trong log xem các từ khóa đó lượt view, click ra sao
            //viết hàm nhận vào danh sách từ khóa tìm số lượt view, số lượt click
            var dic_tong_hop_theo_chien_dich = new Dictionary<string, object[]>();
            foreach (var tu_khoa in all_tu_khoa)
            {
                var dic = JobokoAdsES.LogRepository.Instance.TraCuuTuKhoaV2(tu_khoa.tu_khoa, new List<string>() { "ext.v.c", "ext.c.c" }, tu_khoa.kieu_doi_sanh, DateTime.Now.AddDays(-5).Date.Ticks, DateTime.Now.Ticks);

                if (!dic_tong_hop_theo_chien_dich.ContainsKey(tu_khoa.id_chien_dich))
                    dic_tong_hop_theo_chien_dich.Add(tu_khoa.id_chien_dich,new object[3]);
                dic_tong_hop_theo_chien_dich[tu_khoa.id_chien_dich][0] = 0;
                dic_tong_hop_theo_chien_dich[tu_khoa.id_chien_dich][1] = 0;
                dic_tong_hop_theo_chien_dich[tu_khoa.id_chien_dich][2] = 0;

            }
            //1.3 tính toán so sánh các chiến dịch để xem khả năng chiến dịch nào tiêu được nhiều tiền
            //2 dồn tiền cho các chiến dịch đó
        }
    }
}
