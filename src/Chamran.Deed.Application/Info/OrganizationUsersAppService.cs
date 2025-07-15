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
        private readonly IRepository<DeedChart> _deedchartRepository;

        public OrganizationUsersAppService(IRepository<OrganizationUser> organizationUserRepository, IOrganizationUsersExcelExporter organizationUsersExcelExporter, IRepository<User, long> lookup_userRepository, IRepository<OrganizationChart, int> lookup_organizationChartRepository, IRepository<User, long> userRepository, IRepository<GroupMember> groupMemberRepository, IRepository<DeedChart> deedchartRepository)
        {
            _organizationUserRepository = organizationUserRepository;
            _organizationUsersExcelExporter = organizationUsersExcelExporter;
            _lookup_userRepository = lookup_userRepository;
            _lookup_organizationChartRepository = lookup_organizationChartRepository;
            _userRepository = userRepository;
            _groupMemberRepository = groupMemberRepository;
            _deedchartRepository = deedchartRepository;
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

                                    join dc in _deedchartRepository.GetAll() on s2.OrganizationId equals dc.OrganizationId into j3
                                    from s3 in j3.DefaultIfEmpty()

                                    select new
                                    {
                                        UserId = s1 == null || s1.Name == null ? 0 : s1.Id,
                                        Id = o.Id,
                                        OrganizationId=s2.OrganizationId,
                                        UserName = s1 == null || s1.Name == null ? "" : s1.Name.ToString(),
                                        OrganizationChartCaption = s2 == null || s2.Caption == null ? "" : s2.Caption.ToString(),
                                        OrganizationChartId = s2 == null || s2.Caption == null ? 0 : s2.Id,
                                        DeedChartCaption=s3.Caption
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
                    OrganizationId=o.OrganizationId,
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
        public virtual async Task CreateGlobal(CreateOrEditGlobalUserDto input)
        {
            //var organizationUser = ObjectMapper.Map<OrganizationUser>(input);
            var query = from x in _lookup_organizationChartRepository.GetAll()
                        where x.OrganizationId == input.OrganizationId && x.ParentId == null
                        select x;
            var headEntity = await query.FirstOrDefaultAsync();
            if (headEntity == null) throw new UserFriendlyException("سرشاخه در سازمان انتخابی ایجاد نشده است");
            var query2 = from x in _lookup_organizationChartRepository.GetAll()
                         where x.ParentId == headEntity.Id
                         select x;
            var targetChart = query2.FirstOrDefault();
            if (targetChart == null) throw new UserFriendlyException("مدیریت در سازمان انتخابی ایجاد نشده است");
            var organizationUser = new OrganizationUser()
            {
                IsGlobal = true,
                OrganizationChartId = targetChart.Id,
                UserId = input.UserId,

            };
            await _organizationUserRepository.InsertAsync(organizationUser);

        }

        protected virtual async Task Create(CreateOrEditOrganizationUserDto input)
        {
            var organizationUser = ObjectMapper.Map<OrganizationUser>(input);
            var chart = await _lookup_organizationChartRepository.GetAsync(input.OrganizationChartId);
            if (!_groupMemberRepository.GetAll().Any(x => x.UserId == input.UserId && x.OrganizationId== chart.OrganizationId))
            {
                throw new UserFriendlyException(
                    "این کاربر در این سازمان عضو نیست لذا نمی تواند در چارت سازمانی قرار گیرد");
            }
            await _organizationUserRepository.InsertAsync(organizationUser);

        }


        [AbpAuthorize(AppPermissions.Pages_OrganizationUsers_Edit)]
        protected virtual async Task Update(CreateOrEditOrganizationUserDto input)
        {
            var organizationUser = await _organizationUserRepository.FirstOrDefaultAsync((int)input.Id);
            var chart = await _lookup_organizationChartRepository.GetAsync(input.OrganizationChartId);
            if (!_groupMemberRepository.GetAll().Any(x => x.UserId == input.UserId && x.OrganizationId == chart.OrganizationId))
            {
                throw new UserFriendlyException(
                    "این کاربر در این سازمان عضو نیست لذا نمی تواند در چارت سازمانی قرار گیرد");
            }
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
        public async Task<PagedResultDto<LeafUserDto>> GetGlobalUserLeaves(GetLeavesInput input)
        {
            if (AbpSession.UserId == null) throw new UserFriendlyException("User is not logged in!");
            //var userposition = await _organizationUserRepository.GetAll().Include(x => x.OrganizationChartFk).FirstOrDefaultAsync(x => x.UserId == AbpSession.UserId && x.OrganizationChartFk.OrganizationId == input.OrganizationId);
            var headerEntity = await _lookup_organizationChartRepository.GetAll()
                .FirstOrDefaultAsync(x => x.OrganizationId == input.OrganizationId && x.ParentId == null);
            if (headerEntity == null)
            {
                throw new UserFriendlyException("سرشاخه در چارت سازمانی ایجاد نشده است");
            }

            var targetChart = _lookup_organizationChartRepository.GetAll()
                .FirstOrDefault(x => x.OrganizationId == input.OrganizationId && x.ParentId == headerEntity.Id);
            if (targetChart == null)
            {
                throw new UserFriendlyException("مدیریت در چارت سازمانی ایجاد نشده است");
            }
            //var targetChart = await _lookup_organizationChartRepository.GetAsync(userposition.Id);
            //if (targetChart == null)
            //{
            //    throw new UserFriendlyException("Organization chart not found.");
            //}
            var leafPath = targetChart.LeafPath;

            var globalUsers = from user in _userRepository.GetAll()
                              join orgUser in _organizationUserRepository.GetAll()
                                  on user.Id equals orgUser.UserId
                              join chart in _lookup_organizationChartRepository.GetAll()
                                  on orgUser.OrganizationChartId equals chart.Id
                              join grp in _groupMemberRepository.GetAll()
                                  on user.Id equals grp.UserId
                              where chart.LeafPath == leafPath && orgUser.IsGlobal
                              where string.IsNullOrWhiteSpace(input.Filter) || user.Surname.Contains(input.Filter) || user.Name.Contains(input.Filter)
                              select new
                              {
                                  User = user,
                                  LeafPath = chart.LeafPath,
                                  MemberPosition = grp.MemberPosition
                              };


            //return usersInSameLeaf;
            var outputList = await globalUsers
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
                globalUsers.Count(),
                leafUserDtoList
            );
        }

        [AbpAuthorize(AppPermissions.Pages_OrganizationUsers)]
        public async Task<PagedResultDto<LeafUserDto>> GetUsersInSameLeaf(GetLeavesInput input)
        {
            if (AbpSession.UserId == null) throw new UserFriendlyException("User is not logged in!");
            var userposition = await _organizationUserRepository.GetAll().Include(x => x.OrganizationChartFk).FirstOrDefaultAsync(x => x.UserId == AbpSession.UserId && x.OrganizationChartFk.OrganizationId == input.OrganizationId);
            if (userposition == null) throw new UserFriendlyException("کاربر در درخت سطوح دسترسی حضور ندارد");
            var targetChart = await _lookup_organizationChartRepository.GetAsync(userposition.OrganizationChartId);
            if (targetChart == null)
            {
                throw new UserFriendlyException("Organization chart not found.");
            }

            var leafPath = targetChart.LeafPath;

            var usersInSameLeaf =
                (from user in _userRepository.GetAll()
                 join orgUser in _organizationUserRepository.GetAll()
                     on user.Id equals orgUser.UserId
                 join chart in _lookup_organizationChartRepository.GetAll()
                     on orgUser.OrganizationChartId equals chart.Id
                 join grp in _groupMemberRepository.GetAll()
                     on user.Id equals grp.UserId
                 where chart.LeafPath == leafPath && user.Id != AbpSession.UserId
                 where !orgUser.IsGlobal
                 where string.IsNullOrWhiteSpace(input.Filter) ||
                       user.Surname.Contains(input.Filter) || user.Name.Contains(input.Filter)
                 select new
                 {
                     User = user,
                     LeafPath = chart.LeafPath,
                     MemberPosition = grp.MemberPosition
                 });

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
            var userposition = await _organizationUserRepository.GetAll().Include(x => x.OrganizationChartFk).FirstOrDefaultAsync(x => x.UserId == AbpSession.UserId && x.OrganizationChartFk.OrganizationId == input.OrganizationId);
            if (userposition == null) throw new UserFriendlyException("کاربر در درخت سطوح دسترسی حضور ندارد");

            var targetChart = await _lookup_organizationChartRepository.GetAsync(userposition.OrganizationChartId);
            if (targetChart == null)
            {
                throw new UserFriendlyException("Organization chart not found.");
            }

            var leafPath = targetChart.LeafPath;
            var usersInChildrenLeaves = (from user in _userRepository.GetAll()
                                         join orgUser in _organizationUserRepository.GetAll() on user.Id equals orgUser.UserId
                                         join chart in _lookup_organizationChartRepository.GetAll() on orgUser.OrganizationChartId equals chart
                                             .Id
                                         join grp in _groupMemberRepository.GetAll() on user.Id equals grp.UserId
                                         where chart.LeafPath.StartsWith(leafPath) && chart.LeafPath != leafPath
                                         where !orgUser.IsGlobal
                                         where string.IsNullOrWhiteSpace(input.Filter) ||
                                               user.Surname.Contains(input.Filter) || user.Name.Contains(input.Filter)
                                         select new
                                         {
                                             user,
                                             chart.LeafPath,
                                             grp.MemberPosition
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
                    UserId = row.user.Id,
                    UserName = row.user.UserName,
                    FirstName = row.user.Name,
                    LastName = row.user.Surname,
                    TenantId = 1,
                    ProfilePictureId = row.user.ProfilePictureId,
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
            var userposition = await _organizationUserRepository.GetAll().Include(x => x.OrganizationChartFk).FirstOrDefaultAsync(x => x.UserId == AbpSession.UserId && x.OrganizationChartFk.OrganizationId == input.OrganizationId);
            if (userposition == null) throw new UserFriendlyException("کاربر در درخت سطوح دسترسی حضور ندارد");

            var targetChart = await _lookup_organizationChartRepository.GetAsync(userposition.OrganizationChartId);
            if (targetChart == null)
            {
                throw new UserFriendlyException("Organization chart not found.");
            }

            var leafPath = targetChart.LeafPath;
            var parentLeafPathWithoutLastPart = GetParentPath(leafPath);

            var usersInOneLevelHigherParent = (from user in _userRepository.GetAll()
                join orgUser in _organizationUserRepository.GetAll() on user.Id equals orgUser.UserId
                join chart in _lookup_organizationChartRepository.GetAll() on orgUser.OrganizationChartId equals chart
                    .Id
                join grp in _groupMemberRepository.GetAll() on user.Id equals grp.UserId
                where chart.LeafPath == parentLeafPathWithoutLastPart
                                               where !orgUser.IsGlobal
                where string.IsNullOrWhiteSpace(input.Filter) ||
                      user.Surname.Contains(input.Filter) || user.Name.Contains(input.Filter)
                select new
                {
                    user,
                    chart.LeafPath,
                    grp.MemberPosition
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
                    UserId = row.user.Id,
                    UserName = row.user.UserName,
                    FirstName = row.user.Name,
                    LastName = row.user.Surname,
                    TenantId = 1,
                    ProfilePictureId = row.user.ProfilePictureId,
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
            if (AbpSession.UserId == null)
                throw new UserFriendlyException("User is not logged in!");

            var currentUserId = AbpSession.UserId.Value;
            var chartId = input.OrganizationChartId;
            var filterUserId = false;

            if (chartId <= 0)
            {
                var userPosition = await _organizationUserRepository.GetAll()
                    .Include(x => x.OrganizationChartFk)
                    .FirstOrDefaultAsync(x => x.UserId == currentUserId && x.OrganizationChartFk.OrganizationId == input.OrganizationId);

                if (userPosition == null)
                    throw new UserFriendlyException("کاربر در درخت سطوح دسترسی حضور ندارد");

                chartId = userPosition.OrganizationChartId;
                filterUserId = true;
            }

            var targetChart = await _lookup_organizationChartRepository.GetAsync(chartId);
            if (targetChart == null)
                throw new UserFriendlyException("Organization chart not found.");

            var leafPath = targetChart.LeafPath;
            var parentLeafPath = GetParentPath(leafPath);

            var query = from user in _userRepository.GetAll()
                        join orgUser in _organizationUserRepository.GetAll() on user.Id equals orgUser.UserId
                        join chart in _lookup_organizationChartRepository.GetAll() on orgUser.OrganizationChartId equals chart.Id
                        join grp in _groupMemberRepository.GetAll() on user.Id equals grp.UserId
                        where !orgUser.IsGlobal
                        where chart.LeafPath == leafPath ||
                              chart.LeafPath.StartsWith(leafPath + ".") ||
                              chart.LeafPath == parentLeafPath
                        select new
                        {
                            User = user,
                            OrgUser = orgUser,
                            chart.LeafPath,
                            grp.MemberPosition,
                            LevelType = chart.LeafPath == leafPath ? 0 : (chart.LeafPath == parentLeafPath ? 1 : 2)
                        };

            if (filterUserId)
                query = query.Where(x => x.User.Id != currentUserId);

            if (!string.IsNullOrWhiteSpace(input.Filter))
                query = query.Where(x => x.User.Surname.Contains(input.Filter) || x.User.Name.Contains(input.Filter));

            var totalCount = await query.CountAsync();

            var result = await query
                .OrderBy(input.Sorting ?? "User.Name asc")
                .PageBy(input)
                .Select(x => new LeafUserDto
                {
                    UserId = x.User.Id,
                    UserName = x.User.UserName,
                    FirstName = x.User.Name,
                    LastName = x.User.Surname,
                    ProfilePictureId = x.User.ProfilePictureId,
                    LevelType = x.LevelType,
                    MemberPosition = x.MemberPosition,
                    TenantId = 1,
                    OrganizationUserId = x.OrgUser.Id
                })
                .ToListAsync();

            return new PagedResultDto<LeafUserDto>(totalCount, result);
        }

    //    public async Task<PagedResultDto<LeafUserDto>> GetAllUsersForLeaf(GetAllUsersForLeafInput input)
    //    {
    //        var chartId = input.OrganizationChartId;
    //        var filterUserId = false;
    //        if (input.OrganizationChartId <= 0)
    //        {
    //            if (AbpSession.UserId == null) throw new UserFriendlyException("User is not logged in!");
    //            var userposition = await _organizationUserRepository.GetAll().Include(x => x.OrganizationChartFk).FirstOrDefaultAsync(x => x.UserId == AbpSession.UserId && x.OrganizationChartFk.OrganizationId == input.OrganizationId);
    //            //var userposition = await _organizationUserRepository.GetAll().Include(x => x.OrganizationChartFk).FirstOrDefaultAsync(x => x.UserId == AbpSession.UserId && x.OrganizationChartFk.OrganizationId == input.OrganizationId);
    //            if (userposition == null) throw new UserFriendlyException("کاربر در درخت سطوح دسترسی حضور ندارد");
    //            chartId = userposition.OrganizationChartId;
    //            filterUserId = true;
    //        }
    //        var targetChart = await _lookup_organizationChartRepository.GetAsync(chartId);
    //        if (targetChart == null)
    //        {
    //            throw new UserFriendlyException("Organization chart not found.");
    //        }

    //        var leafPath = targetChart.LeafPath;

    //        //LAMBDA EDITION
    //        //var usersInSameLeaf = _userRepository.GetAll()
    //        //    .Join(_organizationUserRepository.GetAll(),
    //        //        user => user.Id,
    //        //        orgUser => orgUser.UserId,
    //        //        (user, orgUser) => new { User = user, orgUser.OrganizationChartId })
    //        //    .Join(_lookup_organizationChartRepository.GetAll(),
    //        //        join => join.OrganizationChartId,
    //        //        chart => chart.Id,
    //        //        (join, chart) => new { User = join.User, LeafPath = chart.LeafPath })
    //        //    .Join(_groupMemberRepository.GetAll(),
    //        //        join2 => join2.User.Id,
    //        //        grp => grp.UserId,
    //        //        (join2, grp) => new { grp.MemberPosition, join2.LeafPath, join2.User }
    //        //    )
    //        //    .Where(join => join.LeafPath == leafPath)
    //        //    .WhereIf(filterUserId,x=>x.User.Id!=AbpSession.UserId)
    //        //    .WhereIf(!string.IsNullOrWhiteSpace(input.Filter), f => f.User.Surname.Contains(input.Filter) || f.User.Name.Contains(input.Filter))
    //        //    .Select(x => new
    //        //    {
    //        //        x.User,
    //        //        x.LeafPath,
    //        //        x.MemberPosition,
    //        //        LevelType = 0,

    //        //    });


    //        //var usersInChildrenLeaves = _userRepository.GetAll()
    //        //    .Join(_organizationUserRepository.GetAll(),
    //        //        user => user.Id,
    //        //        orgUser => orgUser.UserId,
    //        //        (user, orgUser) => new { User = user, orgUser.OrganizationChartId })
    //        //    .Join(_lookup_organizationChartRepository.GetAll(),
    //        //        join => join.OrganizationChartId,
    //        //        chart => chart.Id,
    //        //        (join, chart) => new { User = join.User, LeafPath = chart.LeafPath })
    //        //    .Join(_groupMemberRepository.GetAll(),
    //        //        join2 => join2.User.Id,
    //        //        grp => grp.UserId,
    //        //        (join2, grp) => new { grp.MemberPosition, join2.LeafPath, join2.User }
    //        //    )
    //        //    .Where(join => join.LeafPath.StartsWith(leafPath) && join.LeafPath != leafPath)
    //        //    .WhereIf(!string.IsNullOrWhiteSpace(input.Filter), f => f.User.Surname.Contains(input.Filter) || f.User.Name.Contains(input.Filter))
    //        //    .Select(x => new
    //        //    {
    //        //        x.User,
    //        //        x.LeafPath,
    //        //        x.MemberPosition,
    //        //        LevelType = 2
    //        //    });

    //        //var parentLeafPathWithoutLastPart = GetParentPath(leafPath);
    //        //var usersInOneLevelHigherParent = _userRepository.GetAll()
    //        //    .Join(_organizationUserRepository.GetAll(),
    //        //        user => user.Id,
    //        //        orgUser => orgUser.UserId,
    //        //        (user, orgUser) => new { User = user, orgUser.OrganizationChartId })
    //        //    .Join(_lookup_organizationChartRepository.GetAll(),
    //        //        join => join.OrganizationChartId,
    //        //        chart => chart.Id,
    //        //        (join, chart) => new { User = join.User, LeafPath = chart.LeafPath })
    //        //    .Join(_groupMemberRepository.GetAll(),
    //        //        join2 => join2.User.Id,
    //        //        grp => grp.UserId,
    //        //        (join2, grp) => new { grp.MemberPosition, join2.LeafPath, join2.User }
    //        //    )
    //        //    .Where(join => join.LeafPath == parentLeafPathWithoutLastPart)
    //        //    .WhereIf(!string.IsNullOrWhiteSpace(input.Filter), f => f.User.Surname.Contains(input.Filter) || f.User.Name.Contains(input.Filter))
    //        //    .Select(x => new
    //        //    {
    //        //        x.User,
    //        //        x.LeafPath,
    //        //        x.MemberPosition,
    //        //        LevelType = 1
    //        //    });

    //        //var allUsers = usersInOneLevelHigherParent
    //        //    .Union(usersInSameLeaf)
    //        //    .Union(usersInChildrenLeaves);

    //        var usersInSameLeaf = from user in _userRepository.GetAll()
    //                              join orgUser in _organizationUserRepository.GetAll()
    //                              on user.Id equals orgUser.UserId
    //                              join chart in _lookup_organizationChartRepository.GetAll()
    //                              on orgUser.OrganizationChartId equals chart.Id
    //                              join grp in _groupMemberRepository.GetAll()
    //                              on user.Id equals grp.UserId
    //                              where chart.LeafPath == leafPath
    //                              where !orgUser.IsGlobal
    //                              where !filterUserId || user.Id != AbpSession.UserId
    //                              where string.IsNullOrWhiteSpace(input.Filter) || user.Surname.Contains(input.Filter) || user.Name.Contains(input.Filter)
    //                              select new
    //                              {
    //                                  User = user,
    //                                  LeafPath = chart.LeafPath,
    //                                  MemberPosition = grp.MemberPosition,
    //                                  OrgUser = orgUser,
    //                                  LevelType = 0
    //                              };

    //        var usersInChildrenLeaves = from user in _userRepository.GetAll()
    //                                    join orgUser in _organizationUserRepository.GetAll()
    //                                    on user.Id equals orgUser.UserId
    //                                    join chart in _lookup_organizationChartRepository.GetAll()
    //                                    on orgUser.OrganizationChartId equals chart.Id
    //                                    join grp in _groupMemberRepository.GetAll()
    //                                    on user.Id equals grp.UserId
    //                                    where !orgUser.IsGlobal
    //                                    where chart.LeafPath.StartsWith(leafPath) && chart.LeafPath != leafPath
    //                                    where string.IsNullOrWhiteSpace(input.Filter) || user.Surname.Contains(input.Filter) || user.Name.Contains(input.Filter)
    //                                    select new
    //                                    {
    //                                        User = user,
    //                                        LeafPath = chart.LeafPath,
    //                                        MemberPosition = grp.MemberPosition,
    //                                        OrgUser = orgUser,
    //                                        LevelType = 2
    //                                    };

    //        var parentLeafPathWithoutLastPart = GetParentPath(leafPath);

    //        var usersInOneLevelHigherParent = from user in _userRepository.GetAll()
    //                                          join orgUser in _organizationUserRepository.GetAll()
    //                                          on user.Id equals orgUser.UserId
    //                                          join chart in _lookup_organizationChartRepository.GetAll()
    //                                          on orgUser.OrganizationChartId equals chart.Id
    //                                          join grp in _groupMemberRepository.GetAll()
    //                                          on user.Id equals grp.UserId
    //                                          where !orgUser.IsGlobal
    //                                          where chart.LeafPath == parentLeafPathWithoutLastPart
    //                                          where string.IsNullOrWhiteSpace(input.Filter) || user.Surname.Contains(input.Filter) || user.Name.Contains(input.Filter)
    //                                          select new
    //                                          {
    //                                              User = user,
    //                                              LeafPath = chart.LeafPath,
    //                                              MemberPosition = grp.MemberPosition,
    //                                              OrgUser = orgUser,
    //                                              LevelType = 1
    //                                          };

    //        var allUsers = usersInOneLevelHigherParent
    //            .Union(usersInSameLeaf)
    //            .Union(usersInChildrenLeaves);

    //        //return allUsers;
    //        var outputList = await allUsers
    //.PageBy(input)
    //.ToListAsync();
    //        var leafUserDtoList = new List<LeafUserDto>();
    //        foreach (var row in outputList)
    //        {
    //            leafUserDtoList.Add(new LeafUserDto()
    //            {
    //                UserId = row.User.Id,
    //                UserName = row.User.UserName,
    //                FirstName = row.User.Name,
    //                LastName = row.User.Surname,
    //                TenantId = 1,
    //                ProfilePictureId = row.User.ProfilePictureId,
    //                LevelType = row.LevelType,
    //                MemberPosition = row.MemberPosition,
    //                OrganizationUserId = row.OrgUser.Id
    //            });


    //        }
    //        return new PagedResultDto<LeafUserDto>(
    //            allUsers.Count(),
    //            leafUserDtoList
    //        );


    //    }

        [AbpAuthorize(AppPermissions.Pages_OrganizationUsers)]
        public async Task<PagedResultDto<SameLeafDto>> GetAllUsersInLeaf(GetAllUsersInLeafInput input)
        {
            if (input.OrganizationChartId <= 0)
                throw new UserFriendlyException("شناسه شاخه می‌بایست بزرگتر از صفر باشد");

            var query = _organizationUserRepository.GetAll()
                .Include(x => x.OrganizationChartFk)
                .Include(x => x.UserFk)
                .Where(x =>
                    x.OrganizationChartId == input.OrganizationChartId &&
                    x.OrganizationChartFk.OrganizationId == input.OrganizationId &&
                    !x.IsGlobal);

            // اعمال فیلتر اختیاری روی نام
            if (!string.IsNullOrWhiteSpace(input.Filter))
            {
                query = query.Where(x =>
                    x.UserFk.Name.Contains(input.Filter) || x.UserFk.Surname.Contains(input.Filter));
            }

            // Join با MemberPosition
            var joinedQuery = from x in query
                join y in _groupMemberRepository.GetAll()
                        .Where(g => g.OrganizationId == input.OrganizationId)
                    on x.UserId equals y.UserId into joiner
                from y in joiner.DefaultIfEmpty()
                select new SameLeafDto
                {
                    OrganizationUserId = x.Id,
                    FirstName = x.UserFk.Name,
                    LastName = x.UserFk.Surname,
                    UserName = x.UserFk.UserName,
                    MemberPosition = y.MemberPosition,
                    UserId = x.UserId
                };

            // محاسبه تعداد کل
            var totalCount = await joinedQuery.CountAsync();

            // ترتیب (با fall-back به Id)
            var sorted = joinedQuery.OrderBy(input.Sorting ?? "OrganizationUserId asc");

            // صفحه‌بندی و اجرا
            var result = await sorted.PageBy(input).ToListAsync();

            return new PagedResultDto<SameLeafDto>(totalCount, result);
        }

        //public Task<PagedResultDto<SameLeafDto>> GetAllUsersInLeaf(GetAllUsersInLeafInput input)
        //{
        //    if (input.OrganizationChartId <= 0)
        //    {
        //        throw new UserFriendlyException("شناسه شاخه می بایست بزرگتر از صفر باشد");
        //    }

        //    var users = _organizationUserRepository.GetAll().Include(x => x.OrganizationChartFk).Include(x => x.UserFk)
        //        .Where(x => x.OrganizationChartId == input.OrganizationChartId && x.OrganizationChartFk.OrganizationId==input.OrganizationId && !x.IsGlobal);
        //    var joindUsers = from x in users
        //                     join y in _groupMemberRepository.GetAll().Where(x=>x.OrganizationId==input.OrganizationId) on x.UserId equals y.UserId into joiner
        //                     from y in joiner.DefaultIfEmpty()
        //                     select new
        //                     {
        //                         x.Id,
        //                         x.UserFk.Name,
        //                         x.UserFk.Surname,
        //                         x.UserFk.UserName,
        //                         MemberPos = (int?)y.MemberPos,
        //                         y.MemberPosition,
        //                         x.UserId,

        //                     };

        //    var pagedAndFilteredOrganizationUsers = joindUsers
        //        .OrderBy(input.Sorting ?? "id asc")
        //        .PageBy(input);
        //    var ls = new List<SameLeafDto>();
        //    foreach (var row in pagedAndFilteredOrganizationUsers)
        //    {
        //        ls.Add(new SameLeafDto()
        //        {
        //            OrganizationUserId=row.Id,
        //            FirstName = row.Name,
        //            LastName = row.Surname,
        //            MemberPosition = row.MemberPosition,
        //            UserId = row.UserId,
        //            UserName = row.UserName
        //        });
        //    }


        //    return Task.FromResult(new PagedResultDto<SameLeafDto>(
        //        joindUsers.Count(),
        //        ls
        //    ));
        //}
    }
}