using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Chamran.Deed.Info.Dtos;
using Chamran.Deed.Dto;

namespace Chamran.Deed.Info
{
    public interface IUserLocationsAppService : IApplicationService
    {
        Task<PagedResultDto<GetUserLocationForViewDto>> GetAll(GetAllUserLocationsInput input);

        Task<GetUserLocationForViewDto> GetUserLocationForView(int id);

        Task<GetUserLocationForEditOutput> GetUserLocationForEdit(EntityDto input);

        Task CreateOrEdit(CreateOrEditUserLocationDto input);

        Task Delete(EntityDto input);

        Task<FileDto> GetUserLocationsToExcel(GetAllUserLocationsForExcelInput input);

        Task<PagedResultDto<UserLocationUserLookupTableDto>> GetAllUserForLookupTable(GetAllForLookupTableInput input);

        Task CreateLocationsByDate(List<CreateLocationsDto> input);

    }
}