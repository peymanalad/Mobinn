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
using Abp.Timing;
using Microsoft.EntityFrameworkCore;
using Abp.UI;

namespace Chamran.Deed.Info
{
    [AbpAuthorize(AppPermissions.Pages_PostLikes)]
    public class PostLikesAppService : DeedAppServiceBase, IPostLikesAppService
    {
        private readonly IRepository<PostLike> _postLikeRepository;
        private readonly IPostLikesExcelExporter _postLikesExcelExporter;
        private readonly IRepository<Post, int> _lookup_postRepository;
        private readonly IRepository<User, long> _lookup_userRepository;

        public PostLikesAppService(IRepository<PostLike> postLikeRepository, IPostLikesExcelExporter postLikesExcelExporter, IRepository<Post, int> lookup_postRepository, IRepository<User, long> lookup_userRepository)
        {
            _postLikeRepository = postLikeRepository;
            _postLikesExcelExporter = postLikesExcelExporter;
            _lookup_postRepository = lookup_postRepository;
            _lookup_userRepository = lookup_userRepository;

        }

        public async Task<PagedResultDto<GetPostLikeForViewDto>> GetAll(GetAllPostLikesInput input)
        {

            var filteredPostLikes = _postLikeRepository.GetAll()
                        .Include(e => e.PostFk)
                        .Include(e => e.UserFk)
                        .Include(x => x.PostFk.PostGroupFk)
                        .Where(x => x.PostFk.PostGroupFk.OrganizationId == input.OrganizationId)
                        .WhereIf(input.UserId.HasValue, e => e.UserId == input.UserId.Value)
                        .WhereIf(!string.IsNullOrWhiteSpace(input.Filter), e => false)
                        .WhereIf(input.MinLikeTimeFilter != null, e => e.LikeTime >= input.MinLikeTimeFilter)
                        .WhereIf(input.MaxLikeTimeFilter != null, e => e.LikeTime <= input.MaxLikeTimeFilter)
                        .WhereIf(!string.IsNullOrWhiteSpace(input.PostPostTitleFilter), e => e.PostFk != null && e.PostFk.PostTitle == input.PostPostTitleFilter)
                        .WhereIf(!string.IsNullOrWhiteSpace(input.UserNameFilter), e => e.UserFk != null && e.UserFk.Name == input.UserNameFilter);
            var filtered = from o in filteredPostLikes
                join o1 in _lookup_postRepository.GetAll() on o.PostId equals o1.Id into j1
                           from s1 in j1.DefaultIfEmpty()
                           join o2 in _lookup_userRepository.GetAll() on o.UserId equals o2.Id into j2
                           from s2 in j2.DefaultIfEmpty()
                           select new
                {
                    o.UserId,
                    o.PostId,
                    o.LikeTime,
                    o.Id,
                    PostPostTitle = s1 == null || s1.PostTitle == null ? "" : s1.PostTitle,
                    UserName = s2 == null || s2.Name == null ? "" : s2.Name
                };

            var pagedAndFilteredPostLikes = filtered
                .OrderBy(input.Sorting ?? "id asc")
                .PageBy(input);

         

            var totalCount = await filtered.CountAsync();

            var dbList = await pagedAndFilteredPostLikes.ToListAsync();
            var results = new List<GetPostLikeForViewDto>();

            foreach (var o in dbList)
            {
                var res = new GetPostLikeForViewDto()
                {
                    PostLike = new PostLikeDto
                    {
                        PostId = o.PostId,
                        LikeTime = o.LikeTime,
                        Id = o.Id,
                    },
                    PostPostTitle = o.PostPostTitle,
                    UserName = o.UserName
                };

                results.Add(res);
            }

            return new PagedResultDto<GetPostLikeForViewDto>(
                totalCount,
                results
            );

        }

