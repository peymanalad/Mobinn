﻿using Chamran.Deed.People;
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
using Chamran.Deed.Authorization.Users.Dto;

namespace Chamran.Deed.Info
{
    [AbpAuthorize(AppPermissions.Pages_Posts)]
    public class PostsAppService : DeedAppServiceBase, IPostsAppService
    {
        private readonly IRepository<Post> _postRepository;
        private readonly IPostsExcelExporter _postsExcelExporter;
        private readonly IRepository<GroupMember, int> _lookup_groupMemberRepository;
        private readonly IRepository<PostGroup, int> _lookup_postGroupRepository;
        private readonly IRepository<UserPostGroup, int> _userPostGroupRepository;
        private readonly IRepository<Seen, int> _seenRepository;
        private readonly IRepository<PostLike, int> _postLikeRepository;

        private readonly ITempFileCacheManager _tempFileCacheManager;
        private readonly IBinaryObjectManager _binaryObjectManager;
        private readonly IRepository<Organization> organizationRepository;
        private readonly IAppNotifier _appNotifier;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        //private readonly IRepository<PostCategory> _postCategoryRepository;
        //private readonly IDbContextProvider<DeedDbContext> _dbContextProvider;

        //private CultureInfo _originalCulture;
        //private readonly CultureInfo _targetCulture=new CultureInfo("fa-IR");

        public PostsAppService(IRepository<Post> postRepository, IPostsExcelExporter postsExcelExporter, IRepository<GroupMember, int> lookup_groupMemberRepository, IRepository<PostGroup, int> lookup_postGroupRepository, ITempFileCacheManager tempFileCacheManager, IBinaryObjectManager binaryObjectManager, IRepository<Organization> organization, IAppNotifier appNotifier, IRepository<UserPostGroup, int> userPostGroupRepository, IUnitOfWorkManager unitOfWorkManager, IRepository<PostLike, int> postLikeRepository, IRepository<Seen, int> seenRepository)
        {
            _postRepository = postRepository;
            _postsExcelExporter = postsExcelExporter;
            _lookup_groupMemberRepository = lookup_groupMemberRepository;
            _lookup_postGroupRepository = lookup_postGroupRepository;
            _tempFileCacheManager = tempFileCacheManager;
            _binaryObjectManager = binaryObjectManager;
            organizationRepository = organization;
            _appNotifier = appNotifier;
            _userPostGroupRepository = userPostGroupRepository;
            _unitOfWorkManager = unitOfWorkManager;
            _postLikeRepository = postLikeRepository;
            _seenRepository = seenRepository;
            //_postCategoryRepository = postCategoryRepository;
            //_dbContextProvider= dbContextProvider;
        }

        public async Task<PagedResultDto<GetPostForViewDto>> GetAll(GetAllPostsInput input)
        {
            var filteredPosts = _postRepository.GetAll()
                .Include(e => e.GroupMemberFk)
                .Include(e => e.PostGroupFk)
                .WhereIf(input.OrganizationId.HasValue, x => x.PostGroupFk.OrganizationId == input.OrganizationId.Value)
                .WhereIf(!string.IsNullOrWhiteSpace(input.Filter), e => false || e.PostCaption.Contains(input.Filter) || e.PostTitle.Contains(input.Filter))
                .WhereIf(!string.IsNullOrWhiteSpace(input.PostCaptionFilter), e => e.PostCaption.Contains(input.PostCaptionFilter))
                .WhereIf(input.IsSpecialFilter.HasValue && input.IsSpecialFilter > -1, e => (input.IsSpecialFilter == 1 && e.IsSpecial) || (input.IsSpecialFilter == 0 && !e.IsSpecial))
                .WhereIf(!string.IsNullOrWhiteSpace(input.PostTitleFilter), e => e.PostTitle.Contains(input.PostTitleFilter))
                .WhereIf(!string.IsNullOrWhiteSpace(input.GroupMemberMemberPositionFilter), e => e.GroupMemberFk != null && e.GroupMemberFk.MemberPosition == input.GroupMemberMemberPositionFilter)
                .WhereIf(!string.IsNullOrWhiteSpace(input.PostGroupPostGroupDescriptionFilter), e => e.PostGroupFk != null && e.PostGroupFk.PostGroupDescription == input.PostGroupPostGroupDescriptionFilter)
                .WhereIf(input.FromDate.HasValue, e => e.CreationTime >= input.FromDate.Value)
                .WhereIf(input.ToDate.HasValue, e => e.CreationTime <= input.ToDate.Value);

            var pagedAndFilteredPosts = await filteredPosts
                .OrderBy(input.Sorting ?? "Id asc")
                .PageBy(input)
                .ToListAsync();

            var results = new List<GetPostForViewDto>();

            foreach (var post in pagedAndFilteredPosts)
            {
                var seenGroup = await _seenRepository.GetAll().Where(seen => seen.PostId == post.Id).ToListAsync();
                var likeGroup = await _postLikeRepository.GetAll().Where(postLike => postLike.PostId == post.Id).ToListAsync();

                var resultDto = new GetPostForViewDto
                {
                    Post = new PostDto
                    {
                        PostFile = post.PostFile,
                        PostCaption = post.PostCaption,
                        IsSpecial = post.IsSpecial,
                        IsPublished = post.IsPublished,
                        PostTitle = post.PostTitle,
                        PostRefLink = post.PostRefLink,
                        Id = post.Id,
                        CreationTime = post.CreationTime,
                        LastModificationTime = post.LastModificationTime
                    },
                    GroupMemberMemberPosition = post.GroupMemberFk?.MemberPosition ?? "",
                    PostGroupPostGroupDescription = post.PostGroupFk?.PostGroupDescription ?? "",
                    GroupFile = post.PostGroupFk.GroupFile,
                    TotalVisits = seenGroup.Count,
                    TotalLikes = likeGroup.Count,
                    OrganizationId = post.PostGroupFk?.OrganizationFk?.Id ?? 0,
                    OrganizationName = post.PostGroupFk?.OrganizationFk?.OrganizationName ?? ""
                };

                resultDto.Post.PostFileFileName = await GetBinaryFileName(post.PostFile);

                results.Add(resultDto);
            }

            var totalCount = await filteredPosts.CountAsync();

            return new PagedResultDto<GetPostForViewDto>(totalCount, results);
        }


