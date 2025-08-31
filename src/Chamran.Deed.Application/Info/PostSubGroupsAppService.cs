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
using Chamran.Deed.Authorization.Users;
using Chamran.Deed.Common;
using Chamran.Deed.Storage;
using System.IO;

namespace Chamran.Deed.Info
{
    [AbpAuthorize(AppPermissions.Pages_PostGroups)]
    public class PostSubGroupsAppService : DeedAppServiceBase, IPostSubGroupsAppService
    {
        private readonly IRepository<User, long> _userRepository;
        private readonly IRepository<PostSubGroup> _PostSubGroupRepository;
        private readonly IRepository<GroupMember> _groupMemberRepository;
        private readonly IPostSubGroupsExcelExporter _PostSubGroupsExcelExporter;
        private readonly ITempFileCacheManager _tempFileCacheManager;
        private readonly IBinaryObjectManager _binaryObjectManager;
        private readonly IRepository<Post> _postRepository;


        public PostSubGroupsAppService(IRepository<PostSubGroup> PostSubGroupRepository,
            IPostSubGroupsExcelExporter PostSubGroupsExcelExporter,
            ITempFileCacheManager tempFileCacheManager, IBinaryObjectManager binaryObjectManager,
            IRepository<User, long> userRepository, IRepository<GroupMember> groupMemberRepository,
            IRepository<Post> postRepository)
        {
            _PostSubGroupRepository = PostSubGroupRepository;
            _PostSubGroupsExcelExporter = PostSubGroupsExcelExporter;
            _userRepository = userRepository;
            _tempFileCacheManager = tempFileCacheManager;
            _binaryObjectManager = binaryObjectManager;
            _groupMemberRepository = groupMemberRepository;
            _postRepository = postRepository;
        }

        public async Task<PagedResultDto<GetPostSubGroupForViewDto>> GetAll(GetAllPostSubGroupsInput input)
        {
            if (AbpSession.UserId == null) throw new UserFriendlyException("User Must be Logged in!");
            var user = await _userRepository.GetAsync(AbpSession.UserId.Value);

            var filteredPostSubGroups = _PostSubGroupRepository.GetAll()
                .Where(x => x.PostGroupId == input.PostGroupId)
                .WhereIf(!string.IsNullOrWhiteSpace(input.Filter), e => false || e.PostSubGroupDescription.Contains(input.Filter))
                .WhereIf(!string.IsNullOrWhiteSpace(input.PostSubGroupDescriptionFilter),
                    e => e.PostSubGroupDescription.Contains(input.PostSubGroupDescriptionFilter)
                );

            //if (!user.IsSuperUser)
            //{
            //    var orgQuery =
            //        from org in _lookup_organizationRepository.GetAll().Where(x => !x.IsDeleted)
            //        join grpMember in _groupMemberRepository.GetAll() on org.Id equals grpMember
            //            .OrganizationId into joined2
            //        from grpMember in joined2.DefaultIfEmpty()
            //        where grpMember.UserId == AbpSession.UserId
            //        select org;

            //    if (!orgQuery.Any())
            //    {
            //        throw new UserFriendlyException("کاربر عضو هیچ گروهی در هیچ سازمانی نمی باشد");
            //    }
            //    var orgEntity = orgQuery.First();

            //    filteredPostSubGroups = filteredPostSubGroups.Where(x => x.OrganizationId == orgEntity.Id);

            //}




            var pagedAndFilteredPostSubGroups = filteredPostSubGroups
                .OrderBy(input.Sorting ?? "id asc")
                .PageBy(input);

            var PostSubGroups = from o in pagedAndFilteredPostSubGroups
                                //join o1 in _lookup_organizationRepository.GetAll() on o.OrganizationId equals o1.Id into j1
                                //from s1 in j1.DefaultIfEmpty()

                                select new
                                {

                                    o.PostSubGroupDescription,
                                    o.GroupFile,
                                    Id = o.Id,
                                    PostSubGroupLatestPic = _postRepository.GetAll()
                                        .Where(p => p.PostSubGroupId == o.Id) // فیلتر فایل‌ها بر اساس SubGroup
                                        .OrderByDescending(p => p.CreationTime) // جدیدترین بر اساس زمان
                                        .FirstOrDefault().PostFile // فایل آخرین پست
                                    //OrganizationGroupGroupName = s1 == null || s1.OrganizationName == null ? "" : s1.OrganizationName.ToString()
                                };

            var totalCount = await filteredPostSubGroups.CountAsync();

            var dbList = await PostSubGroups.ToListAsync();
            var results = new List<GetPostSubGroupForViewDto>();

            foreach (var o in dbList)
            {
                var res = new GetPostSubGroupForViewDto()
                {
                    PostSubGroup = new PostSubGroupDto
                    {
                        PostSubGroupDescription = o.PostSubGroupDescription,
                        SubGroupFile = o.GroupFile,
                        Id = o.Id,
                        //PostSubGroupLatestPic = o.PostSubGroupLatestPic
                    },
                     
                };
                res.PostSubGroup.SubGroupFileFileName = await GetBinaryFileName(o.GroupFile);

                results.Add(res);
            }

            return new PagedResultDto<GetPostSubGroupForViewDto>(
                totalCount,
                results
            );

        }

