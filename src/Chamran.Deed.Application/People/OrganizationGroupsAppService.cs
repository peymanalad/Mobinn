using Chamran.Deed.People;

using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using Abp.Linq.Extensions;
using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using Chamran.Deed.People.Exporting;
using Chamran.Deed.People.Dtos;
using Chamran.Deed.Dto;
using Abp.Application.Services.Dto;
using Chamran.Deed.Authorization;
using Abp.Extensions;
using Abp.Authorization;
using Microsoft.EntityFrameworkCore;
using Abp.UI;
using Chamran.Deed.Storage;

namespace Chamran.Deed.People
{
    [AbpAuthorize(AppPermissions.Pages_OrganizationGroups)]
    public class OrganizationGroupsAppService : DeedAppServiceBase, IOrganizationGroupsAppService
    {
        private readonly IRepository<OrganizationGroup> _organizationGroupRepository;
        private readonly IOrganizationGroupsExcelExporter _organizationGroupsExcelExporter;
        private readonly IRepository<Organization, int> _lookup_organizationRepository;

        public OrganizationGroupsAppService(IRepository<OrganizationGroup> organizationGroupRepository, IOrganizationGroupsExcelExporter organizationGroupsExcelExporter, IRepository<Organization, int> lookup_organizationRepository)
        {
            _organizationGroupRepository = organizationGroupRepository;
            _organizationGroupsExcelExporter = organizationGroupsExcelExporter;
            _lookup_organizationRepository = lookup_organizationRepository;

        }

