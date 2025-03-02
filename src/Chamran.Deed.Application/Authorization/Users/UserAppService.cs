using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Configuration;
using Abp.Authorization;
using Abp.Authorization.Roles;
using Abp.Authorization.Users;
using Abp.Domain.Repositories;
using Abp.Extensions;
using Abp.Linq.Extensions;
using Abp.Notifications;
using Abp.Organizations;
using Abp.Runtime.Session;
using Abp.UI;
using Abp.Zero.Configuration;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Chamran.Deed.Authorization.Permissions;
using Chamran.Deed.Authorization.Permissions.Dto;
using Chamran.Deed.Authorization.Roles;
using Chamran.Deed.Authorization.Users.Dto;
using Chamran.Deed.Authorization.Users.Exporting;
using Chamran.Deed.Dto;
using Chamran.Deed.Info;
using Chamran.Deed.Info.Dtos;
using Chamran.Deed.Net.Emailing;
using Chamran.Deed.Notifications;
using Chamran.Deed.Url;
using Chamran.Deed.Organizations.Dto;
using Chamran.Deed.People;
using Chamran.Deed.People.Dtos;
using Chamran.Deed.Authorization.Delegation;
using Chamran.Deed.Authorization.Users.Delegation.Dto;
using NUglify.Helpers;
using Abp.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using Chamran.Deed.EntityFrameworkCore;
using Chamran.Deed.Authorization.Accounts.Dto;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Crypto;

namespace Chamran.Deed.Authorization.Users
{
    [AbpAuthorize(AppPermissions.Pages_Administration_Users)]
    public class UserAppService : DeedAppServiceBase, IUserAppService
    {
        public IAppUrlService AppUrlService { get; set; }

        private readonly RoleManager _roleManager;
        private readonly IUserEmailer _userEmailer;
        private readonly IUserListExcelExporter _userListExcelExporter;
        private readonly INotificationSubscriptionManager _notificationSubscriptionManager;
        private readonly IAppNotifier _appNotifier;
        private readonly IRepository<RolePermissionSetting, long> _rolePermissionRepository;
        private readonly IRepository<UserPermissionSetting, long> _userPermissionRepository;
        private readonly IRepository<UserLoginAttempt, long> _userLoginAttemptRepository;
        private readonly IRepository<UserRole, long> _userRoleRepository;
        private readonly IRepository<Role> _roleRepository;

        private readonly IUserPolicy _userPolicy;
        private readonly IEnumerable<IPasswordValidator<User>> _passwordValidators;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly IRepository<OrganizationUnit, long> _organizationUnitRepository;
        private readonly IRoleManagementConfig _roleManagementConfig;
        private readonly UserManager _userManager;
        private readonly IRepository<UserOrganizationUnit, long> _userOrganizationUnitRepository;
        private readonly IRepository<OrganizationUnitRole, long> _organizationUnitRoleRepository;
        private readonly IOptions<UserOptions> _userOptions;
        private readonly IEmailSettingsChecker _emailSettingsChecker;
        private readonly IRepository<User, long> _userRepository;
        private readonly IRepository<Organization, int> _lookup_organizationRepository;
        private readonly IRepository<Seen, int> _seenRepository;
        private readonly IRepository<Post, int> _postRepository;
        private readonly IRepository<PostLike, int> _postLikeRepository;
        private readonly IRepository<CommentLike, int> _commentLikeRepository;
        private readonly IRepository<Comment, int> _commentRepository;
        private readonly IDbContextProvider<DeedDbContext> _dbContextProvider;

        private readonly IRepository<GroupMember> _groupMemberRepository;
        private readonly IOrganizationsAppService _organizationAppService;
        private readonly IGroupMembersAppService _groupMembersAppService;
        private readonly IOrganizationChartsAppService _organizationChartsAppService;
        private readonly IDeedChartsAppService _deedChartsAppService;
        private readonly IOrganizationUsersAppService _organizationUsersAppService;
        public UserAppService(
            RoleManager roleManager,
            IUserEmailer userEmailer,
            IUserListExcelExporter userListExcelExporter,
            INotificationSubscriptionManager notificationSubscriptionManager,
            IAppNotifier appNotifier,
            IRepository<RolePermissionSetting, long> rolePermissionRepository,
            IRepository<UserPermissionSetting, long> userPermissionRepository,
            IRepository<UserRole, long> userRoleRepository,
            IRepository<Role> roleRepository,
            IUserPolicy userPolicy,
            IEnumerable<IPasswordValidator<User>> passwordValidators,
            IPasswordHasher<User> passwordHasher,
            IRepository<OrganizationUnit, long> organizationUnitRepository,
            IRoleManagementConfig roleManagementConfig,
            UserManager userManager,
            IRepository<UserOrganizationUnit, long> userOrganizationUnitRepository,
            IRepository<OrganizationUnitRole, long> organizationUnitRoleRepository,
            IOptions<UserOptions> userOptions, IEmailSettingsChecker emailSettingsChecker,
            IRepository<User, long> userRepository, IRepository<Organization, int> lookupOrganizationRepository, IRepository<GroupMember> groupMemberRepository, IOrganizationsAppService organizationsAppService, IGroupMembersAppService groupMembersAppService, IOrganizationChartsAppService organizationChartsAppService, IOrganizationUsersAppService organizationUsersAppService, IDeedChartsAppService deedChartsAppService, IRepository<UserLoginAttempt, long> userLoginAttemptRepository, IRepository<Seen, int> seenRepository, IRepository<Post, int> postRepository, IRepository<PostLike, int> postLikeRepository, IRepository<CommentLike, int> commentLikeRepository, IRepository<Comment, int> commentRepository, IDbContextProvider<DeedDbContext> dbContextProvider)
        {
            _roleManager = roleManager;
            _userEmailer = userEmailer;
            _userListExcelExporter = userListExcelExporter;
            _notificationSubscriptionManager = notificationSubscriptionManager;
            _appNotifier = appNotifier;
            _rolePermissionRepository = rolePermissionRepository;
            _userPermissionRepository = userPermissionRepository;
            _userRoleRepository = userRoleRepository;
            _userPolicy = userPolicy;
            _passwordValidators = passwordValidators;
            _passwordHasher = passwordHasher;
            _organizationUnitRepository = organizationUnitRepository;
            _roleManagementConfig = roleManagementConfig;
            _userManager = userManager;
            _userOrganizationUnitRepository = userOrganizationUnitRepository;
            _organizationUnitRoleRepository = organizationUnitRoleRepository;
            _userOptions = userOptions;
            _emailSettingsChecker = emailSettingsChecker;
            _userRepository = userRepository;
            _lookup_organizationRepository = lookupOrganizationRepository;
            _groupMemberRepository = groupMemberRepository;
            _roleRepository = roleRepository;
            _organizationAppService = organizationsAppService;
            _groupMembersAppService = groupMembersAppService;
            _organizationChartsAppService = organizationChartsAppService;
            _organizationUsersAppService = organizationUsersAppService;
            _deedChartsAppService = deedChartsAppService;
            _userLoginAttemptRepository = userLoginAttemptRepository;
            _seenRepository = seenRepository;
            _postRepository = postRepository;
            _postLikeRepository = postLikeRepository;
            _commentLikeRepository = commentLikeRepository;
            _commentRepository = commentRepository;
            _dbContextProvider = dbContextProvider;
            AppUrlService = NullAppUrlService.Instance;
        }

