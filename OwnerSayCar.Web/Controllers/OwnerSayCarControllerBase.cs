using Abp.Web.Mvc.Controllers;

namespace OwnerSayCar.Web.Controllers
{
    /// <summary>
    /// Derive all Controllers from this class.
    /// </summary>
    public abstract class OwnerSayCarControllerBase : AbpController
    {
        protected OwnerSayCarControllerBase()
        {
            LocalizationSourceName = OwnerSayCarConsts.LocalizationSourceName;
        }
    }
}