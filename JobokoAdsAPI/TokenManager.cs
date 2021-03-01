using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace JobokoAdsAPI
{
    public static class TokenManager
    {
        public static TokenValidationParameters GetValidationParameters()
        {
            return new TokenValidationParameters()
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = XMedia.XUtil.ConfigurationManager.AppSetting["Jwt:Issuer"],
                ValidAudience = XMedia.XUtil.ConfigurationManager.AppSetting["Jwt:Issuer"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(XMedia.XUtil.ConfigurationManager.AppSetting["Jwt:Key"])),
                ClockSkew = TimeSpan.Zero
            };
        }
        public static string BuildToken(string user_id, IEnumerable<string> roles, string full_name, string ip)
        {
            try
            {
                var claims = new List<Claim>() {
                    new Claim(JwtRegisteredClaimNames.NameId, user_id),
                    new Claim(JwtRegisteredClaimNames.GivenName, full_name),
                    new Claim("ipad", ip)
                };
                if (roles != null && roles.Count() > 0)
                    claims.AddRange(roles.Select(role => new Claim(ClaimsIdentity.DefaultRoleClaimType, role)));

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(XMedia.XUtil.ConfigurationManager.AppSetting["Jwt:Key"]));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var token = new JwtSecurityToken(XMedia.XUtil.ConfigurationManager.AppSetting["Jwt:Issuer"],
                  XMedia.XUtil.ConfigurationManager.AppSetting["Jwt:Issuer"],
                  claims,
                  expires: DateTime.Now.AddDays(Convert.ToInt32(XMedia.XUtil.ConfigurationManager.AppSetting["Jwt:ExpireInMinute"])),
                  signingCredentials: creds);

                return new JwtSecurityTokenHandler().WriteToken(token);
            }
            catch (Exception)
            {
            }
            return "";
        }
    }
}
