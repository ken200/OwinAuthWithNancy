using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using Nancy;
using Nancy.Security;
using System.Security.Claims;
using System.Diagnostics;
using Microsoft.Owin.Security.Cookies;

namespace OwinAuthWithNancy
{
    public class MyNancyModule : NancyModule
    {
        public MyNancyModule() : base()
        {
            Get["/"] = _ => 
            {
                return "hello";
            };

            Get["/hoge"] = _ =>
            {
                return "hoge";
            };

            Get["/login"] = _ =>
            {
                return new Nancy.Responses.HtmlResponse()
                {
                    Contents = (s) => 
                    {
                        using (var sw = new StreamWriter(s, System.Text.Encoding.UTF8)) 
                        {
                            sw.Write(@"
<html>
<head>
<title>ログイン</title>
</head>
<body>
<form action=""/login?RedirectUrl=/secure"" method=""post"">
<label for=""username"">ユーザー名</label>
<input type=""text"" name=""username"" />
<input type=""submit"" />
</form>
</html>
");
                        }
                    }
                };
            };
        }
    }

    public class SecureModule : NancyModule
    {
        public SecureModule()
        {
            Get["/secure"] = _ =>
            {
                this.RequiresMSOwinAuthentication();
                var user = this.Context.GetMSOwinUser();
                return string.Format("hello, {0}. this page is secure!!", user.Identity.Name);
            };
        }
    }
}