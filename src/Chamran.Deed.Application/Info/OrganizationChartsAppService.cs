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
    [AbpAuthorize(AppPermissions.Pages_OrganizationCharts)]
    public class OrganizationChartsAppService : DeedAppServiceBase, IOrganizationChartsAppService
    {
        private readonly IRepository<OrganizationChart> _organizationChartRepository;
        private readonly IRepository<OrganizationChart, int> _lookup_organizationChartRepository;

        public OrganizationChartsAppService(IRepository<OrganizationChart> organizationChartRepository, IRepository<OrganizationChart, int> lookup_organizationChartRepository)
        {
            _organizationChartRepository = organizationChartRepository;
            _lookup_organizationChartRepository = lookup_organizationChartRepository;

        }

        public virtual async Task<PagedResultDto<GetOrganizationChartForViewDto>> GetAll(GetAllOrganizationChartsInput input)
        {

            var filteredOrganizationCharts = _organizationChartRepository.GetAll()
                        .Include(e => e.ParentFk)
                        .WhereIf(!string.IsNullOrWhiteSpace(input.Filter), e => false || e.Caption.Contains(input.Filter) || e.LeafPath.Contains(input.Filter))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.CaptionFilter), e => e.Caption.Contains(input.CaptionFilter))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.LeafPathFilter), e => e.LeafPath.Contains(input.LeafPathFilter))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.OrganizationChartCaptionFilter), e => e.ParentFk != null && e.ParentFk.Caption == input.OrganizationChartCaptionFilter);

            var pagedAndFilteredOrganizationCharts = filteredOrganizationCharts
                .OrderBy(input.Sorting ?? "id asc")
                .PageBy(input);

            var organizationCharts = from o in pagedAndFilteredOrganizationCharts
                                     join o1 in _lookup_organizationChartRepository.GetAll() on o.ParentId equals o1.Id into j1
                                     from s1 in j1.DefaultIfEmpty()

                                     select new
                                     {

                                         o.Caption,
                                         o.LeafPath,
                                         Id = o.Id,
                                         OrganizationChartCaption = s1 == null || s1.Caption == null ? "" : s1.Caption.ToString()
                                     };

            var totalCount = await filteredOrganizationCharts.CountAsync();

            var dbList = await organizationCharts.ToListAsync();
            var results = new List<GetOrganizationChartForViewDto>();

            foreach (var o in dbList)
            {
                var res = new GetOrganizationChartForViewDto()
                {
                    OrganizationChart = new OrganizationChartDto
                    {

                        Caption = o.Caption,
                        LeafPath = o.LeafPath,
                        Id = o.Id,
                    },
                    OrganizationChartCaption = o.OrganizationChartCaption
                };

                results.Add(res);
            }

            return new PagedResultDto<GetOrganizationChartForViewDto>(
                totalCount,
                results
            );

        }

        public virtual async Task<GetOrganizationChartForViewDto> GetOrganizationChartForView(int id)
        {
            var organizationChart = await _organizationChartRepository.GetAsync(id);

            var output = new GetOrganizationChartForViewDto { OrganizationChart = ObjectMapper.Map<OrganizationChartDto>(organizationChart) };

            if (output.OrganizationChart.ParentId != null)
            {
                var _lookupOrganizationChart = await _lookup_organizationChartRepository.FirstOrDefaultAsync((int)output.OrganizationChart.ParentId);
                output.OrganizationChartCaption = _lookupOrganizationChart?.Caption?.ToString();
            }

            return output;
        }

        [AbpAuthorize(AppPermissions.Pages_OrganizationCharts_Edit)]
        public virtual async Task<GetOrganizationChartForEditOutput> GetOrganizationChartForEdit(EntityDto input)
        {
            var organizationChart = await _organizationChartRepository.FirstOrDefaultAsync(input.Id);

            var output = new GetOrganizationChartForEditOutput { OrganizationChart = ObjectMapper.Map<CreateOrEditOrganizationChartDto>(organizationChart) };

            if (output.OrganizationChart.ParentId != null)
            {
                var _lookupOrganizationChart = await _lookup_organizationChartRepository.FirstOrDefaultAsync((int)output.OrganizationChart.ParentId);
                output.OrganizationChartCaption = _lookupOrganizationChart?.Caption?.ToString();
            }

            return output;
        }

        public virtual async Task CreateOrEdit(CreateOrEditOrganizationChartDto input)
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

        [AbpAuthorize(AppPermissions.Pages_OrganizationCharts_Create)]
        protected virtual async Task Create(CreateOrEditOrganizationChartDto input)
        {
            var organizationChart = ObjectMapper.Map<OrganizationChart>(input);

            await _organizationChartRepository.InsertAsync(organizationChart);

        }

        [AbpAuthorize(AppPermissions.Pages_OrganizationCharts_Edit)]
        protected virtual async Task Update(CreateOrEditOrganizationChartDto input)
        {
            var organizationChart = await _organizationChartRepository.FirstOrDefaultAsync((int)input.Id);
            ObjectMapper.Map(input, organizationChart);

        }

        [AbpAuthorize(AppPermissions.Pages_OrganizationCharts_Delete)]
        public virtual async Task Delete(EntityDto input)
        {
            await _organizationChartRepository.DeleteAsync(input.Id);
        }

        [AbpAuthorize(AppPermissions.Pages_OrganizationCharts)]
        public async Task<PagedResultDto<OrganizationChartOrganizationChartLookupTableDto>> GetAllOrganizationChartForLookupTable(GetAllForLookupTableInput input)
        {
            var query = _lookup_organizationChartRepository.GetAll().WhereIf(
                   !string.IsNullOrWhiteSpace(input.Filter),
                  e => e.Caption != null && e.Caption.Contains(input.Filter)
               );

            var totalCount = await query.CountAsync();

            var organizationChartList = await query
                .PageBy(input)
                .ToListAsync();

            var lookupTableDtoList = new List<OrganizationChartOrganizationChartLookupTableDto>();
            foreach (var organizationChart in organizationChartList)
            {
                lookupTableDtoList.Add(new OrganizationChartOrganizationChartLookupTableDto
                {
                    Id = organizationChart.Id,
                    DisplayName = organizationChart.Caption?.ToString()
                });
            }

            return new PagedResultDto<OrganizationChartOrganizationChartLookupTableDto>(
                totalCount,
                lookupTableDtoList
            );
        }

    }
}