        [HttpPost]
        public async Task<PagedResultDto<UserListDto>> GetUsers(GetUsersInput input)
        {
            var query = GetUsersFilteredQuery(input);

            var userCount = await query.CountAsync();

            var users = await query
                .OrderBy(input.Sorting)
                .PageBy(input)
                .ToListAsync();

            var userListDtos = ObjectMapper.Map<List<UserListDto>>(users);
            await FillRoleNames(userListDtos);

            return new PagedResultDto<UserListDto>(
                userCount,
                userListDtos
            );
        }
        [HttpGet]
        public async Task<PagedResultDto<UserListDto>> GetListOfUsers(GetUsersInput input)
        {
            if (AbpSession.UserId == null)
                throw new UserFriendlyException("User Must be Logged in!");

            var parameters = new[]
            {
        new SqlParameter("@NationalIdFilter", input.NationalIdFilter ?? (object)DBNull.Value),
        new SqlParameter("@NameFilter", string.IsNullOrWhiteSpace(input.NameFilter) ? (object)DBNull.Value : (object)input.NameFilter.Replace('ي', 'ی').Replace('ك', 'ک')),
        new SqlParameter("@SurNameFilter", string.IsNullOrWhiteSpace(input.SurNameFilter) ? (object)DBNull.Value : (object)input.SurNameFilter.Replace('ي', 'ی').Replace('ك', 'ک')),
        new SqlParameter("@UserNameFilter", string.IsNullOrWhiteSpace(input.UserNameFilter) ? (object)DBNull.Value : (object)input.UserNameFilter.Replace('ي', 'ی').Replace('ك', 'ک')),
        new SqlParameter("@PhoneNumberFilter", string.IsNullOrWhiteSpace(input.PhoneNumberFilter) ? (object)DBNull.Value : (object)input.PhoneNumberFilter.Replace('ي', 'ی').Replace('ك', 'ک')),
        new SqlParameter("@IsActiveFilter", input.IsActiveFilter ?? (object)DBNull.Value),
        new SqlParameter("@FromCreationDate", input.FromCreationDate ?? (object)DBNull.Value),
        new SqlParameter("@ToCreationDate", input.ToCreationDate ?? (object)DBNull.Value),
        new SqlParameter("@FromLastLoginDate", input.FromLastLoginDate ?? (object)DBNull.Value),
        new SqlParameter("@ToLastLoginDate", input.ToLastLoginDate ?? (object)DBNull.Value),
        new SqlParameter("@Role", input.Role ?? (object)DBNull.Value),
        new SqlParameter("@OnlyLockedUsers", input.OnlyLockedUsers),
        new SqlParameter("@Filter", string.IsNullOrWhiteSpace(input.Filter) ? (object)DBNull.Value : (object)input.Filter),
        new SqlParameter("@Permissions", (object)DBNull.Value),
        new SqlParameter("@OrganizationId", input.OrganizationId ?? (object)DBNull.Value),
        new SqlParameter("@Sorting", input.Sorting ?? "CreationTime DESC"),
        new SqlParameter("@MaxResultCount", input.MaxResultCount),
        new SqlParameter("@SkipCount", input.SkipCount),
        new SqlParameter("@UserType", input.UserType ?? (object)DBNull.Value)
    };

            var dbContext = await _dbContextProvider.GetDbContextAsync();

            var queryResult = await dbContext.Set<GetListOfUsers>()
                .FromSqlRaw("EXEC GetListOfUsers @NationalIdFilter, @NameFilter, @SurNameFilter, @UserNameFilter, @PhoneNumberFilter, @IsActiveFilter, @FromCreationDate, @ToCreationDate, @FromLastLoginDate, @ToLastLoginDate, @Role, @OnlyLockedUsers, @Filter, @Permissions, @OrganizationId, @Sorting, @MaxResultCount, @SkipCount, @UserType", parameters)
                .ToListAsync();

            // ✅ Debug Output (Check SQL output before processing)
            foreach (var user in queryResult)
            {
                Console.WriteLine($"[SQL Debug] UserId: {user.Id}, RoleId: {user.AssignedRoleId}, RoleName: {user.AssignedRoleName}");
            }

            var result = queryResult
                .GroupBy(u => u.Id) // Groups by user Id
                .Select(groupedUsers =>
                {
                    var user = groupedUsers.OrderByDescending(u => u.CreationTime).First(); // Select the most recent record
                    return new UserListDto
                    {
                        Id = user.Id, // ✅ Ensured correct selection
                        NationalId = user.NationalId,
                        Name = user.Name,
                        Surname = user.Surname,
                        UserName = user.UserName,
                        EmailAddress = user.EmailAddress,
                        PhoneNumber = user.PhoneNumber,
                        ProfilePictureId = user.ProfilePictureId,
                        IsActive = user.IsActive,
                        CreationTime = user.CreationTime,
                        LastLoginAttemptTime = user.LastLoginAttemptTime,
                        LockoutEndDateUtc = user.LockoutEndDate,
                        UserType = (AccountUserType)user.UserType,

                        // ✅ Role assignment remains exactly the same
                        Roles = groupedUsers.Select(user => new UserListRoleDto
                        {
                            RoleId = user.AssignedRoleId ?? 3,
                            RoleName = string.IsNullOrEmpty(user.AssignedRoleName) ? "User" : user.AssignedRoleName
                        }).Distinct().ToList()
                    };
                })
                .ToList();

            return new PagedResultDto<UserListDto>(queryResult.Count, result);
        }


