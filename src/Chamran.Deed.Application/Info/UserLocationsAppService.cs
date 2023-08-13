using Chamran.Deed.Authorization.Users;
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
using Abp.UI;

namespace Chamran.Deed.Info
{
    [AbpAuthorize(AppPermissions.Pages_UserLocations)]
    public class UserLocationsAppService : DeedAppServiceBase, IUserLocationsAppService
    {
        private readonly IRepository<UserLocation> _userLocationRepository;
        private readonly IUserLocationsExcelExporter _userLocationsExcelExporter;
        private readonly IRepository<User, long> _lookup_userRepository;

        public UserLocationsAppService(IRepository<UserLocation> userLocationRepository, IUserLocationsExcelExporter userLocationsExcelExporter, IRepository<User, long> lookup_userRepository)
        {
            _userLocationRepository = userLocationRepository;
            _userLocationsExcelExporter = userLocationsExcelExporter;
            _lookup_userRepository = lookup_userRepository;

        }

        public async Task<PagedResultDto<GetUserLocationForViewDto>> GetAll(GetAllUserLocationsInput input)
        {

            var filteredUserLocations = _userLocationRepository.GetAll()
                        .Include(e => e.UserFk)
                        .WhereIf(!string.IsNullOrWhiteSpace(input.Filter), e => false)
                        .WhereIf(input.MinUserLatFilter != null, e => e.UserLat >= input.MinUserLatFilter)
                        .WhereIf(input.MaxUserLatFilter != null, e => e.UserLat <= input.MaxUserLatFilter)
                        .WhereIf(input.MinUserLongFilter != null, e => e.UserLong >= input.MinUserLongFilter)
                        .WhereIf(input.MaxUserLongFilter != null, e => e.UserLong <= input.MaxUserLongFilter)
                        .WhereIf(!string.IsNullOrWhiteSpace(input.UserNameFilter), e => e.UserFk != null && e.UserFk.Name == input.UserNameFilter);

            var pagedAndFilteredUserLocations = filteredUserLocations
                .OrderBy(input.Sorting ?? "id asc")
                .PageBy(input);

            var userLocations = from o in pagedAndFilteredUserLocations
                                join o1 in _lookup_userRepository.GetAll() on o.UserId equals o1.Id into j1
                                from s1 in j1.DefaultIfEmpty()

                                select new
                                {

                                    o.UserLat,
                                    o.UserLong,
                                    Id = o.Id,
                                    UserName = s1 == null || s1.Name == null ? "" : s1.Name.ToString()
                                };

            var totalCount = await filteredUserLocations.CountAsync();

            var dbList = await userLocations.ToListAsync();
            var results = new List<GetUserLocationForViewDto>();

            foreach (var o in dbList)
            {
                var res = new GetUserLocationForViewDto()
                {
                    UserLocation = new UserLocationDto
                    {

                        UserLat = o.UserLat,
                        UserLong = o.UserLong,
                        Id = o.Id,
                    },
                    UserName = o.UserName
                };

                results.Add(res);
            }

            return new PagedResultDto<GetUserLocationForViewDto>(
                totalCount,
                results
            );

        }

        public async Task<GetUserLocationForViewDto> GetUserLocationForView(int id)
        {
            var userLocation = await _userLocationRepository.GetAsync(id);

            var output = new GetUserLocationForViewDto { UserLocation = ObjectMapper.Map<UserLocationDto>(userLocation) };

            if (output.UserLocation.UserId != null)
            {
                var _lookupUser = await _lookup_userRepository.FirstOrDefaultAsync((long)output.UserLocation.UserId);
                output.UserName = _lookupUser?.Name?.ToString();
            }

            return output;
        }

        [AbpAuthorize(AppPermissions.Pages_UserLocations_Edit)]
        public async Task<GetUserLocationForEditOutput> GetUserLocationForEdit(EntityDto input)
        {
            var userLocation = await _userLocationRepository.FirstOrDefaultAsync(input.Id);

            var output = new GetUserLocationForEditOutput { UserLocation = ObjectMapper.Map<CreateOrEditUserLocationDto>(userLocation) };

            if (output.UserLocation.UserId != null)
            {
                var _lookupUser = await _lookup_userRepository.FirstOrDefaultAsync((long)output.UserLocation.UserId);
                output.UserName = _lookupUser?.Name?.ToString();
            }

            return output;
        }

