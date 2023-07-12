using System;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Chamran.Deed.Common.Dtos;
using Chamran.Deed.Dto;

namespace Chamran.Deed.Common
{
    public interface IUserTokensAppService : IApplicationService
    {
        Task<PagedResultDto<GetUserTokenForViewDto>> GetAll(GetAllUserTokensInput input);

        Task<GetUserTokenForViewDto> GetUserTokenForView(int id);

        Task<GetUserTokenForEditOutput> GetUserTokenForEdit(EntityDto input);

        Task CreateOrEdit(CreateOrEditUserTokenDto input);

        Task Delete(EntityDto input);

        Task<FileDto> GetUserTokensToExcel(GetAllUserTokensForExcelInput input);

        Task<PagedResultDto<UserTokenUserLookupTableDto>> GetAllUserForLookupTable(GetAllForLookupTableInput input);

        Task RegisterDevice(string token);

    }
}