        public async Task<GetPostForViewDto> GetPostForView(int id)
        {
            var post = await _postRepository.GetAsync(id);

            var output = new GetPostForViewDto { Post = ObjectMapper.Map<PostDto>(post) };

            if (output.Post.GroupMemberId != null)
            {
                var _lookupGroupMember = await _lookup_groupMemberRepository.FirstOrDefaultAsync((int)output.Post.GroupMemberId);
                output.GroupMemberMemberPosition = _lookupGroupMember?.MemberPosition;
            }

            if (output.Post.PostGroupId != null)
            {
                var _lookupPostGroup = await _lookup_postGroupRepository.FirstOrDefaultAsync((int)output.Post.PostGroupId);
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
                var _lookupGroupMember = await _lookup_groupMemberRepository.FirstOrDefaultAsync((int)output.Post.GroupMemberId);
                output.GroupMemberMemberPosition = _lookupGroupMember?.MemberPosition;
            }

            if (output.Post.PostGroupId != null)
            {
                var _lookupPostGroup = await _lookup_postGroupRepository.FirstOrDefaultAsync((int)output.Post.PostGroupId);
                output.PostGroupPostGroupDescription = _lookupPostGroup?.PostGroupDescription;
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

            await _unitOfWorkManager.Current.SaveChangesAsync();
            await unitOfWork.CompleteAsync();
            if (post.PostGroupId.HasValue)
                await PublishNewPostNotifications(post);
        }

        private async Task PublishNewPostNotifications(Post post)
        {
            var query = _userPostGroupRepository.GetAll().Where(x => x.PostGroupId == post.PostGroupId.Value);
            var ids = new List<UserIdentifier>();
            foreach (var row in query)
            {
                ids.Add(new UserIdentifier(AbpSession.TenantId, row.UserId));
            }

            await _appNotifier.SendPostNotificationAsync(JsonConvert.SerializeObject(ObjectMapper.Map<PostDto>(post), new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new CamelCaseNamingStrategy() // Use PascalCaseNamingStrategy for Pascal case
                }
            }),
                userIds: ids.ToArray(),
                NotificationSeverity.Info
            );
        }