        public async Task CreateOrEdit(CreateOrEditUserLocationDto input)
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

        [AbpAuthorize(AppPermissions.Pages_UserLocations_Create)]
        protected virtual async Task Create(CreateOrEditUserLocationDto input)
        {
            var userLocation = ObjectMapper.Map<UserLocation>(input);

            await _userLocationRepository.InsertAsync(userLocation);

        }

        [AbpAuthorize(AppPermissions.Pages_UserLocations_Edit)]
        protected virtual async Task Update(CreateOrEditUserLocationDto input)
        {
            var userLocation = await _userLocationRepository.FirstOrDefaultAsync((int)input.Id);
            ObjectMapper.Map(input, userLocation);

        }

        [AbpAuthorize(AppPermissions.Pages_UserLocations_Delete)]
        public async Task Delete(EntityDto input)
        {
            await _userLocationRepository.DeleteAsync(input.Id);
        }

        public async Task<FileDto> GetUserLocationsToExcel(GetAllUserLocationsForExcelInput input)
        {

            var filteredUserLocations = _userLocationRepository.GetAll()
                        .Include(e => e.UserFk)
                        .WhereIf(!string.IsNullOrWhiteSpace(input.Filter), e => false)
                        .WhereIf(input.MinUserLatFilter != null, e => e.UserLat >= input.MinUserLatFilter)
                        .WhereIf(input.MaxUserLatFilter != null, e => e.UserLat <= input.MaxUserLatFilter)
                        .WhereIf(input.MinUserLongFilter != null, e => e.UserLong >= input.MinUserLongFilter)
                        .WhereIf(input.MaxUserLongFilter != null, e => e.UserLong <= input.MaxUserLongFilter)
                        .WhereIf(!string.IsNullOrWhiteSpace(input.UserNameFilter), e => e.UserFk != null && e.UserFk.Name == input.UserNameFilter);

            var query = (from o in filteredUserLocations
                         join o1 in _lookup_userRepository.GetAll() on o.UserId equals o1.Id into j1
                         from s1 in j1.DefaultIfEmpty()

                         select new GetUserLocationForViewDto()
                         {
                             UserLocation = new UserLocationDto
                             {
                                 UserLat = o.UserLat,
                                 UserLong = o.UserLong,
                                 Id = o.Id
                             },
                             UserName = s1 == null || s1.Name == null ? "" : s1.Name.ToString()
                         });

            var userLocationListDtos = await query.ToListAsync();

            return _userLocationsExcelExporter.ExportToFile(userLocationListDtos);
        }

        [AbpAuthorize(AppPermissions.Pages_UserLocations)]
        public async Task<PagedResultDto<UserLocationUserLookupTableDto>> GetAllUserForLookupTable(GetAllForLookupTableInput input)
        {
            var query = _lookup_userRepository.GetAll().WhereIf(
                   !string.IsNullOrWhiteSpace(input.Filter),
                  e => e.Name != null && e.Name.Contains(input.Filter)
               );

            var totalCount = await query.CountAsync();

            var userList = await query
                .PageBy(input)
                .ToListAsync();

            var lookupTableDtoList = new List<UserLocationUserLookupTableDto>();
            foreach (var user in userList)
            {
                lookupTableDtoList.Add(new UserLocationUserLookupTableDto
                {
                    Id = user.Id,
                    DisplayName = user.Name?.ToString()
                });
            }

            return new PagedResultDto<UserLocationUserLookupTableDto>(
                totalCount,
                lookupTableDtoList
            );
        }

        public async Task CreateLocationsByDate(List<CreateLocationsDto> input)
        {
            if (AbpSession.UserId == null) throw new UserFriendlyException("User must be logged in!");
            foreach (var row in input)
            {
                var loc = new UserLocation()
                {
                    UserId = (long)AbpSession.UserId,
                    CreationTime = row.CreationTime,
                    CreatorUserId = (long)AbpSession.UserId,
                    UserLat = row.UserLat,
                    UserLong = row.UserLong
                };
                await _userLocationRepository.InsertAsync(loc);
            }
            
        }
    }
}