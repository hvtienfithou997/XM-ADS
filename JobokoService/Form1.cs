using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JobokoService
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void btnSearchLog_Click(object sender, EventArgs e)
        {
            rtbLog.Text = "";
            var dic = JobokoAdsES.LogRepository.Instance.TraCuuTuKhoaV2(txtTerm.Text, new List<string>() { "ext.s.c", "ext.c.c", "ext.v.c" }, JobokoAdsModels.KieuDoiSanh.KHOP_CHINH_XAC,
                DateTime.Now.AddDays(-7).Date.Ticks, DateTime.Now.Ticks);
            rtbLog.Text = Newtonsoft.Json.JsonConvert.SerializeObject(dic, Newtonsoft.Json.Formatting.Indented);



            var all_cam = JobokoAdsES.ChienDichRepository.Instance.GetAll(new List<string>() { }, new string[] { "id" });
            foreach (var item in all_cam)
            {
                JobokoAdsES.QuangCaoRepository.Instance.TinhDiemToiUuChienDich(item.id);
            }

        }
        void TuDongDonTienChienDich(string nguoi_dung)
        {
            //Logic.TuDongDonTienChienDich(nguoi_dung);
            Dictionary<string, Dictionary<string, Dictionary<string, double>>> dic = new Dictionary<string, Dictionary<string, Dictionary<string, double>>>();
            dic.Add("id_chien_dich", new Dictionary<string, Dictionary<string, double>>() { 
                { "nhan_vien_kinh_doanh", new Dictionary<string, double>() { { "sum_joboko_", 122 }, { "avg_joboko_", 1.2 } } } ,
                { "ke_toan", new Dictionary<string, double>() { { "sum_joboko_", 213 }, { "avg_joboko_", 23.5 } } }
            });


        }
    }
}