        public async Task<PagedResultDto<LoginInfosDto>> GetUserLoginAttempts(GetUserInformationDto input)
        {
            var query = _userLoginAttemptRepository.GetAll().Where(x => x.UserId == input.UserId);
            var pagedSorted = await query
                .OrderBy(input.Sorting ?? "CreationTime Desc")
                .PageBy(input)
                .ToListAsync();
            var totalCount = await query.CountAsync();
            var data = new List<LoginInfosDto>();

            foreach (var row in pagedSorted)
            {
                data.Add(new LoginInfosDto()
                {
                    CreationTime = row.CreationTime,
                    BrowserInfo = row.BrowserInfo,
                    ClientIpAddress = row.ClientIpAddress,
                });
            }
            return new PagedResultDto<LoginInfosDto>(
                totalCount,
                data
            );
        }

        public async Task<PagedResultDto<BriefSeenPostsDto>> GetUserSeenPosts(GetUserInformationDto input)
        {
            var query = from s in _seenRepository.GetAll()
                        where s.UserId == input.UserId
                        join p in _postRepository.GetAll() on s.PostId equals p.Id
                        select new
                        {
                            s.SeenTime,
                            s.PostId,
                            p.PostTitle,
                            p.PostFile,
                            PostTime = p.CreationTime
                        };
            var pagedSorted = await query
            .OrderBy(input.Sorting ?? "SeenTime Desc")
            .PageBy(input)
            .ToListAsync();
            var totalCount = await query.CountAsync();
            var data = new List<BriefSeenPostsDto>();

            foreach (var row in pagedSorted)
            {
                data.Add(new BriefSeenPostsDto()
                {
                    SeenTime = row.SeenTime,
                    PostId = row.PostId,
                    PostTitle = row.PostTitle,
                    PostFile = row.PostFile,
                    PostTime = row.PostTime

                });
            }
            return new PagedResultDto<BriefSeenPostsDto>(
                totalCount,
                data
            );
        }

        public async Task<PagedResultDto<BriefCommentsDto>> GetUserComments(GetUserInformationDto input)
        {
            var query = from c in _commentRepository.GetAll()
                        where c.UserId == input.UserId
                        join p in _postRepository.GetAll() on c.PostId equals p.Id
                        select new
                        {
                            PostId = p.Id,
                            CommentId = c.Id,
                            c.CommentCaption,
                            c.CreationTime,
                            c.InsertDate,
                            p.PostTitle,
                            p.PostFile,
                            PostTime = p.CreationTime
                        };
            var pagedSorted = await query
                .OrderBy(input.Sorting ?? "CreationTime Desc")
                .PageBy(input)
                .ToListAsync();
            var totalCount = await query.CountAsync();
            var data = new List<BriefCommentsDto>();

            foreach (var row in pagedSorted)
            {
                data.Add(new BriefCommentsDto()
                {
                    PostId = row.PostId,
                    PostTitle = row.PostTitle,
                    PostFile = row.PostFile,
                    PostTime = row.PostTime,
                    CommentId = row.CommentId,
                    CommentCaption = row.CommentCaption,
                    CreationTime = row.CreationTime,
                    InsertDate = row.InsertDate,


                });
            }
            return new PagedResultDto<BriefCommentsDto>(
                totalCount,
                data
            );
        }

        public async Task<PagedResultDto<BriefLikedPostsDto>> GetUserPostLikes(GetUserInformationDto input)
        {
            var query = from l in _postLikeRepository.GetAll()
                        where l.UserId == input.UserId
                        join p in _postRepository.GetAll() on l.PostId equals p.Id
                        select new
                        {
                            l.LikeTime,
                            p.Id,
                            p.PostTitle,
                            p.PostFile,
                            PostTime = p.CreationTime
                        };
            var pagedSorted = await query
                .OrderBy(input.Sorting ?? "LikeTime Desc")
                .PageBy(input)
                .ToListAsync();
            var totalCount = await query.CountAsync();
            var data = new List<BriefLikedPostsDto>();

            foreach (var row in pagedSorted)
            {
                data.Add(new BriefLikedPostsDto()
                {
                    LikeTime = row.LikeTime,
                    PostId = row.Id,
                    PostTitle = row.PostTitle,
                    PostFile = row.PostFile,
                    PostTime = row.PostTime

                });
            }
            return new PagedResultDto<BriefLikedPostsDto>(
                totalCount,
                data
            );
        }

        public async Task<PagedResultDto<BriefCommentsDto>> GetUserCommentLikes(GetUserInformationDto input)
        {
            var query = from c in _commentLikeRepository.GetAll().Include(x => x.CommentFk)
                        where c.UserId == input.UserId
                        join p in _postRepository.GetAll() on c.CommentFk.PostId equals p.Id
                        select new
                        {
                            PostId = p.Id,
                            CommentId = c.Id,
                            c.CommentFk.CommentCaption,
                            c.CommentFk.CreationTime,
                            c.CommentFk.InsertDate,
                            p.PostTitle,
                            p.PostFile,
                            PostTime = p.CreationTime
                        };
            var pagedSorted = await query
                .OrderBy(input.Sorting ?? "CreationTime Desc")
                .PageBy(input)
                .ToListAsync();
            var totalCount = await query.CountAsync();
            var data = new List<BriefCommentsDto>();

            foreach (var row in pagedSorted)
            {
                data.Add(new BriefCommentsDto()
                {
                    PostId = row.PostId,
                    PostTitle = row.PostTitle,
                    PostFile = row.PostFile,
                    PostTime = row.PostTime,
                    CommentId = row.CommentId,
                    CommentCaption = row.CommentCaption,
                    CreationTime = row.CreationTime,
                    InsertDate = row.InsertDate,


                });
            }
            return new PagedResultDto<BriefCommentsDto>(
                totalCount,
                data
            );
        }

