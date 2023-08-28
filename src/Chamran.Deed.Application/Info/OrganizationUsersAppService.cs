using Chamran.Deed.Authorization.Users;
using System;
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
using Abp.UI;
using Chamran.Deed.People;
using Microsoft.EntityFrameworkCore;

namespace Chamran.Deed.Info
{
    [AbpAuthorize(AppPermissions.Pages_OrganizationUsers)]
    public class OrganizationUsersAppService : DeedAppServiceBase, IOrganizationUsersAppService
    {
        private readonly IRepository<OrganizationUser> _organizationUserRepository;
        private readonly IOrganizationUsersExcelExporter _organizationUsersExcelExporter;
        private readonly IRepository<User, long> _lookup_userRepository;
        private readonly IRepository<OrganizationChart, int> _lookup_organizationChartRepository;
        private readonly IRepository<User, long> _userRepository;
        private readonly IRepository<GroupMember> _groupMemberRepository;

        public OrganizationUsersAppService(IRepository<OrganizationUser> organizationUserRepository, IOrganizationUsersExcelExporter organizationUsersExcelExporter, IRepository<User, long> lookup_userRepository, IRepository<OrganizationChart, int> lookup_organizationChartRepository, IRepository<User, long> userRepository, IRepository<GroupMember> groupMemberRepository)
        {
            _organizationUserRepository = organizationUserRepository;
            _organizationUsersExcelExporter = organizationUsersExcelExporter;
            _lookup_userRepository = lookup_userRepository;
            _lookup_organizationChartRepository = lookup_organizationChartRepository;
            _userRepository = userRepository;
            _groupMemberRepository = groupMemberRepository;
        }

        public virtual async Task<PagedResultDto<GetOrganizationUserForViewDto>> GetAll(GetAllOrganizationUsersInput input)
        {

            var filteredOrganizationUsers = _organizationUserRepository.GetAll()
                        .Include(e => e.UserFk)
                        .Include(e => e.OrganizationChartFk)
                        .WhereIf(!string.IsNullOrWhiteSpace(input.Filter), e => false)
                        .WhereIf(!string.IsNullOrWhiteSpace(input.UserNameFilter), e => e.UserFk != null && e.UserFk.Name == input.UserNameFilter)
                        .WhereIf(!string.IsNullOrWhiteSpace(input.OrganizationChartCaptionFilter), e => e.OrganizationChartFk != null && e.OrganizationChartFk.Caption == input.OrganizationChartCaptionFilter);

            var pagedAndFilteredOrganizationUsers = filteredOrganizationUsers
                .OrderBy(input.Sorting ?? "id asc")
                .PageBy(input);

            var organizationUsers = from o in pagedAndFilteredOrganizationUsers
                                    join o1 in _lookup_userRepository.GetAll() on o.UserId equals o1.Id into j1
                                    from s1 in j1.DefaultIfEmpty()

                                    join o2 in _lookup_organizationChartRepository.GetAll() on o.OrganizationChartId equals o2.Id into j2
                                    from s2 in j2.DefaultIfEmpty()

                                    select new
                                    {
                                        UserId = s1 == null || s1.Name == null ? 0 : s1.Id,
                                        Id = o.Id,
                                        UserName = s1 == null || s1.Name == null ? "" : s1.Name.ToString(),
                                        OrganizationChartCaption = s2 == null || s2.Caption == null ? "" : s2.Caption.ToString(),
                                        OrganizationChartId = s2 == null || s2.Caption == null ? 0 : s2.Id
                                    };

            var totalCount = await filteredOrganizationUsers.CountAsync();

            var dbList = await organizationUsers.ToListAsync();
            var results = new List<GetOrganizationUserForViewDto>();

            foreach (var o in dbList)
            {
                var res = new GetOrganizationUserForViewDto()
                {
                    OrganizationUser = new OrganizationUserDto
                    {
                        OrganizationChartId = o.OrganizationChartId,
                        UserId = o.UserId,
                        Id = o.Id,
                    },
                    UserName = o.UserName,
                    OrganizationChartCaption = o.OrganizationChartCaption
                };

                results.Add(res);
            }

            return new PagedResultDto<GetOrganizationUserForViewDto>(
                totalCount,
                results
            );

        }

