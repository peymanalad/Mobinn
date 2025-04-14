using Chamran.Deed.People;
using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using Abp.Linq.Extensions;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Abp;
using Abp.Domain.Repositories;
using Chamran.Deed.Info.Exporting;
using Chamran.Deed.Info.Dtos;
using Chamran.Deed.Dto;
using Abp.Application.Services.Dto;
using Chamran.Deed.Authorization;
using Abp.Extensions;
using Abp.Authorization;
using Abp.Notifications;
using Microsoft.EntityFrameworkCore;
using Abp.UI;
using Chamran.Deed.Common;
using Chamran.Deed.Notifications;
using Chamran.Deed.Storage;
using Abp.Domain.Uow;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Globalization;
using Abp.Auditing;
using Abp.Authorization.Users;
using AutoMapper.Internal.Mappers;
using Chamran.Deed.Authorization.Users.Dto;
using Microsoft.Data.SqlClient;
using Abp.EntityFrameworkCore;
using Abp.Events.Bus;
using Chamran.Deed.Authorization.Accounts.Dto;
using Chamran.Deed.Authorization.Users;
using Chamran.Deed.EntityFrameworkCore;
using Chamran.Deed.Net.Sms;
using NPOI.SS.Formula.Functions;
using Stripe.Identity;
using Abp.Runtime.Session;

namespace Chamran.Deed.Info
{
    [AbpAuthorize(AppPermissions.Pages_Posts)]
    public class PostsAppService : DeedAppServiceBase, IPostsAppService
    {
        private readonly IRepository<Post> _postRepository;
        private readonly IRepository<Comment> _commentRepository;
        private readonly IPostsExcelExporter _postsExcelExporter;
        private readonly IRepository<GroupMember, int> _lookup_groupMemberRepository;
        private readonly IRepository<PostGroup, int> _lookup_postGroupRepository;
        private readonly IRepository<PostSubGroup, int> _lookup_postSubGroupRepository;
        private readonly IRepository<UserPostGroup, int> _userPostGroupRepository;
        private readonly IRepository<Seen, int> _seenRepository;
        private readonly IRepository<User, long> _userRepository;
        private readonly IRepository<PostLike, int> _postLikeRepository;
        private readonly IDbContextProvider<DeedDbContext> _dbContextProvider;
        private readonly ISmsSender _smsSender;
        private readonly IRepository<PostEditHistory> _postEditHistoryRespoRepository;

        private readonly ITempFileCacheManager _tempFileCacheManager;
        private readonly IBinaryObjectManager _binaryObjectManager;
        private readonly IRepository<Organization> organizationRepository;
        private readonly IAppNotifier _appNotifier;
        private readonly IUnitOfWorkManager _unitOfWorkManager;

        private readonly IRepository<UserRole,long> _userRoleRepository;
        //private readonly IRepository<PostCategory> _postCategoryRepository;
        //private readonly IDbContextProvider<DeedDbContext> _dbContextProvider;

        //private CultureInfo _originalCulture;
        //private readonly CultureInfo _targetCulture=new CultureInfo("fa-IR");

        public PostsAppService(IRepository<Post> postRepository, IRepository<Comment> commentRepository,
            IPostsExcelExporter postsExcelExporter, IRepository<GroupMember, int> lookup_groupMemberRepository,
            IRepository<PostGroup, int> lookup_postGroupRepository, IRepository<PostSubGroup, int> lookup_postSubGroupRepository, ITempFileCacheManager tempFileCacheManager,
            IBinaryObjectManager binaryObjectManager, IRepository<Organization> organization, IAppNotifier appNotifier,
            IRepository<UserPostGroup, int> userPostGroupRepository, IUnitOfWorkManager unitOfWorkManager,
            IRepository<PostLike, int> postLikeRepository, IRepository<Seen, int> seenRepository, IRepository<AllowedUserPostGroup, int> allowedUserPostGroupRepository,
            IRepository<User, long> userRepository, IDbContextProvider<DeedDbContext> dbContextProvider,ISmsSender smsSender,
            IRepository<PostEditHistory> postEditHistoryRespoRepository,IRepository<UserRole,long> userRoleRepository)
        {
            _postRepository = postRepository;
            _commentRepository = commentRepository;
            _postsExcelExporter = postsExcelExporter;
            _lookup_groupMemberRepository = lookup_groupMemberRepository;
            _lookup_postGroupRepository = lookup_postGroupRepository;
            _lookup_postSubGroupRepository = lookup_postSubGroupRepository;
            _tempFileCacheManager = tempFileCacheManager;
            _binaryObjectManager = binaryObjectManager;
            organizationRepository = organization;
            _appNotifier = appNotifier;
            _userPostGroupRepository = userPostGroupRepository;
            _unitOfWorkManager = unitOfWorkManager;
            _postLikeRepository = postLikeRepository;
            _seenRepository = seenRepository;
            _userRepository = userRepository;
            _dbContextProvider = dbContextProvider;
            _smsSender = smsSender;
            _postEditHistoryRespoRepository = postEditHistoryRespoRepository;
            //_postCategoryRepository = postCategoryRepository;
            //_dbContextProvider= dbContextProvider;
            _userRoleRepository = userRoleRepository;

        }

