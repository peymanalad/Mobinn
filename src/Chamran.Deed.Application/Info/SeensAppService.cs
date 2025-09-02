using Chamran.Deed.Authorization.Users;

using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using Abp.Linq.Extensions;
using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using Chamran.Deed.Info.Dtos;
using Abp.Application.Services.Dto;
using Chamran.Deed.Authorization;
using Abp.Authorization;
using Microsoft.EntityFrameworkCore;
using Abp.UI;
using Abp.Timing;
using Chamran.Deed.Timing;

namespace Chamran.Deed.Info
{
    [AbpAuthorize(AppPermissions.Pages_Seens)]
    public class SeensAppService : DeedAppServiceBase, ISeensAppService
    {
        private readonly IRepository<Seen> _seenRepository;
        private readonly IRepository<Post, int> _lookup_postRepository;
        private readonly IRepository<User, long> _lookup_userRepository;

        public SeensAppService(IRepository<Seen> seenRepository, IRepository<Post, int> lookup_postRepository, IRepository<User, long> lookup_userRepository)
        {
            _seenRepository = seenRepository;
            _lookup_postRepository = lookup_postRepository;
            _lookup_userRepository = lookup_userRepository;

        }

        [AbpAuthorize(AppPermissions.Pages_Seens_Create)]
        public async Task CreateCurrentSeen(int postId)
        {
            if (AbpSession.UserId == null)
                throw new UserFriendlyException("Not Logged In!");

            var userId = AbpSession.UserId.Value;

            // Check if the record already exists
            var existingSeen = await _seenRepository.FirstOrDefaultAsync(s => s.UserId == userId && s.PostId == postId);

            if (existingSeen == null)
            {
                var seen = new Seen
                {
                    UserId = userId,
                    PostId = postId,
                    //SeenTime = Clock.Now
                    SeenTime = IranTimeZoneHelper.Now
                };

                await _seenRepository.InsertAsync(seen);
            }
        }

        [AbpAuthorize(AppPermissions.Pages_Seens_Create)]
        public async Task CreateSeenByDate(int postId, DateTime seenDateTime)
        {
            if (AbpSession.UserId == null) throw new UserFriendlyException("Not Logged In!");

            var seen = new Seen()
            {
                UserId = AbpSession.UserId,
                PostId = postId,
                SeenTime = seenDateTime
            };
            await _seenRepository.InsertAsync(seen);
        }


        public async Task<PagedResultDto<GetSeenForViewDto>> GetAll(GetAllSeensInput input)
        {

            var filteredSeens = _seenRepository.GetAll()
                        .Include(e => e.PostFk)
                        .Include(e => e.UserFk)
                        .WhereIf(!string.IsNullOrWhiteSpace(input.Filter), e => false)
                        .WhereIf(input.MinSeenTimeFilter != null, e => e.SeenTime >= input.MinSeenTimeFilter)
                        .WhereIf(input.MaxSeenTimeFilter != null, e => e.SeenTime <= input.MaxSeenTimeFilter)
                        .WhereIf(!string.IsNullOrWhiteSpace(input.PostPostTitleFilter), e => e.PostFk != null && e.PostFk.PostTitle == input.PostPostTitleFilter)
                        .WhereIf(!string.IsNullOrWhiteSpace(input.UserNameFilter), e => e.UserFk != null && e.UserFk.Name == input.UserNameFilter);

            var pagedAndFilteredSeens = filteredSeens
                .OrderBy(input.Sorting ?? "id asc")
                .PageBy(input);

            var seens = from o in pagedAndFilteredSeens
                        join o1 in _lookup_postRepository.GetAll() on o.PostId equals o1.Id into j1
                        from s1 in j1.DefaultIfEmpty()

                        join o2 in _lookup_userRepository.GetAll() on o.UserId equals o2.Id into j2
                        from s2 in j2.DefaultIfEmpty()

                        select new
                        {

                            o.SeenTime,
                            Id = o.Id,
                            PostPostTitle = s1 == null || s1.PostTitle == null ? "" : s1.PostTitle.ToString(),
                            UserName = s2 == null || s2.Name == null ? "" : s2.Name.ToString()
                        };

            var totalCount = await filteredSeens.CountAsync();

            var dbList = await seens.ToListAsync();
            var results = new List<GetSeenForViewDto>();

            foreach (var o in dbList)
            {
                var res = new GetSeenForViewDto()
                {
                    Seen = new SeenDto
                    {

                        SeenTime = o.SeenTime,
                        Id = o.Id,
                    },
                    PostPostTitle = o.PostPostTitle,
                    UserName = o.UserName
                };

                results.Add(res);
            }

            return new PagedResultDto<GetSeenForViewDto>(
                totalCount,
                results
            );

        }

