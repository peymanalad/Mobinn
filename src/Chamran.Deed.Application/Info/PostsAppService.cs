using Chamran.Deed.People;
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
using Abp.Extensions;
using Abp.Authorization;
using Microsoft.EntityFrameworkCore;
using Abp.UI;
using Chamran.Deed.Storage;

namespace Chamran.Deed.Info
{
    [AbpAuthorize(AppPermissions.Pages_Posts)]
    public class PostsAppService : DeedAppServiceBase, IPostsAppService
    {
        private readonly IRepository<Post> _postRepository;
        private readonly IPostsExcelExporter _postsExcelExporter;
        private readonly IRepository<GroupMember, int> _lookup_groupMemberRepository;
        private readonly IRepository<PostGroup, int> _lookup_postGroupRepository;

        private readonly ITempFileCacheManager _tempFileCacheManager;
        private readonly IBinaryObjectManager _binaryObjectManager;

        private readonly IRepository<PostCategory> _postCategoryRepository;
        //private readonly IDbContextProvider<DeedDbContext> _dbContextProvider;

        //private CultureInfo _originalCulture;
        //private readonly CultureInfo _targetCulture=new CultureInfo("fa-IR");

        public PostsAppService(IRepository<Post> postRepository, IPostsExcelExporter postsExcelExporter, IRepository<GroupMember, int> lookup_groupMemberRepository, IRepository<PostGroup, int> lookup_postGroupRepository, ITempFileCacheManager tempFileCacheManager, IBinaryObjectManager binaryObjectManager, IRepository<PostCategory> postCategoryRepository)
        {
            _postRepository = postRepository;
            _postsExcelExporter = postsExcelExporter;
            _lookup_groupMemberRepository = lookup_groupMemberRepository;
            _lookup_postGroupRepository = lookup_postGroupRepository;
            _tempFileCacheManager = tempFileCacheManager;
            _binaryObjectManager = binaryObjectManager;
            _postCategoryRepository = postCategoryRepository;
            //_dbContextProvider= dbContextProvider;
        }