        public async Task<PagedResultDto<GetPostForViewDto>> GetAll(GetAllPostsInput input)
        {
            User user = GetCurrentUser();
            
            // Prepare parameters
            var parameters = new[]
            {
        new SqlParameter("@OrganizationId", input.OrganizationId ?? (object)DBNull.Value),
        new SqlParameter("@Filter", string.IsNullOrWhiteSpace(input.Filter) ? (object)DBNull.Value : (object)input.Filter),
        new SqlParameter("@PostCaptionFilter", string.IsNullOrWhiteSpace(input.PostCaptionFilter) ? (object)DBNull.Value : (object)input.PostCaptionFilter),
        new SqlParameter("@IsSpecialFilter", input.IsSpecialFilter ?? (object)DBNull.Value),
        new SqlParameter("@PostTitleFilter", string.IsNullOrWhiteSpace(input.PostTitleFilter) ? (object)DBNull.Value : (object)input.PostTitleFilter),
        new SqlParameter("@GroupMemberMemberPositionFilter", string.IsNullOrWhiteSpace(input.GroupMemberMemberPositionFilter) ? (object)DBNull.Value : (object)input.GroupMemberMemberPositionFilter),
        new SqlParameter("@PostGroupPostGroupDescriptionFilter", string.IsNullOrWhiteSpace(input.PostGroupPostGroupDescriptionFilter) ? (object)DBNull.Value : (object)input.PostGroupPostGroupDescriptionFilter),
        new SqlParameter("@PostGroupPostSubGroupDescriptionFilter", string.IsNullOrWhiteSpace(input.PostGroupPostSubGroupDescriptionFilter) ? (object)DBNull.Value : (object)input.PostGroupPostSubGroupDescriptionFilter), // New parameter
        new SqlParameter("@FromDate", input.FromDate ?? (object)DBNull.Value),
        new SqlParameter("@ToDate", input.ToDate ?? (object)DBNull.Value),
        new SqlParameter("@OrderBy", input.Sorting ?? "CreationTime DESC"),
        new SqlParameter("@MaxResultCount", input.MaxResultCount),
        new SqlParameter("@SkipCount", input.SkipCount)
    };

            var dbContext = await _dbContextProvider.GetDbContextAsync();

            // Execute the SQL and get all results as a list
            try
            {
                var queryResult = await dbContext.Set<GetPostsForView>()
                    .FromSqlRaw(
                        "EXEC GetFilteredPosts @OrganizationId, @Filter, @PostCaptionFilter, @IsSpecialFilter, @PostTitleFilter, @GroupMemberMemberPositionFilter, @PostGroupPostGroupDescriptionFilter, @PostGroupPostSubGroupDescriptionFilter, @FromDate, @ToDate, @OrderBy, @MaxResultCount, @SkipCount",
                        parameters)
                    .AsNoTracking()
                    .ToListAsync();

                // Count the total results (without paging)
                var totalCount = queryResult.Count;

                var editHistories = await dbContext.PostEditHistories
                    .Where(e => queryResult.Select(p => p.Id).Contains(e.PostId))
                    .ToListAsync();

                // Map results to DTOs
                var result = queryResult.GroupBy(post => post.Id).Select(post => new GetPostForViewDto
                {
                    Post = new PostDto
                    {
                        PostFile = post.First().PostFile,
                        PostCaption = post.First().PostCaption,
                        IsSpecial = post.First().IsSpecial,
                        IsPublished = post.First().IsPublished,
                        PostTitle = post.First().PostTitle,
                        PostRefLink = post.First().PostRefLink,
                        Id = post.Key,
                        CreationTime = post.First().CreationTime,
                        LastModificationTime = post.First().LastModificationTime,
                        CurrentPostStatus = (PostStatus)post.First().CurrentPostStatus,
                        PublisherUserId = post.First().PublisherUserId,
                        DatePublished = post.First().DatePublished,
                        PublisherUserFirstName = post.First().PublisherUserFirstName,
                        PublisherUserLastName = post.First().PublisherUserLastName,
                        PublisherUserName = post.First().PublisherUserName,
                        CreatorUserFirstName = post.First().CreatorUserFirstName,
                        CreatorUserLastName = post.First().CreatorUserLastName,
                        CreatorUserName = post.First().CreatorUserName,
                        PostSubGroupId = post.First().PostSubGroupId,


                    },
                    GroupMemberMemberPosition = post.First().GroupMemberMemberPosition ?? "",
                    PostGroupPostGroupDescription = post.First().PostGroupPostGroupDescription ?? "",
                    PostGroupPostSubGroupDescription = post.First().PostGroupPostSubGroupDescription ?? "", // New field
                    GroupFile = post.First().GroupFile,
                    TotalVisits = post.First().TotalVisits,
                    TotalLikes = post.First().TotalLikes,
                    OrganizationId = post.First().OrganizationId,
                    OrganizationName = post.First().OrganizationName,
                    PostSubGroupId = post.First().PostSubGroupId,
                    PublisherUserFirstName = post.First().PublisherUserFirstName,
                    PublisherUserLastName = post.First().PublisherUserLastName,
                    PublisherUserName = post.First().PublisherUserName,
                    CreatorUserFirstName = post.First().CreatorUserFirstName,
                    CreatorUserLastName = post.First().CreatorUserLastName,
                    CreatorUserName = post.First().CreatorUserName,
                    PostEditHistories = editHistories
                        .Where(e => e.PostId == post.Key)
                        .Select(e => new PostEditHistoryDto
                        {
                            EditorName = e.EditorName,
                            EditTime = e.EditTime,
                            Changes = e.Changes,
                        }).ToList()
                }).ToList();

                // Return paged result
                return new PagedResultDto<GetPostForViewDto>(totalCount, result);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                if (ex.InnerException != null)
                    Console.WriteLine("Inner: " + ex.InnerException.Message);
                throw;
            }
        }



        public async Task<GetPostForViewDto> GetPostForView(int id)
        {
            var post = await _postRepository.GetAsync(id);

            var output = new GetPostForViewDto { Post = ObjectMapper.Map<PostDto>(post) };

            if (output.Post.GroupMemberId != null)
            {
                var _lookupGroupMember =
                    await _lookup_groupMemberRepository.FirstOrDefaultAsync((int)output.Post.GroupMemberId);
                output.GroupMemberMemberPosition = _lookupGroupMember?.MemberPosition;
            }

            if (output.Post.PostGroupId != null)
            {
                var _lookupPostGroup =
                    await _lookup_postGroupRepository.FirstOrDefaultAsync((int)output.Post.PostGroupId);
                output.PostGroupPostGroupDescription = _lookupPostGroup?.PostGroupDescription;
            }

            output.Post.PostFileFileName = await GetBinaryFileName(post.PostFile);

            return output;
        }

        [AbpAuthorize(AppPermissions.Pages_Posts_Edit)]
        public async Task<GetPostForEditOutput> GetPostForEdit(EntityDto input)
        {
            var post = await _postRepository.FirstOrDefaultAsync(input.Id);

            var output = new GetPostForEditOutput { Post = ObjectMapper.Map<CreateOrEditPostDto>(post) };

            if (output.Post.GroupMemberId != null)
            {
                var _lookupGroupMember = await _lookup_groupMemberRepository.GetAll().Include(x => x.OrganizationFk)
                    .FirstOrDefaultAsync(x => x.Id == output.Post.GroupMemberId);
                output.GroupMemberMemberPosition = _lookupGroupMember?.MemberPosition;
                output.OrganizationId = _lookupGroupMember?.OrganizationId;
                output.OrganizationName = _lookupGroupMember?.OrganizationFk.OrganizationName;
            }

            if (output.Post.PostGroupId != null)
            {
                //var _lookupPostGroup = await _lookup_postGroupRepository.FirstOrDefaultAsync((int)output.Post.PostGroupId);
                //output.PostGroupPostGroupDescription = _lookupPostGroup?.PostGroupDescription;
                var _lookupPostGroup = await _lookup_postGroupRepository.GetAll().Include(x => x.OrganizationFk)
                    .FirstOrDefaultAsync(x => x.Id == output.Post.PostGroupId);
                output.PostGroupPostGroupDescription = _lookupPostGroup?.PostGroupDescription;
                var _lookupPostSubGroup = await _lookup_postSubGroupRepository.GetAll()
                    .FirstOrDefaultAsync(x => x.Id == output.Post.PostSubGroupId);
                output.PostGroupPostSubGroupDescription = _lookupPostSubGroup?.PostSubGroupDescription;
                output.OrganizationId = _lookupPostGroup?.OrganizationId;
                output.OrganizationName = _lookupPostGroup?.OrganizationFk.OrganizationName;
            }

            output.PostFileFileName = await GetBinaryFileName(post.PostFile);
            output.PostFile2FileName = await GetBinaryFileName(post.PostFile2);
            output.PostFile3FileName = await GetBinaryFileName(post.PostFile3);
            output.PostFile4FileName = await GetBinaryFileName(post.PostFile4);
            output.PostFile5FileName = await GetBinaryFileName(post.PostFile5);
            output.PostFile6FileName = await GetBinaryFileName(post.PostFile6);
            output.PostFile7FileName = await GetBinaryFileName(post.PostFile7);
            output.PostFile8FileName = await GetBinaryFileName(post.PostFile8);
            output.PostFile9FileName = await GetBinaryFileName(post.PostFile9);
            output.PostFile10FileName = await GetBinaryFileName(post.PostFile10);

            return output;
        }

        public async Task CreateOrEdit(CreateOrEditPostDto input)
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