        public async Task<PagedResultDto<BriefCreatedPostsDto>> GetUserPosts(GetUserInformationDto input)
        {
            var query = from p in _postRepository.GetAll()
                        where p.GroupMemberFk.UserId == input.UserId
                        //join p in _postRepository.GetAll() on s.PostId equals p.Id
                        select new
                        {
                            p.Id,
                            p.PostTitle,
                            p.PostFile,
                            PostTime = p.CreationTime,
                        };
            var pagedSorted = await query
                .OrderBy(input.Sorting ?? "PostTime Desc")
                .PageBy(input)
                .ToListAsync();
            var totalCount = await query.CountAsync();
            var data = new List<BriefCreatedPostsDto>();

            foreach (var row in pagedSorted)
            {
                data.Add(new BriefCreatedPostsDto()
                {
                    PostId = row.Id,
                    PostTitle = row.PostTitle,
                    PostFile = row.PostFile,
                    PostTime = row.PostTime

                });
            }
            return new PagedResultDto<BriefCreatedPostsDto>(
                totalCount,
                data
            );
        }

        public async Task<FileDto> GetUsersToExcel(GetUsersToExcelInput input)
        {
            var query = GetUsersFilteredQuery(input);

            var users = await query
                .OrderBy(input.Sorting)
                .ToListAsync();

            var userListDtos = ObjectMapper.Map<List<UserListDto>>(users);
            await FillRoleNames(userListDtos);

            return _userListExcelExporter.ExportToFile(userListDtos);
        }

        public Task<List<OrganizationDto>> GetListOfOrganizations(int userId)
        {
            var result = new List<OrganizationDto>();
            var query = from x in _groupMemberRepository.GetAll().Include(x => x.OrganizationFk)
                        where x.UserId == userId
                        select x;
            foreach (var row in query)
            {
                if (row.OrganizationFk != null)
                    result.Add(ObjectMapper.Map<OrganizationDto>(row.OrganizationFk));
            }

            return Task.FromResult(result);
        }

        [AbpAuthorize(AppPermissions.Pages_Administration_Users_Create, AppPermissions.Pages_Administration_Users_Edit)]
        public async Task<GetUserForEditOutput> GetUserForEdit(NullableIdDto<long> input)
        {
            //Getting all available roles
            var userRoleDtos = await _roleManager.Roles
                .OrderBy(r => r.DisplayName)
                .Select(r => new UserRoleDto
                {
                    RoleId = r.Id,
                    RoleName = r.Name,
                    RoleDisplayName = r.DisplayName
                })
                .ToArrayAsync();

            var allOrganizationUnits = await _organizationUnitRepository.GetAllListAsync();

            var output = new GetUserForEditOutput
            {
                Roles = userRoleDtos,
                AllOrganizationUnits = ObjectMapper.Map<List<OrganizationUnitDto>>(allOrganizationUnits),
                MemberedOrganizationUnits = new List<string>(),
                AllowedUserNameCharacters = _userOptions.Value.AllowedUserNameCharacters,
                IsSMTPSettingsProvided = await _emailSettingsChecker.EmailSettingsValidAsync()
            };

            if (!input.Id.HasValue)
            {
                // Creating a new user
                output.User = new UserEditDto
                {
                    IsActive = true,
                    ShouldChangePasswordOnNextLogin = true,
                    IsTwoFactorEnabled =
                        await SettingManager.GetSettingValueAsync<bool>(AbpZeroSettingNames.UserManagement
                            .TwoFactorLogin.IsEnabled),
                    IsLockoutEnabled =
                        await SettingManager.GetSettingValueAsync<bool>(AbpZeroSettingNames.UserManagement.UserLockOut
                            .IsEnabled)
                };

                foreach (var defaultRole in await _roleManager.Roles.Where(r => r.IsDefault).ToListAsync())
                {
                    var defaultUserRole = userRoleDtos.FirstOrDefault(ur => ur.RoleName == defaultRole.Name);
                    if (defaultUserRole != null)
                    {
                        defaultUserRole.IsAssigned = true;
                    }
                }
            }
            else
            {
                //Editing an existing user
                var user = await UserManager.GetUserByIdAsync(input.Id.Value);

                output.User = ObjectMapper.Map<UserEditDto>(user);
                output.ProfilePictureId = user.ProfilePictureId;
                var organizationUnits = await UserManager.GetOrganizationUnitsAsync(user);
                output.MemberedOrganizationUnits = organizationUnits.Select(ou => ou.Code).ToList();

                var allRolesOfUsersOrganizationUnits = GetAllRoleNamesOfUsersOrganizationUnits(input.Id.Value);

                foreach (var userRoleDto in userRoleDtos)
                {
                    //userRoleDto.IsAssigned = await UserManager.IsInRoleAsync(user, userRoleDto.RoleName);
                    userRoleDto.IsAssigned = await UserManager.IsInRoleInternalAsync(user, userRoleDto.RoleName);
                    userRoleDto.InheritedFromOrganizationUnit =
                        allRolesOfUsersOrganizationUnits.Contains(userRoleDto.RoleName);
                }
            }

            return output;
        }

        private List<string> GetAllRoleNamesOfUsersOrganizationUnits(long userId)
        {
            return (from userOu in _userOrganizationUnitRepository.GetAll()
                    join roleOu in _organizationUnitRoleRepository.GetAll() on userOu.OrganizationUnitId equals roleOu
                        .OrganizationUnitId
                    join userOuRoles in _roleRepository.GetAll() on roleOu.RoleId equals userOuRoles.Id
                    where userOu.UserId == userId
                    select userOuRoles.Name).ToList();
        }

        [AbpAuthorize(AppPermissions.Pages_Administration_Users_ChangePermissions)]
        public async Task<GetUserPermissionsForEditOutput> GetUserPermissionsForEdit(EntityDto<long> input)
        {
            var user = await UserManager.GetUserByIdAsync(input.Id);
            var permissions = PermissionManager.GetAllPermissions();
            var grantedPermissions = await UserManager.GetGrantedPermissionsAsync(user);

            return new GetUserPermissionsForEditOutput
            {
                Permissions = ObjectMapper.Map<List<FlatPermissionDto>>(permissions).OrderBy(p => p.DisplayName)
                    .ToList(),
                GrantedPermissionNames = grantedPermissions.Select(p => p.Name).ToList()
            };
        }

        [AbpAuthorize(AppPermissions.Pages_Administration_Users_ChangePermissions)]
        public async Task ResetUserSpecificPermissions(EntityDto<long> input)
        {
            var user = await UserManager.GetUserByIdAsync(input.Id);
            await UserManager.ResetAllPermissionsAsync(user);
        }

