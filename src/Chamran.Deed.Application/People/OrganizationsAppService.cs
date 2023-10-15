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
using Chamran.Deed.Common;
using Chamran.Deed.Storage;

namespace Chamran.Deed.People
{
    [AbpAuthorize(AppPermissions.Pages_Organizations)]
    public class OrganizationsAppService : DeedAppServiceBase, IOrganizationsAppService
    {
        private readonly IRepository<Organization> _organizationRepository;
        private readonly IOrganizationsExcelExporter _organizationsExcelExporter;

        private readonly ITempFileCacheManager _tempFileCacheManager;
        private readonly IBinaryObjectManager _binaryObjectManager;

        public OrganizationsAppService(IRepository<Organization> organizationRepository, IOrganizationsExcelExporter organizationsExcelExporter, ITempFileCacheManager tempFileCacheManager, IBinaryObjectManager binaryObjectManager)
        {
            _organizationRepository = organizationRepository;
            _organizationsExcelExporter = organizationsExcelExporter;

            _tempFileCacheManager = tempFileCacheManager;
            _binaryObjectManager = binaryObjectManager;

        }

        public virtual async Task<PagedResultDto<GetOrganizationForViewDto>> GetAll(GetAllOrganizationsInput input)
        {

            var filteredOrganizations = _organizationRepository.GetAll()
                        .WhereIf(!string.IsNullOrWhiteSpace(input.Filter), e => false || e.OrganizationName.Contains(input.Filter) || e.NationalId.Contains(input.Filter) || e.OrganizationLocation.Contains(input.Filter) || e.OrganizationPhone.Contains(input.Filter) || e.OrganizationContactPerson.Contains(input.Filter) || e.Comment.Contains(input.Filter))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.OrganizationNameFilter), e => e.OrganizationName.Contains(input.OrganizationNameFilter))
                        .WhereIf(input.IsGovernmentalFilter.HasValue && input.IsGovernmentalFilter > -1, e => (input.IsGovernmentalFilter == 1 && e.IsGovernmental) || (input.IsGovernmentalFilter == 0 && !e.IsGovernmental))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.NationalIdFilter), e => e.NationalId.Contains(input.NationalIdFilter))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.OrganizationLocationFilter), e => e.OrganizationLocation.Contains(input.OrganizationLocationFilter))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.OrganizationPhoneFilter), e => e.OrganizationPhone.Contains(input.OrganizationPhoneFilter))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.OrganizationContactPersonFilter), e => e.OrganizationContactPerson.Contains(input.OrganizationContactPersonFilter))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.CommentFilter), e => e.Comment.Contains(input.CommentFilter));

            var pagedAndFilteredOrganizations = filteredOrganizations
                .OrderBy(input.Sorting ?? "id asc")
                .PageBy(input);

            var organizations = from o in pagedAndFilteredOrganizations
                                select new
                                {

                                    o.OrganizationName,
                                    o.IsGovernmental,
                                    o.NationalId,
                                    o.OrganizationLocation,
                                    o.OrganizationPhone,
                                    o.OrganizationContactPerson,
                                    o.Comment,
                                    o.OrganizationLogo,
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
                        IsGovernmental = o.IsGovernmental,
                        NationalId = o.NationalId,
                        OrganizationLocation = o.OrganizationLocation,
                        OrganizationPhone = o.OrganizationPhone,
                        OrganizationContactPerson = o.OrganizationContactPerson,
                        Comment = o.Comment,
                        OrganizationLogo = o.OrganizationLogo,
                        Id = o.Id,
                    }
                };
                res.Organization.OrganizationLogoFileName = await GetBinaryFileName(o.OrganizationLogo);

                results.Add(res);
            }

            return new PagedResultDto<GetOrganizationForViewDto>(
                totalCount,
                results
            );

        }

        public virtual async Task<GetOrganizationForViewDto> GetOrganizationForView(int id)
        {
            var organization = await _organizationRepository.GetAsync(id);

            var output = new GetOrganizationForViewDto { Organization = ObjectMapper.Map<OrganizationDto>(organization) };

            output.Organization.OrganizationLogoFileName = await GetBinaryFileName(organization.OrganizationLogo);

            return output;
        }

        [AbpAuthorize(AppPermissions.Pages_Organizations_Edit)]
        public virtual async Task<GetOrganizationForEditOutput> GetOrganizationForEdit(EntityDto input)
        {
            var organization = await _organizationRepository.FirstOrDefaultAsync(input.Id);

            var output = new GetOrganizationForEditOutput { Organization = ObjectMapper.Map<CreateOrEditOrganizationDto>(organization) };

            output.OrganizationLogoFileName = await GetBinaryFileName(organization.OrganizationLogo);

            return output;
        }

        public virtual async Task CreateOrEdit(CreateOrEditOrganizationDto input)
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
            organization.OrganizationLogo = await GetBinaryObjectFromCache(input.OrganizationLogoToken);

        }

        [AbpAuthorize(AppPermissions.Pages_Organizations_Edit)]
        protected virtual async Task Update(CreateOrEditOrganizationDto input)
        {
            var organization = await _organizationRepository.FirstOrDefaultAsync((int)input.Id);
            ObjectMapper.Map(input, organization);
            organization.OrganizationLogo = await GetBinaryObjectFromCache(input.OrganizationLogoToken);

        }

        [AbpAuthorize(AppPermissions.Pages_Organizations_Delete)]
        public virtual async Task Delete(EntityDto input)
        {
            await _organizationRepository.DeleteAsync(input.Id);
        }

        public virtual async Task<FileDto> GetOrganizationsToExcel(GetAllOrganizationsForExcelInput input)
        {

            var filteredOrganizations = _organizationRepository.GetAll()
                        .WhereIf(!string.IsNullOrWhiteSpace(input.Filter), e => false || e.OrganizationName.Contains(input.Filter) || e.NationalId.Contains(input.Filter) || e.OrganizationLocation.Contains(input.Filter) || e.OrganizationPhone.Contains(input.Filter) || e.OrganizationContactPerson.Contains(input.Filter) || e.Comment.Contains(input.Filter))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.OrganizationNameFilter), e => e.OrganizationName.Contains(input.OrganizationNameFilter))
                        .WhereIf(input.IsGovernmentalFilter.HasValue && input.IsGovernmentalFilter > -1, e => (input.IsGovernmentalFilter == 1 && e.IsGovernmental) || (input.IsGovernmentalFilter == 0 && !e.IsGovernmental))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.NationalIdFilter), e => e.NationalId.Contains(input.NationalIdFilter))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.OrganizationLocationFilter), e => e.OrganizationLocation.Contains(input.OrganizationLocationFilter))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.OrganizationPhoneFilter), e => e.OrganizationPhone.Contains(input.OrganizationPhoneFilter))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.OrganizationContactPersonFilter), e => e.OrganizationContactPerson.Contains(input.OrganizationContactPersonFilter))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.CommentFilter), e => e.Comment.Contains(input.CommentFilter));

            var query = (from o in filteredOrganizations
                         select new GetOrganizationForViewDto()
                         {
                             Organization = new OrganizationDto
                             {
                                 OrganizationName = o.OrganizationName,
                                 IsGovernmental = o.IsGovernmental,
                                 NationalId = o.NationalId,
                                 OrganizationLocation = o.OrganizationLocation,
                                 OrganizationPhone = o.OrganizationPhone,
                                 OrganizationContactPerson = o.OrganizationContactPerson,
                                 Comment = o.Comment,
                                 OrganizationLogo = o.OrganizationLogo,
                                 Id = o.Id
                             }
                         });

            var organizationListDtos = await query.ToListAsync();

            return _organizationsExcelExporter.ExportToFile(organizationListDtos);
        }

        protected virtual async Task<Guid?> GetBinaryObjectFromCache(string fileToken)
        {
            if (fileToken.IsNullOrWhiteSpace())
            {
                return null;
            }

            var fileCache = _tempFileCacheManager.GetFileInfo(fileToken);

            if (fileCache == null)
            {
                throw new UserFriendlyException("There is no such file with the token: " + fileToken);
            }

            var storedFile = new BinaryObject(AbpSession.TenantId, fileCache.File, BinarySourceType.OrganizationLogo, fileCache.FileName);
            await _binaryObjectManager.SaveAsync(storedFile);

            return storedFile.Id;
        }

        protected virtual async Task<string> GetBinaryFileName(Guid? fileId)
        {
            if (!fileId.HasValue)
            {
                return null;
            }

            var file = await _binaryObjectManager.GetOrNullAsync(fileId.Value);
            return file?.Description;
        }

        [AbpAuthorize(AppPermissions.Pages_Organizations_Edit)]
        public virtual async Task RemoveOrganizationLogoFile(EntityDto input)
        {
            var organization = await _organizationRepository.FirstOrDefaultAsync(input.Id);
            if (organization == null)
            {
                throw new UserFriendlyException(L("EntityNotFound"));
            }

            if (!organization.OrganizationLogo.HasValue)
            {
                throw new UserFriendlyException(L("FileNotFound"));
            }

            await _binaryObjectManager.DeleteAsync(organization.OrganizationLogo.Value);
            organization.OrganizationLogo = null;
        }

    }
}