        [AbpAuthorize(AppPermissions.Pages_Posts_Create)]
        protected virtual async Task Create(CreateOrEditPostDto input)
        {
            using var unitOfWork = _unitOfWorkManager.Begin();
            var grpMemberId = await _lookup_groupMemberRepository.GetAll()
                .Where(x => x.UserId == AbpSession.UserId)
                .FirstOrDefaultAsync();

            if (grpMemberId == null)
            {
                throw new UserFriendlyException("کاربر حاضر به هیچ سازمانی تعلق ندارد");
            }

            var post = ObjectMapper.Map<Post>(input);
            var userRole = await _userRoleRepository.FirstOrDefaultAsync(ur => ur.UserId == AbpSession.UserId);
            if (userRole == null)
            {
                Console.WriteLine("Current user role not found.");
                return;
            }

            post.CreatorUserId = AbpSession.UserId;
            post.GroupMemberId = grpMemberId.Id;

            await _postRepository.InsertAsync(post);

            post.PostFile = await GetBinaryObjectFromCache(input.PostFileToken, post.Id);
            post.PostFile2 = await GetBinaryObjectFromCache(input.PostFileToken2, post.Id);
            post.PostFile3 = await GetBinaryObjectFromCache(input.PostFileToken3, post.Id);
            post.PostFile4 = await GetBinaryObjectFromCache(input.PostFileToken4, post.Id);
            post.PostFile5 = await GetBinaryObjectFromCache(input.PostFileToken5, post.Id);
            post.PostFile6 = await GetBinaryObjectFromCache(input.PostFileToken6, post.Id);
            post.PostFile7 = await GetBinaryObjectFromCache(input.PostFileToken7, post.Id);
            post.PostFile8 = await GetBinaryObjectFromCache(input.PostFileToken8, post.Id);
            post.PostFile9 = await GetBinaryObjectFromCache(input.PostFileToken9, post.Id);
            post.PostFile10 = await GetBinaryObjectFromCache(input.PostFileToken10, post.Id);
            if (post.PostFile == null && post.PostFile2 == null && post.PostFile3 == null && post.PostFile4 == null &&
                post.PostFile5 == null && post.PostFile6 == null && post.PostFile7 == null && post.PostFile8 == null &&
                post.PostFile9 == null && post.PostFile10 == null)
                throw new UserFriendlyException("پست ارسالی هیچش مدیایی ندارد");
            //await _unitOfWorkManager.Current.SaveChangesAsync();

            try
            {
                await _unitOfWorkManager.Current.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine($"DbUpdateException: {ex.Message}");

                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }

                if (ex.Entries != null && ex.Entries.Any())
                {
                    foreach (var entry in ex.Entries)
                    {
                        Console.WriteLine($"Entity: {entry.Entity.GetType().Name}, State: {entry.State}");
                    }
                }

                throw; // برای مشاهده StackTrace
            }
            catch (Exception ex)
            {
                Console.WriteLine($"General Exception: {ex.Message}");
                throw;
            }


            await unitOfWork.CompleteAsync();
            if (post.PostGroupId.HasValue && post.CurrentPostStatus == PostStatus.Published)
            {
                await PublishNewPostNotifications(post/*, input.OrganizationId*/);
            }

            //var currentUser = GetCurrentUserAsync().Result;

            //if (currentUser.UserType == AccountUserType.Distributer 
            //    || currentUser.UserType == AccountUserType.Admin 
            //    || currentUser.UserType == AccountUserType.SuperAdmin 
            //    || currentUser.IsSuperUser)
            //{
            //    post.IsPublished = true;
            //    post.CurrentPostStatus = PostStatus.Published;
            //    //await SendSmsNotification(post,input.OrganizationId);

            //}




            //if (userRole.RoleId == 5 || userRole.RoleId == 2 || GetCurrentUserAsync().Result.IsSuperUser)
            //{
            //    await SendSmsNotification(post);
            //}
            //else if (userRole.RoleId == 4)
            //{
            //    post.IsPublished = false;
            //    post.CurrentPostStatus = PostStatus.Pending;
            //    await PublishNewPostNotifications(post,organizationId:input.OrganizationId);
            //}
            //else
            //{
            //    throw new UnauthorizedAccessException("شما اجازه انتشار خبر را نداريد.");
            //}

            //await SendSmsNotification(post,input.OrganizationId);
            await SendSmsNotification(post);

        }

