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
using Microsoft.EntityFrameworkCore;

namespace Chamran.Deed.Info
{
    [AbpAuthorize(AppPermissions.Pages_Administration_Hashtags)]
    public class HashtagsAppService : DeedAppServiceBase, IHashtagsAppService
    {
        private readonly IRepository<Hashtag> _hashtagRepository;
        private readonly IHashtagsExcelExporter _hashtagsExcelExporter;
        private readonly IRepository<Post, int> _lookup_postRepository;

        public HashtagsAppService(IRepository<Hashtag> hashtagRepository, IHashtagsExcelExporter hashtagsExcelExporter, IRepository<Post, int> lookup_postRepository)
        {
            _hashtagRepository = hashtagRepository;
            _hashtagsExcelExporter = hashtagsExcelExporter;
            _lookup_postRepository = lookup_postRepository;

        }

        public async Task<PagedResultDto<GetHashtagForViewDto>> GetAll(GetAllHashtagsInput input)
        {

            var filteredHashtags = _hashtagRepository.GetAll()
                        .Include(e => e.PostFk)
                        .WhereIf(!string.IsNullOrWhiteSpace(input.Filter), e => false || e.HashtagTitle.Contains(input.Filter))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.HashtagTitleFilter), e => e.HashtagTitle.Contains(input.HashtagTitleFilter))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.PostPostTitleFilter), e => e.PostFk != null && e.PostFk.PostTitle == input.PostPostTitleFilter)
                        .WhereIf(input.PostIdFilter.HasValue, e => false || e.PostId == input.PostIdFilter.Value);

            var pagedAndFilteredHashtags = filteredHashtags
                .OrderBy(input.Sorting ?? "id asc")
                .PageBy(input);

            var hashtags = from o in pagedAndFilteredHashtags
                           join o1 in _lookup_postRepository.GetAll() on o.PostId equals o1.Id into j1
                           from s1 in j1.DefaultIfEmpty()

                           select new
                           {

                               o.HashtagTitle,
                               Id = o.Id,
                               PostPostTitle = s1 == null || s1.PostTitle == null ? "" : s1.PostTitle.ToString()
                           };

            var totalCount = await filteredHashtags.CountAsync();

            var dbList = await hashtags.ToListAsync();
            var results = new List<GetHashtagForViewDto>();

            foreach (var o in dbList)
            {
                var res = new GetHashtagForViewDto()
                {
                    Hashtag = new HashtagDto
                    {

                        HashtagTitle = o.HashtagTitle,
                        Id = o.Id,
                    },
                    PostPostTitle = o.PostPostTitle
                };

                results.Add(res);
            }

            return new PagedResultDto<GetHashtagForViewDto>(
                totalCount,
                results
            );

        }

        public async Task<GetHashtagForViewDto> GetHashtagForView(int id)
        {
            var hashtag = await _hashtagRepository.GetAsync(id);

            var output = new GetHashtagForViewDto { Hashtag = ObjectMapper.Map<HashtagDto>(hashtag) };

            if (output.Hashtag.PostId != null)
            {
                var _lookupPost = await _lookup_postRepository.FirstOrDefaultAsync((int)output.Hashtag.PostId);
                output.PostPostTitle = _lookupPost?.PostTitle?.ToString();
            }

            return output;
        }

        [AbpAuthorize(AppPermissions.Pages_Administration_Hashtags_Edit)]
        public async Task<GetHashtagForEditOutput> GetHashtagForEdit(EntityDto input)
        {
            var hashtag = await _hashtagRepository.FirstOrDefaultAsync(input.Id);

            var output = new GetHashtagForEditOutput { Hashtag = ObjectMapper.Map<CreateOrEditHashtagDto>(hashtag) };

            if (output.Hashtag.PostId != null)
            {
                var _lookupPost = await _lookup_postRepository.FirstOrDefaultAsync((int)output.Hashtag.PostId);
                output.PostPostTitle = _lookupPost?.PostTitle?.ToString();
            }

            return output;
        }

        public async Task CreateOrEdit(CreateOrEditHashtagDto input)
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

        [AbpAuthorize(AppPermissions.Pages_Administration_Hashtags_Create)]
        protected virtual async Task Create(CreateOrEditHashtagDto input)
        {
            var hashtag = ObjectMapper.Map<Hashtag>(input);

            await _hashtagRepository.InsertAsync(hashtag);

        }

        [AbpAuthorize(AppPermissions.Pages_Administration_Hashtags_Edit)]
        protected virtual async Task Update(CreateOrEditHashtagDto input)
        {
            var hashtag = await _hashtagRepository.FirstOrDefaultAsync((int)input.Id);
            ObjectMapper.Map(input, hashtag);

        }

        [AbpAuthorize(AppPermissions.Pages_Administration_Hashtags_Delete)]
        public async Task Delete(EntityDto input)
        {
            await _hashtagRepository.DeleteAsync(input.Id);
        }

        public async Task<FileDto> GetHashtagsToExcel(GetAllHashtagsForExcelInput input)
        {

            var filteredHashtags = _hashtagRepository.GetAll()
                        .Include(e => e.PostFk)
                        .WhereIf(!string.IsNullOrWhiteSpace(input.Filter), e => false || e.HashtagTitle.Contains(input.Filter))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.HashtagTitleFilter), e => e.HashtagTitle.Contains(input.HashtagTitleFilter))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.PostPostTitleFilter), e => e.PostFk != null && e.PostFk.PostTitle == input.PostPostTitleFilter);

            var query = (from o in filteredHashtags
                         join o1 in _lookup_postRepository.GetAll() on o.PostId equals o1.Id into j1
                         from s1 in j1.DefaultIfEmpty()

                         select new GetHashtagForViewDto()
                         {
                             Hashtag = new HashtagDto
                             {
                                 HashtagTitle = o.HashtagTitle,
                                 Id = o.Id
                             },
                             PostPostTitle = s1 == null || s1.PostTitle == null ? "" : s1.PostTitle.ToString()
                         });

            var hashtagListDtos = await query.ToListAsync();

            return _hashtagsExcelExporter.ExportToFile(hashtagListDtos);
        }

        [AbpAuthorize(AppPermissions.Pages_Administration_Hashtags)]
        public async Task<PagedResultDto<HashtagPostLookupTableDto>> GetAllPostForLookupTable(GetAllForLookupTableInput input)
        {
            var query = _lookup_postRepository.GetAll().WhereIf(
                   !string.IsNullOrWhiteSpace(input.Filter),
                  e => e.PostTitle != null && e.PostTitle.Contains(input.Filter)
               );

            var totalCount = await query.CountAsync();

            var postList = await query
                .PageBy(input)
                .ToListAsync();

            var lookupTableDtoList = new List<HashtagPostLookupTableDto>();
            foreach (var post in postList)
            {
                lookupTableDtoList.Add(new HashtagPostLookupTableDto
                {
                    Id = post.Id,
                    DisplayName = post.PostTitle?.ToString()
                });
            }

            return new PagedResultDto<HashtagPostLookupTableDto>(
                totalCount,
                lookupTableDtoList
            );
        }

    }
}