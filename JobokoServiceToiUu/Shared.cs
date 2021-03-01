using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobokoServiceToiUu
{
    public static class Shared
    {
        public static void Test()
        {
            var all_cam = JobokoAdsES.ChienDichRepository.Instance.GetAllTrangThaiBat();
            foreach (var item in all_cam)
            {
                JobokoAdsES.QuangCaoRepository.Instance.TinhDiemToiUuChienDich(item);
            }
        }

    }
}
