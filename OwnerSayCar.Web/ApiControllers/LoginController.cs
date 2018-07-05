using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Configuration;
using Abp.UI;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Text;
using System.Web.Script.Serialization;
using OwnerSayCar.Web.Models;
using OwnerSayCar.Utilities.Http;
using Abp.WebApi.Controllers;
using Abp.Web.Models;
using OwnerSayCar.User;
using OwnerSayCar.User.Dto;
using Abp.WebApi.Authorization;

namespace OwnerSayCar.Web.ApiControllers
{
    public class LoginController : AbpApiController
    {
        private const string wxLoginApi = "https://api.weixin.qq.com/sns/jscode2session?appid={0}&secret={1}&js_code={2}&grant_type=authorization_code";
        private readonly IUserAppService _userAppService;

        public LoginController(IUserAppService userAppService)
        {
            _userAppService = userAppService;
        }

        [WrapResult]
        public LoginOutput Login([Required]string Code)
        {
            string apiUrl = string.Format(wxLoginApi, ConfigurationManager.AppSettings["wxAppid"], ConfigurationManager.AppSettings["wxAppsercret"], Code);
            JavaScriptSerializer js = new JavaScriptSerializer();
            WechatLoginMsg msg = js.Deserialize<WechatLoginMsg>(HttpHelper.HttpGet(apiUrl));
            if (!string.IsNullOrWhiteSpace(msg.Openid) && !string.IsNullOrWhiteSpace(msg.Session_key))
            {
                LoginOutput output = _userAppService.WechatLogin(new WechatLoginInput { Openid = msg.Openid, Session_key = msg.Session_key });
                return output;
            }
            else throw new UserFriendlyException(msg.Errcode, msg.Errmsg);
        }

    }
}