        public async Task<PagedResultDto<GetSeenOfPostDto>> GetSeensOfPost(GetSeensOfPostInput input)
        {
            var filteredSeens = _seenRepository.GetAll()
                .Include(e => e.UserFk)
                .WhereIf(input.PostId > 0, e => e.PostId == input.PostId);



            var pagedAndFilteredSeens = filteredSeens
                .OrderBy(input.Sorting ?? "id asc")
                .PageBy(input);

            var seens = from o in pagedAndFilteredSeens
                            //join o1 in _lookup_postRepository.GetAll() on o.PostId equals o1.Id into j1
                            //from s1 in j1.DefaultIfEmpty()
                        join o2 in _lookup_userRepository.GetAll() on o.UserId equals o2.Id into j2
                        from s2 in j2.DefaultIfEmpty()

                        select new
                        {
                            s2.ProfilePictureId,
                            s2.FullName

                        };

            var totalCount = await filteredSeens.CountAsync();

            var dbList = await seens.ToListAsync();
            var results = new List<GetSeenOfPostDto>();

            foreach (var o in dbList)
            {
                var res = new GetSeenOfPostDto()
                {
                    ProfilePictureId = o.ProfilePictureId,
                    FullName = o.FullName,
                };

                results.Add(res);
            }

            return new PagedResultDto<GetSeenOfPostDto>(
                totalCount,
                results
            );
        }

        public async Task<PagedResultDto<GetSeenOfPostDto>> GetSeensOfPostFiltered(GetSeensOfPostFilteredInput input)
        {
            var filteredSeens = _seenRepository.GetAll()
                .Include(e => e.UserFk)
                .WhereIf(input.PostId > 0, e => e.PostId == input.PostId)
                .WhereIf(!string.IsNullOrWhiteSpace(input.Filter), e => e.UserFk.Surname.Contains(input.Filter) || e.UserFk.Name.Contains(input.Filter));


            var pagedAndFilteredSeens = filteredSeens
                .OrderBy(input.Sorting ?? "id asc")
                .PageBy(input);

            var seens = from o in pagedAndFilteredSeens
                            //join o1 in _lookup_postRepository.GetAll() on o.PostId equals o1.Id into j1
                            //from s1 in j1.DefaultIfEmpty()
                        join o2 in _lookup_userRepository.GetAll() on o.UserId equals o2.Id into j2
                        from s2 in j2.DefaultIfEmpty()

                        select new
                        {
                            s2.ProfilePictureId,
                            FullName = s2.Name + " " + s2.Surname

                        };

            var totalCount = await filteredSeens.CountAsync();

            var dbList = await seens.ToListAsync();
            var results = new List<GetSeenOfPostDto>();

            foreach (var o in dbList)
            {
                var res = new GetSeenOfPostDto()
                {
                    ProfilePictureId = o.ProfilePictureId,
                    FullName = o.FullName,
                };

                results.Add(res);
            }

            return new PagedResultDto<GetSeenOfPostDto>(
                totalCount,
                results
            );
        }

        public async Task<int> GetSeenCountOfPost(int postId)
        {
            if (postId <= 0) throw new UserFriendlyException("PostId should be greater than zero");
            return await _seenRepository.GetAll().Where(e => e.PostId == postId).CountAsync();

        }