        [AbpAuthorize(AppPermissions.Pages_Posts_Edit)]
        protected virtual async Task Update(CreateOrEditPostDto input)
        {
            var post = await _postRepository.FirstOrDefaultAsync((int)input.Id);
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
                        .WhereIf(!string.IsNullOrWhiteSpace(input.Filter), e => false || e.PostCaption.Contains(input.Filter) || e.PostTitle.Contains(input.Filter))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.PostCaptionFilter), e => e.PostCaption.Contains(input.PostCaptionFilter))
                        .WhereIf(input.IsSpecialFilter.HasValue && input.IsSpecialFilter > -1, e => (input.IsSpecialFilter == 1 && e.IsSpecial) || (input.IsSpecialFilter == 0 && !e.IsSpecial))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.PostTitleFilter), e => e.PostTitle.Contains(input.PostTitleFilter))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.GroupMemberMemberPositionFilter), e => e.GroupMemberFk != null && e.GroupMemberFk.MemberPosition == input.GroupMemberMemberPositionFilter)
                        .WhereIf(!string.IsNullOrWhiteSpace(input.PostGroupPostGroupDescriptionFilter), e => e.PostGroupFk != null && e.PostGroupFk.PostGroupDescription == input.PostGroupPostGroupDescriptionFilter)
                        .WhereIf(input.OrganizationId.HasValue, e => e.PostGroupFk.OrganizationId == input.OrganizationId);
            var persianCalendar = new PersianCalendar();
            var query = (from o in filteredPosts
                         join o1 in _lookup_groupMemberRepository.GetAll() on o.GroupMemberId equals o1.Id into j1
                         from s1 in j1.DefaultIfEmpty()

                         join o2 in _lookup_postGroupRepository.GetAll() on o.PostGroupId equals o2.Id into j2
                         from s2 in j2.DefaultIfEmpty()

                         select new GetPostForViewDto()
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
                             PostGroupPostGroupDescription = s2 == null || s2.PostGroupDescription == null ? "" : s2.PostGroupDescription.ToString(),
                             PersianCreationTime = o.CreationTime != null ? persianCalendar.GetYear(o.CreationTime).ToString("D4") + "/" + persianCalendar.GetMonth(o.CreationTime).ToString("D2") + "/" + persianCalendar.GetDayOfMonth(o.CreationTime).ToString("D2") : null
                         });
            var postListDtos = await query.ToListAsync();
            return _postsExcelExporter.ExportToFile(postListDtos);
        }
        [AbpAuthorize(AppPermissions.Pages_Posts)]
        public async Task<PagedResultDto<PostGroupMemberLookupTableDto>> GetAllGroupMemberForLookupTable(GetAllForLookupTableInput input)
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
        public async Task<PagedResultDto<PostPostGroupLookupTableDto>> GetAllPostGroupForLookupTable(GetAllForLookupTableInput input)
        {
            var query = _lookup_postGroupRepository.GetAll().WhereIf(
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
                    DisplayName = postGroup.PostGroupDescription
                });
            }

            return new PagedResultDto<PostPostGroupLookupTableDto>(
                totalCount,
                lookupTableDtoList
            );
        }

        private async Task<Guid?> GetBinaryObjectFromCache(string fileToken, int? refId)
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

            var storedFile = new BinaryObject(AbpSession.TenantId, fileCache.File, BinarySourceType.Post, fileCache.FileName);
            await _binaryObjectManager.SaveAsync(storedFile);
            if (refId != null) storedFile.SourceId = refId;
            return storedFile.Id;
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

        public Task<PagedResultDto<GetPostCategoriesForViewDto>> GetPostCategoriesForView(int organizationId)
        {
            try
            {
                var cat = new List<GetPostCategoriesForViewDto>();
                var queryPostCat = from pc in _lookup_postGroupRepository.GetAll().Where(x => !x.IsDeleted)
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
                                       PostGroupLatestPicFile = _postRepository.GetAll().Where(p => p.PostGroupId == pc.Id).OrderByDescending(p => p.CreationTime).FirstOrDefault().PostFile
                                   };

                foreach (var postCategory in queryPostCat)
                {
                    cat.Add(new GetPostCategoriesForViewDto()
                    {
                        PostGroupLatestPicFile = postCategory.PostGroupLatestPicFile,
                        PostGroupHeaderPicFile = postCategory.PostGroupHeaderPicFile,
                        Id = postCategory.Id,
                        PostGroupDescription = postCategory.PostGroupDescription
                    });
                }
                return Task.FromResult(new PagedResultDto<GetPostCategoriesForViewDto>(cat.Count, cat));
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
                    var datam = new GetPostsForViewDto()
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
                        .Where(x => x.PostGroupFk.OrganizationId == input.OrganizationId)
                        .WhereIf(input.PostGroupId > 0, p => p.PostGroupId == input.PostGroupId)
                                    join pg in _lookup_postGroupRepository.GetAll().Where(x => !x.IsDeleted) on p.PostGroupId equals
                                        pg.Id into joiner1
                                    from pg in joiner1.DefaultIfEmpty()
                                    join og in organizationRepository.GetAll().Where(x => !x.IsDeleted) on pg.OrganizationId
                                        equals og.Id into joiner2
                                    from og in joiner2.DefaultIfEmpty()
                                    join gm in _lookup_groupMemberRepository.GetAll() on og.Id equals gm.OrganizationId into
                                        joiner3
                                    from gm in joiner3.DefaultIfEmpty()
                                    where gm.UserId == AbpSession.UserId
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

                var pagedAndFilteredPosts = filteredPosts
    .OrderBy(input.Sorting ?? "id desc")
    .PageBy(input);

                foreach (var post in pagedAndFilteredPosts)
                {
                    var datam = new GetPostsForViewDto()
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

                    posts.Add(datam);


                }

                return Task.FromResult(new PagedResultDto<GetPostsForViewDto>(posts.Count, posts));
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
                var datam = new GetLikedUsersDto()
                {
                    UserId = row.UserId,
                    Surname = row.Surname,
                    PhoneNumber = row.PhoneNumber,
                    UserName = row.UserName,
                    LikeTime = row.LikeTime,
                    IsSuperUser= row.IsSuperUser,
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
                var datam = new GetSeenUsersDto()
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
    }
}