        private async Task SendSmsNotification(Post post/*, int organization*/)
        {
            try
            {
                // Fetch the OrganizationId for the current post
                var organizationId = await _lookup_groupMemberRepository.GetAll()
                    .Where(gm => gm.Id == post.GroupMemberId)
                    .Select(gm => gm.OrganizationId)
                    .FirstOrDefaultAsync();

                if (organizationId == null)
                {
                    throw new UserFriendlyException("سازمان مرتبط با این پست پیدا نشد");
                }

                switch (post.CurrentPostStatus)
                {
                    case PostStatus.Pending:
                    {
                        var monitorUsers = _userRepository.GetAll()
                            .Where(x => x.UserType == AccountUserType.Monitor
                                        && x.PhoneNumber.StartsWith("09")
                                        && x.PhoneNumber.Length == 11);

                        var organizationMemberIds = _lookup_groupMemberRepository.GetAll()
                            .Where(gm => gm.OrganizationId == organizationId)
                            .Select(gm => gm.UserId)
                            .ToList();

                        foreach (var monitorUser in monitorUsers)
                        {
                            if (organizationMemberIds.Contains(monitorUser.Id))
                            {
                                await _smsSender.SendAsync(monitorUser.PhoneNumber, "پست جدیدی در انتظار بررسی است");
                            }
                        }

                        break;
                    }
                    case PostStatus.Revised:
                    {
                        var publisherUsers = _userRepository.GetAll()
                            .Where(x => x.UserType == AccountUserType.Distributer
                                        && x.PhoneNumber.StartsWith("09")
                                        && x.PhoneNumber.Length == 11);

                        var organizationMemberIds = _lookup_groupMemberRepository.GetAll()
                            .Where(gm => gm.OrganizationId == organizationId)
                            .Select(gm => gm.UserId)
                            .ToList();

                        foreach (var publisherUser in publisherUsers)
                        {
                            if (organizationMemberIds.Contains(publisherUser.Id))
                            {
                                await _smsSender.SendAsync(publisherUser.PhoneNumber, "پست جدیدی در انتظار انتشار است");
                            }
                        }

                        break;
                    }
                    //case PostStatus.Published:
                    //    {
                    //        var allOrganizationUsers = _lookup_groupMemberRepository.GetAll()
                    //            .Join(_userRepository.GetAll()
                    //                , g => g.UserId
                    //                , u => u.Id
                    //                , (g, u) => new { GroupMember = g, User = u })
                    //            .Where(x => x.GroupMember.OrganizationId == organization
                    //                && x.User.PhoneNumber.StartsWith("09")
                    //                && x.User.PhoneNumber.Length == 11)
                    //            .Select(x => x.User)
                    //            .ToListAsync();
                    //        foreach (var user in allOrganizationUsers.Result)
                    //        {
                    //            await _smsSender.SendAsync(user.PhoneNumber, "پست جديد منتشر شد.");
                    //        }
                    //        break;
                    //    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                
            }
        }

        private async Task PublishNewPostNotifications(Post post/*, int organizationId*/)
        {
            //var allOrganizationUsers = _lookup_groupMemberRepository.GetAll()
            //    .Join(_userRepository.GetAll()
            //        , g => g.UserId
            //        , u => u.Id
            //        , (g, u) => new { GroupMember = g, User = u })
            //    .Where(x => x.GroupMember.OrganizationId == organizationId 
            //                && (x.User.UserType == AccountUserType.Distributer || x.User.UserType == AccountUserType.Admin))
            //    .Select(x => x.User)
            //    .ToListAsync();
            //var ids = new List<UserIdentifier>();
            //foreach (var row in allOrganizationUsers.Result)
            //{
            //    ids.Add(new UserIdentifier(AbpSession.TenantId, row.Id));
            //}
            var query = _userPostGroupRepository.GetAll().Where(x => x.PostGroupId == post.PostGroupId.Value);
            var ids = new List<UserIdentifier>();
            foreach (var row in query)
            {
                ids.Add(new UserIdentifier(AbpSession.TenantId, row.UserId));
            }

            var notifData = ObjectMapper.Map<SendPostNotificationDto>(post);
            if (post.PostGroupId != null)
            {
                var groupDataQuery = await _lookup_postGroupRepository.GetAsync(post.PostGroupId.Value);
                notifData.GroupDescription = groupDataQuery.PostGroupDescription;
                notifData.GroupFile = groupDataQuery.GroupFile;
            }

            await _appNotifier.SendPostNotificationAsync(JsonConvert.SerializeObject(post, new JsonSerializerSettings
                {
                    ContractResolver = new DefaultContractResolver
                    {
                        NamingStrategy = new CamelCaseNamingStrategy() // Use PascalCaseNamingStrategy for Pascal case
                    }
                }),
                userIds: ids.ToArray()
            );

        }

        [AbpAuthorize(AppPermissions.Pages_Posts_Edit)]
        protected virtual async Task Update(CreateOrEditPostDto input)
        {
            if (input.GroupMemberId == null)
            {
                throw new Exception("کاربر سازمان نمی تواند خالی باشد");
            }
            var post = await _postRepository.FirstOrDefaultAsync((int)input.Id);
            bool shouldSendSmsNotification = post.CurrentPostStatus != input.CurrentPostStatus;
            //if (input.PublisherUserId == null)
            //{
            //    input.PublisherUserId = post.PublisherUserId;
            //}

            //if (input.CreatorUserId == null)
            //{
            //    input.PublisherUserId = post.PublisherUserId;

            //}

            bool isPublishingNow = !post.IsPublished && input.IsPublished;
            if (isPublishingNow)
            {
                input.PublisherUserId = AbpSession.UserId ?? throw new UserFriendlyException("کاربر وارد نشده است.");
                input.CreatorUserId = post.CreatorUserId;
            }

            //if (input.DatePublished == null)
            //{
            //    input.PublisherUserId = post.PublisherUserId;
            //}
            else
            {
                input.PublisherUserId = post.PublisherUserId;
                input.CreatorUserId = post.CreatorUserId;
            }
            var changes = GetChanges(post, input);
            var currentUserName = await GetCurrentUserName();
            if (await changes != "")
            {
                post.EditHistories.Add(new PostEditHistory
                {
                    EditorName = currentUserName,
                    EditTime = DateTime.Now,
                    Changes = await changes
                });
            }


            ObjectMapper.Map(input, post);
            
            try
            {
                if (!string.IsNullOrEmpty(input.PostFileToken))
                    post.PostFile = await GetBinaryObjectFromCache(input.PostFileToken, post.Id);

            }
            catch (UserFriendlyException ex)
            {
                //ignore
            }

            try
            {
                if (!string.IsNullOrEmpty(input.PostFileToken2))
                    post.PostFile2 = await GetBinaryObjectFromCache(input.PostFileToken2, post.Id);


            }
            catch (UserFriendlyException ex)
            {
                //ignore
            }

            try
            {
                if (!string.IsNullOrEmpty(input.PostFileToken3))
                    post.PostFile3 = await GetBinaryObjectFromCache(input.PostFileToken3, post.Id);

            }
            catch (UserFriendlyException ex)
            {
                //ignore
            }

            try
            {
                if (!string.IsNullOrEmpty(input.PostFileToken4))
                    post.PostFile4 = await GetBinaryObjectFromCache(input.PostFileToken4, post.Id);

            }
            catch (UserFriendlyException ex)
            {
                //ignore
            }

            try
            {
                if (!string.IsNullOrEmpty(input.PostFileToken5))
                    post.PostFile5 = await GetBinaryObjectFromCache(input.PostFileToken5, post.Id);

            }
            catch (UserFriendlyException ex)
            {
                //ignore
            }

            try
            {
                if (!string.IsNullOrEmpty(input.PostFileToken6))
                    post.PostFile6 = await GetBinaryObjectFromCache(input.PostFileToken6, post.Id);

            }
            catch (UserFriendlyException ex)
            {
                //ignore
            }

            try
            {
                if (!string.IsNullOrEmpty(input.PostFileToken7))
                    post.PostFile7 = await GetBinaryObjectFromCache(input.PostFileToken7, post.Id);

            }
            catch (UserFriendlyException ex)
            {
                //ignore
            }

            try
            {
                if (!string.IsNullOrEmpty(input.PostFileToken8))
                    post.PostFile8 = await GetBinaryObjectFromCache(input.PostFileToken8, post.Id);

            }
            catch (UserFriendlyException ex)
            {
                //ignore
            }

            try
            {
                if (!string.IsNullOrEmpty(input.PostFileToken9))
                    post.PostFile9 = await GetBinaryObjectFromCache(input.PostFileToken9, post.Id);

            }
            catch (UserFriendlyException ex)
            {
                //ignore
            }

            try
            {
                if (!string.IsNullOrEmpty(input.PostFileToken10))
                    post.PostFile10 = await GetBinaryObjectFromCache(input.PostFileToken10, post.Id);

            }
            catch (UserFriendlyException ex)
            {
                //ignore
            }

            if (shouldSendSmsNotification)
            {
                //await SendSmsNotification(post,input.OrganizationId);
                await SendSmsNotification(post);

            }

            if (post.PostGroupId.HasValue && post.CurrentPostStatus == PostStatus.Published)
                await PublishNewPostNotifications(post/*,input.OrganizationId*/);

        }

        [AbpAuthorize(AppPermissions.Pages_Posts_Delete)]
        public async Task Delete(EntityDto input)
        { 
            await _postRepository.DeleteAsync(input.Id);
        }

        public async Task<FileDto> GetPostsToExcel(GetAllPostsForExcelInput input)
        {

            var filteredPosts = _postRepository.GetAll()
                .Include(e => e.GroupMemberFk)
                .Include(e => e.PostGroupFk)
                .WhereIf(!string.IsNullOrWhiteSpace(input.Filter),
                    e => false || e.PostCaption.Contains(input.Filter) || e.PostTitle.Contains(input.Filter))
                .WhereIf(!string.IsNullOrWhiteSpace(input.PostCaptionFilter),
                    e => e.PostCaption.Contains(input.PostCaptionFilter))
                .WhereIf(input.IsSpecialFilter.HasValue && input.IsSpecialFilter > -1,
                    e => (input.IsSpecialFilter == 1 && e.IsSpecial) || (input.IsSpecialFilter == 0 && !e.IsSpecial))
                .WhereIf(!string.IsNullOrWhiteSpace(input.PostTitleFilter),
                    e => e.PostTitle.Contains(input.PostTitleFilter))
                .WhereIf(!string.IsNullOrWhiteSpace(input.GroupMemberMemberPositionFilter),
                    e => e.GroupMemberFk != null &&
                         e.GroupMemberFk.MemberPosition == input.GroupMemberMemberPositionFilter)
                .WhereIf(!string.IsNullOrWhiteSpace(input.PostGroupPostGroupDescriptionFilter),
                    e => e.PostGroupFk != null &&
                         e.PostGroupFk.PostGroupDescription == input.PostGroupPostGroupDescriptionFilter)
                .WhereIf(input.OrganizationId.HasValue, e => e.PostGroupFk.OrganizationId == input.OrganizationId);
            var persianCalendar = new PersianCalendar();
            var query = (from o in filteredPosts
                         join o1 in _lookup_groupMemberRepository.GetAll() on o.GroupMemberId equals o1.Id into j1
                         from s1 in j1.DefaultIfEmpty()

                         join o2 in _lookup_postGroupRepository.GetAll() on o.PostGroupId equals o2.Id into j2
                         from s2 in j2.DefaultIfEmpty()

                         select new GetPostForViewDto
                         {
                             Post = new PostDto
                             {
                                 PostFile = o.PostFile,
                                 PostCaption = o.PostCaption,
                                 IsSpecial = o.IsSpecial,
                                 IsPublished = o.IsPublished,
                                 PostTitle = o.PostTitle,
                                 Id = o.Id,
                                 CreationTime = o.CreationTime,
                                 PostRefLink = o.PostRefLink

                             },
                             GroupMemberMemberPosition = s1 == null || s1.MemberPosition == null ? "" : s1.MemberPosition,
                             PostGroupPostGroupDescription = s2 == null || s2.PostGroupDescription == null
                                 ? ""
                                 : s2.PostGroupDescription.ToString(),
                             PersianCreationTime = o.CreationTime != null
                                 ? persianCalendar.GetYear(o.CreationTime).ToString("D4") + "/" +
                                   persianCalendar.GetMonth(o.CreationTime).ToString("D2") + "/" +
                                   persianCalendar.GetDayOfMonth(o.CreationTime).ToString("D2")
                                 : null
                         });
            var postListDtos = await query.ToListAsync();
            return _postsExcelExporter.ExportToFile(postListDtos);
        }

        [AbpAuthorize(AppPermissions.Pages_Posts)]
        public async Task<PagedResultDto<PostGroupMemberLookupTableDto>> GetAllGroupMemberForLookupTable(
            GetAllForLookupTableInput input)
        {
            var query = _lookup_groupMemberRepository.GetAll().Include(x => x.UserFk).WhereIf(
                !string.IsNullOrWhiteSpace(input.Filter),
                e => e.MemberPosition != null && e.MemberPosition.Contains(input.Filter)
            );

            var totalCount = await query.CountAsync();

            var groupMemberList = await query
                .PageBy(input)
                .ToListAsync();

            var lookupTableDtoList = new List<PostGroupMemberLookupTableDto>();
            foreach (var groupMember in groupMemberList)
            {
                lookupTableDtoList.Add(new PostGroupMemberLookupTableDto
                {
                    Id = groupMember.Id,
                    DisplayName = groupMember.UserFk.Name + " " + groupMember.UserFk.Surname
                });
            }

            return new PagedResultDto<PostGroupMemberLookupTableDto>(
                totalCount,
                lookupTableDtoList
            );
        }

        [AbpAuthorize(AppPermissions.Pages_Posts)]
        public async Task<PagedResultDto<PostPostGroupLookupTableDto>> GetAllPostGroupForLookupTable(
            GetAllForLookupTableInput input)
        {
            var query = _lookup_postGroupRepository.GetAll().Include(x => x.OrganizationFk)
                .WhereIf(input.OrganizationId.HasValue, x => x.OrganizationId == input.OrganizationId)
                .WhereIf(
                    !string.IsNullOrWhiteSpace(input.Filter),
                    e => e.PostGroupDescription != null && e.PostGroupDescription.Contains(input.Filter)
                );

            var totalCount = await query.CountAsync();

            var postGroupList = await query
                .PageBy(input)
                .ToListAsync();

            var lookupTableDtoList = new List<PostPostGroupLookupTableDto>();
            foreach (var postGroup in postGroupList)
            {
                lookupTableDtoList.Add(new PostPostGroupLookupTableDto
                {
                    Id = postGroup.Id,
                    DisplayName = postGroup.PostGroupDescription,
                    OrganizationId = postGroup.OrganizationId,
                    OrganizationName = postGroup.OrganizationFk.OrganizationName

                });
            }

            return new PagedResultDto<PostPostGroupLookupTableDto>(
                totalCount,
                lookupTableDtoList
            );
        }

        private async Task<Guid?> GetBinaryObjectFromCache(string fileToken, int? refId)
        {
            try
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

                var storedFile = new BinaryObject(AbpSession.TenantId, fileCache.File, BinarySourceType.Post,
                    fileCache.FileName);
                await _binaryObjectManager.SaveAsync(storedFile);
                if (refId != null) storedFile.SourceId = refId;
                return storedFile.Id;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private async Task<string> GetBinaryFileName(Guid? fileId)
        {
            if (!fileId.HasValue)
            {
                return null;
            }

            var file = await _binaryObjectManager.GetOrNullAsync(fileId.Value);
            return file?.Description;
        }

        [AbpAuthorize(AppPermissions.Pages_Posts_Edit)]
        public async Task RemovePostFileFile(EntityDto input)
        {
            var post = await _postRepository.FirstOrDefaultAsync(input.Id);
            if (post == null)
            {
                throw new UserFriendlyException(L("EntityNotFound"));
            }

            if (!post.PostFile.HasValue)
            {
                throw new UserFriendlyException(L("FileNotFound"));
            }

            await _binaryObjectManager.DeleteAsync(post.PostFile.Value);
            post.PostFile = null;
        }

        public async Task<PagedResultDto<GetPostCategoriesForViewDto>> GetPostCategoriesForView(int organizationId)
        {
            try
            {
                var cat = new List<GetPostCategoriesForViewDto>();
                var queryPostCat = await (from pc in _lookup_postGroupRepository.GetAll().Where(x => !x.IsDeleted)
                                       //join g in organizationRepository.GetAll().Where(x => !x.IsDeleted) on pc.OrganizationId equals g.Id into joiner1
                                       //from g in joiner1.DefaultIfEmpty()
                                       //join gm in _lookup_groupMemberRepository.GetAll() on g.Id equals gm.OrganizationId into joiner2
                                       //from gm in joiner2.DefaultIfEmpty()
                                       //where gm.UserId == AbpSession.UserId
                                   where pc.OrganizationId == organizationId
                                   orderby pc.Ordering
                                   select new
                                   {
                                       pc.Id,
                                       pc.PostGroupDescription,
                                       PostGroupHeaderPicFile = pc.GroupFile,
                                       PostGroupLatestPicFile = _postRepository.GetAll().Where(p => p.PostGroupId == pc.Id)
                                           .OrderByDescending(p => p.CreationTime).FirstOrDefault().PostFile,
                                       
                                   }).ToListAsync();

                foreach (var postCategory in queryPostCat)
                {
                    var filteredPostSubGroups = await _lookup_postSubGroupRepository.GetAll()
                        .Where(p => p.PostGroupId == postCategory.Id).CountAsync();

                    cat.Add(new GetPostCategoriesForViewDto
                    {
                        PostGroupLatestPicFile = postCategory.PostGroupLatestPicFile,
                        PostGroupHeaderPicFile = postCategory.PostGroupHeaderPicFile,
                        Id = postCategory.Id,
                        PostGroupDescription = postCategory.PostGroupDescription,
                        HasSubGroups = filteredPostSubGroups > 0,
                    });
                }

                return await Task.FromResult(new PagedResultDto<GetPostCategoriesForViewDto>(cat.Count, cat));
            }
            catch (Exception ex)
            {
                throw new UserFriendlyException(ex.Message);
            }
        }

        public Task<PagedResultDto<GetPostsForViewDto>> GetPostsForView(int postGroupId)
        {
            try
            {
                var res = new List<GetPostsForViewDto>();
                var filteredPosts = _postRepository.GetAll()
                    .Include(e => e.GroupMemberFk)
                    .Include(e => e.PostGroupFk)
                    .Include(e => e.GroupMemberFk.UserFk)
                    //.WhereIf(input.IsSpecialFilter.HasValue && input.IsSpecialFilter > -1, e => (input.IsSpecialFilter == 1 && e.IsSpecial) || (input.IsSpecialFilter == 0 && !e.IsSpecial))
                    .WhereIf(postGroupId > 0, e => e.PostGroupId == postGroupId);

                foreach (var post in filteredPosts)
                {
                    var datam = new GetPostsForViewDto
                    {
                        //Base64Image = "data:image/png;base64,"+Convert.ToBase64String(postCategory.Bytes, 0, postCategory.Bytes.Length) ,
                        Id = post.Id,
                        GroupMemberId = post.GroupMemberId ?? 0,
                        IsSpecial = post.IsSpecial,
                        IsPublished = post.IsPublished,
                        PostCaption = post.PostCaption,
                        PostFile = post.PostFile,
                        PostTitle = post.PostTitle
                    };
                    if (post.GroupMemberFk != null)
                    {

                        datam.MemberFullName = post.GroupMemberFk.UserFk.FullName;
                        datam.MemberPosition = post.GroupMemberFk.MemberPosition;
                        datam.MemberUserName = post.GroupMemberFk.UserFk.UserName;
                    }

                    res.Add(datam);


                }

                return Task.FromResult(new PagedResultDto<GetPostsForViewDto>(res.Count, res));
            }
            catch (Exception ex)
            {
                throw new UserFriendlyException(ex.Message);
            }
        }

        public Task<PagedResultDto<GetPostsForViewDto>> GetPostsByGroupIdForView(GetPostsByGroupIdInput input)
        {
            try
            {
                var posts = new List<GetPostsForViewDto>();
                var filteredPosts = from p in _postRepository.GetAll().Where(x => !x.IsDeleted)
                        .Include(e => e.GroupMemberFk)
                        .Include(e => e.PostGroupFk)
                        .Include(e => e.GroupMemberFk.UserFk)
                        .Include(e => e.AppBinaryObjectFk)
                        .Include(e => e.AppBinaryObjectFk2)
                        .Include(e => e.AppBinaryObjectFk3)
                        .Include(e => e.AppBinaryObjectFk4)
                        .Include(e => e.AppBinaryObjectFk5)
                        .Include(e => e.AppBinaryObjectFk6)
                        .Include(e => e.AppBinaryObjectFk7)
                        .Include(e => e.AppBinaryObjectFk8)
                        .Include(e => e.AppBinaryObjectFk9)
                        .Include(e => e.AppBinaryObjectFk10)
                        .Where(x => x.PostGroupFk.OrganizationId == input.OrganizationId)
                        .Where(x=>x.IsPublished)
                        .WhereIf(input.PostGroupId > 0, p => p.PostGroupId == input.PostGroupId)
                        .WhereIf(input.PostSubGroupId > 0, p => p.PostSubGroupId == input.PostSubGroupId)
                                    join pg in _lookup_postGroupRepository.GetAll().Where(x => !x.IsDeleted) on p.PostGroupId equals
                                        pg.Id into joiner1
                                    from pg in joiner1.DefaultIfEmpty()
                                    join og in organizationRepository.GetAll().Where(x => !x.IsDeleted) on pg.OrganizationId
                                        equals og.Id into joiner2
                                    from og2 in joiner2.DefaultIfEmpty()
                                        //join gm in _lookup_groupMemberRepository.GetAll() on og2.Id equals gm.OrganizationId into
                                        //joiner3
                                        //from gm2 in joiner3.DefaultIfEmpty()
                                        //where gm2.UserId == AbpSession.UserId
                                    select new
                                    {
                                        p.Id,
                                        p.GroupMemberId,
                                        p.IsSpecial,
                                        p.IsPublished,
                                        p.PostCaption,
                                        p.CreationTime,
                                        p.PostFile,
                                        p.PostFile2,
                                        p.PostFile3,
                                        p.PostFile4,
                                        p.PostFile5,
                                        p.PostFile6,
                                        p.PostFile7,
                                        p.PostFile8,
                                        p.PostFile9,
                                        p.PostFile10,
                                        p.PostTitle,
                                        p.PostRefLink,
                                        p.PostGroupId,
                                        p.GroupMemberFk,
                                        p.PostGroupFk,
                                        p.PostSubGroupFk,
                                        p.AppBinaryObjectFk,
                                        p.AppBinaryObjectFk2,
                                        p.AppBinaryObjectFk3,
                                        p.AppBinaryObjectFk4,
                                        p.AppBinaryObjectFk5,
                                        p.AppBinaryObjectFk6,
                                        p.AppBinaryObjectFk7,
                                        p.AppBinaryObjectFk8,
                                        p.AppBinaryObjectFk9,
                                        p.AppBinaryObjectFk10,

                                    };
                var count = filteredPosts.Count();
                var pagedAndFilteredPosts = filteredPosts
                    .OrderBy(input.Sorting ?? "id desc")
                    .PageBy(input);

                foreach (var post in pagedAndFilteredPosts)
                {
                    var datam = new GetPostsForViewDto
                    {
                        //Base64Image = "data:image/png;base64,"+Convert.ToBase64String(postCategory.Bytes, 0, postCategory.Bytes.Length) ,
                        Id = post.Id,
                        GroupMemberId = post.GroupMemberId ?? 0,
                        IsSpecial = post.IsSpecial,
                        IsPublished = post.IsPublished,
                        PostCaption = post.PostCaption,
                        PostFile = post.PostFile,
                        PostFile2 = post.PostFile2,
                        PostFile3 = post.PostFile3,
                        PostFile4 = post.PostFile4,
                        PostFile5 = post.PostFile5,
                        PostFile6 = post.PostFile6,
                        PostFile7 = post.PostFile7,
                        PostFile8 = post.PostFile8,
                        PostFile9 = post.PostFile9,
                        PostFile10 = post.PostFile10,
                        PostTitle = post.PostTitle,
                        PostGroupId = post.PostGroupId,
                        PostRefLink = post.PostRefLink,
                        CreationTime = post.CreationTime,
                    };
                    try
                    {
                        if (post.GroupMemberFk != null)
                        {
                            datam.MemberFullName = post.GroupMemberFk.UserFk.FullName;
                            datam.MemberPosition = post.GroupMemberFk.MemberPosition;
                            datam.MemberUserName = post.GroupMemberFk.UserFk.UserName;
                        }

                        if (post.PostGroupFk != null)
                        {
                            datam.GroupFile = post.PostGroupFk.GroupFile;
                            datam.GroupDescription = post.PostGroupFk.PostGroupDescription;

                        }

                        if (post.PostSubGroupFk != null)
                        {
                            datam.PostSubGroupDescription = post.PostSubGroupFk.PostSubGroupDescription;
                        }

                        if (post.AppBinaryObjectFk != null)
                        {
                            datam.Attachment1 = post.AppBinaryObjectFk.Description;
                        }

                        if (post.AppBinaryObjectFk2 != null)
                        {
                            datam.Attachment2 = post.AppBinaryObjectFk2.Description;
                        }

                        if (post.AppBinaryObjectFk3 != null)
                        {
                            datam.Attachment3 = post.AppBinaryObjectFk3.Description;
                        }

                        if (post.AppBinaryObjectFk4 != null)
                        {
                            datam.Attachment4 = post.AppBinaryObjectFk4.Description;
                        }

                        if (post.AppBinaryObjectFk5 != null)
                        {
                            datam.Attachment5 = post.AppBinaryObjectFk5.Description;
                        }

                        if (post.AppBinaryObjectFk6 != null)
                        {
                            datam.Attachment6 = post.AppBinaryObjectFk6.Description;
                        }

                        if (post.AppBinaryObjectFk7 != null)
                        {
                            datam.Attachment7 = post.AppBinaryObjectFk7.Description;
                        }

                        if (post.AppBinaryObjectFk8 != null)
                        {
                            datam.Attachment8 = post.AppBinaryObjectFk8.Description;
                        }

                        if (post.AppBinaryObjectFk9 != null)
                        {
                            datam.Attachment9 = post.AppBinaryObjectFk9.Description;
                        }

                        if (post.AppBinaryObjectFk10 != null)
                        {
                            datam.Attachment10 = post.AppBinaryObjectFk10.Description;
                        }

                    }
                    catch (Exception)
                    {
                        //ignored
                    }

                    posts.Add(datam);


                }

                return Task.FromResult(new PagedResultDto<GetPostsForViewDto>(count, posts));
            }
            catch (Exception ex)
            {
                throw new UserFriendlyException(ex.Message);
            }
        }

        public Task<PagedResultDto<GetLikedUsersDto>> GetLikedUsers(GetLikedUsersInput input)
        {
            var query = from pl in _postLikeRepository.GetAll().Include(x => x.UserFk)

                        where pl.PostId == input.PostId
                        select new
                        {
                            UserId = pl.UserFk.Id,
                            pl.UserFk.NationalId,
                            pl.UserFk.IsSuperUser,
                            pl.UserFk.UserName,
                            pl.UserFk.Name,
                            pl.UserFk.Surname,
                            pl.UserFk.PhoneNumber,
                            pl.UserFk.ProfilePictureId,
                            pl.LikeTime
                        };
            var queryCount = query.Count();
            var pagedAndFiltered = query
                .OrderBy(input.Sorting ?? "UserId desc")
                .PageBy(input);
            List<GetLikedUsersDto> users = new List<GetLikedUsersDto>();
            foreach (var row in pagedAndFiltered)
            {
                var datam = new GetLikedUsersDto
                {
                    UserId = row.UserId,
                    Surname = row.Surname,
                    PhoneNumber = row.PhoneNumber,
                    UserName = row.UserName,
                    LikeTime = row.LikeTime,
                    IsSuperUser = row.IsSuperUser,
                    Name = row.Name,
                    NationalId = row.NationalId,
                    ProfilePictureId = row.ProfilePictureId,
                };

                users.Add(datam);


            }

            return Task.FromResult(new PagedResultDto<GetLikedUsersDto>(queryCount, users));

        }

        public Task<PagedResultDto<GetSeenUsersDto>> GetSeenUsers(GetSeenUsersInput input)
        {
            var query = from pl in _seenRepository.GetAll().Include(x => x.UserFk)

                        where pl.PostId == input.PostId
                        select new
                        {
                            UserId = pl.UserFk.Id,
                            pl.UserFk.NationalId,
                            pl.UserFk.IsSuperUser,
                            pl.UserFk.UserName,
                            pl.UserFk.Name,
                            pl.UserFk.Surname,
                            pl.UserFk.PhoneNumber,
                            pl.UserFk.ProfilePictureId,
                            pl.SeenTime
                        };
            var queryCount = query.Count();
            var pagedAndFiltered = query
                .OrderBy(input.Sorting ?? "UserId desc")
                .PageBy(input);
            List<GetSeenUsersDto> users = new List<GetSeenUsersDto>();
            foreach (var row in pagedAndFiltered)
            {
                var datam = new GetSeenUsersDto
                {
                    UserId = row.UserId,
                    Surname = row.Surname,
                    PhoneNumber = row.PhoneNumber,
                    UserName = row.UserName,
                    SeenTime = row.SeenTime,
                    IsSuperUser = row.IsSuperUser,
                    Name = row.Name,
                    NationalId = row.NationalId,
                    ProfilePictureId = row.ProfilePictureId,

                };

                users.Add(datam);


            }

            return Task.FromResult(new PagedResultDto<GetSeenUsersDto>(queryCount, users));

        }

        public async Task<SuperUserDashboardViewDto> GetSuperUserDashboardView()
        {
            var last30Days = DateTime.Now.AddDays(-30);

            // Total counts
            var totalUserCount = await _lookup_groupMemberRepository.GetAll().CountAsync();
            var totalPostCount = await _postRepository.GetAll().CountAsync();
            var totalCommentCount = await _commentRepository.GetAll().CountAsync();
            var totalPostViewCount = await _seenRepository.GetAll().CountAsync();

            // Category count, filtering out null PostGroupFk
            var categoryCount = await _postRepository.GetAll()
                .Where(p => p.PostGroupFk != null)
                .GroupBy(p => new { p.PostGroupFk.Id, p.PostGroupFk.PostGroupDescription })
                .Select(g => new DashboardViewCategoryInfo(g.Key.Id, g.Key.PostGroupDescription, g.Count()))
                .ToListAsync();

            // Top 5 organizations with highest comment counts per day
            var top5CommentCountPerDay = await _commentRepository.GetAll()
                .Where(c => c.CreationTime >= last30Days && c.PostFk.PostGroupFk != null)
                .GroupBy(c => new
                {
                    c.PostFk.PostGroupFk.OrganizationId,
                    c.PostFk.PostGroupFk.OrganizationFk.OrganizationName,
                    Date = c.CreationTime.Date
                })
                .Select(g => new { g.Key.OrganizationId, g.Key.OrganizationName, g.Key.Date, Count = g.Count() })
                .OrderByDescending(g => g.Count)
                .Take(5)
                .ToListAsync();

            var groupedTop5CommentCountPerDay = top5CommentCountPerDay
                .GroupBy(g => g.OrganizationId)
                .Select(g => new DashboardViewOrganizationCount(
                    g.Key ?? 0,
                    g.First().OrganizationName,
                    g.Select(d => new DashboardViewDate(ToPersianDateString(d.Date), d.Count)).ToList()))
                .ToList();

            // Similar adjustments for likes, posts, and views per day
            var top5LikeCountPerDay = await _postLikeRepository.GetAll()
                .Where(l => l.LikeTime >= last30Days && l.PostFk.PostGroupFk != null)
                .GroupBy(l => new
                {
                    l.PostFk.PostGroupFk.OrganizationId,
                    l.PostFk.PostGroupFk.OrganizationFk.OrganizationName,
                    Date = l.LikeTime.Date
                })
                .Select(g => new { g.Key.OrganizationId, g.Key.OrganizationName, g.Key.Date, Count = g.Count() })
                .OrderByDescending(g => g.Count)
                .Take(5)
                .ToListAsync();

            var groupedTop5LikeCountPerDay = top5LikeCountPerDay
                .GroupBy(g => g.OrganizationId)
                .Select(g => new DashboardViewOrganizationCount(
                    g.Key ?? 0,
                    g.First().OrganizationName,
                    g.Select(d => new DashboardViewDate(ToPersianDateString(d.Date), d.Count)).ToList()))
                .ToList();

            var top5PostCountPerDay = await _postRepository.GetAll()
                .Where(p => p.CreationTime >= last30Days && p.PostGroupFk != null)
                .GroupBy(p => new
                {
                    p.PostGroupFk.OrganizationId,
                    p.PostGroupFk.OrganizationFk.OrganizationName,
                    Date = p.CreationTime.Date
                })
                .Select(g => new { g.Key.OrganizationId, g.Key.OrganizationName, g.Key.Date, Count = g.Count() })
                .OrderByDescending(g => g.Count)
                .Take(5)
                .ToListAsync();

            var groupedTop5PostCountPerDay = top5PostCountPerDay
                .GroupBy(g => g.OrganizationId)
                .Select(g => new DashboardViewOrganizationCount(
                    g.Key ?? 0,
                    g.First().OrganizationName,
                    g.Select(d => new DashboardViewDate(ToPersianDateString(d.Date), d.Count)).ToList()))
                .ToList();

            var top5ViewCountPerDay = await _seenRepository.GetAll()
                .Where(s => s.SeenTime >= last30Days && s.PostFk.PostGroupFk != null)
                .GroupBy(s => new
                {
                    s.PostFk.PostGroupFk.OrganizationId,
                    s.PostFk.PostGroupFk.OrganizationFk.OrganizationName,
                    Date = s.SeenTime.Date
                })
                .Select(g => new { g.Key.OrganizationId, g.Key.OrganizationName, g.Key.Date, Count = g.Count() })
                .OrderByDescending(g => g.Count)
                .Take(5)
                .ToListAsync();

            var groupedTop5ViewCountPerDay = top5ViewCountPerDay
                .GroupBy(g => g.OrganizationId)
                .Select(g => new DashboardViewOrganizationCount(
                    g.Key ?? 0,
                    g.First().OrganizationName,
                    g.Select(d => new DashboardViewDate(ToPersianDateString(d.Date), d.Count)).ToList()))
                .ToList();

            var result = new SuperUserDashboardViewDto
            {
                TotalUserCount = totalUserCount,
                TotalPostCount = totalPostCount,
                TotalCommentCount = totalCommentCount,
                TotalPostViewCount = totalPostViewCount,
                CategoryCount = categoryCount,
                Top5CommentCountPerDay = groupedTop5CommentCountPerDay,
                Top5LikeCountPerDay = groupedTop5LikeCountPerDay,
                Top5PostCountPerDay = groupedTop5PostCountPerDay,
                Top5ViewCountPerDay = groupedTop5ViewCountPerDay
            };

            return result;
        }



        private static string ToPersianDateString(DateTime date)
        {
            PersianCalendar persianCalendar = new PersianCalendar();
            return string.Format("{0:0000}/{1:00}/{2:00}",
                persianCalendar.GetYear(date),
                persianCalendar.GetMonth(date),
                persianCalendar.GetDayOfMonth(date));
        }

        public async Task<OrganizationDashboardViewDto> GetOrganizationDashboardView(int? organizationId)
        {
            if (organizationId == 0) organizationId = null;
            var last30Days = DateTime.Now.AddDays(-30);

            // Total counts
            var totalUserCount = await _lookup_groupMemberRepository.GetAll()
                .Where(u => !organizationId.HasValue || u.OrganizationId == organizationId)
                .CountAsync();
            var totalPostCount = await _postRepository.GetAll()
                .Where(p => !organizationId.HasValue || p.PostGroupFk.OrganizationId == organizationId)
                .CountAsync();
            var totalCommentCount = await _commentRepository.GetAll()
                .Where(c => !organizationId.HasValue || c.PostFk.PostGroupFk.OrganizationId == organizationId)
                .CountAsync();
            var totalPostViewCount = await _seenRepository.GetAll()
                .Where(s => !organizationId.HasValue || s.PostFk.PostGroupFk.OrganizationId == organizationId)
                .CountAsync();

            // Category count, filtering out null PostGroupFk
            var categoryCount = await _postRepository.GetAll()
                .Where(p => p.PostGroupFk != null &&
                            (!organizationId.HasValue || p.PostGroupFk.OrganizationId == organizationId))
                .GroupBy(p => new { p.PostGroupFk.Id, p.PostGroupFk.PostGroupDescription })
                .Select(g => new DashboardViewCategoryInfo(g.Key.Id, g.Key.PostGroupDescription, g.Count()))
                .ToListAsync();

            // Comments per day
            var commentCountPerDay = await _commentRepository.GetAll()
                .Where(c => c.CreationTime >= last30Days && c.PostFk.PostGroupFk != null && (!organizationId.HasValue ||
                    c.PostFk.PostGroupFk.OrganizationId == organizationId))
                .GroupBy(c => c.CreationTime.Date)
                .Select(g => new DashboardViewDate(ToPersianDateString(g.Key), g.Count()))
                .ToListAsync();

            // Likes per day
            var likeCountPerDay = await _postLikeRepository.GetAll()
                .Where(l => l.LikeTime >= last30Days && l.PostFk.PostGroupFk != null && (!organizationId.HasValue ||
                    l.PostFk.PostGroupFk.OrganizationId == organizationId))
                .GroupBy(l => l.LikeTime.Date)
                .Select(g => new DashboardViewDate(ToPersianDateString(g.Key), g.Count()))
                .ToListAsync();

            // Posts per day
            var postCountPerDay = await _postRepository.GetAll()
                .Where(p => p.CreationTime >= last30Days && p.PostGroupFk != null &&
                            (!organizationId.HasValue || p.PostGroupFk.OrganizationId == organizationId))
                .GroupBy(p => p.CreationTime.Date)
                .Select(g => new DashboardViewDate(ToPersianDateString(g.Key), g.Count()))
                .ToListAsync();

            // Views per day
            var viewCountPerDay = await _seenRepository.GetAll()
                .Where(s => s.SeenTime >= last30Days && s.PostFk.PostGroupFk != null && (!organizationId.HasValue ||
                    s.PostFk.PostGroupFk.OrganizationId == organizationId))
                .GroupBy(s => s.SeenTime.Date)
                .Select(g => new DashboardViewDate(ToPersianDateString(g.Key), g.Count()))
                .ToListAsync();

            var result = new OrganizationDashboardViewDto
            {
                TotalUserCount = totalUserCount,
                TotalPostCount = totalPostCount,
                TotalCommentCount = totalCommentCount,
                TotalPostViewCount = totalPostViewCount,
                CategoryCount = categoryCount,
                CommentCountPerDay = commentCountPerDay,
                LikeCountPerDay = likeCountPerDay,
                PostCountPerDay = postCountPerDay,
                ViewCountPerDay = viewCountPerDay
            };

            return result;



        }

        private async Task<string> GetChanges(Post post, CreateOrEditPostDto input)
        {
            var changes = new List<string>();

            if (post.PostTitle != input.PostTitle)
                changes.Add($"عنوان از {post.PostTitle} به {input.PostTitle} تغییر کرد.");

            if (post.PostCaption != input.PostCaption)
                changes.Add($"متن از {post.PostCaption} به {input.PostCaption} تغییر کرد.");
            //if (post.PostCaption != input.PostCaption)
            //    changes.Add($"سازمان از {post.AppBinaryObjectFk4} به {input.PostCaption} تغییر کرد.");
            if (post.PostGroupId != input.PostGroupId)
            {
                var newPostGroup = await _lookup_postGroupRepository.FirstOrDefaultAsync(pg => pg.Id == input.PostGroupId.Value);
                string newGroupDescription = newPostGroup?.PostGroupDescription ?? "نامشخص";
                changes.Add($"گروه خبری از {post.PostGroupFk.PostGroupDescription} به {input.PostCaption} تغییر کرد.");
            }

            if (post.PostSubGroupId != input.PostSubGroupId)
            {
                var newSubGroup = await _lookup_postSubGroupRepository.FirstOrDefaultAsync(sg => sg.Id == input.PostSubGroupId);
                string newSubGroupDescription = newSubGroup?.PostSubGroupDescription ?? "نامشخص";
                changes.Add($"زیر گروه خبری از {post.PostSubGroupFk.PostSubGroupDescription} به {input.PostCaption} تغییر کرد.");
            }

            if (post.PostFile != input.PostFile)
                changes.Add($"فایل اول از {post.PostFile} به {input.PostFile} تغییر کرد.");
            if (post.PostFile2 != input.PostFile2)
                changes.Add($"فایل دوم از {post.PostFile2} به {input.PostFile2} تغییر کرد.");
            if (post.PostFile3 != input.PostFile3)
                changes.Add($"فایل سوم از {post.PostFile3} به {input.PostFile3} تغییر کرد.");
            if (post.PostFile4 != input.PostFile4)
                changes.Add($"فایل چهارم از {post.PostFile4} به {input.PostFile4} تغییر کرد.");
            if (post.PostFile5 != input.PostFile5)
                changes.Add($"فایل پنجم از {post.PostFile5} به {input.PostFile5} تغییر کرد.");
            if (post.PostFile6 != input.PostFile6)
                changes.Add($"فایل ششم از {post.PostFile6} به {input.PostFile6} تغییر کرد.");
            if (post.PostFile7 != input.PostFile7)
                changes.Add($"فایل هفتم از {post.PostFile7} به {input.PostFile7} تغییر کرد.");
            if (post.PostFile8 != input.PostFile8)
                changes.Add($"فایل هشتم از {post.PostFile8} به {input.PostFile8} تغییر کرد.");
            if (post.PostFile9 != input.PostFile9)
                changes.Add($"فایل نهم از {post.PostFile9} به {input.PostFile9} تغییر کرد.");
            if (post.PostFile10 != input.PostFile10)
                changes.Add($"فایل دهم از {post.PostFile10} به {input.PostFile10} تغییر کرد.");
            if (post.PostRefLink != input.PostRefLink)
                changes.Add($"لینک خبر از {post.PostRefLink} به {input.PostRefLink} تغییر کرد.");
            if (post.PostComment != input.PostComment)
                changes.Add($"نظر از {post.PostComment} به {input.PostComment} تغییر کرد.");
            //if (post.IsSpecial != input.IsSpecial)
            //    changes.Add($"ویژه بودن خبر از {post.PostCaption} به {input.PostCaption} تغییر کرد.");
            if (post.IsSpecial == true && input.IsSpecial == false)
                changes.Add("اصالت خبر رد شد.");
            if (post.IsSpecial == false && input.IsSpecial == true)
                changes.Add("اصالت خبر تائيد شد.");
            if (post.IsPublished == true && input.IsPublished == false)
                changes.Add("خبر از حالت انتشار خارج شد.");
            if (post.IsPublished == false && input.IsPublished == true)
                changes.Add("خبر منتشر شد.");

            return string.Join("\n", changes);
        }

        private async Task<string> GetCurrentUserName()
        {
            var user = await UserManager.GetUserAsync(AbpSession.ToUserIdentifier());
            return user?.FullName ?? "Unknown User";
        }

        public async Task<List<User>> GetUsersByOrganizationAsync(int organizationId)
        {
            var users = await _lookup_groupMemberRepository.GetAll()
                .Where(gm => gm.OrganizationId == organizationId) 
                .Select(gm => gm.UserFk) 
                .ToListAsync();

            return users;
        }


    }
}