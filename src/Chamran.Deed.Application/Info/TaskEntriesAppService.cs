using Chamran.Deed.Authorization.Users;
using System.Linq;
using System.Linq.Dynamic.Core;
using Abp.Linq.Extensions;
using System.Collections.Generic;
using System.Threading.Tasks;
using Abp;
using Abp.Domain.Repositories;
using Chamran.Deed.Info.Dtos;
using Abp.Application.Services.Dto;
using Chamran.Deed.Authorization;
using Abp.Authorization;
using Microsoft.EntityFrameworkCore;
using Abp.UI;
using Abp.EntityFrameworkCore;
using Chamran.Deed.EntityFrameworkCore;
using Abp.Domain.Uow;
using Abp.Notifications;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Chamran.Deed.Notifications;

namespace Chamran.Deed.Info
{
    [AbpAuthorize(AppPermissions.Pages_TaskEntries)]
    public class TaskEntriesAppService : DeedAppServiceBase, ITaskEntriesAppService
    {
        private readonly IRepository<TaskEntry> _taskEntryRepository;
        private readonly IRepository<Post, int> _lookup_postRepository;
        private readonly IRepository<User, long> _lookup_userRepository;
        private readonly IRepository<TaskEntry, int> _lookup_taskEntryRepository;
        private readonly IDbContextProvider<DeedDbContext> _dbContextProvider;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly IAppNotifier _appNotifier;
        public TaskEntriesAppService(IRepository<TaskEntry> taskEntryRepository,
            IRepository<Post, int> lookup_postRepository, IRepository<User, long> lookup_userRepository,
            IRepository<TaskEntry, int> lookup_taskEntryRepository, IDbContextProvider<DeedDbContext> dbContextProvider,
            IUnitOfWorkManager unitOfWorkManager, IAppNotifier appNotifier)
        {
            _taskEntryRepository = taskEntryRepository;
            _lookup_postRepository = lookup_postRepository;
            _lookup_userRepository = lookup_userRepository;
            _lookup_taskEntryRepository = lookup_taskEntryRepository;
            _dbContextProvider = dbContextProvider;
            _unitOfWorkManager = unitOfWorkManager;
            _appNotifier = appNotifier;
        }