        public virtual async Task<GetOrganizationUserForViewDto> GetOrganizationUserForView(int id)
        {
            var organizationUser = await _organizationUserRepository.GetAsync(id);

            var output = new GetOrganizationUserForViewDto { OrganizationUser = ObjectMapper.Map<OrganizationUserDto>(organizationUser) };

            if (output.OrganizationUser.UserId != null)
            {
                var _lookupUser = await _lookup_userRepository.FirstOrDefaultAsync((long)output.OrganizationUser.UserId);
                output.UserName = _lookupUser?.Name?.ToString();
            }

            if (output.OrganizationUser.OrganizationChartId != null)
            {
                var _lookupOrganizationChart = await _lookup_organizationChartRepository.FirstOrDefaultAsync((int)output.OrganizationUser.OrganizationChartId);
                output.OrganizationChartCaption = _lookupOrganizationChart?.Caption?.ToString();
            }

            return output;
        }

        [AbpAuthorize(AppPermissions.Pages_OrganizationUsers_Edit)]
        public virtual async Task<GetOrganizationUserForEditOutput> GetOrganizationUserForEdit(EntityDto input)
        {
            var organizationUser = await _organizationUserRepository.FirstOrDefaultAsync(input.Id);

            var output = new GetOrganizationUserForEditOutput { OrganizationUser = ObjectMapper.Map<CreateOrEditOrganizationUserDto>(organizationUser) };

            if (output.OrganizationUser.UserId != null)
            {
                var _lookupUser = await _lookup_userRepository.FirstOrDefaultAsync((long)output.OrganizationUser.UserId);
                output.UserName = _lookupUser?.Name?.ToString();
            }

            if (output.OrganizationUser.OrganizationChartId != null)
            {
                var _lookupOrganizationChart = await _lookup_organizationChartRepository.FirstOrDefaultAsync((int)output.OrganizationUser.OrganizationChartId);
                output.OrganizationChartCaption = _lookupOrganizationChart?.Caption?.ToString();
            }

            return output;
        }

        public virtual async Task CreateOrEdit(CreateOrEditOrganizationUserDto input)
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

        [AbpAuthorize(AppPermissions.Pages_OrganizationUsers_Create)]
        protected virtual async Task Create(CreateOrEditOrganizationUserDto input)
        {
            var organizationUser = ObjectMapper.Map<OrganizationUser>(input);

            await _organizationUserRepository.InsertAsync(organizationUser);

        }

        [AbpAuthorize(AppPermissions.Pages_OrganizationUsers_Edit)]
        protected virtual async Task Update(CreateOrEditOrganizationUserDto input)
        {
            var organizationUser = await _organizationUserRepository.FirstOrDefaultAsync((int)input.Id);
            ObjectMapper.Map(input, organizationUser);

        }

        [AbpAuthorize(AppPermissions.Pages_OrganizationUsers_Delete)]
        public virtual async Task Delete(EntityDto input)
        {
            await _organizationUserRepository.DeleteAsync(input.Id);
        }

        public virtual async Task<FileDto> GetOrganizationUsersToExcel(GetAllOrganizationUsersForExcelInput input)
        {

            var filteredOrganizationUsers = _organizationUserRepository.GetAll()
                        .Include(e => e.UserFk)
                        .Include(e => e.OrganizationChartFk)
                        .WhereIf(!string.IsNullOrWhiteSpace(input.Filter), e => false)
                        .WhereIf(!string.IsNullOrWhiteSpace(input.UserNameFilter), e => e.UserFk != null && e.UserFk.Name == input.UserNameFilter)
                        .WhereIf(!string.IsNullOrWhiteSpace(input.OrganizationChartCaptionFilter), e => e.OrganizationChartFk != null && e.OrganizationChartFk.Caption == input.OrganizationChartCaptionFilter);

            var query = (from o in filteredOrganizationUsers
                         join o1 in _lookup_userRepository.GetAll() on o.UserId equals o1.Id into j1
                         from s1 in j1.DefaultIfEmpty()

                         join o2 in _lookup_organizationChartRepository.GetAll() on o.OrganizationChartId equals o2.Id into j2
                         from s2 in j2.DefaultIfEmpty()

                         select new GetOrganizationUserForViewDto()
                         {
                             OrganizationUser = new OrganizationUserDto
                             {
                                 Id = o.Id
                             },
                             UserName = s1 == null || s1.Name == null ? "" : s1.Name.ToString(),
                             OrganizationChartCaption = s2 == null || s2.Caption == null ? "" : s2.Caption.ToString()
                         });

            var organizationUserListDtos = await query.ToListAsync();

            return _organizationUsersExcelExporter.ExportToFile(organizationUserListDtos);
        }