        public async Task<GetSeenForViewDto> GetSeenForView(int id)
        {
            var seen = await _seenRepository.GetAsync(id);

            var output = new GetSeenForViewDto { Seen = ObjectMapper.Map<SeenDto>(seen) };

            if (output.Seen.PostId != null)
            {
                var _lookupPost = await _lookup_postRepository.FirstOrDefaultAsync((int)output.Seen.PostId);
                output.PostPostTitle = _lookupPost?.PostTitle?.ToString();
            }

            if (output.Seen.UserId != null)
            {
                var _lookupUser = await _lookup_userRepository.FirstOrDefaultAsync((long)output.Seen.UserId);
                output.UserName = _lookupUser?.Name?.ToString();
            }

            return output;
        }

        [AbpAuthorize(AppPermissions.Pages_Seens_Edit)]
        public async Task<GetSeenForEditOutput> GetSeenForEdit(EntityDto input)
        {
            var seen = await _seenRepository.FirstOrDefaultAsync(input.Id);

            var output = new GetSeenForEditOutput { Seen = ObjectMapper.Map<CreateOrEditSeenDto>(seen) };

            if (output.Seen.PostId != null)
            {
                var _lookupPost = await _lookup_postRepository.FirstOrDefaultAsync((int)output.Seen.PostId);
                output.PostPostTitle = _lookupPost?.PostTitle?.ToString();
            }

            if (output.Seen.UserId != null)
            {
                var _lookupUser = await _lookup_userRepository.FirstOrDefaultAsync((long)output.Seen.UserId);
                output.UserName = _lookupUser?.Name?.ToString();
            }

            return output;
        }

        public async Task CreateOrEdit(CreateOrEditSeenDto input)
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

        [AbpAuthorize(AppPermissions.Pages_Seens_Create)]
        protected virtual async Task Create(CreateOrEditSeenDto input)
        {
            var seen = ObjectMapper.Map<Seen>(input);

            await _seenRepository.InsertAsync(seen);

        }

        [AbpAuthorize(AppPermissions.Pages_Seens_Edit)]
        protected virtual async Task Update(CreateOrEditSeenDto input)
        {
            var seen = await _seenRepository.FirstOrDefaultAsync((int)input.Id);
            ObjectMapper.Map(input, seen);

        }

        [AbpAuthorize(AppPermissions.Pages_Seens_Delete)]
        public async Task Delete(EntityDto input)
        {
            await _seenRepository.DeleteAsync(input.Id);
        }

        [AbpAuthorize(AppPermissions.Pages_Seens)]
        public async Task<PagedResultDto<SeenPostLookupTableDto>> GetAllPostForLookupTable(GetAllForLookupTableInput input)
        {
            var query = _lookup_postRepository.GetAll().WhereIf(
                   !string.IsNullOrWhiteSpace(input.Filter),
                  e => e.PostTitle != null && e.PostTitle.Contains(input.Filter)
               );

            var totalCount = await query.CountAsync();

            var postList = await query
                .PageBy(input)
                .ToListAsync();

            var lookupTableDtoList = new List<SeenPostLookupTableDto>();
            foreach (var post in postList)
            {
                lookupTableDtoList.Add(new SeenPostLookupTableDto
                {
                    Id = post.Id,
                    DisplayName = post.PostTitle?.ToString()
                });
            }

            return new PagedResultDto<SeenPostLookupTableDto>(
                totalCount,
                lookupTableDtoList
            );
        }

        [AbpAuthorize(AppPermissions.Pages_Seens)]
        public async Task<PagedResultDto<SeenUserLookupTableDto>> GetAllUserForLookupTable(GetAllForLookupTableInput input)
        {
            var query = _lookup_userRepository.GetAll().WhereIf(
                   !string.IsNullOrWhiteSpace(input.Filter),
                  e => e.Name != null && e.Name.Contains(input.Filter)
               );

            var totalCount = await query.CountAsync();

            var userList = await query
                .PageBy(input)
                .ToListAsync();

            var lookupTableDtoList = new List<SeenUserLookupTableDto>();
            foreach (var user in userList)
            {
                lookupTableDtoList.Add(new SeenUserLookupTableDto
                {
                    Id = user.Id,
                    DisplayName = user.Name?.ToString()
                });
            }

            return new PagedResultDto<SeenUserLookupTableDto>(
                totalCount,
                lookupTableDtoList
            );
        }


    }
}