        public virtual async Task<PagedResultDto<GetEntriesDigestDto>> GetEntriesDigest(GetEntriesDigestInputDto input)
        {
            if (AbpSession.UserId == null) throw new UserFriendlyException("Not Logged In!");

            using var uow = _unitOfWorkManager.Begin();

            // Get the DbContext
            var dbContext = await _dbContextProvider.GetDbContextAsync();

            // Define your custom SQL query with parameters for SkipCount and MaxResultCount
            var sqlQuery = @"
        -- Previous SQL statement here;

        DECLARE @SkipCount INT = @SkipCountParam;
        DECLARE @MaxResultCount INT = @MaxResultCountParam;
        DECLARE @UserId INT = @UserIdParam;
        DECLARE @captionfilter NVARCHAR(200) = @CaptionFilterParam;

      
;WITH MinIdCTE AS (
    SELECT
        MIN(t.[Id]) AS MinId,
        t.[SharedTaskId]
    FROM
        [DeedDb].[dbo].[TaskEntries] t
    WHERE
        t.IssuerId = @UserId OR t.ReceiverId = @UserId
    GROUP BY
        t.[SharedTaskId]
)

, RankedRows AS (
    SELECT
        t.[Id],
        t.[Caption],
        t.[SharedTaskId],
        t.[PostId],
        t.[IssuerId],
        t.[ReceiverId],
        t.[ParentId],
        t.[CreationTime],
        t.[CreatorUserId],
        I.[Name] AS IssuerFirstName,
        I.[Surname] AS IssuerLastName,
        I.[ProfilePictureId] AS IssuerProfilePicture,
        R2.[Name] AS ReceiverFirstName,
        R2.[Surname] AS ReceiverLastName,
        R2.[ProfilePictureId] AS ReceiverProfilePicture,
        IGM.[MemberPosition] AS IssuerMemberPos,
        RGM.[MemberPosition] AS ReceiverMemberPos,
        PST.[PostFile],
        PST.[PostCaption],
        PST.[GroupMemberId] as PostGroupMemberId,
        PST.[CreationTime] as PostCreationTime,
        PST.[CreatorUserId] as PostCreatorUserId,
        PST.[LastModificationTime] as PostLastModificationTime,
        PST.[LastModifierUserId] as PostLastModifierUserId,
        PST.[PostGroupId],
        PST.[IsSpecial],
        PST.[IsPublished],
        PST.[PostTitle],
        PST.[PostFile2],
        PST.[PostFile3],
        PST.[PostRefLink],
        ROW_NUMBER() OVER (PARTITION BY MinIdCTE.[SharedTaskId] ORDER BY t.[Id] DESC) AS RowNum
    FROM
        [DeedDb].[dbo].[TaskEntries] t
    JOIN
        MinIdCTE ON t.[SharedTaskId] = MinIdCTE.[SharedTaskId] AND t.[Id] >= MinIdCTE.MinId
    LEFT JOIN
        [DeedDb].[dbo].[AbpUsers] I ON t.[IssuerId] = I.[Id]
    LEFT JOIN
        [DeedDb].[dbo].[AbpUsers] R2 ON t.[ReceiverId] = R2.[Id]
    LEFT JOIN
        [DeedDb].[dbo].[GroupMembers] IGM ON t.[IssuerId] = IGM.[UserId]
    LEFT JOIN
        [DeedDb].[dbo].[GroupMembers] RGM ON t.[ReceiverId] = RGM.[UserId]
    LEFT JOIN
        [DeedDb].[dbo].[Posts] PST ON t.[PostId] = PST.[Id]
)

SELECT DISTINCT
    [Id],
    [Caption],
    [SharedTaskId],
    [PostId],
    [IssuerId],
    [ReceiverId],
    [ParentId],
    [CreationTime],
    [CreatorUserId],
    IssuerFirstName,
    IssuerLastName,
    IssuerProfilePicture,
    ReceiverFirstName,
    ReceiverLastName,
    ReceiverProfilePicture,
    IssuerMemberPos,
    ReceiverMemberPos,
    [PostFile],
    [PostCaption],
    [PostGroupMemberId],
    [PostCreationTime],
    [PostCreatorUserId],
    [PostLastModificationTime],
    [PostLastModifierUserId],
    [PostGroupId],
    [IsSpecial],
    [IsPublished],
    [PostTitle],
    [PostFile2],
    [PostFile3],
    [PostRefLink]
FROM (
    SELECT
        *,
        ROW_NUMBER() OVER (ORDER BY [CreationTime]) AS PaginationRowNum
    FROM
        RankedRows
    WHERE
        RowNum = 1 AND
        (@captionfilter = '' OR [Caption] LIKE '%' + @captionfilter + '%')
) AS Subquery
WHERE
    PaginationRowNum > @SkipCount
    AND PaginationRowNum <= @SkipCount + @MaxResultCount
ORDER BY
    [CreationTime] DESC;
    ";

            // Create SqlParameter objects for each parameter
            object[] parameters = {
        new SqlParameter("@SkipCountParam", input.SkipCount),
        new SqlParameter("@MaxResultCountParam", input.MaxResultCount),
        new SqlParameter("@UserIdParam", AbpSession.UserId.Value),
        new SqlParameter("@CaptionFilterParam", input.CaptionFilter ?? string.Empty)
    };

            // Execute the query
            var result = await dbContext.Set<GetEntriesDigest>()
                .FromSqlRaw(sqlQuery, parameters)
                .ToListAsync();

            await uow.CompleteAsync();

            // Return the mapped result
            return new PagedResultDto<GetEntriesDigestDto>
            {
                TotalCount = result.Count,
                Items = ObjectMapper.Map<List<GetEntriesDigestDto>>(result)
            };
        }
        public virtual async Task<PagedResultDto<GetEntriesDetailDto>> GetEntriesBySharedMessageId(GetEntriesBySharedMessageIdInputDto input)
        {
            if (AbpSession.UserId == null) throw new UserFriendlyException("Not Logged In!");

            using var uow = _unitOfWorkManager.Begin();

            // Get the DbContext
            var dbContext = await _dbContextProvider.GetDbContextAsync();

            // Define your custom SQL query with parameters
            var sqlQuery = @"
        -- Previous SQL statement here;

        DECLARE @SkipCount INT = @SkipCountParam;
        DECLARE @MaxResultCount INT = @MaxResultCountParam;
        DECLARE @UserId INT = @UserIdParam;
        DECLARE @sharedTaskId UNIQUEIDENTIFIER = @SharedTaskIdParam; -- Change data type
        DECLARE @captionfilter NVARCHAR(200) = @CaptionFilterParam;
WITH MinIdCTE AS (
    SELECT
        MIN(t.[Id]) AS MinId,
        t.[SharedTaskId]
    FROM
        [DeedDb].[dbo].[TaskEntries] t
    WHERE
        t.IssuerId = @UserId OR t.ReceiverId = @UserId
    GROUP BY
        t.[SharedTaskId]
), RankedRows AS (
    SELECT DISTINCT
        t.[Id],
        t.[Caption],
        t.[SharedTaskId],
        t.[PostId],
        t.[IssuerId],
        t.[ReceiverId],
        t.[ParentId],
        t.[CreationTime],
        t.[CreatorUserId],
		t.[IsPrivate],
        I.[Name] AS IssuerFirstName,
        I.[Surname] AS IssuerLastName,
        I.[ProfilePictureId] AS IssuerProfilePicture,
        R2.[Name] AS ReceiverFirstName,
        R2.[Surname] AS ReceiverLastName,
        R2.[ProfilePictureId] AS ReceiverProfilePicture,
        --IGM.[MemberPosition] AS IssuerMemberPos,
        --RGM.[MemberPosition] AS ReceiverMemberPos,
        ROW_NUMBER() OVER (PARTITION BY MinIdCTE.[SharedTaskId] ORDER BY t.[Id] DESC) AS RowNum
    FROM
        [DeedDb].[dbo].[TaskEntries] t
    JOIN
        MinIdCTE ON t.[SharedTaskId] = MinIdCTE.[SharedTaskId] AND t.[Id] >= MinIdCTE.MinId
    LEFT JOIN
        [DeedDb].[dbo].[AbpUsers] I ON t.[IssuerId] = I.[Id]
    LEFT JOIN
        [DeedDb].[dbo].[AbpUsers] R2 ON t.[ReceiverId] = R2.[Id]
    --LEFT JOIN
    --    [DeedDb].[dbo].[GroupMembers] IGM ON t.[IssuerId] = IGM.[UserId]
    --LEFT JOIN
    --    [DeedDb].[dbo].[GroupMembers] RGM ON t.[ReceiverId] = RGM.[UserId]
), PaginatedResults AS (
    SELECT DISTINCT
        [Id],
        [Caption],
        [SharedTaskId],
        [PostId],
        [IssuerId],
        [ReceiverId],
        [ParentId],
        [CreationTime],
        [CreatorUserId],
        IssuerFirstName,
        IssuerLastName,
        IssuerProfilePicture,
        ReceiverFirstName,
        ReceiverLastName,
        ReceiverProfilePicture,
       -- IssuerMemberPos,
       -- ReceiverMemberPos,
        IsPrivate,
        ROW_NUMBER() OVER (ORDER BY [CreationTime] DESC) AS RowNum
    FROM
        RankedRows
    WHERE
        SharedTaskId = @sharedTaskId and
        (@captionfilter = '' OR [Caption] LIKE '%' + @captionfilter + '%')
)
SELECT
    [Id],
    [Caption],
    [SharedTaskId],
    [PostId],
    [IssuerId],
    [ReceiverId],
    [ParentId],
    [CreationTime],
    [CreatorUserId],
    IssuerFirstName,
    IssuerLastName,
    IssuerProfilePicture,
    ReceiverFirstName,
    ReceiverLastName,
    ReceiverProfilePicture,
   -- IssuerMemberPos,
   -- ReceiverMemberPos,
    IsPrivate
FROM
    PaginatedResults
WHERE
    RowNum BETWEEN @SkipCount + 1 AND @SkipCount + @MaxResultCount
ORDER BY
    [CreationTime] DESC;

    ";

            // Create SqlParameter objects for each parameter
            var parameters = new[]
            {
        new SqlParameter("@SkipCountParam", input.SkipCount),
        new SqlParameter("@MaxResultCountParam", input.MaxResultCount),
        new SqlParameter("@UserIdParam", AbpSession.UserId.Value),
        new SqlParameter("@SharedTaskIdParam", input.SharedTaskId.ToString()), // Convert to string
        new SqlParameter("@CaptionFilterParam", input.CaptionFilter ?? string.Empty)
    };

            // Execute the query
            var result = await dbContext.Set<GetEntriesDetail>()
                .FromSqlRaw(sqlQuery, parameters)
                .ToListAsync();

            await uow.CompleteAsync();

            // Return the mapped result
            return new PagedResultDto<GetEntriesDetailDto>
            {
                TotalCount = result.Count,
                Items = ObjectMapper.Map<List<GetEntriesDetailDto>>(result)
            };
        }

