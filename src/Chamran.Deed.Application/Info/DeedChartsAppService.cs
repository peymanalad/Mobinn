using Chamran.Deed.People;
using Chamran.Deed.Info;

using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using Abp.Linq.Extensions;
using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using Chamran.Deed.Info.Dtos;
using Chamran.Deed.Dto;
using Abp.Application.Services.Dto;
using Chamran.Deed.Authorization;
using Abp.Extensions;
using Abp.Authorization;
using Microsoft.EntityFrameworkCore;
using Abp.UI;
using Chamran.Deed.Storage;

namespace Chamran.Deed.Info
{
    [AbpAuthorize(AppPermissions.Pages_DeedCharts)]
    public class DeedChartsAppService : DeedAppServiceBase, IDeedChartsAppService
    {
        private readonly IRepository<DeedChart> _deedChartRepository;
        private readonly IRepository<Organization, int> _lookup_organizationRepository;
        private readonly IRepository<DeedChart, int> _lookup_deedChartRepository;

        public DeedChartsAppService(IRepository<DeedChart> deedChartRepository, IRepository<Organization, int> lookup_organizationRepository, IRepository<DeedChart, int> lookup_deedChartRepository)
        {
            _deedChartRepository = deedChartRepository;
            _lookup_organizationRepository = lookup_organizationRepository;
            _lookup_deedChartRepository = lookup_deedChartRepository;

        }