        public async Task<GetPostLikeForViewDto> GetPostLikeForView(int id)
        {
            var postLike = await _postLikeRepository.GetAsync(id);

            var output = new GetPostLikeForViewDto { PostLike = ObjectMapper.Map<PostLikeDto>(postLike) };

            if (output.PostLike.PostId != null)
            {
                var _lookupPost = await _lookup_postRepository.FirstOrDefaultAsync((int)output.PostLike.PostId);
                output.PostPostTitle = _lookupPost?.PostTitle?.ToString();
            }

            if (output.PostLike.UserId != null)
            {
                var _lookupUser = await _lookup_userRepository.FirstOrDefaultAsync((long)output.PostLike.UserId);
                output.UserName = _lookupUser?.Name?.ToString();
            }

            return output;
        }

        [AbpAuthorize(AppPermissions.Pages_PostLikes_Edit)]
        public async Task<GetPostLikeForEditOutput> GetPostLikeForEdit(EntityDto input)
        {
            var postLike = await _postLikeRepository.FirstOrDefaultAsync(input.Id);

            var output = new GetPostLikeForEditOutput { PostLike = ObjectMapper.Map<CreateOrEditPostLikeDto>(postLike) };

            if (output.PostLike.PostId != null)
            {
                var _lookupPost = await _lookup_postRepository.FirstOrDefaultAsync((int)output.PostLike.PostId);
                output.PostPostTitle = _lookupPost?.PostTitle?.ToString();
            }

            if (output.PostLike.UserId != null)
            {
                var _lookupUser = await _lookup_userRepository.FirstOrDefaultAsync((long)output.PostLike.UserId);
                output.UserName = _lookupUser?.Name?.ToString();
            }

            return output;
        }

        public async Task CreateOrEdit(CreateOrEditPostLikeDto input)
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

        [AbpAuthorize(AppPermissions.Pages_PostLikes_Create)]
        protected virtual async Task Create(CreateOrEditPostLikeDto input)
        {
            var postLike = ObjectMapper.Map<PostLike>(input);

            await _postLikeRepository.InsertAsync(postLike);

        }

        [AbpAuthorize(AppPermissions.Pages_PostLikes_Edit)]
        protected virtual async Task Update(CreateOrEditPostLikeDto input)
        {
            var postLike = await _postLikeRepository.FirstOrDefaultAsync((int)input.Id);
            ObjectMapper.Map(input, postLike);

        }

        [AbpAuthorize(AppPermissions.Pages_PostLikes_Delete)]
        public async Task Delete(EntityDto input)
        {
            await _postLikeRepository.DeleteAsync(input.Id);
        }

        public async Task<FileDto> GetPostLikesToExcel(GetAllPostLikesForExcelInput input)
        {

            var filteredPostLikes = _postLikeRepository.GetAll()
                        .Include(e => e.PostFk)
                        .Include(e => e.UserFk)
                        .WhereIf(!string.IsNullOrWhiteSpace(input.Filter), e => false)
                        .WhereIf(input.MinLikeTimeFilter != null, e => e.LikeTime >= input.MinLikeTimeFilter)
                        .WhereIf(input.MaxLikeTimeFilter != null, e => e.LikeTime <= input.MaxLikeTimeFilter)
                        .WhereIf(!string.IsNullOrWhiteSpace(input.PostPostTitleFilter), e => e.PostFk != null && e.PostFk.PostTitle == input.PostPostTitleFilter)
                        .WhereIf(!string.IsNullOrWhiteSpace(input.UserNameFilter), e => e.UserFk != null && e.UserFk.Name == input.UserNameFilter);

            var query = (from o in filteredPostLikes
                         join o1 in _lookup_postRepository.GetAll() on o.PostId equals o1.Id into j1
                         from s1 in j1.DefaultIfEmpty()

                         join o2 in _lookup_userRepository.GetAll() on o.UserId equals o2.Id into j2
                         from s2 in j2.DefaultIfEmpty()

                         select new GetPostLikeForViewDto()
                         {
                             PostLike = new PostLikeDto
                             {
                                 LikeTime = o.LikeTime,
                                 Id = o.Id
                             },
                             PostPostTitle = s1 == null || s1.PostTitle == null ? "" : s1.PostTitle.ToString(),
                             UserName = s2 == null || s2.Name == null ? "" : s2.Name.ToString()
                         });

            var postLikeListDtos = await query.ToListAsync();

            return _postLikesExcelExporter.ExportToFile(postLikeListDtos);
        }

