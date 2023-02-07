using System.Threading.Tasks;
using Abp.Application.Services;
using Chamran.Deed.Editions.Dto;
using Chamran.Deed.MultiTenancy.Dto;

namespace Chamran.Deed.MultiTenancy
{
    public interface ITenantRegistrationAppService: IApplicationService
    {
        Task<RegisterTenantOutput> RegisterTenant(RegisterTenantInput input);

        Task<EditionsSelectOutput> GetEditionsForSelect();

        Task<EditionSelectDto> GetEdition(int editionId);
    }
}