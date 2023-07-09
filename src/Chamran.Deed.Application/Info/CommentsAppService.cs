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
    [AbpAuthorize(AppPermissions.Pages_Comments)]
    public class CommentsAppService : DeedAppServiceBase, ICommentsAppService
    {
        private readonly IRepository<Comment> _commentRepository;
        private readonly ICommentsExcelExporter _commentsExcelExporter;
        private readonly IRepository<Post, int> _lookup_postRepository;
        private readonly IRepository<User, long> _lookup_userRepository;
        private readonly IRepository<Comment, int> _lookup_commentRepository;

        public CommentsAppService(IRepository<Comment> commentRepository, ICommentsExcelExporter commentsExcelExporter, IRepository<Post, int> lookup_postRepository, IRepository<User, long> lookup_userRepository, IRepository<Comment, int> lookup_commentRepository)
        {
            _commentRepository = commentRepository;
            _commentsExcelExporter = commentsExcelExporter;
            _lookup_postRepository = lookup_postRepository;
            _lookup_userRepository = lookup_userRepository;
            _lookup_commentRepository = lookup_commentRepository;

        }

        public async Task<PagedResultDto<GetCommentForViewDto>> GetAll(GetAllCommentsInput input)
        {

            var filteredComments = _commentRepository.GetAll()
                        .Include(e => e.PostFk)
                        .Include(e => e.UserFk)
                        .Include(e => e.CommentFk)
                        .WhereIf(!string.IsNullOrWhiteSpace(input.Filter), e => false || e.CommentCaption.Contains(input.Filter))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.CommentCaptionFilter), e => e.CommentCaption.Contains(input.CommentCaptionFilter))
                        .WhereIf(input.MinInsertDateFilter != null, e => e.InsertDate >= input.MinInsertDateFilter)
                        .WhereIf(input.MaxInsertDateFilter != null, e => e.InsertDate <= input.MaxInsertDateFilter)
                        .WhereIf(!string.IsNullOrWhiteSpace(input.PostPostTitleFilter), e => e.PostFk != null && e.PostFk.PostTitle == input.PostPostTitleFilter)
                        .WhereIf(!string.IsNullOrWhiteSpace(input.UserNameFilter), e => e.UserFk != null && e.UserFk.Name == input.UserNameFilter)
                        .WhereIf(!string.IsNullOrWhiteSpace(input.CommentCommentCaptionFilter), e => e.CommentFk != null && e.CommentFk.CommentCaption == input.CommentCommentCaptionFilter);

            var pagedAndFilteredComments = filteredComments
                .OrderBy(input.Sorting ?? "id asc")
                .PageBy(input);

            var comments = from o in pagedAndFilteredComments
                           join o1 in _lookup_postRepository.GetAll() on o.PostId equals o1.Id into j1
                           from s1 in j1.DefaultIfEmpty()

                           join o2 in _lookup_userRepository.GetAll() on o.UserId equals o2.Id into j2
                           from s2 in j2.DefaultIfEmpty()

                           join o3 in _lookup_commentRepository.GetAll() on o.CommentId equals o3.Id into j3
                           from s3 in j3.DefaultIfEmpty()

                           select new
                           {

                               o.CommentCaption,
                               o.InsertDate,
                               Id = o.Id,
                               PostPostTitle = s1 == null || s1.PostTitle == null ? "" : s1.PostTitle,
                               UserName = s2 == null || s2.Name == null ? "" : s2.Name,
                               CommentCommentCaption = s3 == null || s3.CommentCaption == null ? "" : s3.CommentCaption.ToString()
                           };

            var totalCount = await filteredComments.CountAsync();

            var dbList = await comments.ToListAsync();
            var results = new List<GetCommentForViewDto>();

            foreach (var o in dbList)
            {
                var res = new GetCommentForViewDto()
                {
                    Comment = new CommentDto
                    {

                        CommentCaption = o.CommentCaption,
                        InsertDate = o.InsertDate,
                        Id = o.Id,
                    },
                    PostPostTitle = o.PostPostTitle,
                    UserName = o.UserName,
                    CommentCommentCaption = o.CommentCommentCaption
                };

                results.Add(res);
            }

            return new PagedResultDto<GetCommentForViewDto>(
                totalCount,
                results
            );

        }

        public async Task<GetCommentForViewDto> GetCommentForView(int id)
        {
            var comment = await _commentRepository.GetAsync(id);

            var output = new GetCommentForViewDto { Comment = ObjectMapper.Map<CommentDto>(comment) };

            if (output.Comment.PostId != null)
            {
                var _lookupPost = await _lookup_postRepository.FirstOrDefaultAsync((int)output.Comment.PostId);
                output.PostPostTitle = _lookupPost?.PostTitle;
            }

            if (output.Comment.UserId != null)
            {
                var _lookupUser = await _lookup_userRepository.FirstOrDefaultAsync((long)output.Comment.UserId);
                output.UserName = _lookupUser?.Name;
            }

            if (output.Comment.CommentId != null)
            {
                var _lookupComment = await _lookup_commentRepository.FirstOrDefaultAsync((int)output.Comment.CommentId);
                output.CommentCommentCaption = _lookupComment?.CommentCaption;
            }

            return output;
        }

        [AbpAuthorize(AppPermissions.Pages_Comments_Edit)]
        public async Task<GetCommentForEditOutput> GetCommentForEdit(EntityDto input)
        {
            var comment = await _commentRepository.FirstOrDefaultAsync(input.Id);

            var output = new GetCommentForEditOutput { Comment = ObjectMapper.Map<CreateOrEditCommentDto>(comment) };

            if (output.Comment.PostId != null)
            {
                var _lookupPost = await _lookup_postRepository.FirstOrDefaultAsync((int)output.Comment.PostId);
                output.PostPostTitle = _lookupPost?.PostTitle;
            }

            if (output.Comment.UserId != null)
            {
                var _lookupUser = await _lookup_userRepository.FirstOrDefaultAsync((long)output.Comment.UserId);
                output.UserName = _lookupUser?.Name;
            }

            if (output.Comment.CommentId != null)
            {
                var _lookupComment = await _lookup_commentRepository.FirstOrDefaultAsync((int)output.Comment.CommentId);
                output.CommentCommentCaption = _lookupComment?.CommentCaption;
            }

            return output;
        }

        public async Task CreateOrEdit(CreateOrEditCommentDto input)
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

        [AbpAuthorize(AppPermissions.Pages_Comments_Create)]
        protected virtual async Task Create(CreateOrEditCommentDto input)
        {
            var comment = ObjectMapper.Map<Comment>(input);

            await _commentRepository.InsertAsync(comment);

        }

        [AbpAuthorize(AppPermissions.Pages_Comments_Edit)]
        protected virtual async Task Update(CreateOrEditCommentDto input)
        {
            var comment = await _commentRepository.FirstOrDefaultAsync((int)input.Id);
            ObjectMapper.Map(input, comment);

        }

        [AbpAuthorize(AppPermissions.Pages_Comments_Delete)]
        public async Task Delete(EntityDto input)
        {
            await _commentRepository.DeleteAsync(input.Id);
        }

        public async Task<FileDto> GetCommentsToExcel(GetAllCommentsForExcelInput input)
        {

            var filteredComments = _commentRepository.GetAll()
                        .Include(e => e.PostFk)
                        .Include(e => e.UserFk)
                        .Include(e => e.CommentFk)
                        .WhereIf(!string.IsNullOrWhiteSpace(input.Filter), e => false || e.CommentCaption.Contains(input.Filter))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.CommentCaptionFilter), e => e.CommentCaption.Contains(input.CommentCaptionFilter))
                        .WhereIf(input.MinInsertDateFilter != null, e => e.InsertDate >= input.MinInsertDateFilter)
                        .WhereIf(input.MaxInsertDateFilter != null, e => e.InsertDate <= input.MaxInsertDateFilter)
                        .WhereIf(!string.IsNullOrWhiteSpace(input.PostPostTitleFilter), e => e.PostFk != null && e.PostFk.PostTitle == input.PostPostTitleFilter)
                        .WhereIf(!string.IsNullOrWhiteSpace(input.UserNameFilter), e => e.UserFk != null && e.UserFk.Name == input.UserNameFilter)
                        .WhereIf(!string.IsNullOrWhiteSpace(input.CommentCommentCaptionFilter), e => e.CommentFk != null && e.CommentFk.CommentCaption == input.CommentCommentCaptionFilter);

            var query = (from o in filteredComments
                         join o1 in _lookup_postRepository.GetAll() on o.PostId equals o1.Id into j1
                         from s1 in j1.DefaultIfEmpty()

                         join o2 in _lookup_userRepository.GetAll() on o.UserId equals o2.Id into j2
                         from s2 in j2.DefaultIfEmpty()

                         join o3 in _lookup_commentRepository.GetAll() on o.CommentId equals o3.Id into j3
                         from s3 in j3.DefaultIfEmpty()

                         select new GetCommentForViewDto()
                         {
                             Comment = new CommentDto
                             {
                                 CommentCaption = o.CommentCaption,
                                 InsertDate = o.InsertDate,
                                 Id = o.Id
                             },
                             PostPostTitle = s1 == null || s1.PostTitle == null ? "" : s1.PostTitle,
                             UserName = s2 == null || s2.Name == null ? "" : s2.Name,
                             CommentCommentCaption = s3 == null || s3.CommentCaption == null ? "" : s3.CommentCaption.ToString()
                         });

            var commentListDtos = await query.ToListAsync();

            return _commentsExcelExporter.ExportToFile(commentListDtos);
        }

        [AbpAuthorize(AppPermissions.Pages_Comments)]
        public async Task<PagedResultDto<CommentPostLookupTableDto>> GetAllPostForLookupTable(GetAllForLookupTableInput input)
        {
            var query = _lookup_postRepository.GetAll().WhereIf(
                   !string.IsNullOrWhiteSpace(input.Filter),
                  e => e.PostTitle != null && e.PostTitle.Contains(input.Filter)
               );

            var totalCount = await query.CountAsync();

            var postList = await query
                .PageBy(input)
                .ToListAsync();

            var lookupTableDtoList = new List<CommentPostLookupTableDto>();
            foreach (var post in postList)
            {
                lookupTableDtoList.Add(new CommentPostLookupTableDto
                {
                    Id = post.Id,
                    DisplayName = post.PostTitle
                });
            }

            return new PagedResultDto<CommentPostLookupTableDto>(
                totalCount,
                lookupTableDtoList
            );
        }

        [AbpAuthorize(AppPermissions.Pages_Comments)]
        public async Task<PagedResultDto<CommentUserLookupTableDto>> GetAllUserForLookupTable(GetAllForLookupTableInput input)
        {
            var query = _lookup_userRepository.GetAll().WhereIf(
                   !string.IsNullOrWhiteSpace(input.Filter),
                  e => e.Name != null && e.Name.Contains(input.Filter)
               );

            var totalCount = await query.CountAsync();

            var userList = await query
                .PageBy(input)
                .ToListAsync();

            var lookupTableDtoList = new List<CommentUserLookupTableDto>();
            foreach (var user in userList)
            {
                lookupTableDtoList.Add(new CommentUserLookupTableDto
                {
                    Id = user.Id,
                    DisplayName = user.Name
                });
            }

            return new PagedResultDto<CommentUserLookupTableDto>(
                totalCount,
                lookupTableDtoList
            );
        }

        [AbpAuthorize(AppPermissions.Pages_Comments)]
        public async Task<PagedResultDto<CommentCommentLookupTableDto>> GetAllCommentForLookupTable(GetAllForLookupTableInput input)
        {
            var query = _lookup_commentRepository.GetAll().WhereIf(
                   !string.IsNullOrWhiteSpace(input.Filter),
                  e => e.CommentCaption != null && e.CommentCaption.Contains(input.Filter)
               );

            var totalCount = await query.CountAsync();

            var commentList = await query
                .PageBy(input)
                .ToListAsync();

            var lookupTableDtoList = new List<CommentCommentLookupTableDto>();
            foreach (var comment in commentList)
            {
                lookupTableDtoList.Add(new CommentCommentLookupTableDto
                {
                    Id = comment.Id,
                    DisplayName = comment.CommentCaption
                });
            }

            return new PagedResultDto<CommentCommentLookupTableDto>(
                totalCount,
                lookupTableDtoList
            );
        }

        [AbpAuthorize(AppPermissions.Pages_Comments_Create)]
        public async Task<int> CreateComment(CreateCommentDto input)
        {
            if (AbpSession.UserId == null) throw new UserFriendlyException("Not Logged In!");
            var comment = ObjectMapper.Map<Comment>(input);
            comment.UserId = AbpSession.UserId.Value;
            comment.InsertDate = Clock.Now;
            var res=await _commentRepository.InsertAsync(comment);
            await CurrentUnitOfWork.SaveChangesAsync(); // Save changes to generate the identity value
            return res.Id;

        }

        public async Task<PagedResultDto<GetCommentForViewDto>> GetListOfComments(GetCommentsOfPostInput input)
        {
            if (input.PostId <= 0) throw new UserFriendlyException("PostId should be greater than zero");
            var filteredComments = _commentRepository.GetAll().Where(x => x.PostId == input.PostId && !x.IsDeleted)
                .Include(e => e.PostFk)
                .Include(e => e.UserFk)
                .Include(e => e.CommentFk);

            var pagedAndFilteredComments = filteredComments
                .OrderBy(input.Sorting ?? "id asc")
                .PageBy(input);

            var comments = from o in pagedAndFilteredComments
                           join o1 in _lookup_postRepository.GetAll() on o.PostId equals o1.Id into j1
                           from s1 in j1.DefaultIfEmpty()

                           join o2 in _lookup_userRepository.GetAll() on o.UserId equals o2.Id into j2
                           from s2 in j2.DefaultIfEmpty()

                           join o3 in _lookup_commentRepository.GetAll()
                           on o.CommentId equals o3.Id into j3
                           from s3 in j3.DefaultIfEmpty()


                           select new
                           {
                               ReplyUserName=s3.UserFk==null?"": s3.UserFk.Name+" "+ s3.UserFk.Surname,
                               o.PostId,
                               o.UserId,
                               o.CommentCaption,
                               o.InsertDate,
                               Id = o.Id,
                               PostPostTitle = s1 == null || s1.PostTitle == null ? "" : s1.PostTitle,
                               UserName = s2 == null || s2.Name == null ? "" : s2.Name+ " "+s2.Surname,
                               CommentCommentCaption = s3 == null || s3.CommentCaption == null ? "" : s3.CommentCaption.ToString()
                           };

            var totalCount = await filteredComments.CountAsync();

            var dbList = await comments.ToListAsync();
            var results = new List<GetCommentForViewDto>();

            foreach (var o in dbList)
            {
                var res = new GetCommentForViewDto()
                {
                    Comment = new CommentDto
                    {
                        PostId = o.PostId,
                        UserId = o.UserId,
                        CommentCaption = o.CommentCaption,
                        InsertDate = o.InsertDate,
                        Id = o.Id,
                    },
                    ReplyUserName=o.ReplyUserName,
                    PostPostTitle = o.PostPostTitle,
                    UserName = o.UserName,
                    CommentCommentCaption = o.CommentCommentCaption
                };

                results.Add(res);
            }

            return new PagedResultDto<GetCommentForViewDto>(
                totalCount,
                results
            );
        }

        public async Task<int> GetCommentCount(int postId)
        {
            if (postId <= 0) throw new UserFriendlyException("PostId should be greater than zero");
            return await _commentRepository.GetAll().Where(e => e.PostId == postId).CountAsync();

        }
    }
}