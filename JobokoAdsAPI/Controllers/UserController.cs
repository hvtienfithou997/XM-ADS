using JobokoAdsAPI.Models;
using JobokoAdsES;
using JobokoAdsModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JobokoAdsAPI.Controllers
{
    public class UserController : APIBase
    {
        [AllowAnonymous]
        [HttpPost]
        [Route("login")]
        public IActionResult Login([FromBody] object value)
        {
            var obj = JToken.Parse(value.ToString());
            JobokoAdsModels.User u_info = new JobokoAdsModels.User();
            bool is_success = false;
            string msg = ""; string token = "";
            var ip_add = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv6().ToString();
            string browser = Request.Headers["User-Agent"];
            if (obj != null)
            {
                string user_name = obj["user"]?.ToString();
                string password = obj["pass"]?.ToString();
                password = XMedia.XUtil.Encode(password);
                u_info = JobokoAdsES.UserRepository.Instance.Login(user_name, password);
                is_success = u_info != null;

                if (is_success)
                {
                    msg = $"Chào {u_info.full_name}!";
                    token = TokenManager.BuildToken(u_info.user_name, u_info.roles, u_info.full_name, ip_add);
                }
                else
                {
                    msg = "Đăng nhập không thành công";
                }
            }
            else
            {
                msg = "Yêu cầu tham số user và pass";
            }

            return Ok(new
            {
                data = !is_success ? new object() : new
                {
                    u_info.full_name,
                    u_info.user_name,
                    u_info.email,
                    roles = u_info.roles == null ? new List<string>() : u_info.roles
                },
                success = is_success,
                msg,
                token
            });
        }

        [HttpGet]
        [Route("all")]
        public IActionResult GetAll(string term, int page = 1, int page_size = 50)
        {
            DataResponsePaging res = new DataResponsePaging();
            try
            {
                if (!is_admin)
                    return Ok(res);

                var user = UserRepository.Instance.GetAll(term, new List<string>(), is_admin, new[] { "*" }, dic_sort, page, page_size, out total_recs).ToList();

                res.success = user != null;
                if (res.success)
                {
                    res.data = user;
                    res.total = total_recs;
                }
            }
            catch (Exception ex)
            {
                res.msg = ex.Message; res.success = false;
            }

            return Ok(res);
        }

        [HttpGet]
        [Route("search")]
        public IActionResult Search(string term, string id_team, int page, int page_size)
        {
            long total_recs = 0; string msg = "";
            string[] fields = new string[] { "user_name", "full_name", "id_team", "email" };
            string[] fields_group = new string[] { "id_team", "team_name" };

            List<JobokoAdsModels.User> data = new List<JobokoAdsModels.User>();
            var lst_map = new List<JobokoAdsModels.User>();
            if (is_admin)
            {
                fields = new string[] { "user_name", "full_name", "id_team", "email", "last_login", "ip", "type" };
                //data = BL.UserBL.Search(app_id, term, id_team, page, out total_recs, out msg, page_size);

                //foreach (var item in data)
                //{
                //    Models.UserMap mm = new Models.UserMap(item, all_team);
                //    lst_map.Add((mm));
                //}
            }
            else
            {
                //var user_obj = BL.UserBL.GetByUserName(user);
                //Models.UserMap mm = new Models.UserMap(user_obj, all_team);
                //lst_map.Add((mm));
            }

            return Ok(new DataResponsePaging() { data = lst_map, success = data != null, msg = msg, total = total_recs });
        }

        // GET: api/User/5
        [HttpGet]
        [Route("view")]
        public IActionResult Get(string id)
        {
            if (!is_admin)
                return Ok(new DataResponse());

            var user = JobokoAdsES.UserRepository.Instance.GetById(id, new string[] { "*" });

            if (user == null)
                return BadRequest();

            string user_str = Newtonsoft.Json.JsonConvert.SerializeObject(user);
            dynamic data = new System.Dynamic.ExpandoObject();
            data = Newtonsoft.Json.JsonConvert.DeserializeObject(user_str);
            return Ok(new DataResponse() { data = data, success = user != null, msg = "" });
        }




        // POST: api/User
        [HttpPost]
        [Route("add")]
        public IActionResult Post([FromBody] object value)
        {
            DataResponse res = new DataResponse();
            try
            {
                var obj = Newtonsoft.Json.JsonConvert.DeserializeObject<JobokoAdsModels.User>(value.ToString());

                SetMetaData(obj, false);
                if (!is_admin)
                {
                    obj.roles.Remove(Role.ADMIN.ToString());
                }
                if (!string.IsNullOrEmpty(obj.password))
                {
                    obj.password = XMedia.XUtil.Encode(obj.password);
                }
                string user_id = JobokoAdsES.UserRepository.Instance.IndexRetId(obj);
                res.success = user_id == obj.user_name;
            }
            catch (Exception ex)
            {
                res.msg = ex.Message; res.success = false;
            }

            return Ok(res);
        }

        // PUT: api/User/5
        [HttpPut("{id}")]

        public IActionResult Put(string id, [FromBody] object value)
        {
            DataResponse res = new DataResponse();
            try
            {
                var obj = Newtonsoft.Json.JsonConvert.DeserializeObject<User>(value.ToString());
                obj.id = id;
                var setting = JToken.Parse(value.ToString());

                if (is_admin || id == user)
                {
                    SetMetaData(obj, true);
                    if (!is_admin)//Không có quyền sys_admin thì ko cho phép thêm role sys_admin và ko cho phép đổi APP_ID
                    {
                        obj.roles.Remove(Role.ADMIN.ToString());
                    }

                    var get_user = JobokoAdsES.UserRepository.Instance.GetById(id, new string[] { "*" });
                    obj.id = get_user.id;
                    if (!string.IsNullOrEmpty(obj.password))
                    {
                        obj.password = XMedia.XUtil.Encode(obj.password);
                    }
                    res.success = JobokoAdsES.UserRepository.Instance.Update(obj);
                }
                else
                    return BadRequest();
            }
            catch (Exception ex)
            {
                res.msg = ex.StackTrace; res.success = false;
            }
            return Ok(res);
        }

        [HttpPut]
        [Route("userpass")]
        public IActionResult UserPass([FromBody] object value)
        {
            var jo = JToken.Parse(value.ToString());
            var id = jo["id"].ToString();
            var pass_cu = jo["pass_cu"].ToString();
            var pass_moi_1 = jo["pass_moi_1"].ToString();
            var pass_moi_2 = jo["pass_moi_2"].ToString();
            DataResponse res = new DataResponse();
            try
            {
                res.success = true;

                if (!string.IsNullOrEmpty(pass_cu) && !string.IsNullOrEmpty(pass_moi_1) &&
                    !string.IsNullOrEmpty(pass_moi_2))
                {
                    var user_pass_endcode = XMedia.XUtil.Encode(pass_cu);
                    var _user = UserRepository.Instance.GetById(id, new[] { "user_name", "password" });

                    if (!string.IsNullOrEmpty(pass_cu) && !string.IsNullOrEmpty(pass_moi_1))
                    {
                        if (user_pass_endcode != _user.password)
                        {
                            res.msg = "Mật khẩu cũ không chính xác!";
                            res.success = false;
                        }
                    }

                    if (!string.IsNullOrEmpty(pass_moi_1) && !string.IsNullOrEmpty(pass_moi_2))
                    {
                        if (pass_moi_1 != pass_moi_2)
                        {
                            res.msg = "Nhập lại mật khẩu mới không khớp!";
                            res.success = false;
                        }
                    }
                    if (!res.success)
                    {
                        return Ok(res);
                    }

                    if (is_admin || id == user)
                    {
                        res.success = UserRepository.Instance.UserPass(id, pass_moi_1);
                    }

                    if (res.success)
                        res.msg = "Thay đổi thành công";
                }
                else
                {
                    res.msg = "Bạn cần nhập đầy đủ các thông tin";
                    res.success = false;
                }
            }
            catch (Exception ex)
            {
                res.msg = ex.StackTrace; res.success = false;
            }
            return Ok(res);
        }

        [HttpPut]
        [Route("userinfo")]
        public IActionResult UserInfo([FromBody] object value)
        {
            var jo = JToken.Parse(value.ToString());
            var id = jo["id"].ToString();
            var ten_day_du = jo["ten_day_du"].ToString();
            DataResponse res = new DataResponse();
            try
            {
                res.success = true;
                if (string.IsNullOrEmpty(ten_day_du))
                {
                    res.msg = "Chưa nhập tên đầy đủ";
                    res.success = false;
                }

                if (!res.success)
                {
                    return Ok(res);
                }
                if (is_admin || id == user)
                {
                    res.success = UserRepository.Instance.UserInfo(id, ten_day_du);
                }

                if (res.success)
                    res.msg = "Thay đổi thành công";
            }
            catch (Exception ex)
            {
                res.msg = ex.StackTrace; res.success = false;
            }
            return Ok(res);
        }

        [HttpPut]
        [Route("reset")]
        public IActionResult ResetPassword([FromBody] object value)
        {
            if (is_admin)
            {
                DataResponse res = new DataResponse();
                try
                {
                    //var user_reset_pass = Newtonsoft.Json.JsonConvert.DeserializeObject<User>(value.ToString());
                    //user_reset_pass.id_user = user_reset_pass.id_user;
                    //SetMetaData(user_reset_pass, true);
                    //user_reset_pass.password = XMedia.XUtil.Encode(user_reset_pass.password);
                    //res.success = QLCUNL.BL.UserBL.ResetPassWord(user_reset_pass.id_user, user_reset_pass.password,
                    //    out string msg, (is_admin || is_app_admin));
                    //res.msg = msg;
                }
                catch (Exception ex)
                {
                    res.msg = ex.StackTrace;
                    res.success = false;
                }

                return Ok(res);
            }

            return BadRequest();
        }

        [HttpGet]
        [Route("viewngansach")]
        public IActionResult NganSach()
        {
            var user = JobokoAdsES.UserRepository.Instance.GetById(base.user, new string[] { "*" });
            UserRepository.Instance.UpdateNganSach(base.user, 400000);
            if (user == null)
                return BadRequest();

            string user_str = Newtonsoft.Json.JsonConvert.SerializeObject(user);
            dynamic data = new System.Dynamic.ExpandoObject();
            data = Newtonsoft.Json.JsonConvert.DeserializeObject(user_str);
            return Ok(new DataResponse() { data = data, success = user != null, msg = "" });
        }

        /// <summary>
        /// API cập nhật ngân sách
        /// </summary>
        /// <param name="ngan_sach">ngân sách</param>
        /// <returns></returns>
        [HttpPut]
        [Route("ngansach")]
        public IActionResult UpdateNganSach(double ngan_sach)
        {
            DataResponse res = new DataResponse();
            try
            {
                res.success = UserRepository.Instance.UpdateNganSach(user, ngan_sach);
                res.msg = res.success ? "Cập nhật thành công" : "Cập nhật thất bại";
            }
            catch (Exception ex)
            {
                res.msg = ex.StackTrace; res.success = false;
            }
            return Ok(res);
        }
    }
}