        [AbpAuthorize(AppPermissions.Pages_OrganizationUsers)]
        public async Task<PagedResultDto<OrganizationUserUserLookupTableDto>> GetAllUserForLookupTable(GetAllForLookupTableInput input)
        {
            var query = _lookup_userRepository.GetAll().WhereIf(
                   !string.IsNullOrWhiteSpace(input.Filter),
                  e => e.Name != null && e.Name.Contains(input.Filter)
               );

            var totalCount = await query.CountAsync();

            var userList = await query
                .PageBy(input)
                .ToListAsync();

            var lookupTableDtoList = new List<OrganizationUserUserLookupTableDto>();
            foreach (var user in userList)
            {
                lookupTableDtoList.Add(new OrganizationUserUserLookupTableDto
                {
                    Id = user.Id,
                    DisplayName = user.Name?.ToString()
                });
            }

            return new PagedResultDto<OrganizationUserUserLookupTableDto>(
                totalCount,
                lookupTableDtoList
            );
        }

        [AbpAuthorize(AppPermissions.Pages_OrganizationUsers)]
        public async Task<PagedResultDto<OrganizationUserOrganizationChartLookupTableDto>> GetAllOrganizationChartForLookupTable(GetAllForLookupTableInput input)
        {
            var query = _lookup_organizationChartRepository.GetAll().WhereIf(
                   !string.IsNullOrWhiteSpace(input.Filter),
                  e => e.Caption != null && e.Caption.Contains(input.Filter)
               );

            var totalCount = await query.CountAsync();

            var organizationChartList = await query
                .PageBy(input)
                .ToListAsync();

            var lookupTableDtoList = new List<OrganizationUserOrganizationChartLookupTableDto>();
            foreach (var organizationChart in organizationChartList)
            {
                lookupTableDtoList.Add(new OrganizationUserOrganizationChartLookupTableDto
                {
                    Id = organizationChart.Id,
                    DisplayName = organizationChart.Caption?.ToString()
                });
            }

            return new PagedResultDto<OrganizationUserOrganizationChartLookupTableDto>(
                totalCount,
                lookupTableDtoList
            );
        }

        [AbpAuthorize(AppPermissions.Pages_OrganizationUsers)]

        public async Task<PagedResultDto<LeafUserDto>> GetUsersInSameLeaf(GetLeavesInput input)
        {
            if (AbpSession.UserId == null) throw new UserFriendlyException("User is not logged in!");
            var userposition = await _organizationUserRepository.GetAll().FirstOrDefaultAsync(x => x.UserId == AbpSession.UserId);
            if (userposition == null) throw new UserFriendlyException("کاربر در درخت سطوح دسترسی حضور ندارد");
            var targetChart = await _lookup_organizationChartRepository.GetAsync(userposition.OrganizationChartId);
            if (targetChart == null)
            {
                throw new UserFriendlyException("Organization chart not found.");
            }

            var leafPath = targetChart.LeafPath;

            var usersInSameLeaf = _userRepository.GetAll()
                    .Join(_organizationUserRepository.GetAll(),
                        user => user.Id,
                        orgUser => orgUser.UserId,
                        (user, orgUser) => new { User = user, orgUser.OrganizationChartId })
                    .Join(_lookup_organizationChartRepository.GetAll(),
                        join => join.OrganizationChartId,
                        chart => chart.Id,
                        (join, chart) => new { User = join.User, LeafPath = chart.LeafPath })
                    .Join(_groupMemberRepository.GetAll(),
                        join2 => join2.User.Id,
                        grp => grp.UserId,
                        (join2, grp) => new { grp.MemberPosition, join2.LeafPath, join2.User }
                    )
                    .Where(join => join.LeafPath == leafPath && join.User.Id!=AbpSession.UserId)
                    .WhereIf(!string.IsNullOrWhiteSpace(input.Filter), f => f.User.Surname.Contains(input.Filter) || f.User.Name.Contains(input.Filter))
                    .Select(x => new
                    {
                        x.User,
                        x.LeafPath,
                        x.MemberPosition
                    });
            ;

            //return usersInSameLeaf;
            var outputList = await usersInSameLeaf
                .PageBy(input)
                .ToListAsync();
            var leafUserDtoList = new List<LeafUserDto>();
            foreach (var row in outputList)
            {
                leafUserDtoList.Add(new LeafUserDto()
                {
                    UserId = row.User.Id,
                    UserName = row.User.UserName,
                    FirstName = row.User.Name,
                    LastName = row.User.Surname,
                    TenantId = 1,
                    ProfilePictureId = row.User.ProfilePictureId,
                    LevelType = 0,
                    MemberPosition = row.MemberPosition,

                });


            }
            return new PagedResultDto<LeafUserDto>(
                usersInSameLeaf.Count(),
                leafUserDtoList
            );
        }

