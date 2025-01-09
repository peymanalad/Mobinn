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

namespace Chamran.Deed.Info
{
    [AbpAuthorize(AppPermissions.Pages_PostGroups)]
    public class PostGroupsAppService : DeedAppServiceBase, IPostGroupsAppService
    {
        private readonly IRepository<User, long> _userRepository;
        private readonly IRepository<PostGroup> _postGroupRepository;
        private readonly IRepository<GroupMember> _groupMemberRepository;
        private readonly IPostGroupsExcelExporter _postGroupsExcelExporter;
        private readonly IRepository<Organization, int> _lookup_organizationRepository;
        private readonly ITempFileCacheManager _tempFileCacheManager;
        private readonly IBinaryObjectManager _binaryObjectManager;

        public PostGroupsAppService(IRepository<PostGroup> postGroupRepository,
            IPostGroupsExcelExporter postGroupsExcelExporter,
            IRepository<Organization, int> lookup_organizationGroupRepository,
            ITempFileCacheManager tempFileCacheManager, IBinaryObjectManager binaryObjectManager,
            IRepository<User, long> userRepository, IRepository<GroupMember> groupMemberRepository)
        {
            _postGroupRepository = postGroupRepository;
            _postGroupsExcelExporter = postGroupsExcelExporter;
            _lookup_organizationRepository = lookup_organizationGroupRepository;
            _userRepository = userRepository;
            _tempFileCacheManager = tempFileCacheManager;
            _binaryObjectManager = binaryObjectManager;
            _groupMemberRepository = groupMemberRepository;
        }

        public async Task<PagedResultDto<GetPostGroupForViewDto>> GetAll(GetAllPostGroupsInput input)
        {
            if (AbpSession.UserId == null) throw new UserFriendlyException("User Must be Logged in!");
            var user = await _userRepository.GetAsync(AbpSession.UserId.Value);

            var filteredPostGroups = _postGroupRepository.GetAll()
                .Include(e => e.OrganizationFk)
                .WhereIf(input.OrganizationId.HasValue,x=>x.OrganizationId==input.OrganizationId.Value)
                .WhereIf(!string.IsNullOrWhiteSpace(input.Filter),
                    e => false || e.PostGroupDescription.Contains(input.Filter))
                .WhereIf(!string.IsNullOrWhiteSpace(input.PostGroupDescriptionFilter),
                    e => e.PostGroupDescription.Contains(input.PostGroupDescriptionFilter))
                .WhereIf(input.MinOrderingFilter != null, e => e.Ordering >= input.MinOrderingFilter)
                .WhereIf(input.MaxOrderingFilter != null, e => e.Ordering <= input.MaxOrderingFilter)
                .WhereIf(!string.IsNullOrWhiteSpace(input.OrganizationGroupGroupNameFilter),
                    e => e.OrganizationFk != null &&
                         e.OrganizationFk.OrganizationName == input.OrganizationGroupGroupNameFilter);

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

            //    filteredPostGroups = filteredPostGroups.Where(x => x.OrganizationId == orgEntity.Id);

            //}
            if (!user.IsSuperUser)
            {
                var orgQuery =
                    from org in _lookup_organizationRepository.GetAll().Where(x => !x.IsDeleted)
                    join grpMember in _groupMemberRepository.GetAll() on org.Id equals grpMember
                        .OrganizationId into joined2
                    from grpMember in joined2.DefaultIfEmpty()
                    where grpMember.UserId == AbpSession.UserId
                    select org.Id;

                var orgIds = orgQuery.ToList(); 

                if (!orgIds.Any())
                {
                    throw new UserFriendlyException("کاربر عضو هیچ گروهی در هیچ سازمانی نمی‌باشد");
                }

                filteredPostGroups = filteredPostGroups.Where(x => orgIds.Contains((int)x.OrganizationId));
            }



            var pagedAndFilteredPostGroups = filteredPostGroups
                .OrderBy(input.Sorting ?? "id asc")
                .PageBy(input);

            var postGroups = from o in pagedAndFilteredPostGroups
                             join o1 in _lookup_organizationRepository.GetAll() on o.OrganizationId equals o1.Id into j1
                             from s1 in j1.DefaultIfEmpty()

                             select new
                             {

                                 o.PostGroupDescription,
                                 o.Ordering,
                                 o.GroupFile,
                                 Id = o.Id,
                                 OrganizationGroupGroupName = s1 == null || s1.OrganizationName == null ? "" : s1.OrganizationName.ToString()
                             };

            var totalCount = await filteredPostGroups.CountAsync();

            var dbList = await postGroups.ToListAsync();
            var results = new List<GetPostGroupForViewDto>();

            foreach (var o in dbList)
            {
                var res = new GetPostGroupForViewDto()
                {
                    PostGroup = new PostGroupDto
                    {

                        PostGroupDescription = o.PostGroupDescription,
                        Ordering = o.Ordering,
                        GroupFile = o.GroupFile,
                        Id = o.Id,
                    },
                    OrganizationGroupGroupName = o.OrganizationGroupGroupName
                };
                res.PostGroup.GroupFileFileName = await GetBinaryFileName(o.GroupFile);

                results.Add(res);
            }

            return new PagedResultDto<GetPostGroupForViewDto>(
                totalCount,
                results
            );

        }