        public virtual async Task<PagedResultDto<GetDeedChartForViewDto>> GetAll(GetAllDeedChartsInput input)
        {

            var filteredDeedCharts = _deedChartRepository.GetAll()
                        .Include(e => e.OrganizationFk)
                        .Include(e => e.ParentFk)
                        .WhereIf(!string.IsNullOrWhiteSpace(input.Filter), e => false || e.Caption.Contains(input.Filter) || e.LeafPath.Contains(input.Filter))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.CaptionFilter), e => e.Caption.Contains(input.CaptionFilter))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.LeafPathFilter), e => e.LeafPath.Contains(input.LeafPathFilter))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.OrganizationOrganizationNameFilter), e => e.OrganizationFk != null && e.OrganizationFk.OrganizationName == input.OrganizationOrganizationNameFilter)
                        .WhereIf(!string.IsNullOrWhiteSpace(input.DeedChartCaptionFilter), e => e.ParentFk != null && e.ParentFk.Caption == input.DeedChartCaptionFilter);

            var pagedAndFilteredDeedCharts = filteredDeedCharts
                .OrderBy(input.Sorting ?? "id asc")
                .PageBy(input);

            var deedCharts = from o in pagedAndFilteredDeedCharts
                             join o1 in _lookup_organizationRepository.GetAll() on o.OrganizationId equals o1.Id into j1
                             from s1 in j1.DefaultIfEmpty()

                             join o2 in _lookup_deedChartRepository.GetAll() on o.ParentId equals o2.Id into j2
                             from s2 in j2.DefaultIfEmpty()

                             select new
                             {

                                 o.Caption,
                                 o.LeafPath,
                                 Id = o.Id,
                                 OrganizationOrganizationName = s1 == null || s1.OrganizationName == null ? "" : s1.OrganizationName.ToString(),
                                 DeedChartCaption = s2 == null || s2.Caption == null ? "" : s2.Caption.ToString()
                             };

            var totalCount = await filteredDeedCharts.CountAsync();

            var dbList = await deedCharts.ToListAsync();
            var results = new List<GetDeedChartForViewDto>();

            foreach (var o in dbList)
            {
                var res = new GetDeedChartForViewDto()
                {
                    DeedChart = new DeedChartDto
                    {

                        Caption = o.Caption,
                        LeafPath = o.LeafPath,
                        Id = o.Id,
                    },
                    OrganizationOrganizationName = o.OrganizationOrganizationName,
                    DeedChartCaption = o.DeedChartCaption
                };

                results.Add(res);
            }

            return new PagedResultDto<GetDeedChartForViewDto>(
                totalCount,
                results
            );

        }

        public virtual async Task<GetDeedChartForViewDto> GetDeedChartForView(int id)
        {
            var deedChart = await _deedChartRepository.GetAsync(id);

            var output = new GetDeedChartForViewDto { DeedChart = ObjectMapper.Map<DeedChartDto>(deedChart) };

            if (output.DeedChart.OrganizationId != null)
            {
                var _lookupOrganization = await _lookup_organizationRepository.FirstOrDefaultAsync((int)output.DeedChart.OrganizationId);
                output.OrganizationOrganizationName = _lookupOrganization?.OrganizationName?.ToString();
            }

            if (output.DeedChart.ParentId != null)
            {
                var _lookupDeedChart = await _lookup_deedChartRepository.FirstOrDefaultAsync((int)output.DeedChart.ParentId);
                output.DeedChartCaption = _lookupDeedChart?.Caption?.ToString();
            }

            return output;
        }

        [AbpAuthorize(AppPermissions.Pages_DeedCharts_Edit)]
        public virtual async Task<GetDeedChartForEditOutput> GetDeedChartForEdit(EntityDto input)
        {
            var deedChart = await _deedChartRepository.FirstOrDefaultAsync(input.Id);

            var output = new GetDeedChartForEditOutput { DeedChart = ObjectMapper.Map<CreateOrEditDeedChartDto>(deedChart) };

            if (output.DeedChart.OrganizationId != null)
            {
                var _lookupOrganization = await _lookup_organizationRepository.FirstOrDefaultAsync((int)output.DeedChart.OrganizationId);
                output.OrganizationOrganizationName = _lookupOrganization?.OrganizationName?.ToString();
            }

            if (output.DeedChart.ParentId != null)
            {
                var _lookupDeedChart = await _lookup_deedChartRepository.FirstOrDefaultAsync((int)output.DeedChart.ParentId);
                output.DeedChartCaption = _lookupDeedChart?.Caption?.ToString();
            }

            return output;
        }

        public virtual async Task CreateOrEdit(CreateOrEditDeedChartDto input)
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

        [AbpAuthorize(AppPermissions.Pages_DeedCharts_Create)]
        protected virtual async Task Create(CreateOrEditDeedChartDto input)
        {
            var deedChart = ObjectMapper.Map<DeedChart>(input);

            await _deedChartRepository.InsertAsync(deedChart);

        }

        [AbpAuthorize(AppPermissions.Pages_DeedCharts_Edit)]
        protected virtual async Task Update(CreateOrEditDeedChartDto input)
        {
            var deedChart = await _deedChartRepository.FirstOrDefaultAsync((int)input.Id);
            ObjectMapper.Map(input, deedChart);

        }

        [AbpAuthorize(AppPermissions.Pages_DeedCharts_Delete)]
        public virtual async Task Delete(EntityDto input)
        {
            await _deedChartRepository.DeleteAsync(input.Id);
        }

        [AbpAuthorize(AppPermissions.Pages_DeedCharts)]
        public async Task<PagedResultDto<DeedChartOrganizationLookupTableDto>> GetAllOrganizationForLookupTable(GetAllForLookupTableInput input)
        {
            var query = _lookup_organizationRepository.GetAll().WhereIf(
                   !string.IsNullOrWhiteSpace(input.Filter),
                  e => e.OrganizationName != null && e.OrganizationName.Contains(input.Filter)
               );

            var totalCount = await query.CountAsync();

            var organizationList = await query
                .PageBy(input)
                .ToListAsync();

            var lookupTableDtoList = new List<DeedChartOrganizationLookupTableDto>();
            foreach (var organization in organizationList)
            {
                lookupTableDtoList.Add(new DeedChartOrganizationLookupTableDto
                {
                    Id = organization.Id,
                    DisplayName = organization.OrganizationName?.ToString()
                });
            }

            return new PagedResultDto<DeedChartOrganizationLookupTableDto>(
                totalCount,
                lookupTableDtoList
            );
        }

        [AbpAuthorize(AppPermissions.Pages_DeedCharts)]
        public async Task<PagedResultDto<DeedChartDeedChartLookupTableDto>> GetAllDeedChartForLookupTable(GetAllForLookupTableInput input)
        {
            var query = _lookup_deedChartRepository.GetAll().WhereIf(
                   !string.IsNullOrWhiteSpace(input.Filter),
                  e => e.Caption != null && e.Caption.Contains(input.Filter)
               );

            var totalCount = await query.CountAsync();

            var deedChartList = await query
                .PageBy(input)
                .ToListAsync();

            var lookupTableDtoList = new List<DeedChartDeedChartLookupTableDto>();
            foreach (var deedChart in deedChartList)
            {
                lookupTableDtoList.Add(new DeedChartDeedChartLookupTableDto
                {
                    Id = deedChart.Id,
                    DisplayName = deedChart.Caption?.ToString()
                });
            }

            return new PagedResultDto<DeedChartDeedChartLookupTableDto>(
                totalCount,
                lookupTableDtoList
            );
        }

    }
}