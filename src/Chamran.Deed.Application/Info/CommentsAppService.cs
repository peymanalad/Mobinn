﻿using Chamran.Deed.Info;
using Chamran.Deed.Authorization.Users;

using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using Abp.Linq.Extensions;
using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
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
    [AbpAuthorize(AppPermissions.Pages_Comments)]
    public class CommentsAppService : DeedAppServiceBase, ICommentsAppService
    {
        private readonly IRepository<Comment> _commentRepository;
        private readonly IRepository<Post, int> _lookup_postRepository;
        private readonly IRepository<User, long> _lookup_userRepository;

        public CommentsAppService(IRepository<Comment> commentRepository, IRepository<Post, int> lookup_postRepository, IRepository<User, long> lookup_userRepository)
        {
            _commentRepository = commentRepository;
            _lookup_postRepository = lookup_postRepository;
            _lookup_userRepository = lookup_userRepository;

        }

        public async Task<PagedResultDto<GetCommentForViewDto>> GetAll(GetAllCommentsInput input)
        {

            var filteredComments = _commentRepository.GetAll()
                        .Include(e => e.PostFk)
                        .Include(e => e.UserFk)
                        .WhereIf(!string.IsNullOrWhiteSpace(input.Filter), e => false || e.CommentCaption.Contains(input.Filter))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.CommentCaptionFilter), e => e.CommentCaption.Contains(input.CommentCaptionFilter))
                        .WhereIf(input.MinCommentDateFilter != null, e => e.CommentDate >= input.MinCommentDateFilter)
                        .WhereIf(input.MaxCommentDateFilter != null, e => e.CommentDate <= input.MaxCommentDateFilter)
                        .WhereIf(!string.IsNullOrWhiteSpace(input.PostPostTitleFilter), e => e.PostFk != null && e.PostFk.PostTitle == input.PostPostTitleFilter)
                        .WhereIf(!string.IsNullOrWhiteSpace(input.UserNameFilter), e => e.UserFk != null && e.UserFk.Name == input.UserNameFilter);

            var pagedAndFilteredComments = filteredComments
                .OrderBy(input.Sorting ?? "id asc")
                .PageBy(input);

            var comments = from o in pagedAndFilteredComments
                           join o1 in _lookup_postRepository.GetAll() on o.PostId equals o1.Id into j1
                           from s1 in j1.DefaultIfEmpty()

                           join o2 in _lookup_userRepository.GetAll() on o.UserId equals o2.Id into j2
                           from s2 in j2.DefaultIfEmpty()

                           select new
                           {

                               o.CommentCaption,
                               o.CommentDate,
                               Id = o.Id,
                               PostPostTitle = s1 == null || s1.PostTitle == null ? "" : s1.PostTitle.ToString(),
                               UserName = s2 == null || s2.Name == null ? "" : s2.Name.ToString()
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
                        CommentDate = o.CommentDate,
                        Id = o.Id,
                    },
                    PostPostTitle = o.PostPostTitle,
                    UserName = o.UserName
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
                output.PostPostTitle = _lookupPost?.PostTitle?.ToString();
            }

            if (output.Comment.UserId != null)
            {
                var _lookupUser = await _lookup_userRepository.FirstOrDefaultAsync((long)output.Comment.UserId);
                output.UserName = _lookupUser?.Name?.ToString();
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
                output.PostPostTitle = _lookupPost?.PostTitle?.ToString();
            }

            if (output.Comment.UserId != null)
            {
                var _lookupUser = await _lookup_userRepository.FirstOrDefaultAsync((long)output.Comment.UserId);
                output.UserName = _lookupUser?.Name?.ToString();
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
                    DisplayName = post.PostTitle?.ToString()
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
                    DisplayName = user.Name?.ToString()
                });
            }

            return new PagedResultDto<CommentUserLookupTableDto>(
                totalCount,
                lookupTableDtoList
            );
        }

    }
}