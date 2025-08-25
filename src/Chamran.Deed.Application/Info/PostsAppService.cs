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
using System.IO;
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
using System.Diagnostics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting.Internal;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System.Threading;

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
        private readonly IRepository<GroupMember> _groupMemberRepository;

        private readonly ITempFileCacheManager _tempFileCacheManager;
        private readonly IBinaryObjectManager _binaryObjectManager;
        private readonly IRepository<Organization> organizationRepository;
        private readonly IAppNotifier _appNotifier;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly IWebHostEnvironment _hostingEnvironment;

        private readonly IRepository<UserRole, long> _userRoleRepository;
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
            IRepository<User, long> userRepository, IDbContextProvider<DeedDbContext> dbContextProvider, ISmsSender smsSender,
            IRepository<PostEditHistory> postEditHistoryRespoRepository, IRepository<UserRole, long> userRoleRepository, IRepository<GroupMember> groupMemberRepository
            , IWebHostEnvironment hostingEnvironment)
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
            _groupMemberRepository = groupMemberRepository;
            _hostingEnvironment = hostingEnvironment;
        }

        public async Task<PagedResultDto<GetPostForViewDto>> GetAll(GetAllPostsInput input)
        {
            var currentUser = await _userRepository.GetAsync(AbpSession.UserId.Value);
            if (currentUser.UserType != AccountUserType.SuperAdmin)
            {

                var currentUserOrgQuery = from x in _groupMemberRepository.GetAll()//.Include(x => x.OrganizationFk)
                                          where x.UserId == currentUser.Id
                                          select x.OrganizationId;
                if (!currentUserOrgQuery.Contains(input.OrganizationId))
                {
                    throw new UserFriendlyException("سازمان انتخابی به این کاربر تعلق ندارد");
                }

            }

            //User user = GetCurrentUser();

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

                var postIds = queryResult.Select(p => p.Id).ToList();
                var pdfLookup = await _postRepository.GetAll()
                    .Where(p => postIds.Contains(p.Id))
                    .Select(p => new { p.Id, p.PdfFile })
                    .ToDictionaryAsync(x => x.Id, x => x.PdfFile);

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
                        CreatorUserId = post.First().CreatorUserId,
                        DatePublished = post.First().DatePublished,
                        PublisherUserFirstName = post.First().PublisherUserFirstName,
                        PublisherUserLastName = post.First().PublisherUserLastName,
                        PublisherUserName = post.First().PublisherUserName,
                        CreatorUserFirstName = post.First().CreatorUserFirstName,
                        CreatorUserLastName = post.First().CreatorUserLastName,
                        CreatorUserName = post.First().CreatorUserName,
                        PostSubGroupId = post.First().PostSubGroupId,
                        PdfFile = pdfLookup.ContainsKey(post.Key) ? pdfLookup[post.Key] : null
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

                foreach (var item in result)
                {
                    if (item.Post.PdfFile.HasValue)
                        item.Post.PdfFileFileName = await GetBinaryFileName(item.Post.PdfFile);
                }

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
            var currentUser = await _userRepository.GetAsync(AbpSession.UserId.Value);
            if (currentUser.UserType != AccountUserType.SuperAdmin)
            {

                var currentUserOrgQuery = from x in _groupMemberRepository.GetAll()//.Include(x => x.OrganizationFk)
                                          where x.UserId == currentUser.Id
                                          select x.OrganizationId;

                var userQuery = from x in _groupMemberRepository.GetAll()
                                join y in _postRepository.GetAll() on x.Id equals y.GroupMemberId
                                where y.Id == input.Id && currentUserOrgQuery.Contains(x.OrganizationId)
                                select x;
                if (!userQuery.Any())
                {
                    throw new UserFriendlyException("پست انتخابی متعلق به هیچ یک از سازمان های شما نمی باشد");
                }

            }

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
            output.PdfFileFileName = await GetBinaryFileName(post.PdfFile);
            return output;
        }

        [IgnoreAntiforgeryToken]
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

        //[AbpAuthorize(AppPermissions.Pages_Posts_Create)]
        //protected virtual async Task Create(CreateOrEditPostDto input)
        //{
        //    using var unitOfWork = _unitOfWorkManager.Begin();
        //    var grpMemberId = await _lookup_groupMemberRepository.GetAll()
        //        .Where(x => x.UserId == AbpSession.UserId)
        //        .FirstOrDefaultAsync();

        //    if (grpMemberId == null)
        //    {
        //        throw new UserFriendlyException("کاربر حاضر به هیچ سازمانی تعلق ندارد");
        //    }

        //    var post = ObjectMapper.Map<Post>(input);
        //    var userRole = await _userRoleRepository.FirstOrDefaultAsync(ur => ur.UserId == AbpSession.UserId);
        //    if (userRole == null)
        //    {
        //        Console.WriteLine("Current user role not found.");
        //        return;
        //    }

        //    post.CreatorUserId = GetCurrentUserAsync().Result.Id;
        //    post.GroupMemberId = grpMemberId.Id;

        //    await _postRepository.InsertAsync(post);

        //    post.PostFile = await GetBinaryObjectFromCache(input.PostFileToken, post.Id);
        //    post.PostFile2 = await GetBinaryObjectFromCache(input.PostFileToken2, post.Id);
        //    post.PostFile3 = await GetBinaryObjectFromCache(input.PostFileToken3, post.Id);
        //    post.PostFile4 = await GetBinaryObjectFromCache(input.PostFileToken4, post.Id);
        //    post.PostFile5 = await GetBinaryObjectFromCache(input.PostFileToken5, post.Id);
        //    post.PostFile6 = await GetBinaryObjectFromCache(input.PostFileToken6, post.Id);
        //    post.PostFile7 = await GetBinaryObjectFromCache(input.PostFileToken7, post.Id);
        //    post.PostFile8 = await GetBinaryObjectFromCache(input.PostFileToken8, post.Id);
        //    post.PostFile9 = await GetBinaryObjectFromCache(input.PostFileToken9, post.Id);
        //    post.PostFile10 = await GetBinaryObjectFromCache(input.PostFileToken10, post.Id);
        //    if (post.PostFile == null && post.PostFile2 == null && post.PostFile3 == null && post.PostFile4 == null &&
        //        post.PostFile5 == null && post.PostFile6 == null && post.PostFile7 == null && post.PostFile8 == null &&
        //        post.PostFile9 == null && post.PostFile10 == null)
        //        throw new UserFriendlyException("پست ارسالی هیچش مدیایی ندارد");
        //    //await _unitOfWorkManager.Current.SaveChangesAsync();

        //    //For Creating Thumbnail(Pictures)
        //    var file = post.AppBinaryObjectFk;
        //    if (file != null && !string.IsNullOrEmpty(file.Description))
        //    {
        //        var ext = Path.GetExtension(file.Description).ToLowerInvariant();
        //        var webRoot = _hostingEnvironment.WebRootPath;

        //        if (ext is ".jpg" or ".jpeg" or ".png")
        //        {
        //            var thumbPath = await GenerateThumbnailAsync(file.Bytes, post.Id, webRoot);
        //            post.PostFileThumb = thumbPath;
        //        }
        //        else if (ext is ".mp4" or ".mov")
        //        {
        //            var videosDir = Path.Combine(webRoot, "videos");
        //            Directory.CreateDirectory(videosDir);

        //            var fullPath = Path.Combine(videosDir, $"{post.Id}{ext}");
        //            await File.WriteAllBytesAsync(fullPath, file.Bytes);

        //            var previewPath = await GenerateVideoPreviewAsync(fullPath, webRoot, post.Id);
        //            post.PostVideoPreview = previewPath;
        //        }
        //        else if (ext == ".pdf")
        //        {
        //            // Do nothing yet
        //        }
        //    }


        //    try
        //    {
        //        await _unitOfWorkManager.Current.SaveChangesAsync();
        //    }
        //    catch (DbUpdateException ex)
        //    {
        //        Console.WriteLine($"DbUpdateException: {ex.Message}");

        //        if (ex.InnerException != null)
        //        {
        //            Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
        //        }

        //        if (ex.Entries != null && ex.Entries.Any())
        //        {
        //            foreach (var entry in ex.Entries)
        //            {
        //                Console.WriteLine($"Entity: {entry.Entity.GetType().Name}, State: {entry.State}");
        //            }
        //        }

        //        throw; // برای مشاهده StackTrace
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"General Exception: {ex.Message}");
        //        throw;
        //    }


        //    await unitOfWork.CompleteAsync();
        //    if (post.PostGroupId.HasValue && post.CurrentPostStatus == PostStatus.Published)
        //    {
        //        await PublishNewPostNotifications(post/*, input.OrganizationId*/);
        //    }

        //    //var currentUser = GetCurrentUserAsync().Result;

        //    //if (currentUser.UserType == AccountUserType.Distributer 
        //    //    || currentUser.UserType == AccountUserType.Admin 
        //    //    || currentUser.UserType == AccountUserType.SuperAdmin 
        //    //    || currentUser.IsSuperUser)
        //    //{
        //    //    post.IsPublished = true;
        //    //    post.CurrentPostStatus = PostStatus.Published;
        //    //    //await SendSmsNotification(post,input.OrganizationId);

        //    //}




        //    //if (userRole.RoleId == 5 || userRole.RoleId == 2 || GetCurrentUserAsync().Result.IsSuperUser)
        //    //{
        //    //    await SendSmsNotification(post);
        //    //}
        //    //else if (userRole.RoleId == 4)
        //    //{
        //    //    post.IsPublished = false;
        //    //    post.CurrentPostStatus = PostStatus.Pending;
        //    //    await PublishNewPostNotifications(post,organizationId:input.OrganizationId);
        //    //}
        //    //else
        //    //{
        //    //    throw new UnauthorizedAccessException("شما اجازه انتشار خبر را نداريد.");
        //    //}

        //    //await SendSmsNotification(post,input.OrganizationId);
        //    await SendSmsNotification(post);

        //}

        [AbpAuthorize(AppPermissions.Pages_Posts_Create)]
        protected virtual async Task Create(CreateOrEditPostDto input, CancellationToken ct = default)
        {
            using var unitOfWork = _unitOfWorkManager.Begin();
            try
            {
                NormalizePdfFileToken(input);
                var grpMemberId = await _lookup_groupMemberRepository.GetAll()
                    .Where(x => x.UserId == AbpSession.UserId)
                    .Select(x => x.Id)
                    .FirstOrDefaultAsync();

                if (grpMemberId == 0)
                    throw new UserFriendlyException("کاربر حاضر به هیچ سازمانی تعلق ندارد");

                var currentUser = await GetCurrentUserAsync();
                if (currentUser.UserType == AccountUserType.Normal)
                    throw new UserFriendlyException("شما اجازه ایجاد خبر را ندارید.");

                var post = ObjectMapper.Map<Post>(input);
                post.CreatorUserId = currentUser.Id;
                post.GroupMemberId = grpMemberId;

                if (currentUser.UserType == AccountUserType.Creator)
                {
                    post.IsPublished = false;
                    post.CurrentPostStatus = PostStatus.Pending;
                }

                await _postRepository.InsertAsync(post);
                await ProcessAllFilesAsync(post, input, mainRequired: true, ct);

                await _unitOfWorkManager.Current.SaveChangesAsync();
                await unitOfWork.CompleteAsync();

                if (post.PostGroupId.HasValue && post.CurrentPostStatus == PostStatus.Published)
                    await PublishNewPostNotifications(post);

                await SendSmsNotification(post);
            }
            catch (UserFriendlyException) { throw; }
            catch (OperationCanceledException) { throw new UserFriendlyException("عملیات ایجاد پست لغو شد."); }
            catch (Exception ex)
            {
                Logger?.Error("Create(Post) failed", ex);
                throw new UserFriendlyException("خطای غیرمنتظره در ایجاد پست.");
            }
        }

        private async Task ProcessAllFilesAsync(Post post, CreateOrEditPostDto input, bool mainRequired, CancellationToken ct = default)
        {
            var orderedTokens = new List<(string token, bool explicitPdf)>
    {
        (input.PostFileToken, false),
        (input.PostFileToken2, false),
        (input.PostFileToken3, false),
        (input.PostFileToken4, false),
        (input.PostFileToken5, false),
        (input.PostFileToken6, false),
        (input.PostFileToken7, false),
        (input.PostFileToken8, false),
        (input.PostFileToken9, false),
        (input.PostFileToken10, false),
        (input.PdfFileToken, true) 
    };

            var sawAnyMedia = post.PostFile.HasValue; 
            var firstMediaHandled = post.PostFile.HasValue; 

            foreach (var (token, explicitPdf) in orderedTokens)
            {
                if (string.IsNullOrWhiteSpace(token))
                    continue;

                if (explicitPdf || IsPdfToken(token))
                {
                    if (!post.PdfFile.HasValue)
                    {
                        var pdfId = await GetBinaryId(token, post.Id);
                        if (pdfId.HasValue)
                            post.PdfFile = pdfId.Value;  
                    }
                    continue; 
                }

                var bin = await SaveAndGetBinaryObject(token, post.Id);
                if (bin == null || bin.Bytes == null || bin.Bytes.Length == 0)
                    continue;

                var ext = (Path.GetExtension(bin.Description ?? string.Empty) ?? string.Empty)
                          .Trim().ToLowerInvariant();
                if (IsPdf(bin.Bytes) || ext == ".pdf")
                {
                    if (!post.PdfFile.HasValue)
                        post.PdfFile = bin.Id;
                    continue;
                }

                var idx = GetNextMediaIndex(post);
                if (idx == -1)
                    break; 

                SetMediaByIndex(post, idx, bin.Id);
                sawAnyMedia = true;

                if (!firstMediaHandled && idx == 0)
                {
                    await BuildThumbOrPreviewForFirstMediaAsync(post, bin, ext, ct);
                    firstMediaHandled = true;
                }
            }

            if (mainRequired && !sawAnyMedia && !post.PdfFile.HasValue)
                throw new UserFriendlyException("فایل اصلی وجود ندارد");
        }

        private static int GetNextMediaIndex(Post post)
        {
            if (!post.PostFile.HasValue) return 0;
            if (!post.PostFile2.HasValue) return 1;
            if (!post.PostFile3.HasValue) return 2;
            if (!post.PostFile4.HasValue) return 3;
            if (!post.PostFile5.HasValue) return 4;
            if (!post.PostFile6.HasValue) return 5;
            if (!post.PostFile7.HasValue) return 6;
            if (!post.PostFile8.HasValue) return 7;
            if (!post.PostFile9.HasValue) return 8;
            if (!post.PostFile10.HasValue) return 9;
            return -1;
        }

        private static void SetMediaByIndex(Post post, int idx, Guid id)
        {
            switch (idx)
            {
                case 0: post.PostFile = id; break;
                case 1: post.PostFile2 = id; break;
                case 2: post.PostFile3 = id; break;
                case 3: post.PostFile4 = id; break;
                case 4: post.PostFile5 = id; break;
                case 5: post.PostFile6 = id; break;
                case 6: post.PostFile7 = id; break;
                case 7: post.PostFile8 = id; break;
                case 8: post.PostFile9 = id; break;
                case 9: post.PostFile10 = id; break;
            }
        }

        private async Task BuildThumbOrPreviewForFirstMediaAsync(Post post, BinaryObject bin, string ext, CancellationToken ct)
        {
            var webRoot = _hostingEnvironment.WebRootPath;
            if (ext is ".jpg" or ".jpeg" or ".png" or ".webp")
            {
                post.PostFileThumb = await GenerateThumbnailAsync(bin.Bytes, post.Id, webRoot);
            }
            else if (ext is ".mp4" or ".mov" or ".m4v" or ".avi" or ".mkv")
            {
                var tempVideosDir = Path.Combine(webRoot, "videos");
                Directory.CreateDirectory(tempVideosDir);
                var fullVideoPath = Path.Combine(tempVideosDir, $"{post.Id}{ext}");
                await File.WriteAllBytesAsync(fullVideoPath, bin.Bytes, ct);
                post.PostVideoPreview = await GenerateVideoPreviewAsync(fullVideoPath, webRoot, post.Id);
            }
        }




        private async Task<Guid?> GetBinaryId(string token, int postId)
        {
            if (string.IsNullOrEmpty(token)) return null;

            var fileId = await GetBinaryObjectFromCache(token, postId);
            return fileId;
        }

        private async Task ProcessMainFileAsync(Post post, string token, bool required)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                if (required && !post.PostFile.HasValue)
                    throw new UserFriendlyException("فایل اصلی وجود ندارد");
                return;
            }

            var mainFile = await SaveAndGetBinaryObject(token, post.Id);
            if (mainFile == null)
                throw new UserFriendlyException("فایل اصلی وجود ندارد");

            if (IsPdf(mainFile.Bytes))
            {
                if (!post.PdfFile.HasValue)
                    post.PdfFile = mainFile.Id;
                return;
            }

            post.PostFile = mainFile.Id;

            var ext = Path.GetExtension(mainFile.Description ?? string.Empty).ToLowerInvariant();
            var webRoot = _hostingEnvironment.WebRootPath;

            if (ext is ".jpg" or ".jpeg" or ".png")
            {
                post.PostFileThumb = await GenerateThumbnailAsync(mainFile.Bytes, post.Id, webRoot);
            }
            else if (ext is ".mp4" or ".mov")
            {
                var videosDir = Path.Combine(webRoot, "videos");
                Directory.CreateDirectory(videosDir);

                var fullVideoPath = Path.Combine(videosDir, $"{post.Id}{ext}");
                await File.WriteAllBytesAsync(fullVideoPath, mainFile.Bytes);

                post.PostVideoPreview = await GenerateVideoPreviewAsync(fullVideoPath, webRoot, post.Id);
            }

            //Guid? pdfId = null;
            //if (!string.IsNullOrWhiteSpace(input.PdfFileToken))
            //{
            //    var pdfFile = await SaveAndGetBinaryObject(input.PdfFileToken, post.Id);
            //    if (pdfFile == null)
            //        throw new UserFriendlyException("فایل PDF معتبر نیست یا در حافظه موقت یافت نشد.");

            //    // ذخیره فایل PDF در دیتابیس قبل از ست کردن به Post
            //    await _unitOfWorkManager.Current.SaveChangesAsync();

            //    post.PdfFile = pdfFile.Id;
            //}

            ////if (!string.IsNullOrWhiteSpace(input.PdfFileToken))
            ////{
            ////    var pdfFile = await SaveAndGetBinaryObject(input.PdfFileToken, post.Id);
            ////    if (pdfFile == null)
            ////        throw new UserFriendlyException("فایل PDF معتبر نیست یا در حافظه موقت یافت نشد.");

            ////    post.PdfFile = pdfFile.Id;
            ////}

            //var fileTokens = new[] {
            //    input.PostFileToken2, input.PostFileToken3, input.PostFileToken4,
            //    input.PostFileToken5, input.PostFileToken6, input.PostFileToken7,
            //    input.PostFileToken8, input.PostFileToken9, input.PostFileToken10
            //};

            //var binaryIds = await Task.WhenAll(fileTokens.Select(token => GetBinaryId(token, post.Id)));


            //post.PostFile2 = binaryIds.ElementAtOrDefault(0);
            //post.PostFile3 = binaryIds.ElementAtOrDefault(1);
            //post.PostFile4 = binaryIds.ElementAtOrDefault(2);
            //post.PostFile5 = binaryIds.ElementAtOrDefault(3);
            //post.PostFile6 = binaryIds.ElementAtOrDefault(4);
            //post.PostFile7 = binaryIds.ElementAtOrDefault(5);
            //post.PostFile8 = binaryIds.ElementAtOrDefault(6);
            //post.PostFile9 = binaryIds.ElementAtOrDefault(7);
            //post.PostFile10 = binaryIds.ElementAtOrDefault(8);

            //if (pdfId != null && pdfId != Guid.Empty)
            //    post.PdfFile = pdfId;

            //await _unitOfWorkManager.Current.SaveChangesAsync();
            //await unitOfWork.CompleteAsync();

            //if (post.PostGroupId.HasValue && post.CurrentPostStatus == PostStatus.Published)
            //    await PublishNewPostNotifications(post);

            //await SendSmsNotification(post);

        }
        private async Task ProcessPdfFileAsync(Post post, string token)
        {
            var pdfId = await GetBinaryId(token, post.Id);
            if (pdfId.HasValue)
                post.PdfFile = pdfId.Value;
        }


        //private async Task<Guid?> GetBinaryId(string token, int postId)
        private async Task ProcessAdditionalFilesAsync(Post post, CreateOrEditPostDto input)
        {
            //if (string.IsNullOrEmpty(token)) return null;
            var tokens = new[]
            {
                input.PostFileToken2,
                input.PostFileToken3,
                input.PostFileToken4,
                input.PostFileToken5,
                input.PostFileToken6,
                input.PostFileToken7,
                input.PostFileToken8,
                input.PostFileToken9,
                input.PostFileToken10
            };

            //var fileId = await GetBinaryObjectFromCache(token, postId);
            //return fileId;
            var setters = new Action<Guid?>[]
            {
                id => post.PostFile2 = id,
                id => post.PostFile3 = id,
                id => post.PostFile4 = id,
                id => post.PostFile5 = id,
                id => post.PostFile6 = id,
                id => post.PostFile7 = id,
                id => post.PostFile8 = id,
                id => post.PostFile9 = id,
                id => post.PostFile10 = id
            };

            //for (int i = 0; i < tokens.Length; i++)
            var fileIndex = 0;
            for (var i = 0; i < tokens.Length; i++)
            {
                //var id = await GetBinaryId(tokens[i], post.Id);
                //if (id.HasValue)
                //    setters[i](id.Value);
                var token = tokens[i];
                if (string.IsNullOrWhiteSpace(token))
                    continue;

                if (IsPdfToken(token))
                {
                    if (!post.PdfFile.HasValue)
                    {
                        var pdfId = await GetBinaryId(token, post.Id);
                        if (pdfId.HasValue)
                            post.PdfFile = pdfId.Value;
                    }
                    continue;
                }

                var id = await GetBinaryId(token, post.Id);
                if (id.HasValue && fileIndex < setters.Length)
                    setters[fileIndex++](id.Value);
            }
        }



        private async Task<string> GenerateThumbnailAsync(byte[] imageBytes, int postId, string webRoot)
        {
            var thumbnailsDir = Path.Combine(webRoot, "thumbnails");
            Directory.CreateDirectory(thumbnailsDir);

            var thumbnailPath = Path.Combine(thumbnailsDir, $"{postId}.jpg");

            using var inputStream = new MemoryStream(imageBytes);
            using var image = SixLabors.ImageSharp.Image.Load(inputStream);

            const int thumbnailWidth = 300;
            var ratio = (double)thumbnailWidth / image.Width;
            var thumbnailHeight = (int)(image.Height * ratio);

            image.Mutate(x => x.Resize(thumbnailWidth, thumbnailHeight));

            await using var outputStream = new FileStream(thumbnailPath, FileMode.Create);
            await image.SaveAsJpegAsync(outputStream);

            // نسبت به ریشه‌ی سایت
            return $"/thumbnails/{postId}.jpg";
        }

        private async Task<string> GenerateVideoPreviewAsync(string inputPath, string webRoot, int postId)
        {
            var previewsDir = Path.Combine(webRoot, "previews");
            Directory.CreateDirectory(previewsDir);

            var previewPath = Path.Combine(previewsDir, $"{postId}.gif");

            // ffmpeg command to generate GIF from the first 5 seconds
            var ffmpegCmd = $"ffmpeg -y -i \"{inputPath}\" -ss 0 -t 5 -vf \"fps=10,scale=320:-1:flags=lanczos\" -loop 0 \"{previewPath}\"";

            var processInfo = new ProcessStartInfo("bash", $"-c \"{ffmpegCmd}\"")
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(processInfo);
            if (process == null)
                throw new Exception("اجرای ffmpeg برای ساخت GIF ناموفق بود");

            string stderr = await process.StandardError.ReadToEndAsync();
            string stdout = await process.StandardOutput.ReadToEndAsync();
            await process.WaitForExitAsync();

            if (process.ExitCode != 0)
            {
                Console.WriteLine("FFmpeg error:\n" + stderr);
                throw new Exception("خطا در ساخت پیش‌نمایش گیف ویدیو");
            }

            return $"/previews/{postId}.gif";
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
                                    await _smsSender.SendAsync(publisherUser.PhoneNumber, "پست جدیدی در انتظار بررسی است.");
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

            //var allTokensForCheck = new[] {
            //    input.PostFileToken, input.PostFileToken2, input.PostFileToken3, input.PostFileToken4,
            //    input.PostFileToken5, input.PostFileToken6, input.PostFileToken7, input.PostFileToken8,
            //    input.PostFileToken9, input.PostFileToken10, input.PdfFileToken
            //};

            //var pdfCount = allTokensForCheck.Count(t => GetFileExtensionFromToken(t) == ".pdf");
            //if (pdfCount > 1)
            //    throw new UserFriendlyException("حداکثر یک فایل PDF مجاز است");

            //if (GetFileExtensionFromToken(input.PostFileToken) == ".pdf")
            //    throw new UserFriendlyException("فایل اصلی نمی‌تواند PDF باشد");
            NormalizePdfFileToken(input);

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
                //input.PublisherUserId = AbpSession.UserId ?? throw new UserFriendlyException("کاربر وارد نشده است.");
                var currentUser = await GetCurrentUserAsync();
                if (currentUser.UserType is not (AccountUserType.Distributer or AccountUserType.Admin or AccountUserType.SuperAdmin))
                    throw new UserFriendlyException("شما اجازه انتشار خبر را ندارید.");

                input.PublisherUserId = currentUser.Id;
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

            // --- KEY FIX: Await GetChanges first, then GetCurrentUserName ---
            var changes = await GetChanges(post, input);
            var currentUserName = await GetCurrentUserName();

            if (changes != "")
            {
                post.EditHistories.Add(new PostEditHistory
                {
                    EditorName = currentUserName,
                    EditTime = DateTime.Now,
                    Changes = changes
                });
            }

            //input.PdfFile = post.PdfFile;
            ObjectMapper.Map(input, post);
            //post.PdfFile = input.PdfFile;
            //try
            //{
            //    if (!string.IsNullOrEmpty(input.PostFileToken))
            //        post.PostFile = await GetBinaryObjectFromCache(input.PostFileToken, post.Id);

            //}
            //catch (UserFriendlyException ex)
            //{
            //    //ignore
            //}

            //try
            //{
            //    if (!string.IsNullOrEmpty(input.PostFileToken2))
            //        post.PostFile2 = await GetBinaryObjectFromCache(input.PostFileToken2, post.Id);


            //}
            //catch (UserFriendlyException ex)
            //{
            //    //ignore
            //}

            //try
            //{
            //    if (!string.IsNullOrEmpty(input.PostFileToken3))
            //        post.PostFile3 = await GetBinaryObjectFromCache(input.PostFileToken3, post.Id);

            //}
            //catch (UserFriendlyException ex)
            //{
            //    //ignore
            //}

            //try
            //{
            //    if (!string.IsNullOrEmpty(input.PostFileToken4))
            //        post.PostFile4 = await GetBinaryObjectFromCache(input.PostFileToken4, post.Id);

            //}
            //catch (UserFriendlyException ex)
            //{
            //    //ignore
            //}

            //try
            //{
            //    if (!string.IsNullOrEmpty(input.PostFileToken5))
            //        post.PostFile5 = await GetBinaryObjectFromCache(input.PostFileToken5, post.Id);

            //}
            //catch (UserFriendlyException ex)
            //{
            //    //ignore
            //}

            //try
            //{
            //    if (!string.IsNullOrEmpty(input.PostFileToken6))
            //        post.PostFile6 = await GetBinaryObjectFromCache(input.PostFileToken6, post.Id);

            //}
            //catch (UserFriendlyException ex)
            //{
            //    //ignore
            //}

            //try
            //{
            //    if (!string.IsNullOrEmpty(input.PostFileToken7))
            //        post.PostFile7 = await GetBinaryObjectFromCache(input.PostFileToken7, post.Id);

            //}
            //catch (UserFriendlyException ex)
            //{
            //    //ignore
            //}

            //try
            //{
            //    if (!string.IsNullOrEmpty(input.PostFileToken8))
            //        post.PostFile8 = await GetBinaryObjectFromCache(input.PostFileToken8, post.Id);

            //}
            //catch (UserFriendlyException ex)
            //{
            //    //ignore
            //}

            //try
            //{
            //    if (!string.IsNullOrEmpty(input.PostFileToken9))
            //        post.PostFile9 = await GetBinaryObjectFromCache(input.PostFileToken9, post.Id);

            //}
            //catch (UserFriendlyException ex)
            //{
            //    //ignore
            //}

            //try
            //{
            //    if (!string.IsNullOrEmpty(input.PostFileToken10))
            //        post.PostFile10 = await GetBinaryObjectFromCache(input.PostFileToken10, post.Id);

            //}
            //catch (UserFriendlyException ex)
            //{
            //    //ignore
            //}

            //try
            //{
            //    if (!string.IsNullOrEmpty(input.PdfFileToken))
            //        post.PdfFile = await GetBinaryObjectFromCache(input.PdfFileToken, post.Id);

            //}
            //catch (UserFriendlyException ex)
            //{
            //    //ignore
            //}
            await ProcessMainFileAsync(post, input.PostFileToken, required: false);
            await ProcessPdfFileAsync(post, input.PdfFileToken);
            await ProcessAdditionalFilesAsync(post, input);

            if (shouldSendSmsNotification)
            {
                //await SendSmsNotification(post,input.OrganizationId);
                await SendSmsNotification(post);

            }

            if (post.PostGroupId.HasValue && post.CurrentPostStatus == PostStatus.Published)
                await PublishNewPostNotifications(post/*,input.OrganizationId*/);

            await _unitOfWorkManager.Current.SaveChangesAsync();

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

        private async Task<BinaryObject> SaveAndGetBinaryObject(string fileToken, int? refId)
        {
            if (fileToken.IsNullOrWhiteSpace())
                return null;

            var fileCache = _tempFileCacheManager.GetFileInfo(fileToken);
            if (fileCache == null)
                throw new UserFriendlyException("فایلی با این توکن یافت نشد: " + fileToken);

            if (fileCache.File.Length > BinaryObjectConsts.BytesMaxSize)
                throw new UserFriendlyException("لطفا فایل با حداکثر حجم 10 مگابایت انتخاب کنید");

            var storedFile = new BinaryObject(AbpSession.TenantId, fileCache.File, BinarySourceType.Post, fileCache.FileName);
            if (refId != null)
                storedFile.SourceId = refId;

            await _binaryObjectManager.SaveAsync(storedFile);
            return storedFile;
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

        //public async Task<PagedResultDto<GetPostCategoriesForViewDto>> GetPostCategoriesForView(int organizationId)
        //{
        //    try
        //    {
        //        var cat = new List<GetPostCategoriesForViewDto>();
        //        var queryPostCat = await (from pc in _lookup_postGroupRepository.GetAll().Where(x => !x.IsDeleted)
        //                                      //join g in organizationRepository.GetAll().Where(x => !x.IsDeleted) on pc.OrganizationId equals g.Id into joiner1
        //                                      //from g in joiner1.DefaultIfEmpty()
        //                                      //join gm in _lookup_groupMemberRepository.GetAll() on g.Id equals gm.OrganizationId into joiner2
        //                                      //from gm in joiner2.DefaultIfEmpty()
        //                                      //where gm.UserId == AbpSession.UserId
        //                                  where pc.OrganizationId == organizationId
        //                                  orderby pc.Ordering
        //                                  select new
        //                                  {
        //                                      pc.Id,
        //                                      pc.PostGroupDescription,
        //                                      PostGroupHeaderPicFile = pc.GroupFile,
        //                                      PostGroupLatestPicFile = _postRepository.GetAll().Where(p => p.PostGroupId == pc.Id)
        //                                          .OrderByDescending(p => p.CreationTime).FirstOrDefault().PostFile,

        //                                  }).ToListAsync();

        //        foreach (var postCategory in queryPostCat)
        //        {
        //            var filteredPostSubGroups = await _lookup_postSubGroupRepository.GetAll()
        //                .Where(p => p.PostGroupId == postCategory.Id).CountAsync();

        //            cat.Add(new GetPostCategoriesForViewDto
        //            {
        //                PostGroupLatestPicFile = postCategory.PostGroupLatestPicFile,
        //                PostGroupHeaderPicFile = postCategory.PostGroupHeaderPicFile,
        //                Id = postCategory.Id,
        //                PostGroupDescription = postCategory.PostGroupDescription,
        //                HasSubGroups = filteredPostSubGroups > 0,
        //            });
        //        }

        //        return await Task.FromResult(new PagedResultDto<GetPostCategoriesForViewDto>(cat.Count, cat));
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new UserFriendlyException(ex.Message);
        //    }
        //}

        public async Task<PagedResultDto<GetPostCategoriesForViewDto>> GetPostCategoriesForView(int organizationId)
        {
            try
            {
                var postGroups = await _lookup_postGroupRepository.GetAll()
                    .Where(pg => !pg.IsDeleted && pg.OrganizationId == organizationId)
                    .OrderBy(pg => pg.Ordering)
                    .ToListAsync();

                var categories = new List<GetPostCategoriesForViewDto>();

                foreach (var group in postGroups)
                {
                    var latestPost = await _postRepository.GetAll()
                        .Where(p => p.PostGroupId == group.Id && p.IsPublished && !p.IsDeleted)
                        .OrderByDescending(p => p.CreationTime)
                        .FirstOrDefaultAsync();

                    string latestMedia = null;
                    if (latestPost?.PostFile != null)
                    {
                        var ext = await GetFileExtensionAsync(latestPost.PostFile);
                        var isImage = ext is ".jpg" or ".jpeg" or ".png";
                        var isVideo = ext is ".mp4" or ".mov";

                        if (isImage)
                            latestMedia = $"/thumbnails/{latestPost.Id}.jpg";
                        else if (isVideo)
                            latestMedia = $"/previews/{latestPost.Id}.gif";
                    }

                    var hasSubGroups = await _lookup_postSubGroupRepository.GetAll()
                        .AnyAsync(x => x.PostGroupId == group.Id);

                    categories.Add(new GetPostCategoriesForViewDto
                    {
                        Id = group.Id,
                        PostGroupDescription = group.PostGroupDescription,
                        PostGroupHeaderPicFile = group.GroupFile,
                        PostGroupLatestPicFile = latestMedia,
                        HasSubGroups = hasSubGroups
                    });
                }

                return new PagedResultDto<GetPostCategoriesForViewDto>(categories.Count, categories);
            }
            catch (Exception ex)
            {
                throw new UserFriendlyException("خطا در دریافت دسته‌بندی‌ها", ex);
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
                        //PostCaption = post.PostCaption,
                        //PostFile = post.PostFile,
                        //PostTitle = post.PostTitle
                    };
                    if (post.GroupMemberFk != null)
                    {

                        //datam.MemberFullName = post.GroupMemberFk.UserFk.FullName;
                        //datam.MemberPosition = post.GroupMemberFk.MemberPosition;
                        //datam.MemberUserName = post.GroupMemberFk.UserFk.UserName;
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


        public async Task<PagedResultDto<GetPostsForViewDto>> GetPostsByGroupIdForView(GetPostsByGroupIdInput input)
        {
            try
            {
                var query = _postRepository.GetAll()
                    .AsNoTracking()
                    .Where(x => !x.IsDeleted && x.IsPublished)
                    .Where(x => x.PostGroupFk != null && x.PostGroupFk.OrganizationId == input.OrganizationId)
                    .WhereIf(input.PostGroupId > 0, x => x.PostGroupId == input.PostGroupId)
                    .WhereIf(input.PostSubGroupId > 0, x => x.PostSubGroupId == input.PostSubGroupId)
                    .Select(p => new
                    {
                        p.Id,
                        p.GroupMemberId,
                        p.IsSpecial,
                        p.IsPublished,
                        p.PostCaption,
                        p.PostTitle,
                        p.PostGroupId,
                        p.PostRefLink,
                        p.CreationTime,
                        p.PdfFile,

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

                        MemberFullName = p.GroupMemberFk.UserFk.FullName,
                        MemberUserName = p.GroupMemberFk.UserFk.UserName,
                        MemberPosition = p.GroupMemberFk.MemberPosition,

                        GroupFile = p.PostGroupFk.GroupFile,
                        GroupDescription = p.PostGroupFk.PostGroupDescription,
                        PostSubGroupDescription = p.PostSubGroupFk.PostSubGroupDescription,
                    });

                var totalCount = await query.CountAsync();
                var pagedItems = await query
                    .OrderBy(input.Sorting ?? "id desc")
                    .PageBy(input)
                    .ToListAsync();

                var result = new List<GetPostsForViewDto>();

                foreach (var p in pagedItems)
                {
                    // بررسی همه‌ی فایل‌ها
                    var allFileIds = new[]
                    {
                p.PostFile, p.PostFile2, p.PostFile3, p.PostFile4, p.PostFile5,
                p.PostFile6, p.PostFile7, p.PostFile8, p.PostFile9, p.PostFile10,p.PdfFile
            }.Where(x => x != null).ToList();

                    var allExtensions = await Task.WhenAll(allFileIds.Select(x => GetFileExtensionAsync(x.Value)));

                    //bool hasPdf = allExtensions.Any(e => e != null && e.ToLowerInvariant() == ".pdf");
                    bool hasPdf = p.PdfFile != null ||
                                  allExtensions.Any(e => e != null && e.ToLowerInvariant() == ".pdf");

                    //string mainExt = allExtensions.FirstOrDefault(e => e != null)?.ToLowerInvariant();

                    var nonPdfExts = allExtensions.Where(e => e != null && e.ToLowerInvariant() != ".pdf").ToList();
                    string mainExt = nonPdfExts.FirstOrDefault();

                    bool isImage = mainExt is ".jpg" or ".jpeg" or ".png";
                    bool isVideo = mainExt is ".mp4" or ".mov";
                    bool isSlide = nonPdfExts.Count > 1;

                    string thumbnailPath = isImage ? $"/thumbnails/{p.Id}.jpg" : null;
                    string previewPath = isVideo ? $"/previews/{p.Id}.gif" : null;
                    //string videoPath = isVideo ? $"/videos/{p.Id}{mainExt}" : null;

                    result.Add(new GetPostsForViewDto
                    {
                        Id = p.Id,
                        GroupMemberId = p.GroupMemberId ?? 0,
                        IsSpecial = p.IsSpecial,
                        IsPublished = p.IsPublished,
                        PostCaption = p.PostCaption,
                        PostTitle = p.PostTitle,
                        PostGroupId = p.PostGroupId,
                        PostRefLink = p.PostRefLink,
                        CreationTime = p.CreationTime,

                        ThumbnailPath = thumbnailPath,
                        PreviewPath = previewPath,
                        IsPdf = hasPdf,
                        IsSlide = isSlide,
                        IsVideo = isVideo,

                        PostFile = p.PostFile,
                        PostFile2 = p.PostFile2,
                        PostFile3 = p.PostFile3,
                        PostFile4 = p.PostFile4,
                        PostFile5 = p.PostFile5,
                        PostFile6 = p.PostFile6,
                        PostFile7 = p.PostFile7,
                        PostFile8 = p.PostFile8,
                        PostFile9 = p.PostFile9,
                        PostFile10 = p.PostFile10,
                        PdfFile = p.PdfFile,

                        MemberFullName = p.MemberFullName,
                        MemberUserName = p.MemberUserName,
                        MemberPosition = p.MemberPosition,

                        GroupFile = p.GroupFile,
                        GroupDescription = p.GroupDescription,
                        PostSubGroupDescription = p.PostSubGroupDescription
                    });
                }

                return new PagedResultDto<GetPostsForViewDto>(totalCount, result);
            }
            catch (Exception ex)
            {
                throw new UserFriendlyException("خطا در دریافت پست‌ها", ex);
            }
        }

        private async Task<string> GetFileExtensionAsync(Guid? fileId)
        {
            if (fileId == null) return null;

            var file = await _binaryObjectManager.GetOrNullAsync(fileId.Value);
            if (file == null || string.IsNullOrWhiteSpace(file.Description))
                return null;

            return Path.GetExtension(file.Description)?.ToLowerInvariant();
        }

        private string GetFileExtensionFromToken(string token)
        {
            if (string.IsNullOrEmpty(token)) return null;

            var info = _tempFileCacheManager.GetFileInfo(token);
            if (info == null || string.IsNullOrWhiteSpace(info.FileName)) return null;

            return Path.GetExtension(info.FileName)?.ToLowerInvariant();
        }

        private bool IsPdfToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return false;

            var info = _tempFileCacheManager.GetFileInfo(token);
            if (info == null)
                return false;

            if (!string.IsNullOrWhiteSpace(info.FileName) &&
                Path.GetExtension(info.FileName)?.Equals(".pdf", StringComparison.OrdinalIgnoreCase) == true)
                return true;

            return IsPdf(info.File);
        }

        private static bool IsPdf(byte[] bytes)
        {
            return bytes != null && bytes.Length > 4 &&
                   bytes[0] == 0x25 && bytes[1] == 0x50 &&
                   bytes[2] == 0x44 && bytes[3] == 0x46;
        }



        private void NormalizePdfFileToken(CreateOrEditPostDto input)
        {
            bool pdfSet = !string.IsNullOrWhiteSpace(input.PdfFileToken);

            void HandleToken(ref string token)
            {
                //if (GetFileExtensionFromToken(token) == ".pdf")
                if (IsPdfToken(token))
                {
                    if (!pdfSet)
                    {
                        input.PdfFileToken = token;
                        pdfSet = true;
                    }
                    token = null;
                }
            }

            var t1 = input.PostFileToken; HandleToken(ref t1); input.PostFileToken = t1;
            var t2 = input.PostFileToken2; HandleToken(ref t2); input.PostFileToken2 = t2;
            var t3 = input.PostFileToken3; HandleToken(ref t3); input.PostFileToken3 = t3;
            var t4 = input.PostFileToken4; HandleToken(ref t4); input.PostFileToken4 = t4;
            var t5 = input.PostFileToken5; HandleToken(ref t5); input.PostFileToken5 = t5;
            var t6 = input.PostFileToken6; HandleToken(ref t6); input.PostFileToken6 = t6;
            var t7 = input.PostFileToken7; HandleToken(ref t7); input.PostFileToken7 = t7;
            var t8 = input.PostFileToken8; HandleToken(ref t8); input.PostFileToken8 = t8;
            var t9 = input.PostFileToken9; HandleToken(ref t9); input.PostFileToken9 = t9;
            var t10 = input.PostFileToken10; HandleToken(ref t10); input.PostFileToken10 = t10;
        }


        //public Task<PagedResultDto<GetPostsForViewDto>> GetPostsByGroupIdForView(GetPostsByGroupIdInput input)
        //{
        //    try
        //    {
        //        var posts = new List<GetPostsForViewDto>();
        //        var filteredPosts = from p in _postRepository.GetAll().Where(x => !x.IsDeleted)
        //                .Include(e => e.GroupMemberFk)
        //                .Include(e => e.PostGroupFk)
        //                .Include(e => e.GroupMemberFk.UserFk)
        //                .Include(e => e.AppBinaryObjectFk)
        //                .Include(e => e.AppBinaryObjectFk2)
        //                .Include(e => e.AppBinaryObjectFk3)
        //                .Include(e => e.AppBinaryObjectFk4)
        //                .Include(e => e.AppBinaryObjectFk5)
        //                .Include(e => e.AppBinaryObjectFk6)
        //                .Include(e => e.AppBinaryObjectFk7)
        //                .Include(e => e.AppBinaryObjectFk8)
        //                .Include(e => e.AppBinaryObjectFk9)
        //                .Include(e => e.AppBinaryObjectFk10)
        //                .Where(x => x.PostGroupFk.OrganizationId == input.OrganizationId)
        //                .Where(x => x.IsPublished)
        //                .WhereIf(input.PostGroupId > 0, p => p.PostGroupId == input.PostGroupId)
        //                .WhereIf(input.PostSubGroupId > 0, p => p.PostSubGroupId == input.PostSubGroupId)
        //                            join pg in _lookup_postGroupRepository.GetAll().Where(x => !x.IsDeleted) on p.PostGroupId equals
        //                                pg.Id into joiner1
        //                            from pg in joiner1.DefaultIfEmpty()
        //                            join og in organizationRepository.GetAll().Where(x => !x.IsDeleted) on pg.OrganizationId
        //                                equals og.Id into joiner2
        //                            from og2 in joiner2.DefaultIfEmpty()
        //                                //join gm in _lookup_groupMemberRepository.GetAll() on og2.Id equals gm.OrganizationId into
        //                                //joiner3
        //                                //from gm2 in joiner3.DefaultIfEmpty()
        //                                //where gm2.UserId == AbpSession.UserId
        //                            select new
        //                            {
        //                                p.Id,
        //                                p.GroupMemberId,
        //                                p.IsSpecial,
        //                                p.IsPublished,
        //                                p.PostCaption,
        //                                p.CreationTime,
        //                                p.PostFile,
        //                                p.PostFile2,
        //                                p.PostFile3,
        //                                p.PostFile4,
        //                                p.PostFile5,
        //                                p.PostFile6,
        //                                p.PostFile7,
        //                                p.PostFile8,
        //                                p.PostFile9,
        //                                p.PostFile10,
        //                                p.PostTitle,
        //                                p.PostRefLink,
        //                                p.PostGroupId,
        //                                p.GroupMemberFk,
        //                                p.PostGroupFk,
        //                                p.PostSubGroupFk,
        //                                p.AppBinaryObjectFk,
        //                                p.AppBinaryObjectFk2,
        //                                p.AppBinaryObjectFk3,
        //                                p.AppBinaryObjectFk4,
        //                                p.AppBinaryObjectFk5,
        //                                p.AppBinaryObjectFk6,
        //                                p.AppBinaryObjectFk7,
        //                                p.AppBinaryObjectFk8,
        //                                p.AppBinaryObjectFk9,
        //                                p.AppBinaryObjectFk10,

        //                            };
        //        var count = filteredPosts.Count();
        //        var pagedAndFilteredPosts = filteredPosts
        //            .OrderBy(input.Sorting ?? "id desc")
        //            .PageBy(input);

        //        foreach (var post in pagedAndFilteredPosts)
        //        {
        //            var datam = new GetPostsForViewDto
        //            {
        //                //Base64Image = "data:image/png;base64,"+Convert.ToBase64String(postCategory.Bytes, 0, postCategory.Bytes.Length) ,
        //                Id = post.Id,
        //                GroupMemberId = post.GroupMemberId ?? 0,
        //                IsSpecial = post.IsSpecial,
        //                IsPublished = post.IsPublished,
        //                PostCaption = post.PostCaption,
        //                PostFile = post.PostFile,
        //                PostFile2 = post.PostFile2,
        //                PostFile3 = post.PostFile3,
        //                PostFile4 = post.PostFile4,
        //                PostFile5 = post.PostFile5,
        //                PostFile6 = post.PostFile6,
        //                PostFile7 = post.PostFile7,
        //                PostFile8 = post.PostFile8,
        //                PostFile9 = post.PostFile9,
        //                PostFile10 = post.PostFile10,
        //                PostTitle = post.PostTitle,
        //                PostGroupId = post.PostGroupId,
        //                PostRefLink = post.PostRefLink,
        //                CreationTime = post.CreationTime,
        //            };
        //            try
        //            {
        //                if (post.GroupMemberFk != null)
        //                {
        //                    datam.MemberFullName = post.GroupMemberFk.UserFk.FullName;
        //                    datam.MemberPosition = post.GroupMemberFk.MemberPosition;
        //                    datam.MemberUserName = post.GroupMemberFk.UserFk.UserName;
        //                }

        //                if (post.PostGroupFk != null)
        //                {
        //                    datam.GroupFile = post.PostGroupFk.GroupFile;
        //                    datam.GroupDescription = post.PostGroupFk.PostGroupDescription;

        //                }

        //                if (post.PostSubGroupFk != null)
        //                {
        //                    datam.PostSubGroupDescription = post.PostSubGroupFk.PostSubGroupDescription;
        //                }

        //                if (post.AppBinaryObjectFk != null)
        //                {
        //                    datam.Attachment1 = post.AppBinaryObjectFk.Description;
        //                }

        //                if (post.AppBinaryObjectFk2 != null)
        //                {
        //                    datam.Attachment2 = post.AppBinaryObjectFk2.Description;
        //                }

        //                if (post.AppBinaryObjectFk3 != null)
        //                {
        //                    datam.Attachment3 = post.AppBinaryObjectFk3.Description;
        //                }

        //                if (post.AppBinaryObjectFk4 != null)
        //                {
        //                    datam.Attachment4 = post.AppBinaryObjectFk4.Description;
        //                }

        //                if (post.AppBinaryObjectFk5 != null)
        //                {
        //                    datam.Attachment5 = post.AppBinaryObjectFk5.Description;
        //                }

        //                if (post.AppBinaryObjectFk6 != null)
        //                {
        //                    datam.Attachment6 = post.AppBinaryObjectFk6.Description;
        //                }

        //                if (post.AppBinaryObjectFk7 != null)
        //                {
        //                    datam.Attachment7 = post.AppBinaryObjectFk7.Description;
        //                }

        //                if (post.AppBinaryObjectFk8 != null)
        //                {
        //                    datam.Attachment8 = post.AppBinaryObjectFk8.Description;
        //                }

        //                if (post.AppBinaryObjectFk9 != null)
        //                {
        //                    datam.Attachment9 = post.AppBinaryObjectFk9.Description;
        //                }

        //                if (post.AppBinaryObjectFk10 != null)
        //                {
        //                    datam.Attachment10 = post.AppBinaryObjectFk10.Description;
        //                }

        //            }
        //            catch (Exception)
        //            {
        //                //ignored
        //            }

        //            posts.Add(datam);


        //        }

        //        return Task.FromResult(new PagedResultDto<GetPostsForViewDto>(count, posts));
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new UserFriendlyException(ex.Message);
        //    }
        //}

        public async Task<PagedResultDto<GetLikedUsersDto>> GetLikedUsers(GetLikedUsersInput input)
        {

            var currentUser = await _userRepository.GetAsync(AbpSession.UserId.Value);
            if (currentUser.UserType != AccountUserType.SuperAdmin)
            {

                var currentUserOrgQuery = from x in _groupMemberRepository.GetAll()//.Include(x => x.OrganizationFk)
                                          where x.UserId == currentUser.Id
                                          select x.OrganizationId;

                var userQuery = from x in _groupMemberRepository.GetAll()
                                join y in _postRepository.GetAll() on x.Id equals y.GroupMemberId
                                where y.Id == input.PostId && currentUserOrgQuery.Contains(x.OrganizationId)
                                select x;
                if (!userQuery.Any())
                {
                    throw new UserFriendlyException("پست انتخابی متعلق به هیچ یک از سازمان های شما نمی باشد");
                }

            }
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

            return await Task.FromResult(new PagedResultDto<GetLikedUsersDto>(queryCount, users));

        }

        public async Task<PagedResultDto<GetSeenUsersDto>> GetSeenUsers(GetSeenUsersInput input)
        {
            var currentUser = await _userRepository.GetAsync(AbpSession.UserId.Value);
            if (currentUser.UserType != AccountUserType.SuperAdmin)
            {

                var currentUserOrgQuery = from x in _groupMemberRepository.GetAll()//.Include(x => x.OrganizationFk)
                                          where x.UserId == currentUser.Id
                                          select x.OrganizationId;

                var userQuery = from x in _groupMemberRepository.GetAll()
                                join y in _postRepository.GetAll() on x.Id equals y.GroupMemberId
                                where y.Id == input.PostId && currentUserOrgQuery.Contains(x.OrganizationId)
                                select x;
                if (!userQuery.Any())
                {
                    throw new UserFriendlyException("پست انتخابی متعلق به هیچ یک از سازمان های شما نمی باشد");
                }

            }
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

            return await Task.FromResult(new PagedResultDto<GetSeenUsersDto>(queryCount, users));

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

            var currentUser = await _userRepository.GetAsync(AbpSession.UserId.Value);
            if (currentUser.UserType != AccountUserType.SuperAdmin)
            {

                var currentUserOrgQuery = from x in _groupMemberRepository.GetAll()//.Include(x => x.OrganizationFk)
                                          where x.UserId == currentUser.Id
                                          select x.OrganizationId;
                if (!currentUserOrgQuery.Contains(organizationId))
                {
                    throw new UserFriendlyException("سازمان انتخابی به این کاربر تعلق ندارد");
                }

            }

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

            if (post.PostGroupId != input.PostGroupId)
            {
                var newPostGroup = await _lookup_postGroupRepository.FirstOrDefaultAsync(pg => pg.Id == input.PostGroupId.Value);
                string newGroupDescription = newPostGroup?.PostGroupDescription ?? "نامشخص";
                string oldGroupDescription = post.PostGroupFk?.PostGroupDescription ?? "نامشخص";
                changes.Add($"گروه خبری از {oldGroupDescription} به {newGroupDescription} تغییر کرد.");
            }

            if (post.PostSubGroupId != input.PostSubGroupId)
            {
                var newSubGroup = await _lookup_postSubGroupRepository.FirstOrDefaultAsync(sg => sg.Id == input.PostSubGroupId);
                string newSubGroupDescription = newSubGroup?.PostSubGroupDescription ?? "نامشخص";
                string oldSubGroupDescription = post.PostSubGroupFk?.PostSubGroupDescription ?? "نامشخص";
                changes.Add($"زیر گروه خبری از {oldSubGroupDescription} به {newSubGroupDescription} تغییر کرد.");
            }

            if (post.PostRefLink != input.PostRefLink)
                changes.Add($"لینک خبر از {post.PostRefLink} به {input.PostRefLink} تغییر کرد.");

            if (post.PostComment != input.PostComment)
                changes.Add($"نظر از {post.PostComment} به {input.PostComment} تغییر کرد.");

            if (post.IsSpecial == false && input.IsSpecial == true)
                changes.Add("اصالت خبر تائید شد.");
            if (post.IsSpecial == true && input.IsSpecial == false)
                changes.Add("اصالت خبر رد شد.");

            if (post.IsPublished == false && input.IsPublished == true)
                changes.Add("خبر منتشر شد.");
            if (post.IsPublished == true && input.IsPublished == false)
                changes.Add("خبر از حالت انتشار خارج شد.");

            var fileChangeSummary = GetFileChangeDescription(post, input);
            if (!string.IsNullOrWhiteSpace(fileChangeSummary))
                changes.Add(fileChangeSummary);

            return string.Join("\n", changes);
        }

        private string GetFileChangeDescription(Post post, CreateOrEditPostDto input)
        {
            var oldFiles = new List<Guid?> {
                post.PostFile, post.PostFile2, post.PostFile3, post.PostFile4, post.PostFile5,
                post.PostFile6, post.PostFile7, post.PostFile8, post.PostFile9, post.PostFile10,post.PostFile
            };

            var newFiles = new List<Guid?> {
                input.PostFile, input.PostFile2, input.PostFile3, input.PostFile4, input.PostFile5,
                input.PostFile6, input.PostFile7, input.PostFile8, input.PostFile9, input.PostFile10,input.PdfFile
            };

            int removed = 0;
            int added = 0;

            for (int i = 0; i < oldFiles.Count; i++)
            {
                var oldId = oldFiles[i];
                var newId = newFiles[i];

                if (oldId.HasValue && !newId.HasValue)
                    removed++;
                else if (!oldId.HasValue && newId.HasValue)
                    added++;
                else if (oldId.HasValue && newId.HasValue && oldId != newId)
                {
                    removed++;
                    added++;
                }
            }

            var parts = new List<string>();
            if (removed > 0)
                parts.Add($"{removed} فایل حذف شد");
            if (added > 0)
                parts.Add($"{added} فایل اضافه شد");

            return string.Join(" و ", parts);
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