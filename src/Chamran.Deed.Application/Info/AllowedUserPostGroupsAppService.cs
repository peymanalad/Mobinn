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
    public class AllowedUserPostGroupsAppService : DeedAppServiceBase, IAllowedUserPostGroupsAppService
    {
        private readonly IRepository<AllowedUserPostGroup> _allowedUserPostGroupRepository;
        private readonly IAllowedUserPostGroupsExcelExporter _allowedUserPostGroupsExcelExporter;
        private readonly IRepository<User, long> _lookup_userRepository;
        private readonly IRepository<PostGroup, int> _lookup_postGroupRepository;
        private readonly IRepository<GroupMember, int> _groupMembersRepository;
        private readonly IRepository<Organization> _organizationGroupsRepository;


        public AllowedUserPostGroupsAppService(IRepository<AllowedUserPostGroup> allowedUserPostGroupRepository, IAllowedUserPostGroupsExcelExporter allowedAllowedUserPostGroupsExcelExporter, IRepository<User, long> lookupUserRepository, IRepository<PostGroup, int> lookupPostGroupRepository, IRepository<GroupMember, int> groupMembersRepository, IRepository<Organization> organizationRepository)
        {
            _allowedUserPostGroupRepository = allowedUserPostGroupRepository;
            _allowedUserPostGroupsExcelExporter = allowedAllowedUserPostGroupsExcelExporter;
            _lookup_userRepository = lookupUserRepository;
            _lookup_postGroupRepository = lookupPostGroupRepository;
            _groupMembersRepository = groupMembersRepository;
            _organizationGroupsRepository = organizationRepository;
        }

        public async Task<PagedResultDto<GetAllowedUserPostGroupForViewDto>> GetAll(GetAllAllowedUserPostGroupsInput input)
        {

            var filteredAllowedUserPostGroups = _allowedUserPostGroupRepository.GetAll()
                        .Include(e => e.UserFk)
                        .Include(e => e.PostGroupFk)
                        .WhereIf(!string.IsNullOrWhiteSpace(input.Filter), e => false)
                        .WhereIf(!string.IsNullOrWhiteSpace(input.UserNameFilter), e => e.UserFk != null && e.UserFk.Name == input.UserNameFilter)
                        .WhereIf(!string.IsNullOrWhiteSpace(input.PostGroupPostGroupDescriptionFilter), e => e.PostGroupFk != null && e.PostGroupFk.PostGroupDescription == input.PostGroupPostGroupDescriptionFilter);

            var pagedAndFilteredAllowedUserPostGroups = filteredAllowedUserPostGroups
                .OrderBy(input.Sorting ?? "id asc")
                .PageBy(input);

            var AllowedUserPostGroups = from o in pagedAndFilteredAllowedUserPostGroups
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

            var totalCount = await filteredAllowedUserPostGroups.CountAsync();

            var dbList = await AllowedUserPostGroups.ToListAsync();
            var results = new List<GetAllowedUserPostGroupForViewDto>();

            foreach (var o in dbList)
            {
                var res = new GetAllowedUserPostGroupForViewDto()
                {
                    UserPostGroup = new AllowedUserPostGroupDto
                    {

                        Id = o.Id,
                    },
                    UserName = o.UserName,
                    PostGroupPostGroupDescription = o.PostGroupPostGroupDescription
                };

                results.Add(res);
            }

            return new PagedResultDto<GetAllowedUserPostGroupForViewDto>(
                totalCount,
                results
            );

        }

        public async Task<GetAllowedUserPostGroupForViewDto> GetAllowedUserPostGroupForView(int id)
        {
            var userPostGroup = await _allowedUserPostGroupRepository.GetAsync(id);

            var output = new GetAllowedUserPostGroupForViewDto { UserPostGroup = ObjectMapper.Map<AllowedUserPostGroupDto>(userPostGroup) };

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
        public async Task<GetAllowedUserPostGroupForEditOutput> GetAllowedUserPostGroupForEdit(EntityDto input)
        {
            var userPostGroup = await _allowedUserPostGroupRepository.FirstOrDefaultAsync(input.Id);

            var output = new GetAllowedUserPostGroupForEditOutput { UserPostGroup = ObjectMapper.Map<CreateOrEditAllowedUserPostGroupDto>(userPostGroup) };

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

        public async Task CreateOrEdit(CreateOrEditAllowedUserPostGroupDto input)
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
        protected virtual async Task Create(CreateOrEditAllowedUserPostGroupDto input)
        {
            var userPostGroup = ObjectMapper.Map<AllowedUserPostGroup>(input);

            await _allowedUserPostGroupRepository.InsertAsync(userPostGroup);

        }

        [AbpAuthorize(AppPermissions.Pages_UserPostGroups_Edit)]
        protected virtual async Task Update(CreateOrEditAllowedUserPostGroupDto input)
        {
            var userPostGroup = await _allowedUserPostGroupRepository.FirstOrDefaultAsync((int)input.Id);
            ObjectMapper.Map(input, userPostGroup);

        }

        [AbpAuthorize(AppPermissions.Pages_UserPostGroups_Delete)]
        public async Task Delete(EntityDto input)
        {
            await _allowedUserPostGroupRepository.DeleteAsync(input.Id);
        }

        public async Task<FileDto> GetAllowedUserPostGroupsToExcel(GetAllAllowedUserPostGroupsForExcelInput input)
        {

            var filteredAllowedUserPostGroups = _allowedUserPostGroupRepository.GetAll()
                        .Include(e => e.UserFk)
                        .Include(e => e.PostGroupFk)
                        .WhereIf(!string.IsNullOrWhiteSpace(input.Filter), e => false)
                        .WhereIf(!string.IsNullOrWhiteSpace(input.UserNameFilter), e => e.UserFk != null && e.UserFk.Name == input.UserNameFilter)
                        .WhereIf(!string.IsNullOrWhiteSpace(input.PostGroupPostGroupDescriptionFilter), e => e.PostGroupFk != null && e.PostGroupFk.PostGroupDescription == input.PostGroupPostGroupDescriptionFilter);

            var query = (from o in filteredAllowedUserPostGroups
                         join o1 in _lookup_userRepository.GetAll() on o.UserId equals o1.Id into j1
                         from s1 in j1.DefaultIfEmpty()

                         join o2 in _lookup_postGroupRepository.GetAll() on o.PostGroupId equals o2.Id into j2
                         from s2 in j2.DefaultIfEmpty()

                         select new GetAllowedUserPostGroupForViewDto()
                         {
                             UserPostGroup = new AllowedUserPostGroupDto
                             {
                                 Id = o.Id
                             },
                             UserName = s1 == null || s1.Name == null ? "" : s1.Name.ToString(),
                             PostGroupPostGroupDescription = s2 == null || s2.PostGroupDescription == null ? "" : s2.PostGroupDescription.ToString()
                         });

            var userPostGroupListDtos = await query.ToListAsync();

            return _allowedUserPostGroupsExcelExporter.ExportToFile(userPostGroupListDtos);
        }

        [AbpAuthorize(AppPermissions.Pages_UserPostGroups)]
        public async Task<PagedResultDto<AllowedUserPostGroupUserLookupTableDto>> GetAllAllowedUserForLookupTable(GetAllForLookupTableInput input)
        {
            var query = _lookup_userRepository.GetAll().WhereIf(
                   !string.IsNullOrWhiteSpace(input.Filter),
                  e => e.Name != null && e.Name.Contains(input.Filter)
               );

            var totalCount = await query.CountAsync();

            var userList = await query
                .PageBy(input)
                .ToListAsync();

            var lookupTableDtoList = new List<AllowedUserPostGroupUserLookupTableDto>();
            foreach (var user in userList)
            {
                lookupTableDtoList.Add(new AllowedUserPostGroupUserLookupTableDto
                {
                    Id = user.Id,
                    DisplayName = user.Name?.ToString()
                });
            }

            return new PagedResultDto<AllowedUserPostGroupUserLookupTableDto>(
                totalCount,
                lookupTableDtoList
            );
        }

        [AbpAuthorize(AppPermissions.Pages_UserPostGroups)]
        public async Task<PagedResultDto<AllowedUserPostGroupPostGroupLookupTableDto>> GetAllAllowedPostGroupForLookupTable(GetAllForLookupTableInput input)
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
            var query = _lookup_postGroupRepository.GetAll().WhereIf(!string.IsNullOrWhiteSpace(input.Filter), e => e.PostGroupDescription != null && e.PostGroupDescription.Contains(input.Filter)
               );
            query = query.Where(x => x.OrganizationId == orgEntity.Id);
            var totalCount = await query.CountAsync();

            var postGroupList = await query
                .PageBy(input)
                .ToListAsync();

            var lookupTableDtoList = new List<AllowedUserPostGroupPostGroupLookupTableDto>();
            foreach (var postGroup in postGroupList)
            {
                lookupTableDtoList.Add(new AllowedUserPostGroupPostGroupLookupTableDto
                {
                    Id = postGroup.Id,
                    DisplayName = postGroup.PostGroupDescription?.ToString()
                });
            }

            return new PagedResultDto<AllowedUserPostGroupPostGroupLookupTableDto>(
                totalCount,
                lookupTableDtoList
            );
        }

        [AbpAuthorize(AppPermissions.Pages_UserPostGroups)]
        public async Task<PagedResultDto<AllowedUserPostGroupselectDto>> GetAllowedUserPostGroupselection(GetAllowedUserPostGroupselectInput input)
        {

            var query = from x in _lookup_postGroupRepository.GetAll().Where(x => !x.IsDeleted && x.OrganizationId == input.OrganizationId)
                        join ug in _allowedUserPostGroupRepository.GetAll().Where(x => x.UserId == AbpSession.UserId) on x.Id equals
                            ug.PostGroupId into joiner1
                        from ug in joiner1.DefaultIfEmpty()
                        select new AllowedUserPostGroupselectDto
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


            var pagedAndFilteredAllowedUserPostGroups = query
.OrderBy(input.Sorting ?? "GroupId asc")
.PageBy(input);

            //var AllowedUserPostGroups = from pg in pagedAndFilteredAllowedUserPostGroups
            //                     join upg in _allowedUserPostGroupRepository.GetAll() on pg.Id equals upg.PostGroupId into upgJoin
            //                     from upg in upgJoin.DefaultIfEmpty()

            //                     select new AllowedUserPostGroupselectDto
            //                     {
            //                         GroupId = pg.Id,
            //                         GroupDescription = pg.PostGroupDescription,
            //                         IsSelected = upg != null
            //                     };

            var totalCount = await query.CountAsync();
            //var dbList = await AllowedUserPostGroups.ToListAsync();

            return new PagedResultDto<AllowedUserPostGroupselectDto>(
                totalCount,
                await pagedAndFilteredAllowedUserPostGroups.ToListAsync()
            );


        }

        public async Task UpdateAllowedUserGroupSelection(List<int> input)
        {
            if (AbpSession.UserId == null) throw new UserFriendlyException("Not Logged In!");
            await _allowedUserPostGroupRepository.DeleteAsync(x => x.UserId == AbpSession.UserId);
            foreach (var row in input)
            {

                await _allowedUserPostGroupRepository.InsertAsync(new AllowedUserPostGroup()
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