using Chamran.Deed.Info;
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
using Abp.Extensions;
using Abp.Authorization;
using Microsoft.EntityFrameworkCore;
using Abp.UI;
using Chamran.Deed.Storage;
using System.ComponentModel.Design;

namespace Chamran.Deed.Info
{
    [AbpAuthorize(AppPermissions.Pages_CommentLikes)]
    public class CommentLikesAppService : DeedAppServiceBase, ICommentLikesAppService
    {
        private readonly IRepository<CommentLike> _commentLikeRepository;
        private readonly ICommentLikesExcelExporter _commentLikesExcelExporter;
        private readonly IRepository<Comment, int> _lookup_commentRepository;
        private readonly IRepository<User, long> _lookup_userRepository;

        public CommentLikesAppService(IRepository<CommentLike> commentLikeRepository, ICommentLikesExcelExporter commentLikesExcelExporter, IRepository<Comment, int> lookup_commentRepository, IRepository<User, long> lookup_userRepository)
        {
            _commentLikeRepository = commentLikeRepository;
            _commentLikesExcelExporter = commentLikesExcelExporter;
            _lookup_commentRepository = lookup_commentRepository;
            _lookup_userRepository = lookup_userRepository;

        }

        public async Task<PagedResultDto<GetCommentLikeForViewDto>> GetAll(GetAllCommentLikesInput input)
        {

            var filteredCommentLikes = _commentLikeRepository.GetAll()
                        .Include(e => e.CommentFk)
                        .Include(e => e.UserFk)
                        .WhereIf(!string.IsNullOrWhiteSpace(input.Filter), e => false)
                        .WhereIf(input.MinLikeTimeFilter != null, e => e.LikeTime >= input.MinLikeTimeFilter)
                        .WhereIf(input.MaxLikeTimeFilter != null, e => e.LikeTime <= input.MaxLikeTimeFilter)
                        .WhereIf(!string.IsNullOrWhiteSpace(input.CommentCommentCaptionFilter), e => e.CommentFk != null && e.CommentFk.CommentCaption == input.CommentCommentCaptionFilter)
                        .WhereIf(!string.IsNullOrWhiteSpace(input.UserNameFilter), e => e.UserFk != null && e.UserFk.Name == input.UserNameFilter);

            var pagedAndFilteredCommentLikes = filteredCommentLikes
                .OrderBy(input.Sorting ?? "id asc")
                .PageBy(input);

            var commentLikes = from o in pagedAndFilteredCommentLikes
                               join o1 in _lookup_commentRepository.GetAll() on o.CommentId equals o1.Id into j1
                               from s1 in j1.DefaultIfEmpty()

                               join o2 in _lookup_userRepository.GetAll() on o.UserId equals o2.Id into j2
                               from s2 in j2.DefaultIfEmpty()

                               select new
                               {

                                   o.LikeTime,
                                   Id = o.Id,
                                   CommentCommentCaption = s1 == null || s1.CommentCaption == null ? "" : s1.CommentCaption.ToString(),
                                   UserName = s2 == null || s2.Name == null ? "" : s2.Name.ToString()
                               };

            var totalCount = await filteredCommentLikes.CountAsync();

            var dbList = await commentLikes.ToListAsync();
            var results = new List<GetCommentLikeForViewDto>();

            foreach (var o in dbList)
            {
                var res = new GetCommentLikeForViewDto()
                {
                    CommentLike = new CommentLikeDto
                    {

                        LikeTime = o.LikeTime,
                        Id = o.Id,
                    },
                    CommentCommentCaption = o.CommentCommentCaption,
                    UserName = o.UserName
                };

                results.Add(res);
            }

            return new PagedResultDto<GetCommentLikeForViewDto>(
                totalCount,
                results
            );

        }