        public async Task<PagedResultDto<GetPostForViewDto>> GetAll(GetAllPostsInput input)
        {
            try
            {
                //_originalCulture = Thread.CurrentThread.CurrentCulture;

                //Thread.CurrentThread.CurrentCulture = _targetCulture;
                //Thread.CurrentThread.CurrentUICulture = _targetCulture;
                var filteredPosts = _postRepository.GetAll()
                    .Include(e => e.GroupMemberFk)
                    .Include(e => e.PostGroupFk)
                    .WhereIf(!string.IsNullOrWhiteSpace(input.Filter),
                        e => false || e.PostCaption.Contains(input.Filter) || e.PostTitle.Contains(input.Filter))
                    .WhereIf(!string.IsNullOrWhiteSpace(input.PostCaptionFilter),
                        e => e.PostCaption.Contains(input.PostCaptionFilter))
                    .WhereIf(input.MinPostTimeFilter != null, e => e.PostTime >= input.MinPostTimeFilter)
                    .WhereIf(input.MaxPostTimeFilter != null, e => e.PostTime <= input.MaxPostTimeFilter)
                    .WhereIf(input.IsSpecialFilter.HasValue && input.IsSpecialFilter > -1,
                        e => (input.IsSpecialFilter == 1 && e.IsSpecial) ||
                             (input.IsSpecialFilter == 0 && !e.IsSpecial))
                    .WhereIf(!string.IsNullOrWhiteSpace(input.PostTitleFilter),
                        e => e.PostTitle.Contains(input.PostTitleFilter))
                    .WhereIf(!string.IsNullOrWhiteSpace(input.GroupMemberMemberPositionFilter),
                        e => e.GroupMemberFk != null &&
                             e.GroupMemberFk.MemberPosition == input.GroupMemberMemberPositionFilter)
                    .WhereIf(!string.IsNullOrWhiteSpace(input.PostGroupPostGroupDescriptionFilter),
                        e => e.PostGroupFk != null && e.PostGroupFk.PostGroupDescription ==
                            input.PostGroupPostGroupDescriptionFilter);

                var pagedAndFilteredPosts = filteredPosts
                    .OrderBy(input.Sorting ?? "id asc")
                    .PageBy(input);

                var posts = from o in pagedAndFilteredPosts
                            join o1 in _lookup_groupMemberRepository.GetAll() on o.GroupMemberId equals o1.Id into j1
                            from s1 in j1.DefaultIfEmpty()

                            join o2 in _lookup_postGroupRepository.GetAll() on o.PostGroupId equals o2.Id into j2
                            from s2 in j2.DefaultIfEmpty()

                            select new
                            {
                                o.PostFile,
                                o.PostCaption,
                                o.PostTime,
                                o.IsSpecial,
                                o.PostTitle,
                                o.PostRefLink,
                                s2.GroupFile,
                                o.Id,
                                GroupMemberMemberPosition =
                                    s1 == null || s1.MemberPosition == null ? "" : s1.MemberPosition,
                                PostGroupPostGroupDescription = s2 == null || s2.PostGroupDescription == null
                                    ? ""
                                    : s2.PostGroupDescription.ToString()

                            };

                var totalCount = await filteredPosts.CountAsync();

                var dbList = await posts.ToListAsync();
                var results = new List<GetPostForViewDto>();

                foreach (var o in dbList)
                {
                    var res = new GetPostForViewDto()
                    {
                        Post = new PostDto
                        {
                            PostFile = o.PostFile,
                            PostCaption = o.PostCaption,
                            PostTime = o.PostTime,
                            IsSpecial = o.IsSpecial,
                            PostTitle = o.PostTitle,
                            Id = o.Id,
                            PostRefLink = o.PostRefLink,
                        },
                        GroupMemberMemberPosition = o.GroupMemberMemberPosition,
                        PostGroupPostGroupDescription = o.PostGroupPostGroupDescription,
                        GroupFile = o.GroupFile
                    };
                    res.Post.PostFileFileName = await GetBinaryFileName(o.PostFile);

                    results.Add(res);
                }

                return new PagedResultDto<GetPostForViewDto>(
                    totalCount,
                    results
                );
            }
            finally
            {
                //Thread.CurrentThread.CurrentCulture = _originalCulture;
                //Thread.CurrentThread.CurrentUICulture = _originalCulture;
            }

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
            var post = ObjectMapper.Map<Post>(input);

            await _postRepository.InsertAsync(post);
            post.PostFile = await GetBinaryObjectFromCache(input.PostFileToken);
            post.PostFile2 = await GetBinaryObjectFromCache(input.PostFileToken2);
            post.PostFile3 = await GetBinaryObjectFromCache(input.PostFileToken3);

        }