        [AbpAuthorize(AppPermissions.Pages_Administration_Users_ChangePermissions)]
        public async Task UpdateUserPermissions(UpdateUserPermissionsInput input)
        {
            var user = await UserManager.GetUserByIdAsync(input.Id);
            var grantedPermissions =
                PermissionManager.GetPermissionsFromNamesByValidating(input.GrantedPermissionNames);
            await UserManager.SetGrantedPermissionsAsync(user, grantedPermissions);
        }

        public async Task<long> CreateOrUpdateUser(CreateOrUpdateUserInput input)
        {
            DecryptUserPassword(input);
            if (input.User.Id.HasValue)
            {
                return await UpdateUserAsync(input);
            }
            else
            {
                return await CreateUserAsync(input);
            }
        }
        public async Task<long> CreateNode(CreateNodeDto input)
        {
            if (AbpSession.TenantId.HasValue)
            {
                await _userPolicy.CheckMaxUserCountAsync(AbpSession.GetTenantId());
            }

            // Check for unique username
            if (await UserManager.Users.AnyAsync(u => u.UserName == input.User.PhoneNumber))
            {
                throw new UserFriendlyException($"The username '{input.User.PhoneNumber}' is already taken.");
            }

            // Check for unique phone number
            if (!string.IsNullOrWhiteSpace(input.User.PhoneNumber) &&
                await UserManager.Users.AnyAsync(u => u.PhoneNumber == input.User.PhoneNumber))
            {
                throw new UserFriendlyException($"The phone number '{input.User.PhoneNumber}' is already in use.");
            }

            var user = ObjectMapper.Map<User>(input.User); //Passwords is not mapped (see mapping configuration)
            user.UserName = input.User.PhoneNumber;
            user.TenantId = AbpSession.TenantId;
            user.NationalId = input.User.NationalId;
            user.PhoneNumber = input.User.PhoneNumber;
            user.IsActive = true;
            //Set password
            if (!input.User.Password.IsNullOrEmpty())
            {
                await UserManager.InitializeOptionsAsync(AbpSession.TenantId);
                foreach (var validator in _passwordValidators)
                {
                    CheckErrors(await validator.ValidateAsync(UserManager, user, input.User.Password));
                }

                user.Password = _passwordHasher.HashPassword(user, input.User.Password);
            }

            //Assign roles
            user.Roles = new Collection<UserRole>();
            //foreach (var roleName in input.AssignedRoleNames)
            //{
            //    var role = await _roleManager.GetRoleByNameAsync(roleName);
            //    user.Roles.Add(new UserRole(AbpSession.TenantId, user.Id, role.Id));
            //}
            string[] emailDomains = { "gmail.com", "yahoo.com", "outlook.com", "example.com" };

            string randomUsername = GenerateRandomUsername();
            string randomDomain = emailDomains[new Random().Next(emailDomains.Length)];

            string randomEmail = randomUsername + "@" + randomDomain;
            user.EmailAddress = randomEmail;
            user.IsSuperUser = false;
            var role = await _roleManager.GetRoleByNameAsync("Admin");
            user.Roles.Add(new UserRole(AbpSession.TenantId, user.Id, role.Id));
            CheckErrors(await UserManager.CreateAsync(user));
            await CurrentUnitOfWork.SaveChangesAsync(); //To get new user's Id.
            //Notifications
            await _notificationSubscriptionManager.SubscribeToAllAvailableNotificationsAsync(user.ToUserIdentifier());
            //await _appNotifier.WelcomeToTheApplicationAsync(user);
            //Organization Units
            await UserManager.SetOrganizationUnitsAsync(user, new long[] { 1 });



            var organizationId = await _organizationAppService.CreateOrEdit(new CreateOrEditOrganizationDto()
            {
                IsGovernmental = input.Organization.IsGovernment,
                NationalId = input.Organization.NationalId,
                OrganizationContactPerson = input.User.Name + " " + input.User.Surname,
                OrganizationLogoToken = input.Organization.OrganizationLogoToken,
                OrganizationName = input.Organization.Name,
                Comment = input.Organization.Comment

            });
            await _groupMembersAppService.CreateOrEdit(new CreateOrEditGroupMemberDto()
            {
                UserId = user.Id,
                OrganizationId = organizationId,
                MemberPos = 0,
                MemberPosition = "",
            });

            await _deedChartsAppService.SetOrganizationForChartLeaf(new SetOrganizationForChartLeafInput()
            {
                OrganizationId = organizationId,
                OrganizationChartId = input.OrganizationChartId
            });


            var organizationChartId = await _organizationChartsAppService.CreateCompanyChart(new CreateCompanyChartDto()
            {
                Caption = input.Organization.Name,
                OrganizationId = organizationId
            });

            await _organizationUsersAppService.CreateOrEdit(new CreateOrEditOrganizationUserDto()
            {
                IsGlobal = false,
                OrganizationChartId = organizationChartId,
                UserId = user.Id,

            });


            return user.Id;


        }

