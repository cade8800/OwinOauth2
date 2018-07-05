using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;

namespace OwnerSayCar.Web.ApiControllers
{
    [Authorize]
    public class ProtectedController : ApiController
    {
        public IEnumerable<object> Get()
        {
            var identity = User.Identity as ClaimsIdentity;
            var a = identity.Claims.FirstOrDefault();
            return identity.Claims.Select(c => new
            {
                Type = c.Type,
                Value = c.Value
            });
        }
    }
}
