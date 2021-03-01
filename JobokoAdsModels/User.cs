using Nest;
using System.Collections.Generic;
using System.ComponentModel;

namespace JobokoAdsModels
{
    public class User : BaseModel
    {
        [DisplayName("Tài khoản")]
        [Keyword]
        public string user_name { get; set; }
        [DisplayName("Tên đầy đủ")]
        public string full_name { get; set; }
        [DisplayName("Mật khẩu")]
        public string password { get; set; }
        [DisplayName("Email")]
        public string email { get; set; }
        [DisplayName("Đăng nhập cuối")]
        public long last_login { get; set; }

        [DisplayName("Quyền")]
        [Keyword]
        public List<string> roles { get; set; }
        [DisplayName("Ngân sách")]
        public double ngan_sach { get; set; }
    }
}