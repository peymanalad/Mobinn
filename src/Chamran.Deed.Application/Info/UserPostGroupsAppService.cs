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
using Abp.Timing;
using Microsoft.EntityFrameworkCore;
using Abp.UI;
using Chamran.Deed.People;

namespace Chamran.Deed.Info
{
    [AbpAuthorize(AppPermissions.Pages_UserPostGroups)]
    public class UserPostGroupsAppService : DeedAppServiceBase, IUserPostGroupsAppService
    {
        private readonly IRepository<UserPostGroup> _userPostGroupRepository;
        private readonly IUserPostGroupsExcelExporter _userPostGroupsExcelExporter;
        private readonly IRepository<User, long> _lookup_userRepository;
        private readonly IRepository<PostGroup, int> _lookup_postGroupRepository;
        private readonly IRepository<GroupMember, int> _groupMembersRepository;
        private readonly IRepository<Organization> _organizationGroupsRepository;


        public UserPostGroupsAppService(IRepository<UserPostGroup> userPostGroupRepository, IUserPostGroupsExcelExporter userPostGroupsExcelExporter, IRepository<User, long> lookupUserRepository, IRepository<PostGroup, int> lookupPostGroupRepository, IRepository<GroupMember, int> groupMembersRepository, IRepository<Organization> organizationRepository)
        {
            _userPostGroupRepository = userPostGroupRepository;
            _userPostGroupsExcelExporter = userPostGroupsExcelExporter;
            _lookup_userRepository = lookupUserRepository;
            _lookup_postGroupRepository = lookupPostGroupRepository;
            _groupMembersRepository = groupMembersRepository;
            _organizationGroupsRepository = organizationRepository;
        }

        public async Task<PagedResultDto<GetUserPostGroupForViewDto>> GetAll(GetAllUserPostGroupsInput input)
        {

            var filteredUserPostGroups = _userPostGroupRepository.GetAll()
                        .Include(e => e.UserFk)
                        .Include(e => e.PostGroupFk)
                        .WhereIf(!string.IsNullOrWhiteSpace(input.Filter), e => false)
                        .WhereIf(!string.IsNullOrWhiteSpace(input.UserNameFilter), e => e.UserFk != null && e.UserFk.Name == input.UserNameFilter)
                        .WhereIf(!string.IsNullOrWhiteSpace(input.PostGroupPostGroupDescriptionFilter), e => e.PostGroupFk != null && e.PostGroupFk.PostGroupDescription == input.PostGroupPostGroupDescriptionFilter);

            var pagedAndFilteredUserPostGroups = filteredUserPostGroups
                .OrderBy(input.Sorting ?? "id asc")
                .PageBy(input);

            var userPostGroups = from o in pagedAndFilteredUserPostGroups
                                 join o1 in _lookup_userRepository.GetAll() on o.UserId equals o1.Id into j1
                                 from s1 in j1.DefaultIfEmpty()

                                 join o2 in _lookup_postGroupRepository.GetAll() on o.PostGroupId equals o2.Id into j2
                                 from s2 in j2.DefaultIfEmpty()

                                 select new
                                 {

                                     Id = o.Id,
                                     UserName = s1 == null || s1.Name == null ? "" : s1.Name.ToString(),
                                     PostGroupPostGroupDescription = s2 == null || s2.PostGroupDescription == null ? "" : s2.PostGroupDescription.ToString()
                                 };

            var totalCount = await filteredUserPostGroups.CountAsync();

            var dbList = await userPostGroups.ToListAsync();
            var results = new List<GetUserPostGroupForViewDto>();

            foreach (var o in dbList)
            {
                var res = new GetUserPostGroupForViewDto()
                {
                    UserPostGroup = new UserPostGroupDto
                    {

                        Id = o.Id,
                    },
                    UserName = o.UserName,
                    PostGroupPostGroupDescription = o.PostGroupPostGroupDescription
                };

                results.Add(res);
            }

            return new PagedResultDto<GetUserPostGroupForViewDto>(
                totalCount,
                results
            );

        }

        public async Task<GetUserPostGroupForViewDto> GetUserPostGroupForView(int id)
        {
            var userPostGroup = await _userPostGroupRepository.GetAsync(id);

            var output = new GetUserPostGroupForViewDto { UserPostGroup = ObjectMapper.Map<UserPostGroupDto>(userPostGroup) };

            if (output.UserPostGroup.UserId != null)
            {
                var _lookupUser = await _lookup_userRepository.FirstOrDefaultAsync((long)output.UserPostGroup.UserId);
                output.UserName = _lookupUser?.Name?.ToString();
            }

            if (output.UserPostGroup.PostGroupId != null)
            {
                var _lookupPostGroup = await _lookup_postGroupRepository.FirstOrDefaultAsync((int)output.UserPostGroup.PostGroupId);
                output.PostGroupPostGroupDescription = _lookupPostGroup?.PostGroupDescription?.ToString();
            }

            return output;
        }