        public async Task<GetCommentLikeForViewDto> GetCommentLikeForView(int id)
        {
            var commentLike = await _commentLikeRepository.GetAsync(id);

            var output = new GetCommentLikeForViewDto { CommentLike = ObjectMapper.Map<CommentLikeDto>(commentLike) };

            if (output.CommentLike.CommentId != null)
            {
                var _lookupComment = await _lookup_commentRepository.FirstOrDefaultAsync((int)output.CommentLike.CommentId);
                output.CommentCommentCaption = _lookupComment?.CommentCaption?.ToString();
            }

            if (output.CommentLike.UserId != null)
            {
                var _lookupUser = await _lookup_userRepository.FirstOrDefaultAsync((long)output.CommentLike.UserId);
                output.UserName = _lookupUser?.Name?.ToString();
            }

            return output;
        }

        [AbpAuthorize(AppPermissions.Pages_CommentLikes_Edit)]
        public async Task<GetCommentLikeForEditOutput> GetCommentLikeForEdit(EntityDto input)
        {
            var commentLike = await _commentLikeRepository.FirstOrDefaultAsync(input.Id);

            var output = new GetCommentLikeForEditOutput { CommentLike = ObjectMapper.Map<CreateOrEditCommentLikeDto>(commentLike) };

            if (output.CommentLike.CommentId != null)
            {
                var _lookupComment = await _lookup_commentRepository.FirstOrDefaultAsync((int)output.CommentLike.CommentId);
                output.CommentCommentCaption = _lookupComment?.CommentCaption?.ToString();
            }

            if (output.CommentLike.UserId != null)
            {
                var _lookupUser = await _lookup_userRepository.FirstOrDefaultAsync((long)output.CommentLike.UserId);
                output.UserName = _lookupUser?.Name?.ToString();
            }

            return output;
        }

        public async Task CreateOrEdit(CreateOrEditCommentLikeDto input)
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

        [AbpAuthorize(AppPermissions.Pages_CommentLikes_Create)]
        protected virtual async Task Create(CreateOrEditCommentLikeDto input)
        {
            var commentLike = ObjectMapper.Map<CommentLike>(input);

            await _commentLikeRepository.InsertAsync(commentLike);

        }

        [AbpAuthorize(AppPermissions.Pages_CommentLikes_Edit)]
        protected virtual async Task Update(CreateOrEditCommentLikeDto input)
        {
            var commentLike = await _commentLikeRepository.FirstOrDefaultAsync((int)input.Id);
            ObjectMapper.Map(input, commentLike);

        }

        [AbpAuthorize(AppPermissions.Pages_CommentLikes_Delete)]
        public async Task Delete(EntityDto input)
        {
            await _commentLikeRepository.DeleteAsync(input.Id);
        }

        public async Task<FileDto> GetCommentLikesToExcel(GetAllCommentLikesForExcelInput input)
        {

            var filteredCommentLikes = _commentLikeRepository.GetAll()
                        .Include(e => e.CommentFk)
                        .Include(e => e.UserFk)
                        .WhereIf(!string.IsNullOrWhiteSpace(input.Filter), e => false)
                        .WhereIf(input.MinLikeTimeFilter != null, e => e.LikeTime >= input.MinLikeTimeFilter)
                        .WhereIf(input.MaxLikeTimeFilter != null, e => e.LikeTime <= input.MaxLikeTimeFilter)
                        .WhereIf(!string.IsNullOrWhiteSpace(input.CommentCommentCaptionFilter), e => e.CommentFk != null && e.CommentFk.CommentCaption == input.CommentCommentCaptionFilter)
                        .WhereIf(!string.IsNullOrWhiteSpace(input.UserNameFilter), e => e.UserFk != null && e.UserFk.Name == input.UserNameFilter);

            var query = (from o in filteredCommentLikes
                         join o1 in _lookup_commentRepository.GetAll() on o.CommentId equals o1.Id into j1
                         from s1 in j1.DefaultIfEmpty()

                         join o2 in _lookup_userRepository.GetAll() on o.UserId equals o2.Id into j2
                         from s2 in j2.DefaultIfEmpty()

                         select new GetCommentLikeForViewDto()
                         {
                             CommentLike = new CommentLikeDto
                             {
                                 LikeTime = o.LikeTime,
                                 Id = o.Id
                             },
                             CommentCommentCaption = s1 == null || s1.CommentCaption == null ? "" : s1.CommentCaption.ToString(),
                             UserName = s2 == null || s2.Name == null ? "" : s2.Name.ToString()
                         });

            var commentLikeListDtos = await query.ToListAsync();

            return _commentLikesExcelExporter.ExportToFile(commentLikeListDtos);
        }

