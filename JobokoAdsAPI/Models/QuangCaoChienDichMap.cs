using JobokoAdsModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JobokoAdsAPI.Models
{
    public class QuangCaoChienDichMap: QuangCao
    {
        public string ten_chien_dich { get; set; }
        public QuangCaoChienDichMap(QuangCao qc, IEnumerable<ChienDich> lst_chien_dich)
        {
            this.id = qc.id;
            this.ten_hien_thi = qc.ten_hien_thi;
            this.link_dich = qc.link_dich;
            this.link_hien_thi = qc.link_hien_thi;
            this.tieu_de_1 = qc.tieu_de_1;
            this.tieu_de_2 = qc.tieu_de_2;
            this.mo_ta_1 = qc.mo_ta_1;
            this.mo_ta_2 = qc.mo_ta_2;
            this.ids_tu_khoa = qc.ids_tu_khoa;
            this.luot_click = qc.luot_click;
            this.luot_hien_thi = qc.luot_hien_thi;
            this.trang_thai = qc.trang_thai;
            this.chi_phi = qc.chi_phi;
            if (lst_chien_dich != null)
            {
                Dictionary<string, string> dic_chien_dich = new Dictionary<string, string>();
                foreach (var item in lst_chien_dich)
                {
                    if (!dic_chien_dich.ContainsKey(item.id))
                        dic_chien_dich.Add(item.id, item.ten);
                }
                dic_chien_dich.TryGetValue(qc.id_chien_dich, out string ten);
                this.ten_chien_dich = ten;
            }

            cpc_trung_binh = qc.cpc_trung_binh;
            ty_le_tuong_tac = qc.ty_le_tuong_tac;
            trang_thai_chien_dich = qc.trang_thai_chien_dich;
        }
    }
}
