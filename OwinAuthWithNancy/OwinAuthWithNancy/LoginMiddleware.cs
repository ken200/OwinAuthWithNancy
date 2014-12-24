using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using Owin;
using Microsoft.Owin;
using Microsoft.AspNet.Identity;
using System.Security.Claims;
using System.Diagnostics;

namespace OwinAuthWithNancy
{
    public static class LoginExtentions
    {
        public static IAppBuilder UseLogin(this IAppBuilder app, LoginOptions loginOption, LogoutOptions logoutOption)
        {
            return app.Use<LoginMiddleware>(loginOption, logoutOption);
        }
    }

    public class LoginOptions
    {
        /// <summary>
        /// ログインユーザー名ゲッター
        /// </summary>
        public Func<IOwinContext, Task<string>> UserNameGetter { get; set; }
        /// <summary>
        /// ログインページUrl
        /// </summary>
        /// <remarks>
        /// <para>リクエスト先Urlがこのプロパティ値に一致する場合にログイン認証・リダイレクトを行う。</para>
        /// </remarks>
        public string LoginUrl;
        /// <summary>
        /// ログイン時のHTTPメソッド
        /// </summary>
        public string LoginMethod;
        /// <summary>
        /// ログインページリクエストに含まれるリダイレクト先情報をセットしているクエリパラメーター名
        /// </summary>
        /// <remarks>
        /// <para>指定名勝クエリパラメーターが存在しない場合、DefaultRedirectUrlプロパティに指定したUrlへリダイレクトする。</para>
        /// </remarks>
        public string RedirectUrlQueryName { get; set; }
        /// <summary>
        /// ログイン後のデフォルトリダイレクト先Url
        /// </summary>
        public string DefaultRedirectUrl;

        public LoginOptions()
        {
            UserNameGetter = async (ctx) => {
                return await Task.Run<string>(() => { return ""; });
            };
            this.LoginUrl = "/login";
            this.LoginMethod = "POST";
            this.DefaultRedirectUrl = "/secure";
            this.RedirectUrlQueryName = "RedirectUrl";
        }
    }

    public class LogoutOptions
    {
        /// <summary>
        /// ログアウトページUrl
        /// </summary>
        public string LogoutUrl;
        /// <summary>
        /// ログアウトリクエストに含まれるリダイレクト先情報をセットしているクエリパラメーター名
        /// </summary>
        /// <remarks>
        /// <para>指定名勝クエリパラメーターが存在しない場合、DefaultRedirectUrlプロパティに指定したUrlへリダイレクトする。</para>
        /// </remarks>
        public string RedirectUrlQueryName { get; set; }
        /// <summary>
        /// ログアウト後のデフォルトリダイレクト先Url
        /// </summary>
        public string DefaultRedirectUrl;

        public LogoutOptions()
        {
            this.LogoutUrl = "/logout";
            this.DefaultRedirectUrl = "/";
            this.RedirectUrlQueryName = "RedirectUrl";
        }
    }

    public interface ILoginUserNameGetter
    {
        Task<string> GetAsync(IOwinContext context);
    }

    public class DefaultLoginUserNamgeGetter : ILoginUserNameGetter
    {
        private string _userNameParam;

        public DefaultLoginUserNamgeGetter(string usernameParam)
        {
            this._userNameParam = usernameParam;
        }

        public async Task<string> GetAsync(IOwinContext context)
        {
            var form = await context.Request.ReadFormAsync();
            return form.Get(_userNameParam);
        }
    }

    public class LoginMiddleware : OwinMiddleware
    {
        private LoginOptions _loginOption;
        private LogoutOptions _logoutOption;

        public LoginMiddleware(OwinMiddleware next) 
            : this(next, new LoginOptions(), new LogoutOptions()) { }

        public LoginMiddleware(OwinMiddleware next, LoginOptions loginOption, LogoutOptions logoutOption)
            : base(next)
        {
            this._loginOption = loginOption;
            this._logoutOption = logoutOption;
        }

        public async override Task Invoke(IOwinContext context)
        {
            if(context.Request.Path.Value == _logoutOption.LogoutUrl)
            {
                SignOut(context);
                return;
            }

            if(!IsAuthenticated(context) 
                && context.Request.Path.Value == _loginOption.LoginUrl 
                && context.Request.Method.ToUpper() == _loginOption.LoginMethod)
            {
                await SignIn(context);
                return;
            }

            await Next.Invoke(context);
        }

        private bool IsAuthenticated(IOwinContext context)
        {
            var authUserInfo = context.Authentication.User;
            return authUserInfo != null && !string.IsNullOrEmpty(authUserInfo.Identity.Name) && authUserInfo.Identity.IsAuthenticated;
        }

        private void SignOut(IOwinContext context)
        {
            context.Authentication.SignOut(DefaultAuthenticationTypes.ApplicationCookie);

            var q = context.Request.Query[_logoutOption.RedirectUrlQueryName];
            var redirectPath = !string.IsNullOrEmpty(q) ? q : _logoutOption.DefaultRedirectUrl;
            context.Response.Redirect(redirectPath);
        }

        private async Task SignIn(IOwinContext context)
        {
            var userName = await _loginOption.UserNameGetter(context);

            if (string.IsNullOrEmpty(userName))
            {
                await WriteResponse(context.Response, 401, "invalid parameter.");
                return;
            }

            var uMng = new MyUserManager();
            var userIdentity = await uMng.CreateAsync(userName);
            if (userIdentity == null)
            {
                await WriteResponse(context.Response, 401, "invalid user.");
                return;
            }

            context.Authentication.SignIn(userIdentity);
            var q = context.Request.Query[_loginOption.RedirectUrlQueryName];
            var redirectPath = !string.IsNullOrEmpty(q) ? q : _loginOption.DefaultRedirectUrl;
            context.Response.Redirect(redirectPath);
        }

        private async Task WriteResponse(IOwinResponse response, int status, string content)
        {
            response.StatusCode = status;
            await response.WriteAsync(content);
        }
    }
}