        public async Task<PagedResultDto<GetPostSubGroupForViewDto>> GetPostSubGroupsForExploreViewAsync(GetAllPostSubGroupsInput input)
        {
            try
            {
                var results = new List<GetPostSubGroupForViewDto>();

                var subGroups = await _PostSubGroupRepository.GetAll()
                    .Where(x => x.PostGroupId == input.PostGroupId && !x.IsDeleted)
                    .WhereIf(!string.IsNullOrWhiteSpace(input.Filter), x => x.PostSubGroupDescription.Contains(input.Filter))
                    .WhereIf(!string.IsNullOrWhiteSpace(input.PostSubGroupDescriptionFilter), x => x.PostSubGroupDescription.Contains(input.PostSubGroupDescriptionFilter))
                    .OrderBy(input.Sorting ?? "id asc")
                    .ToListAsync();

                foreach (var subGroup in subGroups)
                {
                    string latestMedia = null;

                    var latestPost = await _postRepository.GetAll()
                        .Where(p => p.PostSubGroupId == subGroup.Id && !p.IsDeleted && p.IsPublished && p.PostFile.HasValue)
                        .OrderByDescending(p => p.CreationTime)
                        .FirstOrDefaultAsync();

                    if (latestPost != null)
                    {
                        var ext = await GetFileExtensionAsync(latestPost.PostFile);
                        bool isImage = ext is ".jpg" or ".jpeg" or ".png";
                        bool isVideo = ext is ".mp4" or ".mov";

                        if (isImage)
                            latestMedia = latestPost.PostFileThumb;
                        else if (isVideo)
                            latestMedia = latestPost.PostVideoPreview;
                    }

                    results.Add(new GetPostSubGroupForViewDto
                    {
                        PostSubGroup = new PostSubGroupDto
                        {
                            Id = subGroup.Id,
                            PostSubGroupDescription = subGroup.PostSubGroupDescription,
                            SubGroupFile = subGroup.GroupFile,
                            SubGroupFileFileName = await GetBinaryFileName(subGroup.GroupFile),
                            PostSubGroupLatestPic = latestMedia
                        }
                    });
                }

                return new PagedResultDto<GetPostSubGroupForViewDto>(results.Count, results);
            }
            catch (Exception ex)
            {
                throw new UserFriendlyException("خطا در دریافت زیر‌دسته‌ها", ex);
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

        public async Task<GetPostSubGroupForViewDto> GetPostSubGroupForView(int id)
        {
            var PostSubGroup = await _PostSubGroupRepository.GetAsync(id);

            var output = new GetPostSubGroupForViewDto { PostSubGroup = ObjectMapper.Map<PostSubGroupDto>(PostSubGroup) };

           
            output.PostSubGroup.SubGroupFileFileName = await GetBinaryFileName(PostSubGroup.GroupFile);

            return output;
        }

        [AbpAuthorize(AppPermissions.Pages_PostGroups_Edit)]
        public async Task<GetPostSubGroupForEditOutput> GetPostSubGroupForEdit(EntityDto input)
        {
            var PostSubGroup = await _PostSubGroupRepository.FirstOrDefaultAsync(input.Id);

            var output = new GetPostSubGroupForEditOutput { PostSubGroup = ObjectMapper.Map<CreateOrEditPostSubGroupDto>(PostSubGroup) };

           
            output.SubGroupFileFileName = await GetBinaryFileName(PostSubGroup.GroupFile);

            return output;
        }

        public async Task CreateOrEdit(CreateOrEditPostSubGroupDto input)
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

        [AbpAuthorize(AppPermissions.Pages_PostGroups_Create)]
        protected virtual async Task Create(CreateOrEditPostSubGroupDto input)
        {
            var PostSubGroup = ObjectMapper.Map<PostSubGroup>(input);

            await _PostSubGroupRepository.InsertAsync(PostSubGroup);
            PostSubGroup.GroupFile = await GetBinaryObjectFromCache(input.SubGroupFileToken, PostSubGroup.Id);

        }

        [AbpAuthorize(AppPermissions.Pages_PostGroups_Edit)]
        protected virtual async Task Update(CreateOrEditPostSubGroupDto input)
        {
            var PostSubGroup = await _PostSubGroupRepository.FirstOrDefaultAsync((int)input.Id);
            ObjectMapper.Map(input, PostSubGroup);
            PostSubGroup.GroupFile = await GetBinaryObjectFromCache(input.SubGroupFileToken, PostSubGroup.Id);

        }

        [AbpAuthorize(AppPermissions.Pages_PostGroups_Delete)]
        public async Task Delete(EntityDto input)
        {
            await _PostSubGroupRepository.DeleteAsync(input.Id);
        }

        public async Task<FileDto> GetPostSubGroupsToExcel(GetAllPostSubGroupsForExcelInput input)
        {

            var filteredPostSubGroups = _PostSubGroupRepository.GetAll()
                        .WhereIf(!string.IsNullOrWhiteSpace(input.Filter), e => false || e.PostSubGroupDescription.Contains(input.Filter))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.PostSubGroupDescriptionFilter), e => e.PostSubGroupDescription.Contains(input.PostSubGroupDescriptionFilter));

            var query = (from o in filteredPostSubGroups

                         select new GetPostSubGroupForViewDto()
                         {
                             PostSubGroup = new PostSubGroupDto
                             {
                                 PostSubGroupDescription = o.PostSubGroupDescription,
                                 SubGroupFile = o.GroupFile,
                                 Id = o.Id
                             },
                         });

            var PostSubGroupListDtos = await query.ToListAsync();

            return _PostSubGroupsExcelExporter.ExportToFile(PostSubGroupListDtos);
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

            var storedFile = new BinaryObject(AbpSession.TenantId, fileCache.File, BinarySourceType.PostGroupLogo, fileCache.FileName);
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

        [AbpAuthorize(AppPermissions.Pages_PostGroups_Edit)]
        public async Task RemoveSubGroupFileFile(EntityDto input)
        {
            var PostSubGroup = await _PostSubGroupRepository.FirstOrDefaultAsync(input.Id);
            if (PostSubGroup == null)
            {
                throw new UserFriendlyException(L("EntityNotFound"));
            }

            if (!PostSubGroup.GroupFile.HasValue)
            {
                throw new UserFriendlyException(L("FileNotFound"));
            }

            await _binaryObjectManager.DeleteAsync(PostSubGroup.GroupFile.Value);
            PostSubGroup.GroupFile = null;
        }



    }
}