        [AbpAuthorize(AppPermissions.Pages_UserPostGroups_Edit)]
        public async Task<GetUserPostGroupForEditOutput> GetUserPostGroupForEdit(EntityDto input)
        {
            var userPostGroup = await _userPostGroupRepository.FirstOrDefaultAsync(input.Id);

            var output = new GetUserPostGroupForEditOutput { UserPostGroup = ObjectMapper.Map<CreateOrEditUserPostGroupDto>(userPostGroup) };

            if (output.UserPostGroup.UserId != null)
            {
                var _lookupUser = await _lookup_userRepository.FirstOrDefaultAsync((long)output.UserPostGroup.UserId);
                output.UserName = _lookupUser?.Name?.ToString();
            }

            if (output.UserPostGroup.PostGroupId != null)
            {
                var _lookupPostGroup = await _lookup_postGroupRepository.FirstOrDefaultAsync((int)output.UserPostGroup.PostGroupId);
                output.PostGroupPostGroupDescription = _lookupPostGroup?.PostGroupDescription?.ToString();
            }

            return output;
        }

        public async Task CreateOrEdit(CreateOrEditUserPostGroupDto input)
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

        [AbpAuthorize(AppPermissions.Pages_UserPostGroups_Create)]
        protected virtual async Task Create(CreateOrEditUserPostGroupDto input)
        {
            var userPostGroup = ObjectMapper.Map<UserPostGroup>(input);

            await _userPostGroupRepository.InsertAsync(userPostGroup);

        }

        [AbpAuthorize(AppPermissions.Pages_UserPostGroups_Edit)]
        protected virtual async Task Update(CreateOrEditUserPostGroupDto input)
        {
            var userPostGroup = await _userPostGroupRepository.FirstOrDefaultAsync((int)input.Id);
            ObjectMapper.Map(input, userPostGroup);

        }

        [AbpAuthorize(AppPermissions.Pages_UserPostGroups_Delete)]
        public async Task Delete(EntityDto input)
        {
            await _userPostGroupRepository.DeleteAsync(input.Id);
        }

        public async Task<FileDto> GetUserPostGroupsToExcel(GetAllUserPostGroupsForExcelInput input)
        {

            var filteredUserPostGroups = _userPostGroupRepository.GetAll()
                        .Include(e => e.UserFk)
                        .Include(e => e.PostGroupFk)
                        .WhereIf(!string.IsNullOrWhiteSpace(input.Filter), e => false)
                        .WhereIf(!string.IsNullOrWhiteSpace(input.UserNameFilter), e => e.UserFk != null && e.UserFk.Name == input.UserNameFilter)
                        .WhereIf(!string.IsNullOrWhiteSpace(input.PostGroupPostGroupDescriptionFilter), e => e.PostGroupFk != null && e.PostGroupFk.PostGroupDescription == input.PostGroupPostGroupDescriptionFilter);

            var query = (from o in filteredUserPostGroups
                         join o1 in _lookup_userRepository.GetAll() on o.UserId equals o1.Id into j1
                         from s1 in j1.DefaultIfEmpty()

                         join o2 in _lookup_postGroupRepository.GetAll() on o.PostGroupId equals o2.Id into j2
                         from s2 in j2.DefaultIfEmpty()

                         select new GetUserPostGroupForViewDto()
                         {
                             UserPostGroup = new UserPostGroupDto
                             {
                                 Id = o.Id
                             },
                             UserName = s1 == null || s1.Name == null ? "" : s1.Name.ToString(),
                             PostGroupPostGroupDescription = s2 == null || s2.PostGroupDescription == null ? "" : s2.PostGroupDescription.ToString()
                         });

            var userPostGroupListDtos = await query.ToListAsync();

            return _userPostGroupsExcelExporter.ExportToFile(userPostGroupListDtos);
        }

        [AbpAuthorize(AppPermissions.Pages_UserPostGroups)]
        public async Task<PagedResultDto<UserPostGroupUserLookupTableDto>> GetAllUserForLookupTable(GetAllForLookupTableInput input)
        {
            var query = _lookup_userRepository.GetAll().WhereIf(
                   !string.IsNullOrWhiteSpace(input.Filter),
                  e => e.Name != null && e.Name.Contains(input.Filter)
               );

            var totalCount = await query.CountAsync();

            var userList = await query
                .PageBy(input)
                .ToListAsync();

            var lookupTableDtoList = new List<UserPostGroupUserLookupTableDto>();
            foreach (var user in userList)
            {
                lookupTableDtoList.Add(new UserPostGroupUserLookupTableDto
                {
                    Id = user.Id,
                    DisplayName = user.Name?.ToString()
                });
            }

            return new PagedResultDto<UserPostGroupUserLookupTableDto>(
                totalCount,
                lookupTableDtoList
            );
        }

