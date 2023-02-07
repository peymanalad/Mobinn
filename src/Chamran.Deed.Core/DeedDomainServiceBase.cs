using Abp.Domain.Services;

namespace Chamran.Deed
{
    public abstract class DeedDomainServiceBase : DomainService
    {
        /* Add your common members for all your domain services. */

        protected DeedDomainServiceBase()
        {
            LocalizationSourceName = DeedConsts.LocalizationSourceName;
        }
    }
}
