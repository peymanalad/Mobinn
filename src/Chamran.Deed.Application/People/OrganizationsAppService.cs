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
    [AbpAuthorize(AppPermissions.Pages_Organizations)]
    public class OrganizationsAppService : DeedAppServiceBase, IOrganizationsAppService
    {
        private readonly IRepository<Organization> _organizationRepository;
        private readonly IOrganizationsExcelExporter _organizationsExcelExporter;

        public OrganizationsAppService(IRepository<Organization> organizationRepository, IOrganizationsExcelExporter organizationsExcelExporter)
        {
            _organizationRepository = organizationRepository;
            _organizationsExcelExporter = organizationsExcelExporter;

        }

        public async Task<PagedResultDto<GetOrganizationForViewDto>> GetAll(GetAllOrganizationsInput input)
        {

            var filteredOrganizations = _organizationRepository.GetAll()
                        .WhereIf(!string.IsNullOrWhiteSpace(input.Filter), e => false || e.OrganizationName.Contains(input.Filter))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.OrganizationNameFilter), e => e.OrganizationName.Contains(input.OrganizationNameFilter));

            var pagedAndFilteredOrganizations = filteredOrganizations
                .OrderBy(input.Sorting ?? "id asc")
                .PageBy(input);

            var organizations = from o in pagedAndFilteredOrganizations
                                select new
                                {

                                    o.OrganizationName,
                                    Id = o.Id
                                };

            var totalCount = await filteredOrganizations.CountAsync();

            var dbList = await organizations.ToListAsync();
            var results = new List<GetOrganizationForViewDto>();

            foreach (var o in dbList)
            {
                var res = new GetOrganizationForViewDto()
                {
                    Organization = new OrganizationDto
                    {

                        OrganizationName = o.OrganizationName,
                        Id = o.Id,
                    }
                };

                results.Add(res);
            }

            return new PagedResultDto<GetOrganizationForViewDto>(
                totalCount,
                results
            );

        }

        public async Task<GetOrganizationForViewDto> GetOrganizationForView(int id)
        {
            var organization = await _organizationRepository.GetAsync(id);

            var output = new GetOrganizationForViewDto { Organization = ObjectMapper.Map<OrganizationDto>(organization) };

            return output;
        }

        [AbpAuthorize(AppPermissions.Pages_Organizations_Edit)]
        public async Task<GetOrganizationForEditOutput> GetOrganizationForEdit(EntityDto input)
        {
            var organization = await _organizationRepository.FirstOrDefaultAsync(input.Id);

            var output = new GetOrganizationForEditOutput { Organization = ObjectMapper.Map<CreateOrEditOrganizationDto>(organization) };

            return output;
        }

        public async Task CreateOrEdit(CreateOrEditOrganizationDto input)
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

        [AbpAuthorize(AppPermissions.Pages_Organizations_Create)]
        protected virtual async Task Create(CreateOrEditOrganizationDto input)
        {
            var organization = ObjectMapper.Map<Organization>(input);

            await _organizationRepository.InsertAsync(organization);

        }

        [AbpAuthorize(AppPermissions.Pages_Organizations_Edit)]
        protected virtual async Task Update(CreateOrEditOrganizationDto input)
        {
            var organization = await _organizationRepository.FirstOrDefaultAsync((int)input.Id);
            ObjectMapper.Map(input, organization);

        }

        [AbpAuthorize(AppPermissions.Pages_Organizations_Delete)]
        public async Task Delete(EntityDto input)
        {
            await _organizationRepository.DeleteAsync(input.Id);
        }

        public async Task<FileDto> GetOrganizationsToExcel(GetAllOrganizationsForExcelInput input)
        {

            var filteredOrganizations = _organizationRepository.GetAll()
                        .WhereIf(!string.IsNullOrWhiteSpace(input.Filter), e => false || e.OrganizationName.Contains(input.Filter))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.OrganizationNameFilter), e => e.OrganizationName.Contains(input.OrganizationNameFilter));

            var query = (from o in filteredOrganizations
                         select new GetOrganizationForViewDto()
                         {
                             Organization = new OrganizationDto
                             {
                                 OrganizationName = o.OrganizationName,
                                 Id = o.Id
                             }
                         });

            var organizationListDtos = await query.ToListAsync();

            return _organizationsExcelExporter.ExportToFile(organizationListDtos);
        }

    }
}