        public async Task<PagedResultDto<GetOrganizationGroupForViewDto>> GetAll(GetAllOrganizationGroupsInput input)
        {

            var filteredOrganizationGroups = _organizationGroupRepository.GetAll()
                        .Include(e => e.OrganizationFk)
                        .WhereIf(!string.IsNullOrWhiteSpace(input.Filter), e => false || e.GroupName.Contains(input.Filter))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.GroupNameFilter), e => e.GroupName.Contains(input.GroupNameFilter))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.OrganizationOrganizationNameFilter), e => e.OrganizationFk != null && e.OrganizationFk.OrganizationName == input.OrganizationOrganizationNameFilter);

            var pagedAndFilteredOrganizationGroups = filteredOrganizationGroups
                .OrderBy(input.Sorting ?? "id asc")
                .PageBy(input);

            var organizationGroups = from o in pagedAndFilteredOrganizationGroups
                                     join o1 in _lookup_organizationRepository.GetAll() on o.OrganizationId equals o1.Id into j1
                                     from s1 in j1.DefaultIfEmpty()

                                     select new
                                     {

                                         o.GroupName,
                                         Id = o.Id,
                                         OrganizationOrganizationName = s1 == null || s1.OrganizationName == null ? "" : s1.OrganizationName.ToString()
                                     };

            var totalCount = await filteredOrganizationGroups.CountAsync();

            var dbList = await organizationGroups.ToListAsync();
            var results = new List<GetOrganizationGroupForViewDto>();

            foreach (var o in dbList)
            {
                var res = new GetOrganizationGroupForViewDto()
                {
                    OrganizationGroup = new OrganizationGroupDto
                    {

                        GroupName = o.GroupName,
                        Id = o.Id,
                    },
                    OrganizationOrganizationName = o.OrganizationOrganizationName
                };

                results.Add(res);
            }

            return new PagedResultDto<GetOrganizationGroupForViewDto>(
                totalCount,
                results
            );

        }

        public async Task<GetOrganizationGroupForViewDto> GetOrganizationGroupForView(int id)
        {
            var organizationGroup = await _organizationGroupRepository.GetAsync(id);

            var output = new GetOrganizationGroupForViewDto { OrganizationGroup = ObjectMapper.Map<OrganizationGroupDto>(organizationGroup) };

            if (output.OrganizationGroup.OrganizationId != null)
            {
                var _lookupOrganization = await _lookup_organizationRepository.FirstOrDefaultAsync((int)output.OrganizationGroup.OrganizationId);
                output.OrganizationOrganizationName = _lookupOrganization?.OrganizationName?.ToString();
            }

            return output;
        }

        [AbpAuthorize(AppPermissions.Pages_OrganizationGroups_Edit)]
        public async Task<GetOrganizationGroupForEditOutput> GetOrganizationGroupForEdit(EntityDto input)
        {
            var organizationGroup = await _organizationGroupRepository.FirstOrDefaultAsync(input.Id);

            var output = new GetOrganizationGroupForEditOutput { OrganizationGroup = ObjectMapper.Map<CreateOrEditOrganizationGroupDto>(organizationGroup) };

            if (output.OrganizationGroup.OrganizationId != null)
            {
                var _lookupOrganization = await _lookup_organizationRepository.FirstOrDefaultAsync((int)output.OrganizationGroup.OrganizationId);
                output.OrganizationOrganizationName = _lookupOrganization?.OrganizationName?.ToString();
            }

            return output;
        }

        public async Task CreateOrEdit(CreateOrEditOrganizationGroupDto input)
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

        [AbpAuthorize(AppPermissions.Pages_OrganizationGroups_Create)]
        protected virtual async Task Create(CreateOrEditOrganizationGroupDto input)
        {
            var organizationGroup = ObjectMapper.Map<OrganizationGroup>(input);

            await _organizationGroupRepository.InsertAsync(organizationGroup);

        }

        [AbpAuthorize(AppPermissions.Pages_OrganizationGroups_Edit)]
        protected virtual async Task Update(CreateOrEditOrganizationGroupDto input)
        {
            var organizationGroup = await _organizationGroupRepository.FirstOrDefaultAsync((int)input.Id);
            ObjectMapper.Map(input, organizationGroup);

        }

        [AbpAuthorize(AppPermissions.Pages_OrganizationGroups_Delete)]
        public async Task Delete(EntityDto input)
        {
            await _organizationGroupRepository.DeleteAsync(input.Id);
        }

        public async Task<FileDto> GetOrganizationGroupsToExcel(GetAllOrganizationGroupsForExcelInput input)
        {

            var filteredOrganizationGroups = _organizationGroupRepository.GetAll()
                        .Include(e => e.OrganizationFk)
                        .WhereIf(!string.IsNullOrWhiteSpace(input.Filter), e => false || e.GroupName.Contains(input.Filter))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.GroupNameFilter), e => e.GroupName.Contains(input.GroupNameFilter))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.OrganizationOrganizationNameFilter), e => e.OrganizationFk != null && e.OrganizationFk.OrganizationName == input.OrganizationOrganizationNameFilter);

            var query = (from o in filteredOrganizationGroups
                         join o1 in _lookup_organizationRepository.GetAll() on o.OrganizationId equals o1.Id into j1
                         from s1 in j1.DefaultIfEmpty()

                         select new GetOrganizationGroupForViewDto()
                         {
                             OrganizationGroup = new OrganizationGroupDto
                             {
                                 GroupName = o.GroupName,
                                 Id = o.Id
                             },
                             OrganizationOrganizationName = s1 == null || s1.OrganizationName == null ? "" : s1.OrganizationName.ToString()
                         });

            var organizationGroupListDtos = await query.ToListAsync();

            return _organizationGroupsExcelExporter.ExportToFile(organizationGroupListDtos);
        }

        [AbpAuthorize(AppPermissions.Pages_OrganizationGroups)]
        public async Task<PagedResultDto<OrganizationGroupOrganizationLookupTableDto>> GetAllOrganizationForLookupTable(GetAllForLookupTableInput input)
        {
            var query = _lookup_organizationRepository.GetAll().WhereIf(
                   !string.IsNullOrWhiteSpace(input.Filter),
                  e => e.OrganizationName != null && e.OrganizationName.Contains(input.Filter)
               );

            var totalCount = await query.CountAsync();

            var organizationList = await query
                .PageBy(input)
                .ToListAsync();

            var lookupTableDtoList = new List<OrganizationGroupOrganizationLookupTableDto>();
            foreach (var organization in organizationList)
            {
                lookupTableDtoList.Add(new OrganizationGroupOrganizationLookupTableDto
                {
                    Id = organization.Id,
                    DisplayName = organization.OrganizationName?.ToString()
                });
            }

            return new PagedResultDto<OrganizationGroupOrganizationLookupTableDto>(
                totalCount,
                lookupTableDtoList
            );
        }

    }
}