using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OwnerSayCar.Web.Models
{
    public class WechatLoginMsg
    {
        public string Errcode { get; set; }
        public string Errmsg { get; set; }
        public string Session_key { get; set; }
        public string Expires_ing { get; set; }
        public string Openid { get; set; }
        public string Unionid { get; set; }
    }
}