using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;

namespace JobokoAdsAPI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = TokenManager.GetValidationParameters();
                options.Events = new JwtBearerEvents
                {
                    OnForbidden = context =>
                    {
                        return Task.CompletedTask;
                    },
                    OnAuthenticationFailed = context =>
                    {


                        if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                        {
                            //context.Response.Headers.Add("Token-Expired", "true");
                            string token = "";
                            if (context.Request.Headers.ContainsKey("Authorization"))
                                token = context.Request.Headers["Authorization"][0];
                            var tokenHandler = new JwtSecurityTokenHandler();
                            var validationParameters = TokenManager.GetValidationParameters();
                            if (token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                                token = token.Substring("Bearer ".Length).Trim();
                            var jwt = tokenHandler.ReadJwtToken(token);

                            if (jwt.Payload["ipad"].ToString() == context.HttpContext.Connection.RemoteIpAddress.MapToIPv6().ToString())
                            {
                                var new_role = new List<string>() { };
                                //QLCUNL.BL.UserBL.GetRoles(jwt.Payload["nameid"].ToString());
                                token = TokenManager.BuildToken(jwt.Payload["nameid"].ToString(), new_role, jwt.Payload["given_name"].ToString(), jwt.Payload["ipad"].ToString());
                                context.Response.Headers.Add("New-Token", token);
                                SecurityToken new_token = null;
                                var prin = tokenHandler.ValidateToken(token, validationParameters, out new_token);

                                context.Principal = prin;
                                context.Success();
                            }
                        }

                        return Task.CompletedTask;
                    },
                    OnMessageReceived = context =>
                    {
                        return Task.CompletedTask;
                    }
                };
            });
            services.AddAuthorization(options =>
            {
                options.AddPolicy("SameIpPolicy", policy =>
                {
                    policy.AuthenticationSchemes.Add(JwtBearerDefaults.AuthenticationScheme);
                    policy.RequireAuthenticatedUser();
                });
            });
            services.AddControllers().AddNewtonsoftJson();
            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy", b =>
                {
                    b.WithOrigins(Configuration["Web:Origin"].Split(",")).AllowCredentials().AllowAnyHeader().AllowAnyMethod().WithExposedHeaders("new-token").Build();
                });
            });
            services.AddApiVersioning(x =>
            {
                x.DefaultApiVersion = new ApiVersion(1, 0);
                x.AssumeDefaultVersionWhenUnspecified = true;
                x.ReportApiVersions = true;
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            //app.UseHttpsRedirection();

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseCors("CorsPolicy");
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                //endpoints.MapRazorPages();
            });

        }
    }
}