        [AbpAuthorize(AppPermissions.Pages_OrganizationUsers)]

        public async Task<PagedResultDto<LeafUserDto>> GetUsersInChildrenLeaves(GetLeavesInput input)
        {
            if (AbpSession.UserId == null) throw new UserFriendlyException("User is not logged in!");
            var userposition = await _organizationUserRepository.GetAll().FirstOrDefaultAsync(x => x.UserId == AbpSession.UserId);
            if (userposition == null) throw new UserFriendlyException("کاربر در درخت سطوح دسترسی حضور ندارد");

            var targetChart = await _lookup_organizationChartRepository.GetAsync(userposition.OrganizationChartId);
            if (targetChart == null)
            {
                throw new UserFriendlyException("Organization chart not found.");
            }

            var leafPath = targetChart.LeafPath;

            var usersInChildrenLeaves = _userRepository.GetAll()
                .Join(_organizationUserRepository.GetAll(),
                    user => user.Id,
                    orgUser => orgUser.UserId,
                    (user, orgUser) => new { User = user, orgUser.OrganizationChartId })
                .Join(_lookup_organizationChartRepository.GetAll(),
                    join => join.OrganizationChartId,
                    chart => chart.Id,
                    (join, chart) => new { User = join.User, LeafPath = chart.LeafPath })
                .Join(_groupMemberRepository.GetAll(),
                    join2 => join2.User.Id,
                    grp => grp.UserId,
                    (join2, grp) => new { grp.MemberPosition, join2.LeafPath, join2.User }
                )
                .Where(join => join.LeafPath.StartsWith(leafPath) && join.LeafPath != leafPath)
                .WhereIf(!string.IsNullOrWhiteSpace(input.Filter), f => f.User.Surname.Contains(input.Filter)|| f.User.Name.Contains(input.Filter))
                .Select(x => new
                {
                    x.User,
                    x.LeafPath,
                    x.MemberPosition
                });

            //return usersInChildrenLeaves;
            var outputList = await usersInChildrenLeaves
                .PageBy(input)
                .ToListAsync();

            var leafUserDtoList = new List<LeafUserDto>();
            foreach (var row in outputList)
            {
                leafUserDtoList.Add(new LeafUserDto()
                {
                    UserId = row.User.Id,
                    UserName = row.User.UserName,
                    FirstName = row.User.Name,
                    LastName = row.User.Surname,
                    TenantId = 1,
                    ProfilePictureId = row.User.ProfilePictureId,
                    LevelType = 2,
                    MemberPosition = row.MemberPosition,

                });


            }
            return new PagedResultDto<LeafUserDto>(
                usersInChildrenLeaves.Count(),
                leafUserDtoList
            );

        }

        [AbpAuthorize(AppPermissions.Pages_OrganizationUsers)]

