using System;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Chamran.Deed.Info.Dtos;
using Chamran.Deed.Dto;

namespace Chamran.Deed.Info
{
    public interface IPostGroupsAppService : IApplicationService
    {
        Task<PagedResultDto<GetPostGroupForViewDto>> GetAll(GetAllPostGroupsInput input);

        Task<GetPostGroupForViewDto> GetPostGroupForView(int id);

        Task<GetPostGroupForEditOutput> GetPostGroupForEdit(EntityDto input);

        Task CreateOrEdit(CreateOrEditPostGroupDto input);

        Task Delete(EntityDto input);

        Task<FileDto> GetPostGroupsToExcel(GetAllPostGroupsForExcelInput input);

        Task<PagedResultDto<PostGroupOrganizationGroupLookupTableDto>> GetAllOrganizationGroupForLookupTable(GetAllForLookupTableInput input);

    }
}