        public async Task<GetPostGroupForViewDto> GetPostGroupForView(int id)
        {
            var postGroup = await _postGroupRepository.GetAsync(id);

            var output = new GetPostGroupForViewDto { PostGroup = ObjectMapper.Map<PostGroupDto>(postGroup) };

            if (output.PostGroup.OrganizationId != null)
            {
                var _lookupOrganizationGroup = await _lookup_organizationRepository.FirstOrDefaultAsync((int)output.PostGroup.OrganizationId);
                output.OrganizationGroupGroupName = _lookupOrganizationGroup?.OrganizationName?.ToString();
            }

            output.PostGroup.GroupFileFileName = await GetBinaryFileName(postGroup.GroupFile);

            return output;
        }

        [AbpAuthorize(AppPermissions.Pages_PostGroups_Edit)]
        public async Task<GetPostGroupForEditOutput> GetPostGroupForEdit(EntityDto input)
        {
            var postGroup = await _postGroupRepository.FirstOrDefaultAsync(input.Id);

            var output = new GetPostGroupForEditOutput { PostGroup = ObjectMapper.Map<CreateOrEditPostGroupDto>(postGroup) };

            if (output.PostGroup.OrganizationId != null)
            {
                var _lookupOrganizationGroup = await _lookup_organizationRepository.FirstOrDefaultAsync((int)output.PostGroup.OrganizationId);
                output.OrganizationGroupGroupName = _lookupOrganizationGroup?.OrganizationName?.ToString();
            }

            output.GroupFileFileName = await GetBinaryFileName(postGroup.GroupFile);

            return output;
        }

        public async Task CreateOrEdit(CreateOrEditPostGroupDto input)
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
        protected virtual async Task Create(CreateOrEditPostGroupDto input)
        {
            var postGroup = ObjectMapper.Map<PostGroup>(input);

            await _postGroupRepository.InsertAsync(postGroup);
            postGroup.GroupFile = await GetBinaryObjectFromCache(input.GroupFileToken, postGroup.Id);

        }

        [AbpAuthorize(AppPermissions.Pages_PostGroups_Edit)]
        protected virtual async Task Update(CreateOrEditPostGroupDto input)
        {
            var postGroup = await _postGroupRepository.FirstOrDefaultAsync((int)input.Id);
            ObjectMapper.Map(input, postGroup);
            postGroup.GroupFile = await GetBinaryObjectFromCache(input.GroupFileToken, postGroup.Id);

        }

