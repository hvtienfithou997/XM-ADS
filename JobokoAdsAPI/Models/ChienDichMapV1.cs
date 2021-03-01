using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JobokoAdsModels;

namespace JobokoAdsAPI.Models
{
    public class ChienDichMapV1
    {

        public string ten { get; set; }
        public TrangThaiChienDich trang_thai { get; set; }
        public double chi_phi { get; set; }
        public long luot_click { get; set; }
        public long luot_hien_thi { get; set; }

    }
}