        public async Task<long> CreateFullNode(CreateFullNodeDto input)
        {

            if (AbpSession.TenantId.HasValue)
            {
                await _userPolicy.CheckMaxUserCountAsync(AbpSession.GetTenantId());
            }

            // Check for unique username
            if (await UserManager.Users.AnyAsync(u => u.UserName == input.User.PhoneNumber))
            {
                throw new UserFriendlyException($"The username '{input.User.PhoneNumber}' is already taken.");
            }

            // Check for unique phone number
            if (!string.IsNullOrWhiteSpace(input.User.PhoneNumber) &&
                await UserManager.Users.AnyAsync(u => u.PhoneNumber == input.User.PhoneNumber))
            {
                throw new UserFriendlyException($"The phone number '{input.User.PhoneNumber}' is already in use.");
            }

            var user = ObjectMapper.Map<User>(input.User); //Passwords is not mapped (see mapping configuration)
            user.UserName = input.User.PhoneNumber;
            user.TenantId = AbpSession.TenantId;
            user.NationalId = input.User.NationalId;
            user.PhoneNumber = input.User.PhoneNumber;
            user.IsActive = true;
            //Set password
            if (!input.User.Password.IsNullOrEmpty())
            {
                await UserManager.InitializeOptionsAsync(AbpSession.TenantId);
                foreach (var validator in _passwordValidators)
                {
                    CheckErrors(await validator.ValidateAsync(UserManager, user, input.User.Password));
                }

                user.Password = _passwordHasher.HashPassword(user, input.User.Password);
            }

            //Assign roles
            user.Roles = new Collection<UserRole>();
            //foreach (var roleName in input.AssignedRoleNames)
            //{
            //    var role = await _roleManager.GetRoleByNameAsync(roleName);
            //    user.Roles.Add(new UserRole(AbpSession.TenantId, user.Id, role.Id));
            //}
            string[] emailDomains = { "gmail.com", "yahoo.com", "outlook.com", "example.com" };

            string randomUsername = GenerateRandomUsername();
            string randomDomain = emailDomains[new Random().Next(emailDomains.Length)];

            string randomEmail = randomUsername + "@" + randomDomain;
            user.EmailAddress = randomEmail;
            user.IsSuperUser = false;
            var role = await _roleManager.GetRoleByNameAsync("Admin");
            user.Roles.Add(new UserRole(AbpSession.TenantId, user.Id, role.Id));
            CheckErrors(await UserManager.CreateAsync(user));
            await CurrentUnitOfWork.SaveChangesAsync(); //To get new user's Id.
            //Notifications
            await _notificationSubscriptionManager.SubscribeToAllAvailableNotificationsAsync(user.ToUserIdentifier());
            //await _appNotifier.WelcomeToTheApplicationAsync(user);
            //Organization Units
            await UserManager.SetOrganizationUnitsAsync(user, new long[] { 1 });



            var organizationId = await _organizationAppService.CreateOrEdit(new CreateOrEditOrganizationDto()
            {
                IsGovernmental = input.Organization.IsGovernment,
                NationalId = input.Organization.NationalId,
                OrganizationContactPerson = input.User.Name + " " + input.User.Surname,
                OrganizationLogoToken = input.Organization.OrganizationLogoToken,
                OrganizationName = input.Organization.Name,
                Comment = input.Organization.Comment

            });
            await _groupMembersAppService.CreateOrEdit(new CreateOrEditGroupMemberDto()
            {
                UserId = user.Id,
                OrganizationId = organizationId,
                MemberPos = 0,
                MemberPosition = "",
            });



            await _deedChartsAppService.CreateOrEdit(new CreateOrEditDeedChartDto()
            {
                OrganizationId = organizationId,
                Caption = input.DeedChartCaption,
                ParentId = input.DeedChartParentId,

            });


            var organizationChartId = await _organizationChartsAppService.CreateCompanyChart(new CreateCompanyChartDto()
            {
                Caption = input.Organization.Name,
                OrganizationId = organizationId
            });

            await _organizationUsersAppService.CreateOrEdit(new CreateOrEditOrganizationUserDto()
            {
                IsGlobal = false,
                OrganizationChartId = organizationChartId,
                UserId = user.Id,

            });


            return user.Id;

        }

        static string GenerateRandomUsername()
        {
            // You can define rules for generating a random username here.
            // For simplicity, we'll use a random combination of letters and numbers.
            int length = 10;
            const string chars = "abcdefghijklmnopqrstuvwxyz0123456789";
            Random random = new Random();
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }


        [AbpAuthorize(AppPermissions.Pages_Administration_Users_Delete)]
        public async Task DeleteUser(EntityDto<long> input)
        {
           
            if (input.Id == AbpSession.GetUserId())
            {
                throw new UserFriendlyException(L("YouCanNotDeleteOwnAccount"));
            }

            var members = _groupMemberRepository.GetAll()
                .Where(x => x.UserId == input.Id)
                .ToList();
            foreach (var groupMember in members)
            {
                if (_postRepository.GetAll().Any(x => x.GroupMemberId == groupMember.Id))
                    throw new UserFriendlyException("پیش از حذف کاربر خبرهای مرتبط با شخص را حذف نمایید");
            }

            var user = await UserManager.GetUserByIdAsync(input.Id);
            if (user == null)
            {
                throw new UserFriendlyException(L("کاربر پیدا نشد."));
            }
            if (user.UserType == Accounts.Dto.AccountUserType.SuperAdmin)
            {
                throw new UserFriendlyException("امکان حذف کاربر سوپر ادمین وجود ندارد");
            }
            CheckErrors(await UserManager.DeleteAsync(user));

        }

        [AbpAuthorize(AppPermissions.Pages_Administration_Users_Unlock)]
        public async Task UnlockUser(EntityDto<long> input)
        {
            var user = await UserManager.GetUserByIdAsync(input.Id);
            user.Unlock();
        }

        public async Task RemoveProfilePicture(long userId)
        {
            var entity = await _userRepository.GetAsync(userId);
            entity.ProfilePictureId = null;
            await _userRepository.UpdateAsync(entity);
            await CurrentUnitOfWork.SaveChangesAsync(); //To get new user's Id.

        }

        public async Task UpdateProfilePictureId(long userId, Guid? profilePictureId)
        {
            using var uow = UnitOfWorkManager.Begin();
            try
            {
                var entity = await _userRepository.GetAsync(userId);
                entity.ProfilePictureId = profilePictureId;

                // Log the current state of the entity before the update
                // This can help identify if the entity is being modified correctly
                Console.WriteLine($"Before Update - UserId: {userId}, ProfilePictureId: {entity.ProfilePictureId}");

                await _userRepository.UpdateAsync(entity);

                // Log the current state of the entity after the update
                Console.WriteLine(($"After Update - UserId: {userId}, ProfilePictureId: {entity.ProfilePictureId}"));

                await uow.CompleteAsync();
            }
            catch (Exception ex)
            {
                // Log or handle the exception
                Console.WriteLine(($"Error updating profile picture for UserId {userId}: {ex.Message}"));
                // Optionally, rethrow the exception if needed
                throw;
            }
        }