        [AbpAuthorize(AppPermissions.Pages_PostGroups_Delete)]
        public async Task Delete(EntityDto input)
        {
            await _postGroupRepository.DeleteAsync(input.Id);
        }

        public async Task<FileDto> GetPostGroupsToExcel(GetAllPostGroupsForExcelInput input)
        {

            var filteredPostGroups = _postGroupRepository.GetAll()
                        .Include(e => e.OrganizationFk)
                        .WhereIf(!string.IsNullOrWhiteSpace(input.Filter), e => false || e.PostGroupDescription.Contains(input.Filter))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.PostGroupDescriptionFilter), e => e.PostGroupDescription.Contains(input.PostGroupDescriptionFilter))
                        .WhereIf(input.MinOrderingFilter != null, e => e.Ordering >= input.MinOrderingFilter)
                        .WhereIf(input.MaxOrderingFilter != null, e => e.Ordering <= input.MaxOrderingFilter)
                        .WhereIf(!string.IsNullOrWhiteSpace(input.OrganizationGroupGroupNameFilter), e => e.OrganizationFk != null && e.OrganizationFk.OrganizationName == input.OrganizationGroupGroupNameFilter);

            var query = (from o in filteredPostGroups
                         join o1 in _lookup_organizationRepository.GetAll() on o.OrganizationId equals o1.Id into j1
                         from s1 in j1.DefaultIfEmpty()

                         select new GetPostGroupForViewDto()
                         {
                             PostGroup = new PostGroupDto
                             {
                                 PostGroupDescription = o.PostGroupDescription,
                                 Ordering = o.Ordering,
                                 GroupFile = o.GroupFile,
                                 Id = o.Id
                             },
                             OrganizationGroupGroupName = s1 == null || s1.OrganizationName == null ? "" : s1.OrganizationName.ToString()
                         });

            var postGroupListDtos = await query.ToListAsync();

            return _postGroupsExcelExporter.ExportToFile(postGroupListDtos);
        }

        [AbpAuthorize(AppPermissions.Pages_PostGroups)]
        public async Task<PagedResultDto<PostGroupOrganizationGroupLookupTableDto>> GetAllOrganizationGroupForLookupTable(GetAllForLookupTableInput input)
        {
            var query = _lookup_organizationRepository.GetAll().WhereIf(
                   !string.IsNullOrWhiteSpace(input.Filter),
                  e => e.OrganizationName != null && e.OrganizationName.Contains(input.Filter)
               );

            var totalCount = await query.CountAsync();

            var organizationGroupList = await query
                .PageBy(input)
                .ToListAsync();

            var lookupTableDtoList = new List<PostGroupOrganizationGroupLookupTableDto>();
            foreach (var organizationGroup in organizationGroupList)
            {
                lookupTableDtoList.Add(new PostGroupOrganizationGroupLookupTableDto
                {
                    Id = organizationGroup.Id,
                    DisplayName = organizationGroup.OrganizationName?.ToString()
                });
            }

            return new PagedResultDto<PostGroupOrganizationGroupLookupTableDto>(
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
        public async Task RemoveGroupFileFile(EntityDto input)
        {
            var postGroup = await _postGroupRepository.FirstOrDefaultAsync(input.Id);
            if (postGroup == null)
            {
                throw new UserFriendlyException(L("EntityNotFound"));
            }

            if (!postGroup.GroupFile.HasValue)
            {
                throw new UserFriendlyException(L("FileNotFound"));
            }

            await _binaryObjectManager.DeleteAsync(postGroup.GroupFile.Value);
            postGroup.GroupFile = null;
        }

        public async Task UpdatePostGroupOrdering(Dictionary<int, int> orderDictionary)
        {
            foreach (var row in orderDictionary)
            {
                var res = await _postGroupRepository.GetAsync(row.Key);
                res.Ordering = row.Value;
            }
        }
    }
}