using System.Linq;
using System.Linq.Dynamic.Core;
using Abp.Linq.Extensions;
using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using Chamran.Deed.Common.Dtos;
using Abp.Application.Services.Dto;
using Chamran.Deed.Authorization;
using Abp.Authorization;
using Microsoft.EntityFrameworkCore;

namespace Chamran.Deed.Common
{
    [AbpAuthorize(AppPermissions.Pages_FCMQueues)]
    public class FCMQueuesAppService : DeedAppServiceBase, IFCMQueuesAppService
    {
        private readonly IRepository<FCMQueue> _fcmQueueRepository;

        public FCMQueuesAppService(IRepository<FCMQueue> fcmQueueRepository)
        {
            _fcmQueueRepository = fcmQueueRepository;

        }

        public async Task<PagedResultDto<GetFCMQueueForViewDto>> GetAll(GetAllFCMQueuesInput input)
        {

            var filteredFCMQueues = _fcmQueueRepository.GetAll()
                        .WhereIf(!string.IsNullOrWhiteSpace(input.Filter), e => false || e.DeviceToken.Contains(input.Filter) || e.PushTitle.Contains(input.Filter) || e.PushBody.Contains(input.Filter))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.PushTitleFilter), e => e.PushTitle.Contains(input.PushTitleFilter))
                        .WhereIf(input.IsSentFilter.HasValue && input.IsSentFilter > -1, e => (input.IsSentFilter == 1 && e.IsSent) || (input.IsSentFilter == 0 && !e.IsSent))
                        .WhereIf(input.MinSentTimeFilter != null, e => e.SentTime >= input.MinSentTimeFilter)
                        .WhereIf(input.MaxSentTimeFilter != null, e => e.SentTime <= input.MaxSentTimeFilter);

            var pagedAndFilteredFCMQueues = filteredFCMQueues
                .OrderBy(input.Sorting ?? "id asc")
                .PageBy(input);

            var fcmQueues = from o in pagedAndFilteredFCMQueues
                            select new
                            {

                                o.DeviceToken,
                                o.PushTitle,
                                o.PushBody,
                                o.IsSent,
                                o.SentTime,
                                Id = o.Id
                            };

            var totalCount = await filteredFCMQueues.CountAsync();

            var dbList = await fcmQueues.ToListAsync();
            var results = new List<GetFCMQueueForViewDto>();

            foreach (var o in dbList)
            {
                var res = new GetFCMQueueForViewDto()
                {
                    FCMQueue = new FCMQueueDto
                    {

                        DeviceToken = o.DeviceToken,
                        PushTitle = o.PushTitle,
                        PushBody = o.PushBody,
                        IsSent = o.IsSent,
                        SentTime = o.SentTime,
                        Id = o.Id,
                    }
                };

                results.Add(res);
            }

            return new PagedResultDto<GetFCMQueueForViewDto>(
                totalCount,
                results
            );

        }

        public async Task<GetFCMQueueForViewDto> GetFCMQueueForView(int id)
        {
            var fcmQueue = await _fcmQueueRepository.GetAsync(id);

            var output = new GetFCMQueueForViewDto { FCMQueue = ObjectMapper.Map<FCMQueueDto>(fcmQueue) };

            return output;
        }

        [AbpAuthorize(AppPermissions.Pages_FCMQueues_Edit)]
        public async Task<GetFCMQueueForEditOutput> GetFCMQueueForEdit(EntityDto input)
        {
            var fcmQueue = await _fcmQueueRepository.FirstOrDefaultAsync(input.Id);

            var output = new GetFCMQueueForEditOutput { FCMQueue = ObjectMapper.Map<CreateOrEditFCMQueueDto>(fcmQueue) };

            return output;
        }

        public async Task CreateOrEdit(CreateOrEditFCMQueueDto input)
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

        [AbpAuthorize(AppPermissions.Pages_FCMQueues_Create)]
        protected virtual async Task Create(CreateOrEditFCMQueueDto input)
        {
            var fcmQueue = ObjectMapper.Map<FCMQueue>(input);

            await _fcmQueueRepository.InsertAsync(fcmQueue);

        }

        [AbpAuthorize(AppPermissions.Pages_FCMQueues_Edit)]
        protected virtual async Task Update(CreateOrEditFCMQueueDto input)
        {
            var fcmQueue = await _fcmQueueRepository.FirstOrDefaultAsync((int)input.Id);
            ObjectMapper.Map(input, fcmQueue);

        }

        [AbpAuthorize(AppPermissions.Pages_FCMQueues_Delete)]
        public async Task Delete(EntityDto input)
        {
            await _fcmQueueRepository.DeleteAsync(input.Id);
        }

    }
}