        public async Task<PagedResultDto<LeafUserDto>> GetUsersInOneLevelHigherParent(GetLeavesInput input)
        {
            if (AbpSession.UserId == null) throw new UserFriendlyException("User is not logged in!");
            var userposition = await _organizationUserRepository.GetAll().FirstOrDefaultAsync(x => x.UserId == AbpSession.UserId);
            if (userposition == null) throw new UserFriendlyException("کاربر در درخت سطوح دسترسی حضور ندارد");

            var targetChart = await _lookup_organizationChartRepository.GetAsync(userposition.OrganizationChartId);
            if (targetChart == null)
            {
                throw new UserFriendlyException("Organization chart not found.");
            }

            var leafPath = targetChart.LeafPath;
            var parentLeafPathWithoutLastPart = GetParentPath(leafPath);

            var usersInOneLevelHigherParent = _userRepository.GetAll()
                .Join(_organizationUserRepository.GetAll(),
                    user => user.Id,
                    orgUser => orgUser.UserId,
                    (user, orgUser) => new { User = user, orgUser.OrganizationChartId })
                .Join(_lookup_organizationChartRepository.GetAll(),
                    join => join.OrganizationChartId,
                    chart => chart.Id,
                    (join, chart) => new { User = join.User, LeafPath = chart.LeafPath })
                .Join(_groupMemberRepository.GetAll(),
                    join2 => join2.User.Id,
                    grp => grp.UserId,
                    (join2, grp) => new { grp.MemberPosition, join2.LeafPath, join2.User }
                )
                .Where(join => join.LeafPath == parentLeafPathWithoutLastPart)
                .WhereIf(!string.IsNullOrWhiteSpace(input.Filter), f => f.User.Surname.Contains(input.Filter) || f.User.Name.Contains(input.Filter))
                .Select(x => new
                {
                    x.User,
                    x.LeafPath,
                    x.MemberPosition
                });

            //return usersInOneLevelHigherParent;
            var outputList = await usersInOneLevelHigherParent
                .PageBy(input)
                .ToListAsync();
            var leafUserDtoList = new List<LeafUserDto>();
            foreach (var row in outputList)
            {
                leafUserDtoList.Add(new LeafUserDto()
                {
                    UserId = row.User.Id,
                    UserName = row.User.UserName,
                    FirstName = row.User.Name,
                    LastName = row.User.Surname,
                    TenantId = 1,
                    ProfilePictureId = row.User.ProfilePictureId,
                    LevelType = 1,
                    MemberPosition = row.MemberPosition,

                });


            }
            return new PagedResultDto<LeafUserDto>(
                usersInOneLevelHigherParent.Count(),
                leafUserDtoList
            );

        }

        private string GetParentPath(string leafPath)
        {
            string[] partsArray = leafPath.Split('\\');
            System.Collections.Generic.List<string> partsList = new List<string>(partsArray);

            if (partsList.Count > 2)
            {
                partsList.RemoveAt(partsList.Count - 1);
                partsList.RemoveAt(partsList.Count - 1);
                return string.Join("\\", partsList) + "\\";

            }
            else
            {
                return "";
            }

        }

        [AbpAuthorize(AppPermissions.Pages_OrganizationUsers)]

