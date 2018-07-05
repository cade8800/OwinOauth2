using Abp.Web.Mvc.Views;

namespace OwnerSayCar.Web.Views
{
    public abstract class OwnerSayCarWebViewPageBase : OwnerSayCarWebViewPageBase<dynamic>
    {

    }

    public abstract class OwnerSayCarWebViewPageBase<TModel> : AbpWebViewPage<TModel>
    {
        protected OwnerSayCarWebViewPageBase()
        {
            LocalizationSourceName = OwnerSayCarConsts.LocalizationSourceName;
        }
    }
}