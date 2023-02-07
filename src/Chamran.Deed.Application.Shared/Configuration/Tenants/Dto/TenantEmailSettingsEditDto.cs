using Abp.Auditing;
using Chamran.Deed.Configuration.Dto;

namespace Chamran.Deed.Configuration.Tenants.Dto
{
    public class TenantEmailSettingsEditDto : EmailSettingsEditDto
    {
        public bool UseHostDefaultEmailSettings { get; set; }
    }
}