using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace GZY.Quartz.MUI.Middleware
{
    public class QuartzUIBasicAuthMiddleware
    {
        private readonly RequestDelegate next;
        private readonly IConfiguration _configuration;

        public QuartzUIBasicAuthMiddleware(RequestDelegate next, IConfiguration configuration)
        {
            this.next = next;
            _configuration = configuration;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            //拦截QuartzUI开头的访问
            if (context.Request.Path.StartsWithSegments("/QuartzUI"))
            {
                string authHeader = context.Request.Headers["Authorization"];
                if (authHeader != null && authHeader.StartsWith("Basic "))
                {
                    //帐户密码读取并解码
                    var encodedUsernamePassword = authHeader.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries)[1]?.Trim();

                    var decodedUsernamePassword = Encoding.UTF8.GetString(Convert.FromBase64String(encodedUsernamePassword));

                    var username = decodedUsernamePassword.Split(':', 2)[0];
                    var password = decodedUsernamePassword.Split(':', 2)[1];


                    if (IsAuthorized(username, password))
                    {
                        await next.Invoke(context);
                        return;
                    }
                }

                context.Response.Headers["WWW-Authenticate"] = "Basic";

                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            }
            else
            {
                await next.Invoke(context);
            }
        }

        /// <summary>
        /// 设置密码
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public bool IsAuthorized(string username, string password)
        {
            // 从配置读取帐户密码,否则默认
            var Username = _configuration["QuartzUI:UserName"] ?? "Admin";
            var Pwd = _configuration["QuartzUI:Pwd"] ?? "123456";
            return username.Equals(Username, StringComparison.InvariantCultureIgnoreCase)
                    && password.Equals(Pwd);
        }

    }
}
