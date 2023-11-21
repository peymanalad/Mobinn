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
    [AbpAuthorize(AppPermissions.Pages_InstagramCrawlerPosts)]
    public class InstagramCrawlerPostsAppService : DeedAppServiceBase, IInstagramCrawlerPostsAppService
    {
        private readonly IRepository<InstagramCrawlerPost> _instagramCrawlerPostRepository;

        public InstagramCrawlerPostsAppService(IRepository<InstagramCrawlerPost> instagramCrawlerPostRepository)
        {
            _instagramCrawlerPostRepository = instagramCrawlerPostRepository;

        }

        public virtual async Task<PagedResultDto<GetInstagramCrawlerPostForViewDto>> GetAll(GetAllInstagramCrawlerPostsInput input)
        {

            var filteredInstagramCrawlerPosts = _instagramCrawlerPostRepository.GetAll()
                        .WhereIf(!string.IsNullOrWhiteSpace(input.Filter), e => false || e.PostCaption.Contains(input.Filter) || e.PageId.Contains(input.Filter) || e.File1Url.Contains(input.Filter) || e.File2Url.Contains(input.Filter) || e.File3Url.Contains(input.Filter) || e.File4Url.Contains(input.Filter) || e.File5Url.Contains(input.Filter) || e.File6Url.Contains(input.Filter) || e.File7Url.Contains(input.Filter) || e.File8Url.Contains(input.Filter) || e.File9Url.Contains(input.Filter) || e.File10Url.Contains(input.Filter) || e.MediaId.Contains(input.Filter))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.PostCaptionFilter), e => e.PostCaption.Contains(input.PostCaptionFilter))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.PageIdFilter), e => e.PageId.Contains(input.PageIdFilter))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.File1UrlFilter), e => e.File1Url.Contains(input.File1UrlFilter))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.File2UrlFilter), e => e.File2Url.Contains(input.File2UrlFilter))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.File3UrlFilter), e => e.File3Url.Contains(input.File3UrlFilter))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.File4UrlFilter), e => e.File4Url.Contains(input.File4UrlFilter))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.File5UrlFilter), e => e.File5Url.Contains(input.File5UrlFilter))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.File6UrlFilter), e => e.File6Url.Contains(input.File6UrlFilter))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.File7UrlFilter), e => e.File7Url.Contains(input.File7UrlFilter))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.File8UrlFilter), e => e.File8Url.Contains(input.File8UrlFilter))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.File9UrlFilter), e => e.File9Url.Contains(input.File9UrlFilter))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.File10UrlFilter), e => e.File10Url.Contains(input.File10UrlFilter))
                        .WhereIf(input.MinPostTimeFilter != null, e => e.PostTime >= input.MinPostTimeFilter)
                        .WhereIf(input.MaxPostTimeFilter != null, e => e.PostTime <= input.MaxPostTimeFilter)
                        .WhereIf(!string.IsNullOrWhiteSpace(input.MediaIdFilter), e => e.MediaId.Contains(input.MediaIdFilter));

            var pagedAndFilteredInstagramCrawlerPosts = filteredInstagramCrawlerPosts
                .OrderBy(input.Sorting ?? "id asc")
                .PageBy(input);

            var instagramCrawlerPosts = from o in pagedAndFilteredInstagramCrawlerPosts
                                        select new
                                        {

                                            o.PostCaption,
                                            o.PageId,
                                            o.File1Url,
                                            o.File2Url,
                                            o.File3Url,
                                            o.File4Url,
                                            o.File5Url,
                                            o.File6Url,
                                            o.File7Url,
                                            o.File8Url,
                                            o.File9Url,
                                            o.File10Url,
                                            o.PostTime,
                                            o.MediaId,
                                            Id = o.Id
                                        };

            var totalCount = await filteredInstagramCrawlerPosts.CountAsync();

            var dbList = await instagramCrawlerPosts.ToListAsync();
            var results = new List<GetInstagramCrawlerPostForViewDto>();

            foreach (var o in dbList)
            {
                var res = new GetInstagramCrawlerPostForViewDto()
                {
                    InstagramCrawlerPost = new InstagramCrawlerPostDto
                    {

                        PostCaption = o.PostCaption,
                        PageId = o.PageId,
                        File1Url = o.File1Url,
                        File2Url = o.File2Url,
                        File3Url = o.File3Url,
                        File4Url = o.File4Url,
                        File5Url = o.File5Url,
                        File6Url = o.File6Url,
                        File7Url = o.File7Url,
                        File8Url = o.File8Url,
                        File9Url = o.File9Url,
                        File10Url = o.File10Url,
                        PostTime = o.PostTime,
                        MediaId = o.MediaId,
                        Id = o.Id,
                    }
                };

                results.Add(res);
            }

            return new PagedResultDto<GetInstagramCrawlerPostForViewDto>(
                totalCount,
                results
            );

        }

        public virtual async Task<GetInstagramCrawlerPostForViewDto> GetInstagramCrawlerPostForView(int id)
        {
            var instagramCrawlerPost = await _instagramCrawlerPostRepository.GetAsync(id);

            var output = new GetInstagramCrawlerPostForViewDto { InstagramCrawlerPost = ObjectMapper.Map<InstagramCrawlerPostDto>(instagramCrawlerPost) };

            return output;
        }

        [AbpAuthorize(AppPermissions.Pages_InstagramCrawlerPosts_Edit)]
        public virtual async Task<GetInstagramCrawlerPostForEditOutput> GetInstagramCrawlerPostForEdit(EntityDto input)
        {
            var instagramCrawlerPost = await _instagramCrawlerPostRepository.FirstOrDefaultAsync(input.Id);

            var output = new GetInstagramCrawlerPostForEditOutput { InstagramCrawlerPost = ObjectMapper.Map<CreateOrEditInstagramCrawlerPostDto>(instagramCrawlerPost) };

            return output;
        }

        public virtual async Task CreateOrEdit(CreateOrEditInstagramCrawlerPostDto input)
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

        //[AbpAuthorize(AppPermissions.Pages_InstagramCrawlerPosts_Create)]
        [AbpAllowAnonymous]
        protected virtual async Task Create(CreateOrEditInstagramCrawlerPostDto input)
        {
            var instagramCrawlerPost = ObjectMapper.Map<InstagramCrawlerPost>(input);

            await _instagramCrawlerPostRepository.InsertAsync(instagramCrawlerPost);

        }

        [AbpAuthorize(AppPermissions.Pages_InstagramCrawlerPosts_Edit)]
        protected virtual async Task Update(CreateOrEditInstagramCrawlerPostDto input)
        {
            var instagramCrawlerPost = await _instagramCrawlerPostRepository.FirstOrDefaultAsync((int)input.Id);
            ObjectMapper.Map(input, instagramCrawlerPost);

        }

        [AbpAuthorize(AppPermissions.Pages_InstagramCrawlerPosts_Delete)]
        public virtual async Task Delete(EntityDto input)
        {
            await _instagramCrawlerPostRepository.DeleteAsync(input.Id);
        }

    }
}