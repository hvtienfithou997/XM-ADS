using JobokoAdsModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JobokoAdsAPI.Models
{
    public class TuKhoaMap : TuKhoa
    {
        public string quang_cao { get; set; }
        public string chien_dich { get; set; }


        public TuKhoaMap Map(TuKhoa tu_khoa, Dictionary<string, ChienDich> dic_chien_dich, Dictionary<string, QuangCao> dic_quang_cao)
        {
            TuKhoaMap tu_khoa_map = new TuKhoaMap()
            {
                chi_phi = tu_khoa.chi_phi, 
                id = tu_khoa.id,
                id_chien_dich = tu_khoa.id_chien_dich,
                id_quang_cao = tu_khoa.id_quang_cao,
                kieu_doi_sanh = tu_khoa.kieu_doi_sanh,
                luot_chuyen_doi = tu_khoa.luot_chuyen_doi,
                luot_click = tu_khoa.luot_click,
                luot_hien_thi = tu_khoa.luot_hien_thi,
                ngay_sua = tu_khoa.ngay_sua,
                ngay_tao = tu_khoa.ngay_tao,
                nguoi_sua = tu_khoa.nguoi_sua,
                nguoi_tao = tu_khoa.nguoi_tao,
                trang_thai = tu_khoa.trang_thai,
                tu_khoa = tu_khoa.tu_khoa,
                url_cuoi = tu_khoa.url_cuoi,
                ty_le_tuong_tac =  tu_khoa.ty_le_tuong_tac,
                cpc_trung_binh =  tu_khoa.cpc_trung_binh,
                trang_thai_chien_dich = tu_khoa.trang_thai_chien_dich,
                trang_thai_quang_cao = tu_khoa.trang_thai_quang_cao
            };
            try
            {
                if (dic_chien_dich == null) dic_chien_dich = new Dictionary<string, ChienDich>();
                if (dic_quang_cao == null) dic_quang_cao = new Dictionary<string, QuangCao>();

                if (dic_chien_dich.TryGetValue(tu_khoa.id_chien_dich, out ChienDich cd))
                {
                    tu_khoa_map.chien_dich = cd.ten;
                }
                if (dic_quang_cao.TryGetValue(tu_khoa.id_quang_cao, out QuangCao qc))
                {
                    tu_khoa_map.quang_cao = qc.ten_hien_thi;
                }
            }
            catch (Exception)
            {

            }
            return tu_khoa_map;
        }
        public List<TuKhoaMap> Map(IEnumerable<TuKhoa> tu_khoa, Dictionary<string, ChienDich> dic_chien_dich, Dictionary<string, QuangCao> dic_quang_cao)
        {
            List<TuKhoaMap> lst = new List<TuKhoaMap>();
            try
            {
                foreach (var item in tu_khoa)
                {
                    lst.Add(Map(item, dic_chien_dich, dic_quang_cao));
                }
            }
            catch (Exception)
            {

            }
            return lst;
        }
    }

}
