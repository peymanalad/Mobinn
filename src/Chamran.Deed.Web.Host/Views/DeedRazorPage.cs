using Abp.AspNetCore.Mvc.Views;

namespace Chamran.Deed.Web.Views
{
    public abstract class DeedRazorPage<TModel> : AbpRazorPage<TModel>
    {
        protected DeedRazorPage()
        {
            LocalizationSourceName = DeedConsts.LocalizationSourceName;
        }
    }
}