        [AbpAuthorize(AppPermissions.Pages_CommentLikes)]
        public async Task<PagedResultDto<CommentLikeCommentLookupTableDto>> GetAllCommentForLookupTable(GetAllForLookupTableInput input)
        {
            var query = _lookup_commentRepository.GetAll().WhereIf(
                   !string.IsNullOrWhiteSpace(input.Filter),
                  e => e.CommentCaption != null && e.CommentCaption.Contains(input.Filter)
               );

            var totalCount = await query.CountAsync();

            var commentList = await query
                .PageBy(input)
                .ToListAsync();

            var lookupTableDtoList = new List<CommentLikeCommentLookupTableDto>();
            foreach (var comment in commentList)
            {
                lookupTableDtoList.Add(new CommentLikeCommentLookupTableDto
                {
                    Id = comment.Id,
                    DisplayName = comment.CommentCaption?.ToString()
                });
            }

            return new PagedResultDto<CommentLikeCommentLookupTableDto>(
                totalCount,
                lookupTableDtoList
            );
        }

        [AbpAuthorize(AppPermissions.Pages_CommentLikes)]
        public async Task<PagedResultDto<CommentLikeUserLookupTableDto>> GetAllUserForLookupTable(GetAllForLookupTableInput input)
        {
            var query = _lookup_userRepository.GetAll().WhereIf(
                   !string.IsNullOrWhiteSpace(input.Filter),
                  e => e.Name != null && e.Name.Contains(input.Filter)
               );

            var totalCount = await query.CountAsync();

            var userList = await query
                .PageBy(input)
                .ToListAsync();

            var lookupTableDtoList = new List<CommentLikeUserLookupTableDto>();
            foreach (var user in userList)
            {
                lookupTableDtoList.Add(new CommentLikeUserLookupTableDto
                {
                    Id = user.Id,
                    DisplayName = user.Name?.ToString()
                });
            }

            return new PagedResultDto<CommentLikeUserLookupTableDto>(
                totalCount,
                lookupTableDtoList
            );
        }


        public async Task<int> GetLikeCountOfComment(int commentId)
        {
            if (commentId <= 0) throw new UserFriendlyException("PostId should be greater than zero");
            return await _commentLikeRepository.GetAll().Where(e => e.CommentId == commentId).CountAsync();
        }

        public async Task<bool> IsCommentLiked(int commentId)
        {
            if (commentId <= 0) throw new UserFriendlyException("PostId should be greater than zero");
            if (!AbpSession.UserId.HasValue) throw new UserFriendlyException("Not Logged In!");
            return await _commentLikeRepository.GetAll()
                .Where(e => e.CommentId == commentId && e.UserId == AbpSession.UserId.Value).AnyAsync();

        }

        [AbpAuthorize(AppPermissions.Pages_CommentLikes_Create)]
        public async Task CreateCurrentCommentLike(int commentId)
        {
            if (AbpSession.UserId == null) throw new UserFriendlyException("Not Logged In!");

            var seen = new CommentLike()
            {
                UserId = AbpSession.UserId.Value,
                CommentId = commentId,
                LikeTime = DateTime.Now,
            };
            await _commentLikeRepository.InsertAsync(seen);
        }

        [AbpAuthorize(AppPermissions.Pages_CommentLikes_Create)]
        public async Task CreateCommentLikeByDate(int commentId, DateTime likeDateTime)
        {
            if (AbpSession.UserId == null) throw new UserFriendlyException("Not Logged In!");

            var seen = new CommentLike()
            {
                UserId = AbpSession.UserId.Value,
                CommentId = commentId,
                LikeTime = likeDateTime
            };
            await _commentLikeRepository.InsertAsync(seen);
        }
    }
}