        public async Task<PagedResultDto<LeafUserDto>> GetAllUsersForLeaf(GetAllUsersForLeafInput input)
        {
            var chartId = input.OrganizationChartId;
            var filterUserId = false;
            if (input.OrganizationChartId <= 0)
            {
                if (AbpSession.UserId == null) throw new UserFriendlyException("User is not logged in!");
                var userposition = await _organizationUserRepository.GetAll().FirstOrDefaultAsync(x => x.UserId == AbpSession.UserId);
                if (userposition == null) throw new UserFriendlyException("کاربر در درخت سطوح دسترسی حضور ندارد");
                chartId = userposition.OrganizationChartId;
                filterUserId = true;
            }
            var targetChart = await _lookup_organizationChartRepository.GetAsync(chartId);
            if (targetChart == null)
            {
                throw new UserFriendlyException("Organization chart not found.");
            }

            var leafPath = targetChart.LeafPath;

            var usersInSameLeaf = _userRepository.GetAll()
                .Join(_organizationUserRepository.GetAll(),
                    user => user.Id,
                    orgUser => orgUser.UserId,
                    (user, orgUser) => new { User = user, orgUser.OrganizationChartId })
                .Join(_lookup_organizationChartRepository.GetAll(),
                    join => join.OrganizationChartId,
                    chart => chart.Id,
                    (join, chart) => new { User = join.User, LeafPath = chart.LeafPath })
                .Join(_groupMemberRepository.GetAll(),
                    join2 => join2.User.Id,
                    grp => grp.UserId,
                    (join2, grp) => new { grp.MemberPosition, join2.LeafPath, join2.User }
                )
                .Where(join => join.LeafPath == leafPath)
                .WhereIf(filterUserId,x=>x.User.Id!=AbpSession.UserId)
                .WhereIf(!string.IsNullOrWhiteSpace(input.Filter), f => f.User.Surname.Contains(input.Filter) || f.User.Name.Contains(input.Filter))
                .Select(x => new
                {
                    x.User,
                    x.LeafPath,
                    x.MemberPosition,
                    LevelType = 0
                });

            var usersInChildrenLeaves = _userRepository.GetAll()
                .Join(_organizationUserRepository.GetAll(),
                    user => user.Id,
                    orgUser => orgUser.UserId,
                    (user, orgUser) => new { User = user, orgUser.OrganizationChartId })
                .Join(_lookup_organizationChartRepository.GetAll(),
                    join => join.OrganizationChartId,
                    chart => chart.Id,
                    (join, chart) => new { User = join.User, LeafPath = chart.LeafPath })
                .Join(_groupMemberRepository.GetAll(),
                    join2 => join2.User.Id,
                    grp => grp.UserId,
                    (join2, grp) => new { grp.MemberPosition, join2.LeafPath, join2.User }
                )
                .Where(join => join.LeafPath.StartsWith(leafPath) && join.LeafPath != leafPath)
                .WhereIf(!string.IsNullOrWhiteSpace(input.Filter), f => f.User.Surname.Contains(input.Filter) || f.User.Name.Contains(input.Filter))
                .Select(x => new
                {
                    x.User,
                    x.LeafPath,
                    x.MemberPosition,
                    LevelType = 2
                });

            var parentLeafPathWithoutLastPart = GetParentPath(leafPath);
            var usersInOneLevelHigherParent = _userRepository.GetAll()
                .Join(_organizationUserRepository.GetAll(),
                    user => user.Id,
                    orgUser => orgUser.UserId,
                    (user, orgUser) => new { User = user, orgUser.OrganizationChartId })
                .Join(_lookup_organizationChartRepository.GetAll(),
                    join => join.OrganizationChartId,
                    chart => chart.Id,
                    (join, chart) => new { User = join.User, LeafPath = chart.LeafPath })
                .Join(_groupMemberRepository.GetAll(),
                    join2 => join2.User.Id,
                    grp => grp.UserId,
                    (join2, grp) => new { grp.MemberPosition, join2.LeafPath, join2.User }
                )
                .Where(join => join.LeafPath == parentLeafPathWithoutLastPart)
                .WhereIf(!string.IsNullOrWhiteSpace(input.Filter), f => f.User.Surname.Contains(input.Filter) || f.User.Name.Contains(input.Filter))
                .Select(x => new
                {
                    x.User,
                    x.LeafPath,
                    x.MemberPosition,
                    LevelType = 1
                });

            var allUsers = usersInOneLevelHigherParent
                .Union(usersInSameLeaf)
                .Union(usersInChildrenLeaves);
            //return allUsers;
            var outputList = await allUsers
                .PageBy(input)
                .ToListAsync();
            var leafUserDtoList = new List<LeafUserDto>();
            foreach (var row in outputList)
            {
                leafUserDtoList.Add(new LeafUserDto()
                {
                    UserId = row.User.Id,
                    UserName = row.User.UserName,
                    FirstName = row.User.Name,
                    LastName = row.User.Surname,
                    TenantId = 1,
                    ProfilePictureId = row.User.ProfilePictureId,
                    LevelType = row.LevelType,
                    MemberPosition = row.MemberPosition,
                });


            }
            return new PagedResultDto<LeafUserDto>(
                allUsers.Count(),
                leafUserDtoList
            );


        }




    }
}