using Abp.Dependency;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OAuth;
using OwnerSayCar.User;
using OwnerSayCar.User.Dto;
using OwnerSayCar.Utilities.Http;
using OwnerSayCar.Web.Models;
using System.Collections.Generic;
using System.Configuration;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace OwnerSayCar.Web.Providers
{
    public class CustomOAuthProvider : OAuthAuthorizationServerProvider
    {
        private const string wxLoginApi = "https://api.weixin.qq.com/sns/jscode2session?appid={0}&secret={1}&js_code={2}&grant_type=authorization_code";

        private readonly IUserAppService _userAppService;

        public override Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            string clientId = string.Empty;
            string clientSecret = string.Empty;
            string symmetricKeyAsBase64 = string.Empty;

            if (!context.TryGetBasicCredentials(out clientId, out clientSecret))
            {
                context.TryGetFormCredentials(out clientId, out clientSecret);
            }

            if (context.ClientId == null)
            {
                context.SetError("invalid_clientId", "client_Id is not set");
                return Task.FromResult<object>(null);
            }

            var audience = AudiencesStore.FindAudience(context.ClientId);

            if (audience == null)
            {
                context.SetError("invalid_clientId", string.Format("Invalid client_id '{0}'", context.ClientId));
                return Task.FromResult<object>(null);
            }

            context.Validated();
            return Task.FromResult<object>(null);
        }

        public override Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {
            context.OwinContext.Response.Headers.Add("Access-Control-Allow-Origin", new[] { "*" });

            string wechatLoginKey = ConfigurationManager.AppSettings["wxLoginKey"];
            if (string.IsNullOrWhiteSpace(wechatLoginKey))
            {
                context.SetError("AppSettings", "Key:wxLoginKey is not found");
                return Task.FromResult<object>(null);
            }

            var identity = new ClaimsIdentity("JWT");

            if (context.Password == wechatLoginKey.Trim())
            {
                string apiUrl = string.Format(wxLoginApi, ConfigurationManager.AppSettings["wxAppid"], ConfigurationManager.AppSettings["wxAppsercret"], context.UserName);
                JavaScriptSerializer js = new JavaScriptSerializer();
                WechatLoginMsg msg = js.Deserialize<WechatLoginMsg>(HttpHelper.HttpGet(apiUrl));

                //msg.Openid = "oqK0I0VG0jE5udoT1jIVBZOkQr3w";
                //msg.Session_key = "87LCUedsESieDCbaABh/4g==";

                if (!string.IsNullOrWhiteSpace(msg.Openid) && !string.IsNullOrWhiteSpace(msg.Session_key))
                {
                    using (var userAppService = IocManager.Instance.ResolveAsDisposable<IUserAppService>())
                    {
                        LoginOutput output = userAppService.Object.WechatLogin(new WechatLoginInput { Openid = msg.Openid, Session_key = msg.Session_key });
                        identity.AddClaim(new Claim("UserId", output.UserId.ToString()));
                        identity.AddClaim(new Claim("IsNewUser", output.IsNewUser.ToString()));
                        if (!string.IsNullOrEmpty(output.NickName))
                            identity.AddClaim(new Claim("nickname", output.NickName));
                        if (!string.IsNullOrEmpty(output.UserName))
                            identity.AddClaim(new Claim("username", output.UserName));
                        if (!string.IsNullOrEmpty(output.UserType))
                            identity.AddClaim(new Claim("usertype", output.UserType));
                    }
                }
                else
                {
                    context.SetError(msg.Errcode, msg.Errmsg);
                    return Task.FromResult<object>(null);
                }
            }
            else
            {
                using (var userAppService = IocManager.Instance.ResolveAsDisposable<IUserAppService>())
                {
                    LoginOutput output = userAppService.Object.ManageLogin(new ManageLoginInput { PassWord = context.Password, UserName = context.UserName });

                    if (!output.UserId.HasValue)
                    {
                        context.SetError("invalid_grant", "The user name or password is incorrect");
                        return Task.FromResult<object>(null);
                    }

                    identity.AddClaim(new Claim("UserId", output.UserId.ToString()));
                    identity.AddClaim(new Claim("IsNewUser", output.IsNewUser.ToString()));
                    if (!string.IsNullOrEmpty(output.NickName))
                        identity.AddClaim(new Claim("nickname", output.NickName));
                    if (!string.IsNullOrEmpty(output.UserName))
                        identity.AddClaim(new Claim("username", output.UserName));
                    if (!string.IsNullOrEmpty(output.UserType))
                        identity.AddClaim(new Claim("usertype", output.UserType));
                }
            }

            var props = new AuthenticationProperties(new Dictionary<string, string>
                {
                    {
                         "audience", context.ClientId ??string.Empty                }
                });

            var ticket = new AuthenticationTicket(identity, props);
            context.Validated(ticket);
            return Task.FromResult<object>(null);
        }

    }
}