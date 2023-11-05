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

namespace Chamran.Deed.Info
{
    [AbpAuthorize(AppPermissions.Pages_TaskStats)]
    public class TaskStatsAppService : DeedAppServiceBase, ITaskStatsAppService
    {
        private readonly IRepository<TaskStat> _taskStatRepository;
        private readonly IRepository<User, long> _lookup_userRepository;

        public TaskStatsAppService(IRepository<TaskStat> taskStatRepository, IRepository<User, long> lookup_userRepository)
        {
            _taskStatRepository = taskStatRepository;
            _lookup_userRepository = lookup_userRepository;

        }



        public virtual async Task<PagedResultDto<GetTaskStatForViewDto>> GetAll(GetAllTaskStatsInput input)
        {

            var filteredTaskStats = _taskStatRepository.GetAll()
                        .Include(e => e.DoneByFk)
                        .WhereIf(!string.IsNullOrWhiteSpace(input.Filter), e => false || e.Caption.Contains(input.Filter))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.CaptionFilter), e => e.Caption.Contains(input.CaptionFilter))
                        .WhereIf(input.MinStatusFilter != null, e => e.Status >= input.MinStatusFilter)
                        .WhereIf(input.MaxStatusFilter != null, e => e.Status <= input.MaxStatusFilter)
                        .WhereIf(!string.IsNullOrWhiteSpace(input.SharedTaskIdFilter.ToString()), e => e.SharedTaskId.ToString() == input.SharedTaskIdFilter.ToString())
                        .WhereIf(!string.IsNullOrWhiteSpace(input.UserNameFilter), e => e.DoneByFk != null && e.DoneByFk.Name == input.UserNameFilter);

            var pagedAndFilteredTaskStats = filteredTaskStats
                .OrderBy(input.Sorting ?? "id asc")
                .PageBy(input);

            var taskStats = from o in pagedAndFilteredTaskStats
                            join o1 in _lookup_userRepository.GetAll() on o.DoneBy equals o1.Id into j1
                            from s1 in j1.DefaultIfEmpty()

                            select new
                            {

                                o.Caption,
                                o.Status,
                                o.SharedTaskId,
                                Id = o.Id,
                                UserName = s1 == null || s1.Name == null ? "" : s1.Name.ToString()
                            };

            var totalCount = await filteredTaskStats.CountAsync();

            var dbList = await taskStats.ToListAsync();
            var results = new List<GetTaskStatForViewDto>();

            foreach (var o in dbList)
            {
                var res = new GetTaskStatForViewDto()
                {
                    TaskStat = new TaskStatDto
                    {

                        Caption = o.Caption,
                        Status = o.Status,
                        SharedTaskId = o.SharedTaskId,
                        Id = o.Id,
                    },
                    UserName = o.UserName
                };

                results.Add(res);
            }

            return new PagedResultDto<GetTaskStatForViewDto>(
                totalCount,
                results
            );

        }

        [AbpAuthorize(AppPermissions.Pages_TaskStats_Edit)]
        public virtual async Task<GetTaskStatForEditOutput> GetTaskStatForEdit(EntityDto input)
        {
            var taskStat = await _taskStatRepository.FirstOrDefaultAsync(input.Id);

            var output = new GetTaskStatForEditOutput { TaskStat = ObjectMapper.Map<CreateOrEditTaskStatDto>(taskStat) };

            if (output.TaskStat.DoneBy != null)
            {
                var _lookupUser = await _lookup_userRepository.FirstOrDefaultAsync((long)output.TaskStat.DoneBy);
                output.UserName = _lookupUser?.Name?.ToString();
            }

            return output;
        }

        public virtual async Task CreateOrEdit(CreateOrEditTaskStatDto input)
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

        [AbpAuthorize(AppPermissions.Pages_TaskStats_Create)]
        protected virtual async Task Create(CreateOrEditTaskStatDto input)
        {
            var taskStat = ObjectMapper.Map<TaskStat>(input);

            await _taskStatRepository.InsertAsync(taskStat);

        }

        [AbpAuthorize(AppPermissions.Pages_TaskStats_Edit)]
        protected virtual async Task Update(CreateOrEditTaskStatDto input)
        {
            var taskStat = await _taskStatRepository.FirstOrDefaultAsync((int)input.Id);
            ObjectMapper.Map(input, taskStat);

        }

        [AbpAuthorize(AppPermissions.Pages_TaskStats_Delete)]
        public virtual async Task Delete(EntityDto input)
        {
            await _taskStatRepository.DeleteAsync(input.Id);
        }

        [AbpAuthorize(AppPermissions.Pages_TaskStats)]
        public async Task<PagedResultDto<TaskStatUserLookupTableDto>> GetAllUserForLookupTable(GetAllForLookupTableInput input)
        {
            var query = _lookup_userRepository.GetAll().WhereIf(
                   !string.IsNullOrWhiteSpace(input.Filter),
                  e => e.Name != null && e.Name.Contains(input.Filter)
               );

            var totalCount = await query.CountAsync();

            var userList = await query
                .PageBy(input)
                .ToListAsync();

            var lookupTableDtoList = new List<TaskStatUserLookupTableDto>();
            foreach (var user in userList)
            {
                lookupTableDtoList.Add(new TaskStatUserLookupTableDto
                {
                    Id = user.Id,
                    DisplayName = user.Name?.ToString()
                });
            }

            return new PagedResultDto<TaskStatUserLookupTableDto>(
                totalCount,
                lookupTableDtoList
            );
        }

        public async Task<GetTaskStatDto> GetTaskStat(Guid sharedTaskId)
        {
            var entity = await _taskStatRepository.GetAll().Include(x => x.DoneByFk)
                .FirstOrDefaultAsync(x => x.SharedTaskId == sharedTaskId);
            if (entity != null)
            {

                return new GetTaskStatDto
                {
                    DoneBy = entity.DoneBy,
                    DoneByName = entity.DoneByFk.Name,
                    DoneByLastName = entity.DoneByFk.Surname,
                    SharedTaskId = entity.SharedTaskId,
                    Caption = entity.Caption,
                    CreationTime = entity.CreationTime,
                    DoneByProfilePicture = entity.DoneByFk.ProfilePictureId,
                    Status = entity.Status,
                };
            }
            else
            {
                return null;
            }
            


        }
    }
}