        [AbpAuthorize(AppPermissions.Pages_PostLikes)]
        public async Task<PagedResultDto<PostLikePostLookupTableDto>> GetAllPostForLookupTable(GetAllForLookupTableInput input)
        {
            var query = _lookup_postRepository.GetAll().WhereIf(
                   !string.IsNullOrWhiteSpace(input.Filter),
                  e => e.PostTitle != null && e.PostTitle.Contains(input.Filter)
               );

            var totalCount = await query.CountAsync();

            var postList = await query
                .PageBy(input)
                .ToListAsync();

            var lookupTableDtoList = new List<PostLikePostLookupTableDto>();
            foreach (var post in postList)
            {
                lookupTableDtoList.Add(new PostLikePostLookupTableDto
                {
                    Id = post.Id,
                    DisplayName = post.PostTitle?.ToString()
                });
            }

            return new PagedResultDto<PostLikePostLookupTableDto>(
                totalCount,
                lookupTableDtoList
            );
        }

        [AbpAuthorize(AppPermissions.Pages_PostLikes)]
        public async Task<PagedResultDto<PostLikeUserLookupTableDto>> GetAllUserForLookupTable(GetAllForLookupTableInput input)
        {
            var query = _lookup_userRepository.GetAll().WhereIf(
                   !string.IsNullOrWhiteSpace(input.Filter),
                  e => e.Name != null && e.Name.Contains(input.Filter)
               );

            var totalCount = await query.CountAsync();

            var userList = await query
                .PageBy(input)
                .ToListAsync();

            var lookupTableDtoList = new List<PostLikeUserLookupTableDto>();
            foreach (var user in userList)
            {
                lookupTableDtoList.Add(new PostLikeUserLookupTableDto
                {
                    Id = user.Id,
                    DisplayName = user.Name?.ToString()
                });
            }

            return new PagedResultDto<PostLikeUserLookupTableDto>(
                totalCount,
                lookupTableDtoList
            );
        }

        public async Task<int> GetLikeCountOfPost(int postId)
        {
            if (postId <= 0) throw new UserFriendlyException("PostId should be greater than zero");
            return await _postLikeRepository.GetAll().Where(e => e.PostId == postId).CountAsync();

        }

        public async Task<bool> IsPostLiked(int postId)
        {
            if (postId <= 0) throw new UserFriendlyException("PostId should be greater than zero");
            if (!AbpSession.UserId.HasValue) throw new UserFriendlyException("Not Logged In!");
            return await _postLikeRepository.GetAll()
                .Where(e => e.PostId == postId && e.UserId == AbpSession.UserId.Value).AnyAsync();

        }

        public async Task<bool> PostDisLike(int postId)
        {
            if (postId <= 0) throw new UserFriendlyException("PostId should be greater than zero");
            if (!AbpSession.UserId.HasValue) throw new UserFriendlyException("Not Logged In!");
            return await _postLikeRepository.GetAll()
                .Where(e => e.PostId == postId && e.UserId == AbpSession.UserId.Value).ExecuteDeleteAsync() > 0;


        }

        [AbpAuthorize(AppPermissions.Pages_PostLikes_Create)]
        public async Task CreateCurrentLike(int postId)
        {
            if (AbpSession.UserId == null) throw new UserFriendlyException("Not Logged In!");

            var seen = new PostLike()
            {
                UserId = AbpSession.UserId.Value,
                PostId = postId,
                LikeTime = Clock.Now
            };
            await _postLikeRepository.InsertAsync(seen);
        }

        [AbpAuthorize(AppPermissions.Pages_PostLikes_Create)]
        public async Task CreateLikeByDate(int postId, DateTime seenDateTime)
        {
            if (AbpSession.UserId == null) throw new UserFriendlyException("Not Logged In!");

            var seen = new PostLike()
            {
                UserId = AbpSession.UserId.Value,
                PostId = postId,
                LikeTime = seenDateTime
            };
            await _postLikeRepository.InsertAsync(seen);
        }


    }
}