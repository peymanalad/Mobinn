using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using Abp.Linq.Extensions;
using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using Chamran.Deed.Common.Dtos;
using Chamran.Deed.Dto;
using Abp.Application.Services.Dto;
using Chamran.Deed.Authorization;
using Abp.Extensions;
using Abp.Authorization;
using Microsoft.EntityFrameworkCore;
using Abp.UI;
using Chamran.Deed.Storage;

namespace Chamran.Deed.Common
{
    [AbpAuthorize(AppPermissions.Pages_SoftwareUpdates)]
    public class SoftwareUpdatesAppService : DeedAppServiceBase, ISoftwareUpdatesAppService
    {
        private readonly IRepository<SoftwareUpdate> _softwareUpdateRepository;

        public SoftwareUpdatesAppService(IRepository<SoftwareUpdate> softwareUpdateRepository)
        {
            _softwareUpdateRepository = softwareUpdateRepository;

        }

        public async Task<PagedResultDto<GetSoftwareUpdateForViewDto>> GetAll(GetAllSoftwareUpdatesInput input)
        {

            var filteredSoftwareUpdates = _softwareUpdateRepository.GetAll()
                        .WhereIf(!string.IsNullOrWhiteSpace(input.Filter), e => false || e.SoftwareVersion.Contains(input.Filter) || e.UpdatePath.Contains(input.Filter) || e.WhatsNew.Contains(input.Filter) || e.Platform.Contains(input.Filter))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.SoftwareVersionFilter), e => e.SoftwareVersion.Contains(input.SoftwareVersionFilter))
                        .WhereIf(input.ForceUpdateFilter.HasValue && input.ForceUpdateFilter > -1, e => (input.ForceUpdateFilter == 1 && e.ForceUpdate) || (input.ForceUpdateFilter == 0 && !e.ForceUpdate))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.UpdatePathFilter), e => e.UpdatePath.Contains(input.UpdatePathFilter))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.WhatsNewFilter), e => e.WhatsNew.Contains(input.WhatsNewFilter))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.PlatformFilter), e => e.Platform.Contains(input.PlatformFilter))
                        .WhereIf(input.MinBuildNoFilter != null, e => e.BuildNo >= input.MinBuildNoFilter)
                        .WhereIf(input.MaxBuildNoFilter != null, e => e.BuildNo <= input.MaxBuildNoFilter);

            var pagedAndFilteredSoftwareUpdates = filteredSoftwareUpdates
                .OrderBy(input.Sorting ?? "id asc")
                .PageBy(input);

            var softwareUpdates = from o in pagedAndFilteredSoftwareUpdates
                                  select new
                                  {

                                      o.SoftwareVersion,
                                      o.ForceUpdate,
                                      o.UpdatePath,
                                      o.WhatsNew,
                                      o.Platform,
                                      o.BuildNo,
                                      Id = o.Id
                                  };

            var totalCount = await filteredSoftwareUpdates.CountAsync();

            var dbList = await softwareUpdates.ToListAsync();
            var results = new List<GetSoftwareUpdateForViewDto>();

            foreach (var o in dbList)
            {
                var res = new GetSoftwareUpdateForViewDto()
                {
                    SoftwareUpdate = new SoftwareUpdateDto
                    {

                        SoftwareVersion = o.SoftwareVersion,
                        ForceUpdate = o.ForceUpdate,
                        UpdatePath = o.UpdatePath,
                        WhatsNew = o.WhatsNew,
                        Platform = o.Platform,
                        BuildNo = o.BuildNo,
                        Id = o.Id,
                    }
                };

                results.Add(res);
            }

            return new PagedResultDto<GetSoftwareUpdateForViewDto>(
                totalCount,
                results
            );

        }

        public async Task<GetSoftwareUpdateForViewDto> GetSoftwareUpdateForView(int id)
        {
            var softwareUpdate = await _softwareUpdateRepository.GetAsync(id);

            var output = new GetSoftwareUpdateForViewDto { SoftwareUpdate = ObjectMapper.Map<SoftwareUpdateDto>(softwareUpdate) };

            return output;
        }

        [AbpAuthorize(AppPermissions.Pages_SoftwareUpdates_Edit)]
        public async Task<GetSoftwareUpdateForEditOutput> GetSoftwareUpdateForEdit(EntityDto input)
        {
            var softwareUpdate = await _softwareUpdateRepository.FirstOrDefaultAsync(input.Id);

            var output = new GetSoftwareUpdateForEditOutput { SoftwareUpdate = ObjectMapper.Map<CreateOrEditSoftwareUpdateDto>(softwareUpdate) };

            return output;
        }

        public async Task CreateOrEdit(CreateOrEditSoftwareUpdateDto input)
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

        [AbpAuthorize(AppPermissions.Pages_SoftwareUpdates_Create)]
        protected virtual async Task Create(CreateOrEditSoftwareUpdateDto input)
        {
            var softwareUpdate = ObjectMapper.Map<SoftwareUpdate>(input);

            await _softwareUpdateRepository.InsertAsync(softwareUpdate);

        }

        [AbpAuthorize(AppPermissions.Pages_SoftwareUpdates_Edit)]
        protected virtual async Task Update(CreateOrEditSoftwareUpdateDto input)
        {
            var softwareUpdate = await _softwareUpdateRepository.FirstOrDefaultAsync((int)input.Id);
            ObjectMapper.Map(input, softwareUpdate);

        }

        [AbpAuthorize(AppPermissions.Pages_SoftwareUpdates_Delete)]
        public async Task Delete(EntityDto input)
        {
            await _softwareUpdateRepository.DeleteAsync(input.Id);
        }

    }
}