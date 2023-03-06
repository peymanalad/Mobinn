using System;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Chamran.Deed.Info.Dtos;
using Chamran.Deed.Dto;

namespace Chamran.Deed.Info
{
    public interface IHashtagsAppService : IApplicationService
    {
        Task<PagedResultDto<GetHashtagForViewDto>> GetAll(GetAllHashtagsInput input);

        Task<GetHashtagForViewDto> GetHashtagForView(int id);

        Task<GetHashtagForEditOutput> GetHashtagForEdit(EntityDto input);

        Task CreateOrEdit(CreateOrEditHashtagDto input);

        Task Delete(EntityDto input);

        Task<FileDto> GetHashtagsToExcel(GetAllHashtagsForExcelInput input);

        Task<PagedResultDto<HashtagPostLookupTableDto>> GetAllPostForLookupTable(GetAllForLookupTableInput input);

    }
}