        public virtual async Task<PagedResultDto<GetTaskEntryForViewDto>> GetAll(GetAllTaskEntriesInput input)
        {

            var filteredTaskEntries = _taskEntryRepository.GetAll()
                        .Include(e => e.PostFk)
                        .Include(e => e.IssuerFk)
                        .Include(e => e.ReceiverFk)
                        .Include(e => e.ParentFk)
                        .WhereIf(!string.IsNullOrWhiteSpace(input.Filter), e => false || e.Caption.Contains(input.Filter))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.CaptionFilter), e => e.Caption.Contains(input.CaptionFilter))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.SharedTaskIdFilter.ToString()), e => e.SharedTaskId.ToString() == input.SharedTaskIdFilter.ToString())
                        .WhereIf(!string.IsNullOrWhiteSpace(input.PostPostTitleFilter), e => e.PostFk != null && e.PostFk.PostTitle == input.PostPostTitleFilter)
                        .WhereIf(!string.IsNullOrWhiteSpace(input.UserNameFilter), e => e.IssuerFk != null && e.IssuerFk.Name == input.UserNameFilter)
                        .WhereIf(!string.IsNullOrWhiteSpace(input.UserName2Filter), e => e.ReceiverFk != null && e.ReceiverFk.Name == input.UserName2Filter)
                        .WhereIf(!string.IsNullOrWhiteSpace(input.TaskEntryCaptionFilter), e => e.ParentFk != null && e.ParentFk.Caption == input.TaskEntryCaptionFilter);

            var pagedAndFilteredTaskEntries = filteredTaskEntries
                .OrderBy(input.Sorting ?? "id asc")
                .PageBy(input);

            var taskEntries = from o in pagedAndFilteredTaskEntries
                              join o1 in _lookup_postRepository.GetAll() on o.PostId equals o1.Id into j1
                              from s1 in j1.DefaultIfEmpty()

                              join o2 in _lookup_userRepository.GetAll() on o.IssuerId equals o2.Id into j2
                              from s2 in j2.DefaultIfEmpty()

                              join o3 in _lookup_userRepository.GetAll() on o.ReceiverId equals o3.Id into j3
                              from s3 in j3.DefaultIfEmpty()

                              join o4 in _lookup_taskEntryRepository.GetAll() on o.ParentId equals o4.Id into j4
                              from s4 in j4.DefaultIfEmpty()

                              select new
                              {

                                  o.Caption,
                                  o.SharedTaskId,
                                  Id = o.Id,
                                  PostPostTitle = s1 == null || s1.PostTitle == null ? "" : s1.PostTitle.ToString(),
                                  UserName = s2 == null || s2.Name == null ? "" : s2.Name.ToString(),
                                  UserName2 = s3 == null || s3.Name == null ? "" : s3.Name.ToString(),
                                  TaskEntryCaption = s4 == null || s4.Caption == null ? "" : s4.Caption.ToString()
                              };

            var totalCount = await filteredTaskEntries.CountAsync();

            var dbList = await taskEntries.ToListAsync();
            var results = new List<GetTaskEntryForViewDto>();

            foreach (var o in dbList)
            {
                var res = new GetTaskEntryForViewDto()
                {
                    TaskEntry = new TaskEntryDto
                    {

                        Caption = o.Caption,
                        SharedTaskId = o.SharedTaskId,
                        Id = o.Id,
                    },
                    PostPostTitle = o.PostPostTitle,
                    UserName = o.UserName,
                    UserName2 = o.UserName2,
                    TaskEntryCaption = o.TaskEntryCaption
                };

                results.Add(res);
            }

            return new PagedResultDto<GetTaskEntryForViewDto>(
                totalCount,
                results
            );

        }

        [AbpAuthorize(AppPermissions.Pages_TaskEntries_Edit)]
        public virtual async Task<GetTaskEntryForEditOutput> GetTaskEntryForEdit(EntityDto input)
        {
            var taskEntry = await _taskEntryRepository.FirstOrDefaultAsync(input.Id);

            var output = new GetTaskEntryForEditOutput { TaskEntry = ObjectMapper.Map<CreateOrEditTaskEntryDto>(taskEntry) };

            if (output.TaskEntry.PostId != null)
            {
                var _lookupPost = await _lookup_postRepository.FirstOrDefaultAsync((int)output.TaskEntry.PostId);
                output.PostPostTitle = _lookupPost?.PostTitle?.ToString();
            }

            if (output.TaskEntry.IssuerId != null)
            {
                var _lookupUser = await _lookup_userRepository.FirstOrDefaultAsync((long)output.TaskEntry.IssuerId);
                output.UserName = _lookupUser?.Name?.ToString();
            }

            if (output.TaskEntry.ReceiverId != null)
            {
                var _lookupUser = await _lookup_userRepository.FirstOrDefaultAsync((long)output.TaskEntry.ReceiverId);
                output.UserName2 = _lookupUser?.Name?.ToString();
            }

            if (output.TaskEntry.ParentId != null)
            {
                var _lookupTaskEntry = await _lookup_taskEntryRepository.FirstOrDefaultAsync((int)output.TaskEntry.ParentId);
                output.TaskEntryCaption = _lookupTaskEntry?.Caption?.ToString();
            }

            return output;
        }

        public virtual async Task CreateOrEdit(CreateOrEditTaskEntryDto input)
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

        [AbpAuthorize(AppPermissions.Pages_TaskEntries_Create)]
        protected virtual async Task Create(CreateOrEditTaskEntryDto input)
        {
            var taskEntry = ObjectMapper.Map<TaskEntry>(input);
            await _taskEntryRepository.InsertAsync(taskEntry);
            
            await _appNotifier.SendTaskNotificationAsync(JsonConvert.SerializeObject(taskEntry, new JsonSerializerSettings
                {
                    ContractResolver = new DefaultContractResolver
                    {
                        NamingStrategy = new CamelCaseNamingStrategy() // Use PascalCaseNamingStrategy for Pascal case
                    }
                }),
                userIds: new[] { new UserIdentifier(AbpSession.TenantId, input.ReceiverId) },
                NotificationSeverity.Info
            );
        }

        [AbpAuthorize(AppPermissions.Pages_TaskEntries_Edit)]
        protected virtual async Task Update(CreateOrEditTaskEntryDto input)
        {
            var taskEntry = await _taskEntryRepository.FirstOrDefaultAsync((int)input.Id);
            ObjectMapper.Map(input, taskEntry);

        }

        [AbpAuthorize(AppPermissions.Pages_TaskEntries_Delete)]
        public virtual async Task Delete(EntityDto input)
        {
            await _taskEntryRepository.DeleteAsync(input.Id);
        }

        [AbpAuthorize(AppPermissions.Pages_TaskEntries)]
        public async Task<PagedResultDto<TaskEntryPostLookupTableDto>> GetAllPostForLookupTable(GetAllForLookupTableInput input)
        {
            var query = _lookup_postRepository.GetAll().WhereIf(
                   !string.IsNullOrWhiteSpace(input.Filter),
                  e => e.PostTitle != null && e.PostTitle.Contains(input.Filter)
               );

            var totalCount = await query.CountAsync();

            var postList = await query
                .PageBy(input)
                .ToListAsync();

            var lookupTableDtoList = new List<TaskEntryPostLookupTableDto>();
            foreach (var post in postList)
            {
                lookupTableDtoList.Add(new TaskEntryPostLookupTableDto
                {
                    Id = post.Id,
                    DisplayName = post.PostTitle?.ToString()
                });
            }

            return new PagedResultDto<TaskEntryPostLookupTableDto>(
                totalCount,
                lookupTableDtoList
            );
        }

        [AbpAuthorize(AppPermissions.Pages_TaskEntries)]
        public async Task<PagedResultDto<TaskEntryUserLookupTableDto>> GetAllUserForLookupTable(GetAllForLookupTableInput input)
        {
            var query = _lookup_userRepository.GetAll().WhereIf(
                   !string.IsNullOrWhiteSpace(input.Filter),
                  e => e.Name != null && e.Name.Contains(input.Filter)
               );

            var totalCount = await query.CountAsync();

            var userList = await query
                .PageBy(input)
                .ToListAsync();

            var lookupTableDtoList = new List<TaskEntryUserLookupTableDto>();
            foreach (var user in userList)
            {
                lookupTableDtoList.Add(new TaskEntryUserLookupTableDto
                {
                    Id = user.Id,
                    DisplayName = user.Name?.ToString()
                });
            }

            return new PagedResultDto<TaskEntryUserLookupTableDto>(
                totalCount,
                lookupTableDtoList
            );
        }

        [AbpAuthorize(AppPermissions.Pages_TaskEntries)]
        public async Task<PagedResultDto<TaskEntryTaskEntryLookupTableDto>> GetAllTaskEntryForLookupTable(GetAllForLookupTableInput input)
        {
            var query = _lookup_taskEntryRepository.GetAll().WhereIf(
                   !string.IsNullOrWhiteSpace(input.Filter),
                  e => e.Caption != null && e.Caption.Contains(input.Filter)
               );

            var totalCount = await query.CountAsync();

            var taskEntryList = await query
                .PageBy(input)
                .ToListAsync();

            var lookupTableDtoList = new List<TaskEntryTaskEntryLookupTableDto>();
            foreach (var taskEntry in taskEntryList)
            {
                lookupTableDtoList.Add(new TaskEntryTaskEntryLookupTableDto
                {
                    Id = taskEntry.Id,
                    DisplayName = taskEntry.Caption?.ToString()
                });
            }

            return new PagedResultDto<TaskEntryTaskEntryLookupTableDto>(
                totalCount,
                lookupTableDtoList
            );
        }

    }
}