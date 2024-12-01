using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Chamran.Deed.Info.Dtos;
using Chamran.Deed.Dto;

namespace Chamran.Deed.Info
{
    public interface IPostSubGroupsAppService : IApplicationService
    {
        Task<PagedResultDto<GetPostSubGroupForViewDto>> GetAll(GetAllPostSubGroupsInput input);

        Task<GetPostSubGroupForViewDto> GetPostSubGroupForView(int id);

        Task<GetPostSubGroupForEditOutput> GetPostSubGroupForEdit(EntityDto input);

        Task CreateOrEdit(CreateOrEditPostSubGroupDto input);

        Task Delete(EntityDto input);

        Task<FileDto> GetPostSubGroupsToExcel(GetAllPostSubGroupsForExcelInput input);

        Task RemoveSubGroupFileFile(EntityDto input);

    }
}