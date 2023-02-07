using Abp;

namespace Chamran.Deed
{
    /// <summary>
    /// This class can be used as a base class for services in this application.
    /// It has some useful objects property-injected and has some basic methods most of services may need to.
    /// It's suitable for non domain nor application service classes.
    /// For domain services inherit <see cref="DeedDomainServiceBase"/>.
    /// For application services inherit DeedAppServiceBase.
    /// </summary>
    public abstract class DeedServiceBase : AbpServiceBase
    {
        protected DeedServiceBase()
        {
            LocalizationSourceName = DeedConsts.LocalizationSourceName;
        }
    }
}