        [AbpAuthorize(AppPermissions.Pages_Administration_Users_Edit)]
        protected virtual async Task<long> UpdateUserAsync(CreateOrUpdateUserInput input)
        {
            Debug.Assert(input.User.Id != null, "input.User.Id should be set.");

            var user = await UserManager.FindByIdAsync(input.User.Id.Value.ToString());
            var currentUserType = user.UserType;
            bool isSuperAdmin = false;
            if (user.UserType == Accounts.Dto.AccountUserType.SuperAdmin)
            {
                isSuperAdmin = true;
            }
            // Check if the username is already in use by another user
            if (await UserManager.Users.AnyAsync(u => u.UserName == input.User.UserName && u.Id != user.Id))
            {
                throw new UserFriendlyException($"The username '{input.User.UserName}' is already taken by another user.");
            }

            // Check if the phone number is already in use by another user
            if (!string.IsNullOrWhiteSpace(input.User.PhoneNumber) &&
                await UserManager.Users.AnyAsync(u => u.PhoneNumber == input.User.PhoneNumber && u.Id != user.Id))
            {
                throw new UserFriendlyException($"The phone number '{input.User.PhoneNumber}' is already in use by another user.");
            }


            //Update user properties
            ObjectMapper.Map(input.User, user); //Passwords is not mapped (see mapping configuration)
            if (isSuperAdmin)
            {
                user.UserType = Accounts.Dto.AccountUserType.SuperAdmin;
                user.IsActive = true;
                user.IsSuperUser = true;

            }
            else
            {
                if (input.User.UserType==AccountUserType.SuperAdmin || input.User.UserType == AccountUserType.Admin)
                {
                    user.UserType = currentUserType;
                }
                user.IsSuperUser = false;
            }
            CheckErrors(await UserManager.UpdateAsync(user));

            if (input.SetRandomPassword)
            {
                var randomPassword = await _userManager.CreateRandomPassword();
                user.Password = _passwordHasher.HashPassword(user, randomPassword);
                input.User.Password = randomPassword;
            }
            else if (!input.User.Password.IsNullOrEmpty())
            {
                await UserManager.InitializeOptionsAsync(AbpSession.TenantId);
                CheckErrors(await UserManager.ChangePasswordAsync(user, input.User.Password));
            }

            //Update roles
            try
            {
                CheckErrors(await UserManager.SetRolesAsync(user, input.AssignedRoleNames));

            }
            catch (Exception)
            {
                //ignored
            }
            //update organization units
            await UserManager.SetOrganizationUnitsAsync(user, input.OrganizationUnits.ToArray());

            if (input.SendActivationEmail)
            {
                user.SetNewEmailConfirmationCode();
                await _userEmailer.SendEmailActivationLinkAsync(
                    user,
                    AppUrlService.CreateEmailActivationUrlFormat(AbpSession.TenantId),
                    input.User.Password
                );
            }
            await _notificationSubscriptionManager.SubscribeToAllAvailableNotificationsAsync(user.ToUserIdentifier());
            return user.Id;
        }

        [AbpAuthorize(AppPermissions.Pages_Administration_Users_Create)]
        [ItemCanBeNull]
        protected virtual async Task<long> CreateUserAsync(CreateOrUpdateUserInput input)
        {
            if (AbpSession.TenantId.HasValue)
            {
                await _userPolicy.CheckMaxUserCountAsync(AbpSession.GetTenantId());
            }

            // Check if the username is already in use
            if (await UserManager.Users.AnyAsync(u => u.UserName == input.User.UserName))
            {
                throw new UserFriendlyException($"The username '{input.User.UserName}' is already taken or IsDeleted = true");
            }

            // Check if the phone number is already in use
            if (!string.IsNullOrWhiteSpace(input.User.PhoneNumber) &&
                await UserManager.Users.AnyAsync(u => u.PhoneNumber == input.User.PhoneNumber))
            {
                throw new UserFriendlyException($"The phone number '{input.User.PhoneNumber}' is already in use or IsDeleted = true");
            }

            var user = ObjectMapper.Map<User>(input.User); //Passwords is not mapped (see mapping configuration)
            user.TenantId = AbpSession.TenantId;

            //Set password
            if (input.SetRandomPassword)
            {
                var randomPassword = await _userManager.CreateRandomPassword();
                user.Password = _passwordHasher.HashPassword(user, randomPassword);
                input.User.Password = randomPassword;
            }
            else if (!input.User.Password.IsNullOrEmpty())
            {
                await UserManager.InitializeOptionsAsync(AbpSession.TenantId);
                foreach (var validator in _passwordValidators)
                {
                    CheckErrors(await validator.ValidateAsync(UserManager, user, input.User.Password));
                }

                user.Password = _passwordHasher.HashPassword(user, input.User.Password);
            }

            user.ShouldChangePasswordOnNextLogin = input.User.ShouldChangePasswordOnNextLogin;

            //Assign roles
            user.Roles = new Collection<UserRole>();
            foreach (var roleName in input.AssignedRoleNames)
            {
                var role = await _roleManager.GetRoleByNameAsync(roleName);
                user.Roles.Add(new UserRole(AbpSession.TenantId, user.Id, role.Id));
            }

            CheckErrors(await UserManager.CreateAsync(user));
            await CurrentUnitOfWork.SaveChangesAsync(); //To get new user's Id.

            //Notifications
            await _notificationSubscriptionManager.SubscribeToAllAvailableNotificationsAsync(user.ToUserIdentifier());
            await _appNotifier.WelcomeToTheApplicationAsync(user);

            //Organization Units
            await UserManager.SetOrganizationUnitsAsync(user, input.OrganizationUnits.ToArray());

            //Send activation email
            if (input.SendActivationEmail)
            {
                user.SetNewEmailConfirmationCode();
                await _userEmailer.SendEmailActivationLinkAsync(
                    user,
                    AppUrlService.CreateEmailActivationUrlFormat(AbpSession.TenantId),
                    input.User.Password
                );
            }

            if (AbpSession.UserId == null) throw new UserFriendlyException("User Must be Logged in!");
            var currentUser = await _userRepository.GetAsync(AbpSession.UserId.Value);

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

                var groupMember = new GroupMember()
                {
                    MemberPos = 0,
                    MemberPosition = "",
                    UserId = user.Id,
                    OrganizationId = orgEntity.Id,

                };
                await _groupMemberRepository.InsertAsync(groupMember);
                //await CurrentUnitOfWork.SaveChangesAsync(); //To get new user's Id.
            }

            return user.Id;
        }

