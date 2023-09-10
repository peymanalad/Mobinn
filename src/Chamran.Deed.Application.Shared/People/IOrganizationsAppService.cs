using System;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Chamran.Deed.People.Dtos;
using Chamran.Deed.Dto;

namespace Chamran.Deed.People
{
    public interface IOrganizationsAppService : IApplicationService
    {
        Task<PagedResultDto<GetOrganizationForViewDto>> GetAll(GetAllOrganizationsInput input);

        Task<GetOrganizationForViewDto> GetOrganizationForView(int id);

        Task<GetOrganizationForEditOutput> GetOrganizationForEdit(EntityDto input);

        Task CreateOrEdit(CreateOrEditOrganizationDto input);

        Task Delete(EntityDto input);

        Task<FileDto> GetOrganizationsToExcel(GetAllOrganizationsForExcelInput input);

        Task RemoveOrganizationLogoFile(EntityDto input);

    }
}