using Chamran.Deed.People;
using System.Linq;
using System.Linq.Dynamic.Core;
using Abp.Linq.Extensions;
using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using Chamran.Deed.Info.Exporting;
using Chamran.Deed.Info.Dtos;
using Chamran.Deed.Dto;
using Abp.Application.Services.Dto;
using Chamran.Deed.Authorization;
using Abp.Authorization;
using Microsoft.EntityFrameworkCore;

namespace Chamran.Deed.Info
{
    [AbpAuthorize(AppPermissions.Pages_Reports)]
    public class ReportsAppService : DeedAppServiceBase, IReportsAppService
    {
        private readonly IRepository<Report> _reportRepository;
        private readonly IReportsExcelExporter _reportsExcelExporter;
        private readonly IRepository<Organization, int> _lookup_organizationRepository;

        public ReportsAppService(IRepository<Report> reportRepository, IReportsExcelExporter reportsExcelExporter, IRepository<Organization, int> lookup_organizationRepository)
        {
            _reportRepository = reportRepository;
            _reportsExcelExporter = reportsExcelExporter;
            _lookup_organizationRepository = lookup_organizationRepository;

        }

        public async Task<PagedResultDto<GetReportForViewDto>> GetAll(GetAllReportsInput input)
        {

            var filteredReports = _reportRepository.GetAll()
                        .Include(e => e.OrganizationFk)
                        .WhereIf(!string.IsNullOrWhiteSpace(input.Filter), e => false || e.ReportDescription.Contains(input.Filter) || e.ReportContent.Contains(input.Filter))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.ReportDescriptionFilter), e => e.ReportDescription.Contains(input.ReportDescriptionFilter))
                        .WhereIf(input.IsDashboardFilter.HasValue && input.IsDashboardFilter > -1, e => (input.IsDashboardFilter == 1 && e.IsDashboard) || (input.IsDashboardFilter == 0 && !e.IsDashboard))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.OrganizationOrganizationNameFilter), e => e.OrganizationFk != null && e.OrganizationFk.OrganizationName == input.OrganizationOrganizationNameFilter);

            var pagedAndFilteredReports = filteredReports
                .OrderBy(input.Sorting ?? "id asc")
                .PageBy(input);

            var reports = from o in pagedAndFilteredReports
                          join o1 in _lookup_organizationRepository.GetAll() on o.OrganizationId equals o1.Id into j1
                          from s1 in j1.DefaultIfEmpty()

                          select new
                          {

                              o.ReportDescription,
                              o.IsDashboard,
                              Id = o.Id,
                              OrganizationOrganizationName = s1 == null || s1.OrganizationName == null ? "" : s1.OrganizationName.ToString()
                          };

            var totalCount = await filteredReports.CountAsync();

            var dbList = await reports.ToListAsync();
            var results = new List<GetReportForViewDto>();

            foreach (var o in dbList)
            {
                var res = new GetReportForViewDto()
                {
                    Report = new ReportDto
                    {

                        ReportDescription = o.ReportDescription,
                        IsDashboard = o.IsDashboard,
                        Id = o.Id,
                    },
                    OrganizationOrganizationName = o.OrganizationOrganizationName
                };

                results.Add(res);
            }

            return new PagedResultDto<GetReportForViewDto>(
                totalCount,
                results
            );

        }

        public async Task<GetReportForViewDto> GetReportForView(int id)
        {
            var report = await _reportRepository.GetAsync(id);

            var output = new GetReportForViewDto { Report = ObjectMapper.Map<ReportDto>(report) };

            if (output.Report.OrganizationId != null)
            {
                var _lookupOrganization = await _lookup_organizationRepository.FirstOrDefaultAsync((int)output.Report.OrganizationId);
                output.OrganizationOrganizationName = _lookupOrganization?.OrganizationName?.ToString();
            }

            return output;
        }

        [AbpAuthorize(AppPermissions.Pages_Reports_Edit)]
        public async Task<GetReportForEditOutput> GetReportForEdit(EntityDto input)
        {
            var report = await _reportRepository.FirstOrDefaultAsync(input.Id);

            var output = new GetReportForEditOutput { Report = ObjectMapper.Map<CreateOrEditReportDto>(report) };

            if (output.Report.OrganizationId != null)
            {
                var _lookupOrganization = await _lookup_organizationRepository.FirstOrDefaultAsync((int)output.Report.OrganizationId);
                output.OrganizationOrganizationName = _lookupOrganization?.OrganizationName?.ToString();
            }

            return output;
        }

        public async Task CreateOrEdit(CreateOrEditReportDto input)
        {
            if (input.Id == null)
            {
                await Create(input);
            }
            else
            {
                await Update(input);
            }
        }

        [AbpAuthorize(AppPermissions.Pages_Reports_Create)]
        protected virtual async Task Create(CreateOrEditReportDto input)
        {
            var report = ObjectMapper.Map<Report>(input);

            await _reportRepository.InsertAsync(report);

        }

        [AbpAuthorize(AppPermissions.Pages_Reports_Edit)]
        protected virtual async Task Update(CreateOrEditReportDto input)
        {
            var report = await _reportRepository.FirstOrDefaultAsync((int)input.Id);
            ObjectMapper.Map(input, report);

        }

        [AbpAuthorize(AppPermissions.Pages_Reports_Delete)]
        public async Task Delete(EntityDto input)
        {
            await _reportRepository.DeleteAsync(input.Id);
        }

        public async Task<FileDto> GetReportsToExcel(GetAllReportsForExcelInput input)
        {

            var filteredReports = _reportRepository.GetAll()
                        .Include(e => e.OrganizationFk)
                        .WhereIf(!string.IsNullOrWhiteSpace(input.Filter), e => false || e.ReportDescription.Contains(input.Filter) || e.ReportContent.Contains(input.Filter))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.ReportDescriptionFilter), e => e.ReportDescription.Contains(input.ReportDescriptionFilter))
                        .WhereIf(input.IsDashboardFilter.HasValue && input.IsDashboardFilter > -1, e => (input.IsDashboardFilter == 1 && e.IsDashboard) || (input.IsDashboardFilter == 0 && !e.IsDashboard))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.OrganizationOrganizationNameFilter), e => e.OrganizationFk != null && e.OrganizationFk.OrganizationName == input.OrganizationOrganizationNameFilter);

            var query = (from o in filteredReports
                         join o1 in _lookup_organizationRepository.GetAll() on o.OrganizationId equals o1.Id into j1
                         from s1 in j1.DefaultIfEmpty()

                         select new GetReportForViewDto()
                         {
                             Report = new ReportDto
                             {
                                 ReportDescription = o.ReportDescription,
                                 IsDashboard = o.IsDashboard,
                                 Id = o.Id
                             },
                             OrganizationOrganizationName = s1 == null || s1.OrganizationName == null ? "" : s1.OrganizationName.ToString()
                         });

            var reportListDtos = await query.ToListAsync();

            return _reportsExcelExporter.ExportToFile(reportListDtos);
        }

        [AbpAuthorize(AppPermissions.Pages_Reports)]
        public async Task<PagedResultDto<ReportOrganizationLookupTableDto>> GetAllOrganizationForLookupTable(GetAllForLookupTableInput input)
        {
            var query = _lookup_organizationRepository.GetAll().WhereIf(
                   !string.IsNullOrWhiteSpace(input.Filter),
                  e => e.OrganizationName != null && e.OrganizationName.Contains(input.Filter)
               );

            var totalCount = await query.CountAsync();

            var organizationList = await query
                .PageBy(input)
                .ToListAsync();

            var lookupTableDtoList = new List<ReportOrganizationLookupTableDto>();
            foreach (var organization in organizationList)
            {
                lookupTableDtoList.Add(new ReportOrganizationLookupTableDto
                {
                    Id = organization.Id,
                    DisplayName = organization.OrganizationName?.ToString()
                });
            }

            return new PagedResultDto<ReportOrganizationLookupTableDto>(
                totalCount,
                lookupTableDtoList
            );
        }

    }
}