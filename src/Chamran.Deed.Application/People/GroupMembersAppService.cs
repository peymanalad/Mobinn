using Chamran.Deed.Authorization.Users;
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
    [AbpAuthorize(AppPermissions.Pages_GroupMembers)]
    public class GroupMembersAppService : DeedAppServiceBase, IGroupMembersAppService
    {
        private readonly IRepository<GroupMember> _groupMemberRepository;
        private readonly IGroupMembersExcelExporter _groupMembersExcelExporter;
        private readonly IRepository<Organization, int> _lookup_organizationRepository;
        private readonly IRepository<User, long> _lookup_userRepository;
        private readonly IRepository<Organization, int> _lookup_organizationGroupRepository;

        public GroupMembersAppService(IRepository<GroupMember> groupMemberRepository, IGroupMembersExcelExporter groupMembersExcelExporter, IRepository<User, long> lookup_userRepository, IRepository<Organization, int> lookup_organizationGroupRepository, IRepository<Organization, int> lookupOrganizationRepository)
        {
            _groupMemberRepository = groupMemberRepository;
            _groupMembersExcelExporter = groupMembersExcelExporter;
            _lookup_userRepository = lookup_userRepository;
            _lookup_organizationGroupRepository = lookup_organizationGroupRepository;
            _lookup_organizationRepository = lookupOrganizationRepository;
        }

        public async Task<PagedResultDto<GetGroupMemberForViewDto>> GetAll(GetAllGroupMembersInput input)
        {
            if (AbpSession.UserId == null) throw new UserFriendlyException("User Must be Logged in!");
            var user = await _lookup_userRepository.GetAsync(AbpSession.UserId.Value);


            var filteredGroupMembers = _groupMemberRepository.GetAll()
                        .Include(e => e.UserFk)
                        .Include(e => e.OrganizationFk)
                        .WhereIf(!string.IsNullOrWhiteSpace(input.Filter), e => false || e.MemberPosition.Contains(input.Filter))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.MemberPositionFilter), e => e.MemberPosition.Contains(input.MemberPositionFilter))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.UserNameFilter), e => e.UserFk != null && e.UserFk.Name == input.UserNameFilter)
                        .WhereIf(!string.IsNullOrWhiteSpace(input.OrganizationGroupGroupNameFilter), e => e.OrganizationFk != null && e.OrganizationFk.OrganizationName == input.OrganizationGroupGroupNameFilter);


            if (!user.IsSuperUser)
            {
                var orgQuery =
                    from org in _lookup_organizationRepository.GetAll().Where(x => !x.IsDeleted)
                    join grpMember in _groupMemberRepository.GetAll() on org.Id equals grpMember
                        .OrganizationId into joined2
                    from grpMember in joined2.DefaultIfEmpty()
                    where grpMember.UserId == AbpSession.UserId
                    select org;

                if (!orgQuery.Any())
                {
                    throw new UserFriendlyException("کاربر عضو هیچ گروهی در هیچ سازمانی نمی باشد");
                }
                var orgEntity = orgQuery.First();
                filteredGroupMembers = filteredGroupMembers.Where(x => x.OrganizationId == orgEntity.Id);
            }

            var pagedAndFilteredGroupMembers = filteredGroupMembers
                .OrderBy(input.Sorting ?? "id asc")
                .PageBy(input);

            var groupMembers = from o in pagedAndFilteredGroupMembers
                               join o1 in _lookup_userRepository.GetAll() on o.UserId equals o1.Id into j1
                               from s1 in j1.DefaultIfEmpty()

                               join o2 in _lookup_organizationGroupRepository.GetAll() on o.OrganizationId equals o2.Id into j2
                               from s2 in j2.DefaultIfEmpty()

                               select new
                               {
                                   o.MemberPos,
                                   o.MemberPosition,
                                   Id = o.Id,
                                   UserName = s1 == null || s1.Name == null ? "" : s1.Name.ToString(),
                                   OrganizationGroupGroupName = s2 == null || s2.OrganizationName == null ? "" : s2.OrganizationName.ToString()
                               };

            var totalCount = await filteredGroupMembers.CountAsync();

            var dbList = await groupMembers.ToListAsync();
            var results = new List<GetGroupMemberForViewDto>();

            foreach (var o in dbList)
            {
                var res = new GetGroupMemberForViewDto()
                {
                    GroupMember = new GroupMemberDto
                    {
                        MemberPos = o.MemberPos,
                        MemberPosition = o.MemberPosition,
                        Id = o.Id,
                    },
                    UserName = o.UserName,
                    OrganizationGroupGroupName = o.OrganizationGroupGroupName
                };

                results.Add(res);
            }

            return new PagedResultDto<GetGroupMemberForViewDto>(
                totalCount,
                results
            );

        }

        public async Task<GetGroupMemberForViewDto> GetGroupMemberForView(int id)
        {
            var groupMember = await _groupMemberRepository.GetAsync(id);

            var output = new GetGroupMemberForViewDto { GroupMember = ObjectMapper.Map<GroupMemberDto>(groupMember) };

            if (output.GroupMember.UserId != null)
            {
                var _lookupUser = await _lookup_userRepository.FirstOrDefaultAsync((long)output.GroupMember.UserId);
                output.UserName = _lookupUser?.Name?.ToString();
            }

            if (output.GroupMember.OrganizationId != null)
            {
                var _lookupOrganizationGroup = await _lookup_organizationGroupRepository.FirstOrDefaultAsync((int)output.GroupMember.OrganizationId);
                output.OrganizationGroupGroupName = _lookupOrganizationGroup?.OrganizationName?.ToString();
            }

            return output;
        }

        [AbpAuthorize(AppPermissions.Pages_GroupMembers_Edit)]
        public async Task<GetGroupMemberForEditOutput> GetGroupMemberForEdit(EntityDto input)
        {
            var groupMember = await _groupMemberRepository.FirstOrDefaultAsync(input.Id);

            var output = new GetGroupMemberForEditOutput { GroupMember = ObjectMapper.Map<CreateOrEditGroupMemberDto>(groupMember) };

            if (output.GroupMember.UserId != null)
            {
                var _lookupUser = await _lookup_userRepository.FirstOrDefaultAsync((long)output.GroupMember.UserId);
                output.UserName = _lookupUser?.Name?.ToString();
            }

            if (output.GroupMember.OrganizationId != null)
            {
                var _lookupOrganizationGroup = await _lookup_organizationGroupRepository.FirstOrDefaultAsync((int)output.GroupMember.OrganizationId);
                output.OrganizationGroupGroupName = _lookupOrganizationGroup?.OrganizationName?.ToString();
            }

            return output;
        }

        public async Task CreateOrEdit(CreateOrEditGroupMemberDto input)
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

        [AbpAuthorize(AppPermissions.Pages_GroupMembers_Create)]
        protected virtual async Task Create(CreateOrEditGroupMemberDto input)
        {
            var groupMember = ObjectMapper.Map<GroupMember>(input);

            await _groupMemberRepository.InsertAsync(groupMember);

        }

        [AbpAuthorize(AppPermissions.Pages_GroupMembers_Edit)]
        protected virtual async Task Update(CreateOrEditGroupMemberDto input)
        {
            var groupMember = await _groupMemberRepository.FirstOrDefaultAsync((int)input.Id);
            ObjectMapper.Map(input, groupMember);

        }

        [AbpAuthorize(AppPermissions.Pages_GroupMembers_Delete)]
        public async Task Delete(EntityDto input)
        {
            await _groupMemberRepository.DeleteAsync(input.Id);
        }

        public async Task<FileDto> GetGroupMembersToExcel(GetAllGroupMembersForExcelInput input)
        {

            var filteredGroupMembers = _groupMemberRepository.GetAll()
                        .Include(e => e.UserFk)
                        .Include(e => e.OrganizationFk)
                        .WhereIf(!string.IsNullOrWhiteSpace(input.Filter), e => false || e.MemberPosition.Contains(input.Filter))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.MemberPositionFilter), e => e.MemberPosition.Contains(input.MemberPositionFilter))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.UserNameFilter), e => e.UserFk != null && e.UserFk.Name == input.UserNameFilter)
                        .WhereIf(!string.IsNullOrWhiteSpace(input.OrganizationGroupGroupNameFilter), e => e.OrganizationFk != null && e.OrganizationFk.OrganizationName == input.OrganizationGroupGroupNameFilter);

            var query = (from o in filteredGroupMembers
                         join o1 in _lookup_userRepository.GetAll() on o.UserId equals o1.Id into j1
                         from s1 in j1.DefaultIfEmpty()

                         join o2 in _lookup_organizationGroupRepository.GetAll() on o.OrganizationId equals o2.Id into j2
                         from s2 in j2.DefaultIfEmpty()

                         select new GetGroupMemberForViewDto()
                         {
                             GroupMember = new GroupMemberDto
                             {
                                 MemberPosition = o.MemberPosition,
                                 Id = o.Id
                             },
                             UserName = s1 == null || s1.Name == null ? "" : s1.Name.ToString(),
                             OrganizationGroupGroupName = s2 == null || s2.OrganizationName == null ? "" : s2.OrganizationName.ToString()
                         });

            var groupMemberListDtos = await query.ToListAsync();

            return _groupMembersExcelExporter.ExportToFile(groupMemberListDtos);
        }

        [AbpAuthorize(AppPermissions.Pages_GroupMembers)]
        public async Task<PagedResultDto<GroupMemberUserLookupTableDto>> GetAllUserForLookupTable(GetAllForLookupTableInput input)
        {
            if (AbpSession.UserId == null) throw new UserFriendlyException("User Must be Logged in!");
            var currentUser = await _lookup_userRepository.GetAsync(AbpSession.UserId.Value);


            var query = _lookup_userRepository.GetAll()
                .WhereIf(
                   !string.IsNullOrWhiteSpace(input.Filter),
                  e => e.Name != null && e.Name.Contains(input.Filter)
               );

            var joinedQuery = from x in query
                join y in _groupMemberRepository.GetAll() on x.Id equals y.UserId 
                select new
                {
                    x, y
                };


            if (!currentUser.IsSuperUser)
            {
                var orgQuery =
                    from org in _lookup_organizationRepository.GetAll().Where(x => !x.IsDeleted)
                    join grpMember in _groupMemberRepository.GetAll() on org.Id equals grpMember
                        .OrganizationId into joined2
                    from grpMember in joined2.DefaultIfEmpty()
                    where grpMember.UserId == AbpSession.UserId
                    select org;

                if (!orgQuery.Any())
                {
                    throw new UserFriendlyException("کاربر عضو هیچ گروهی در هیچ سازمانی نمی باشد");
                }
                var orgEntity = orgQuery.First();
                joinedQuery = joinedQuery.Where(x => x.y.OrganizationId == orgEntity.Id);
            }

            var totalCount = await joinedQuery.CountAsync();

            var userList = await joinedQuery
                .PageBy(input)
                .ToListAsync();

            var lookupTableDtoList = new List<GroupMemberUserLookupTableDto>();
            foreach (var user in userList)
            {
                lookupTableDtoList.Add(new GroupMemberUserLookupTableDto
                {
                    Id = user.x.Id,
                    DisplayName = user.x.Name?.ToString()
                });
            }

            return new PagedResultDto<GroupMemberUserLookupTableDto>(
                totalCount,
                lookupTableDtoList
            );
        }

        [AbpAuthorize(AppPermissions.Pages_GroupMembers)]
        public async Task<PagedResultDto<GroupMemberOrganizationGroupLookupTableDto>> GetAllOrganizationGroupForLookupTable(GetAllForLookupTableInput input)
        {
            var query = _lookup_organizationGroupRepository.GetAll().WhereIf(
                   !string.IsNullOrWhiteSpace(input.Filter),
                  e => e.OrganizationName != null && e.OrganizationName.Contains(input.Filter)
               );

            var totalCount = await query.CountAsync();

            var organizationGroupList = await query
                .PageBy(input)
                .ToListAsync();

            var lookupTableDtoList = new List<GroupMemberOrganizationGroupLookupTableDto>();
            foreach (var organizationGroup in organizationGroupList)
            {
                lookupTableDtoList.Add(new GroupMemberOrganizationGroupLookupTableDto
                {
                    Id = organizationGroup.Id,
                    DisplayName = organizationGroup.OrganizationName?.ToString()
                });
            }

            return new PagedResultDto<GroupMemberOrganizationGroupLookupTableDto>(
                totalCount,
                lookupTableDtoList
            );
        }

    }
}