        [AbpAuthorize(AppPermissions.Pages_Posts_Edit)]
        protected virtual async Task Update(CreateOrEditPostDto input)
        {
            var post = await _postRepository.FirstOrDefaultAsync((int)input.Id);
            ObjectMapper.Map(input, post);
            post.PostFile = await GetBinaryObjectFromCache(input.PostFileToken);
            post.PostFile2 = await GetBinaryObjectFromCache(input.PostFileToken2);
            post.PostFile3 = await GetBinaryObjectFromCache(input.PostFileToken3);

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
                        .WhereIf(input.MinPostTimeFilter != null, e => e.PostTime >= input.MinPostTimeFilter)
                        .WhereIf(input.MaxPostTimeFilter != null, e => e.PostTime <= input.MaxPostTimeFilter)
                        .WhereIf(input.IsSpecialFilter.HasValue && input.IsSpecialFilter > -1, e => (input.IsSpecialFilter == 1 && e.IsSpecial) || (input.IsSpecialFilter == 0 && !e.IsSpecial))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.PostTitleFilter), e => e.PostTitle.Contains(input.PostTitleFilter))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.GroupMemberMemberPositionFilter), e => e.GroupMemberFk != null && e.GroupMemberFk.MemberPosition == input.GroupMemberMemberPositionFilter)
                        .WhereIf(!string.IsNullOrWhiteSpace(input.PostGroupPostGroupDescriptionFilter), e => e.PostGroupFk != null && e.PostGroupFk.PostGroupDescription == input.PostGroupPostGroupDescriptionFilter);

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
                                 PostTime = o.PostTime,
                                 IsSpecial = o.IsSpecial,
                                 PostTitle = o.PostTitle,
                                 Id = o.Id
                             },
                             GroupMemberMemberPosition = s1 == null || s1.MemberPosition == null ? "" : s1.MemberPosition,
                             PostGroupPostGroupDescription = s2 == null || s2.PostGroupDescription == null ? "" : s2.PostGroupDescription.ToString()
                         });

            var postListDtos = await query.ToListAsync();

            return _postsExcelExporter.ExportToFile(postListDtos);
        }

        [AbpAuthorize(AppPermissions.Pages_Posts)]
        public async Task<PagedResultDto<PostGroupMemberLookupTableDto>> GetAllGroupMemberForLookupTable(GetAllForLookupTableInput input)
        {
            var query = _lookup_groupMemberRepository.GetAll().WhereIf(
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
                    DisplayName = groupMember.MemberPosition
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

        private async Task<Guid?> GetBinaryObjectFromCache(string fileToken)
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

            var storedFile = new BinaryObject(AbpSession.TenantId, fileCache.File, fileCache.FileName);
            await _binaryObjectManager.SaveAsync(storedFile);

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

        public Task<PagedResultDto<GetPostCategoriesForViewDto>> GetPostCategoriesForView()
        {
            try
            {
                var res = new List<GetPostCategoriesForViewDto>();
                foreach (var postCategory in _postCategoryRepository.GetAll())
                {
                    res.Add(new GetPostCategoriesForViewDto()
                    {
                        //Base64Image = "data:image/png;base64,"+Convert.ToBase64String(postCategory.Bytes, 0, postCategory.Bytes.Length) ,
                        Base64Image = postCategory.FileId,
                        Id = postCategory.Id,
                        PostGroupDescription = postCategory.PostGroupDescription
                    });
                }
                return Task.FromResult(new PagedResultDto<GetPostCategoriesForViewDto>(res.Count, res));
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
                        PostCaption = post.PostCaption,
                        PostTime = post.CreationTime,
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
                //_originalCulture = Thread.CurrentThread.CurrentCulture;

                //Thread.CurrentThread.CurrentCulture = _targetCulture;
                //Thread.CurrentThread.CurrentUICulture = _targetCulture;
                var res = new List<GetPostsForViewDto>();
                var filteredPosts = _postRepository.GetAll()
                    .Include(e => e.GroupMemberFk)
                    .Include(e => e.PostGroupFk)
                    .Include(e => e.GroupMemberFk.UserFk)
                    .Include(e => e.AppBinaryObjectFk)
                    .Include(e => e.AppBinaryObjectFk2)
                    .Include(e => e.AppBinaryObjectFk3)
                    //.WhereIf(input.IsSpecialFilter.HasValue && input.IsSpecialFilter > -1, e => (input.IsSpecialFilter == 1 && e.IsSpecial) || (input.IsSpecialFilter == 0 && !e.IsSpecial))
                    .WhereIf(input.PostGroupId > 0, e => e.PostGroupId == input.PostGroupId);
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
                        PostCaption = post.PostCaption,
                        PostTime = post.CreationTime,
                        PostFile = post.PostFile,
                        PostFile2 = post.PostFile2,
                        PostFile3 = post.PostFile3,
                        PostTitle = post.PostTitle,
                        PostGroupId = post.PostGroupId,
                        PostRefLink=post.PostRefLink,
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

                    res.Add(datam);


                }

                return Task.FromResult(new PagedResultDto<GetPostsForViewDto>(res.Count, res));
            }
            catch (Exception ex)
            {
                throw new UserFriendlyException(ex.Message);
            }
            finally
            {
                //Thread.CurrentThread.CurrentCulture = _originalCulture;
                //Thread.CurrentThread.CurrentUICulture = _originalCulture;
            }
        }
    }
}