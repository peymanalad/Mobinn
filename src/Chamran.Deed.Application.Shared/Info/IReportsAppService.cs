using System;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Chamran.Deed.Info.Dtos;
using Chamran.Deed.Dto;

namespace Chamran.Deed.Info
{
    public interface IReportsAppService : IApplicationService
    {
        Task<PagedResultDto<GetReportForViewDto>> GetAll(GetAllReportsInput input);

        Task<GetReportForViewDto> GetReportForView(int id);

        Task<GetReportForEditOutput> GetReportForEdit(EntityDto input);

        Task CreateOrEdit(CreateOrEditReportDto input);

        Task Delete(EntityDto input);

        Task<FileDto> GetReportsToExcel(GetAllReportsForExcelInput input);

        Task<PagedResultDto<ReportOrganizationLookupTableDto>> GetAllOrganizationForLookupTable(GetAllForLookupTableInput input);

    }
}