        [AbpAuthorize(AppPermissions.Pages_UserPostGroups)]
        public async Task<PagedResultDto<UserPostGroupPostGroupLookupTableDto>> GetAllPostGroupForLookupTable(GetAllForLookupTableInput input)
        {
            if (AbpSession.UserId == null) throw new UserFriendlyException("Not Logged In!");
            var orgQuery =
                from org in _organizationGroupsRepository.GetAll().Where(x => !x.IsDeleted)
                join grpMember in _groupMembersRepository.GetAll() on org.Id equals grpMember
                    .OrganizationId into joined2
                from grpMember in joined2.DefaultIfEmpty()
                where grpMember.UserId == AbpSession.UserId
                select org;

            if (!orgQuery.Any())
            {
                throw new UserFriendlyException("کاربر عضو هیچ گروهی در هیچ سازمانی نمی باشد");
            }
            var orgEntity = orgQuery.First();
            var query = _lookup_postGroupRepository.GetAll().WhereIf(!string.IsNullOrWhiteSpace(input.Filter),e => e.PostGroupDescription != null && e.PostGroupDescription.Contains(input.Filter)
               );
            query = query.Where(x => x.OrganizationId == orgEntity.Id);
            var totalCount = await query.CountAsync();

            var postGroupList = await query
                .PageBy(input)
                .ToListAsync();

            var lookupTableDtoList = new List<UserPostGroupPostGroupLookupTableDto>();
            foreach (var postGroup in postGroupList)
            {
                lookupTableDtoList.Add(new UserPostGroupPostGroupLookupTableDto
                {
                    Id = postGroup.Id,
                    DisplayName = postGroup.PostGroupDescription?.ToString()
                });
            }

            return new PagedResultDto<UserPostGroupPostGroupLookupTableDto>(
                totalCount,
                lookupTableDtoList
            );
        }

        [AbpAuthorize(AppPermissions.Pages_UserPostGroups)]
        public async Task<PagedResultDto<UserPostGroupSelectDto>> GetUserPostGroupSelection(GetUserPostGroupSelectInput input)
        {

            var query = from x in _lookup_postGroupRepository.GetAll().Where(x => !x.IsDeleted && x.OrganizationId==input.OrganizationId)
                join ug in _userPostGroupRepository.GetAll().Where(x => x.UserId == AbpSession.UserId) on x.Id equals
                    ug.PostGroupId into joiner1
                from ug in joiner1.DefaultIfEmpty()
                select new UserPostGroupSelectDto
                {

                    IsSelected = ug != null,
                    GroupDescription = x.PostGroupDescription,
                    GroupId = x.Id
                };

                  //var query = from pg in _lookup_postGroupRepository.GetAll().Where(x => !x.IsDeleted)
                  //            join og in _organizationGroupsRepository.GetAll().Where(x => !x.IsDeleted) on pg.OrganizationId
                  //                equals og.Id into joiner1
                  //            from og in joiner1.DefaultIfEmpty()
                  //            join gm in _groupMembersRepository.GetAll() on og.Id equals gm.OrganizationId into joiner2
                  //            from gm in joiner2.DefaultIfEmpty()

                  //            where gm.UserId == AbpSession.UserId
                  //            select new
                  //            {
                  //                pg.Id,
                  //                pg.PostGroupDescription
                  //            };


                  var pagedAndFilteredUserPostGroups = query
    .OrderBy(input.Sorting ?? "GroupId asc")
    .PageBy(input);

            //var userPostGroups = from pg in pagedAndFilteredUserPostGroups
            //                     join upg in _userPostGroupRepository.GetAll() on pg.Id equals upg.PostGroupId into upgJoin
            //                     from upg in upgJoin.DefaultIfEmpty()

            //                     select new UserPostGroupSelectDto
            //                     {
            //                         GroupId = pg.Id,
            //                         GroupDescription = pg.PostGroupDescription,
            //                         IsSelected = upg != null
            //                     };

            var totalCount = await query.CountAsync();
            //var dbList = await userPostGroups.ToListAsync();

            return new PagedResultDto<UserPostGroupSelectDto>(
                totalCount,
                await pagedAndFilteredUserPostGroups.ToListAsync()
            );


        }

        public async Task UpdateUserGroupSelection(List<int> input)
        {
            if (AbpSession.UserId == null) throw new UserFriendlyException("Not Logged In!");
            await _userPostGroupRepository.DeleteAsync(x => x.UserId == AbpSession.UserId);
            foreach (var row in input)
            {

                await _userPostGroupRepository.InsertAsync(new UserPostGroup()
                {
                    UserId = (long)AbpSession.UserId,
                    PostGroupId = row,
                    CreationTime = Clock.Now,
                    CreatorUserId = AbpSession.UserId,
                });
            }

        }
    }
}