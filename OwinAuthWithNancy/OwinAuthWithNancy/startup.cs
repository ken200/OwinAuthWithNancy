using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using Microsoft.Owin;
using Owin;
using Microsoft.Owin.Security.Cookies;
using Microsoft.AspNet.Identity;

namespace OwinAuthWithNancy
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseCookieAuthentication(new CookieAuthenticationOptions() {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie
            });
            app.UseLogin(
                new LoginOptions() { 
                    LoginUrl = "/login", 
                    LoginMethod = "POST", 
                    UserNameGetter = async (ctx) => {
                        var form = await ctx.Request.ReadFormAsync();
                        return form.Get("userName");
                    },
                    DefaultRedirectUrl = "/secure"},
                new LogoutOptions() { 
                    LogoutUrl = "/logout", 
                    DefaultRedirectUrl = "/" });
            app.UseNancy();
        }
    }
}