        private async Task FillRoleNames(IReadOnlyCollection<UserListDto> userListDtos)
        {
            /* This method is optimized to fill role names to given list. */
            var userIds = userListDtos.Select(u => u.Id);

            var userRoles = await _userRoleRepository.GetAll()
                .Where(userRole => userIds.Contains(userRole.UserId))
                .Select(userRole => userRole).ToListAsync();

            var distinctRoleIds = userRoles.Select(userRole => userRole.RoleId).Distinct();

            foreach (var user in userListDtos)
            {
                var rolesOfUser = userRoles.Where(userRole => userRole.UserId == user.Id).ToList();
                user.Roles = ObjectMapper.Map<List<UserListRoleDto>>(rolesOfUser);
            }

            var roleNames = new Dictionary<int, string>();
            foreach (var roleId in distinctRoleIds)
            {
                var role = await _roleManager.FindByIdAsync(roleId.ToString());
                if (role != null)
                {
                    roleNames[roleId] = role.DisplayName;
                }
            }

            foreach (var userListDto in userListDtos)
            {
                foreach (var userListRoleDto in userListDto.Roles)
                {
                    if (roleNames.ContainsKey(userListRoleDto.RoleId))
                    {
                        userListRoleDto.RoleName = roleNames[userListRoleDto.RoleId];
                    }
                }

                userListDto.Roles = userListDto.Roles.Where(r => r.RoleName != null).OrderBy(r => r.RoleName).ToList();
            }
        }

        private IQueryable<User> GetUsersFilteredQuery(IGetUsersInput input)
        {
            var query = UserManager.Users
                .WhereIf(!string.IsNullOrWhiteSpace(input.NationalIdFilter), n => n.NationalId == input.NationalIdFilter)
                .WhereIf(!string.IsNullOrWhiteSpace(input.NameFilter), n => n.Name == input.NameFilter)
                .WhereIf(!string.IsNullOrWhiteSpace(input.SurNameFilter), n => n.Surname == input.SurNameFilter)
                .WhereIf(!string.IsNullOrWhiteSpace(input.UserNameFilter), n => n.UserName == input.UserNameFilter)
                .WhereIf(!string.IsNullOrWhiteSpace(input.PhoneNumberFilter), n => n.PhoneNumber == input.PhoneNumberFilter)
                .WhereIf(input.IsActiveFilter.HasValue, n => n.IsActive == input.IsActiveFilter.Value)
                .WhereIf(input.FromCreationDate.HasValue, u => u.CreationTime >= input.FromCreationDate.Value)
                .WhereIf(input.ToCreationDate.HasValue, u => u.CreationTime <= input.ToCreationDate.Value)
                .WhereIf(
                    input.FromLastLoginDate.HasValue || input.ToLastLoginDate.HasValue,
                    u => _userLoginAttemptRepository
                        .GetAll()
                        .Any(l =>
                            l.UserId == u.Id &&
                            (!input.FromLastLoginDate.HasValue || l.CreationTime >= input.FromLastLoginDate.Value) &&
                            (!input.ToLastLoginDate.HasValue || l.CreationTime <= input.ToLastLoginDate.Value)))
                .WhereIf(input.Role.HasValue, u => u.Roles.Any(r => r.RoleId == input.Role.Value))
                .WhereIf(input.OnlyLockedUsers,
                    u => u.LockoutEndDateUtc.HasValue && u.LockoutEndDateUtc.Value > DateTime.UtcNow)
                .WhereIf(
                    !string.IsNullOrWhiteSpace(input.Filter),
                    u =>
                        u.Name.Contains(input.Filter) ||
                        u.Surname.Contains(input.Filter) ||
                        u.UserName.Contains(input.Filter) ||
                        u.EmailAddress.Contains(input.Filter)
                );

            if (input.Permissions != null && input.Permissions.Any(p => !string.IsNullOrWhiteSpace(p)))
            {
                var staticRoleNames = _roleManagementConfig.StaticRoles.Where(
                    r => r.GrantAllPermissionsByDefault &&
                         r.Side == AbpSession.MultiTenancySide
                ).Select(r => r.RoleName).ToList();

                input.Permissions = input.Permissions.Where(p => !string.IsNullOrEmpty(p)).ToList();

                var userIds = from user in query
                              join ur in _userRoleRepository.GetAll() on user.Id equals ur.UserId into urJoined
                              from ur in urJoined.DefaultIfEmpty()
                              join urr in _roleRepository.GetAll() on ur.RoleId equals urr.Id into urrJoined
                              from urr in urrJoined.DefaultIfEmpty()
                              join up in _userPermissionRepository.GetAll()
                                  .Where(userPermission => input.Permissions.Contains(userPermission.Name)) on user.Id equals up.UserId into upJoined
                              from up in upJoined.DefaultIfEmpty()
                              join rp in _rolePermissionRepository.GetAll()
                                  .Where(rolePermission => input.Permissions.Contains(rolePermission.Name)) on
                                  new { RoleId = ur == null ? 0 : ur.RoleId } equals new { rp.RoleId } into rpJoined
                              from rp in rpJoined.DefaultIfEmpty()
                              where (up != null && up.IsGranted) ||
                                    (up == null && rp != null && rp.IsGranted) ||
                                    (up == null && rp == null && staticRoleNames.Contains(urr.Name))
                              group user by user.Id
                    into userGrouped
                              select userGrouped.Key;

                query = UserManager.Users.Where(e => userIds.Contains(e.Id));
            }

            return query;
        }
        private void DecryptUserPassword(CreateOrUpdateUserInput input)
        {
            if (!string.IsNullOrWhiteSpace(input.User.Password))
            {
                input.User.Password = DecryptPassword(input.User.Password);
            }
        }

        private string DecryptPassword(string encryptedPassword)
        {
            var privateKeyInput = Environment.GetEnvironmentVariable("PRIVATE_KEY_PATH");
            if (string.IsNullOrWhiteSpace(privateKeyInput))
            {
                throw new InvalidOperationException("Private key path is not set.");
            }

            string privateKeyPem = System.IO.File.Exists(privateKeyInput)
                ? System.IO.File.ReadAllText(privateKeyInput)
                : privateKeyInput;

            using (var rsa = RSA.Create())
            {
                rsa.ImportFromPem(privateKeyPem.ToCharArray());
                byte[] encryptedBytes = Convert.FromBase64String(encryptedPassword);
                byte[] decryptedBytes = rsa.Decrypt(encryptedBytes, RSAEncryptionPadding.Pkcs1);
                return Encoding.UTF8.GetString(decryptedBytes);
            }
        }



    }
}
