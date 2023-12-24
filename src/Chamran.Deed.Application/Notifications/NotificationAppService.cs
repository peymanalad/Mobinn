using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp;
using Abp.Application.Services.Dto;
using Abp.Auditing;
using Abp.Authorization;
using Abp.BackgroundJobs;
using Abp.Collections.Extensions;
using Abp.Configuration;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.Notifications;
using Abp.Organizations;
using Abp.Runtime.Session;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using Chamran.Deed.Authorization;
using Chamran.Deed.Authorization.Users;
using Chamran.Deed.Authorization.Users.Dto;
using Chamran.Deed.Info.Dtos;
using Chamran.Deed.Notifications.Dto;
using Chamran.Deed.Organizations;
using MathNet.Numerics.Financial;
using Newtonsoft.Json;
using GetAllForLookupTableInput = Chamran.Deed.Notifications.Dto.GetAllForLookupTableInput;

namespace Chamran.Deed.Notifications
{
    [AbpAuthorize]
    public class NotificationAppService : DeedAppServiceBase, INotificationAppService
    {
        private readonly INotificationDefinitionManager _notificationDefinitionManager;
        private readonly IUserNotificationManager _userNotificationManager;
        private readonly INotificationSubscriptionManager _notificationSubscriptionManager;
        private readonly IRepository<User, long> _userRepository;
        private readonly IRepository<OrganizationUnit, long> _organizationUnitRepository;
        private readonly IAppNotifier _appNotifier;
        private readonly IUserOrganizationUnitRepository _userOrganizationUnitRepository;
        private readonly INotificationConfiguration _notificationConfiguration;
        private readonly INotificationStore _notificationStore;
        private readonly IBackgroundJobManager _backgroundJobManager;
        private readonly IRepository<UserNotificationInfo, Guid> _userNotificationRepository;
        private readonly IUserAppService _userAppService;

        public NotificationAppService(
            INotificationDefinitionManager notificationDefinitionManager,
            IUserNotificationManager userNotificationManager,
            INotificationSubscriptionManager notificationSubscriptionManager,
            IRepository<User, long> userRepository,
            IRepository<OrganizationUnit, long> organizationUnitRepository,
            IAppNotifier appNotifier,
            IUserOrganizationUnitRepository userOrganizationUnitRepository,
            INotificationConfiguration notificationConfiguration,
            INotificationStore notificationStore,
            IBackgroundJobManager backgroundJobManager,
            IRepository<UserNotificationInfo, Guid> userNotificationRepository, IUserAppService userAppService)
        {
            _notificationDefinitionManager = notificationDefinitionManager;
            _userNotificationManager = userNotificationManager;
            _notificationSubscriptionManager = notificationSubscriptionManager;
            _userRepository = userRepository;
            _organizationUnitRepository = organizationUnitRepository;
            _appNotifier = appNotifier;
            _userOrganizationUnitRepository = userOrganizationUnitRepository;
            _notificationConfiguration = notificationConfiguration;
            _notificationStore = notificationStore;
            _backgroundJobManager = backgroundJobManager;
            _userNotificationRepository = userNotificationRepository;
            _userAppService = userAppService;
        }

        [DisableAuditing]
        public async Task<GetNotificationsOutput> GetUserNotifications(GetUserNotificationsInput input)
        {
            var totalCount = await _userNotificationManager.GetUserNotificationCountAsync(
                AbpSession.ToUserIdentifier(), input.State, input.StartDate, input.EndDate
            );

            var unreadCount = await _userNotificationManager.GetUserNotificationCountAsync(
                AbpSession.ToUserIdentifier(), UserNotificationState.Unread, input.StartDate, input.EndDate
            );
            if (!string.IsNullOrWhiteSpace(input.NotificationName))
            {
                var notifications = await _userNotificationManager.GetUserNotificationsAsync(
                    AbpSession.ToUserIdentifier(), input.State, 0, int.MaxValue, input.StartDate,
                    input.EndDate
                );
                notifications = notifications.Where(x => x.Notification.NotificationName == input.NotificationName).ToList();
                notifications = notifications.Skip(input.SkipCount).Take(input.MaxResultCount).ToList();
                //var notificationsDto = GetNotificationsDto(notifications);
                return new GetNotificationsOutput(totalCount, unreadCount, notifications);

            }
            else
            {
                var notifications = await _userNotificationManager.GetUserNotificationsAsync(
                    AbpSession.ToUserIdentifier(), input.State, input.SkipCount, input.MaxResultCount, input.StartDate,
                    input.EndDate
                );
                //var notificationsDto = GetNotificationsDto(notifications);
                return new GetNotificationsOutput(totalCount, unreadCount, notifications);
            }

        }

        private List<UserNotificationDto> GetNotificationsDto(List<UserNotification> notifications)
        {
            var ls = new List<UserNotificationDto>();
            foreach (var row in notifications)
            {
                ls.Add(new UserNotificationDto()
                {
                    Id = row.Id,
                    UserId = row.UserId,
                    Notification = GetTenantNotificationDto(row.Notification),
                    State = row.State,
                    TargetNotifiers = row.TargetNotifiers,
                    TenantId = row.TenantId,

                });
            }

            return ls;
        }

        private TenantNotificationDto GetTenantNotificationDto(TenantNotification tn)
        {
            return new TenantNotificationDto()
            {
                CreationTime = tn.CreationTime,
                Id = tn.Id,
                TenantId = tn.TenantId,
                Data = GetNotficationDateDto(tn.Data, tn.NotificationName),
                EntityId = tn.EntityId,
                EntityTypeName = tn.EntityTypeName,
                NotificationName = tn.NotificationName,
                Severity = tn.Severity

            };

        }

        private NotificationDataDto GetNotficationDateDto(NotificationData nd, string tnNotificationName)
        {
            var result = new NotificationDataDto();
            ;
            if (nd.Type == "Abp.Notifications.MessageNotificationData" && nd.Properties.Any())
            {
                
                var message= JsonConvert.DeserializeObject<MessageNotificationData>(nd.Properties["Message"].ToString() ?? string.Empty);
                switch (tnNotificationName)
                {
                    case AppNotificationNames.ChatMessage:
                        {
                            if (message != null)
                            {
                                result.Post = JsonConvert.DeserializeObject<PostDto>(message.Message);

                            }
                        }

                        break;
                    
                }
            }
            return result;
        }

        public async Task<bool> ShouldUserUpdateApp()
        {
            var notifications = await _userNotificationManager.GetUserNotificationsAsync(
                AbpSession.ToUserIdentifier(), UserNotificationState.Unread
            );

            return notifications.Any(x => x.Notification.NotificationName == AppNotificationNames.NewVersionAvailable);
        }

        public async Task<SetNotificationAsReadOutput> SetAllAvailableVersionNotificationAsRead()
        {
            var notifications = await _userNotificationManager.GetUserNotificationsAsync(
                AbpSession.ToUserIdentifier(), UserNotificationState.Unread
            );

            var filteredNotifications = notifications
                .Where(x => x.Notification.NotificationName == AppNotificationNames.NewVersionAvailable)
                .ToList();

            if (!filteredNotifications.Any())
            {
                return new SetNotificationAsReadOutput(false);
            }

            foreach (var notification in filteredNotifications)
            {
                if (notification.State == UserNotificationState.Read)
                {
                    continue;
                }

                await _userNotificationManager.UpdateUserNotificationStateAsync(
                    notification.TenantId,
                    notification.Id,
                    UserNotificationState.Read
                );
            }

            return new SetNotificationAsReadOutput(true);
        }

        public async Task SetAllNotificationsAsRead()
        {
            await _userNotificationManager.UpdateAllUserNotificationStatesAsync(
                AbpSession.ToUserIdentifier(),
                UserNotificationState.Read
            );
        }

        public async Task<SetNotificationAsReadOutput> SetNotificationAsRead(EntityDto<Guid> input)
        {
            var userNotification =
                await _userNotificationManager.GetUserNotificationAsync(AbpSession.TenantId, input.Id);
            if (userNotification == null)
            {
                return new SetNotificationAsReadOutput(false);
            }

            if (userNotification.UserId != AbpSession.GetUserId())
            {
                throw new UserFriendlyException(
                    $"Given user notification id ({input.Id}) is not belong to the current user ({AbpSession.GetUserId()})"
                );
            }

            if (userNotification.State == UserNotificationState.Read)
            {
                return new SetNotificationAsReadOutput(false);
            }

            await _userNotificationManager.UpdateUserNotificationStateAsync(AbpSession.TenantId, input.Id,
                UserNotificationState.Read);
            return new SetNotificationAsReadOutput(true);
        }

        public async Task<GetNotificationSettingsOutput> GetNotificationSettings()
        {
            var output = new GetNotificationSettingsOutput();

            output.ReceiveNotifications =
                await SettingManager.GetSettingValueAsync<bool>(NotificationSettingNames.ReceiveNotifications);

            //Get general notifications, not entity related notifications.
            var notificationDefinitions =
                (await _notificationDefinitionManager.GetAllAvailableAsync(AbpSession.ToUserIdentifier())).Where(nd =>
                    nd.EntityType == null);

            output.Notifications =
                ObjectMapper.Map<List<NotificationSubscriptionWithDisplayNameDto>>(notificationDefinitions);

            var subscribedNotifications = (await _notificationSubscriptionManager
                    .GetSubscribedNotificationsAsync(AbpSession.ToUserIdentifier()))
                .Select(ns => ns.NotificationName)
                .ToList();

            output.Notifications.ForEach(n => n.IsSubscribed = subscribedNotifications.Contains(n.Name));

            return output;
        }

        public async Task UpdateNotificationSettings(UpdateNotificationSettingsInput input)
        {
            await SettingManager.ChangeSettingForUserAsync(AbpSession.ToUserIdentifier(),
                NotificationSettingNames.ReceiveNotifications, input.ReceiveNotifications.ToString());

            foreach (var notification in input.Notifications)
            {
                if (notification.IsSubscribed)
                {
                    await _notificationSubscriptionManager.SubscribeAsync(AbpSession.ToUserIdentifier(),
                        notification.Name);
                }
                else
                {
                    await _notificationSubscriptionManager.UnsubscribeAsync(AbpSession.ToUserIdentifier(),
                        notification.Name);
                }
            }
        }

        public async Task DeleteNotification(EntityDto<Guid> input)
        {
            var notification = await _userNotificationManager.GetUserNotificationAsync(AbpSession.TenantId, input.Id);
            if (notification == null)
            {
                return;
            }

            if (notification.UserId != AbpSession.GetUserId())
            {
                throw new UserFriendlyException(L("ThisNotificationDoesntBelongToYou"));
            }

            await _userNotificationManager.DeleteUserNotificationAsync(AbpSession.TenantId, input.Id);
        }

        public async Task DeleteAllUserNotifications(DeleteAllUserNotificationsInput input)
        {
            await _userNotificationManager.DeleteAllUserNotificationsAsync(
                AbpSession.ToUserIdentifier(),
                input.State,
                input.StartDate,
                input.EndDate);
        }

        [AbpAuthorize(AppPermissions.Pages_Administration_MassNotification)]
        public async Task<PagedResultDto<MassNotificationUserLookupTableDto>> GetAllUserForLookupTable(
            GetAllForLookupTableInput input)
        {
            var query = _userRepository.GetAll()
                .WhereIf(!string.IsNullOrWhiteSpace(input.Filter),
                    e =>
                        (e.Name != null && e.Name.Contains(input.Filter)) ||
                        (e.Surname != null && e.Surname.Contains(input.Filter)) ||
                        (e.EmailAddress != null && e.EmailAddress.Contains(input.Filter))
                );

            var totalCount = await query.CountAsync();

            var userList = await query
                .PageBy(input)
                .ToListAsync();

            var lookupTableDtoList = new List<MassNotificationUserLookupTableDto>();
            foreach (var user in userList)
            {
                lookupTableDtoList.Add(new MassNotificationUserLookupTableDto
                {
                    Id = user.Id,
                    DisplayName = user.Name + " " + user.Surname + " (" + user.EmailAddress + ")"
                });
            }

            return new PagedResultDto<MassNotificationUserLookupTableDto>(
                totalCount,
                lookupTableDtoList
            );
        }

        [AbpAuthorize(AppPermissions.Pages_Administration_MassNotification)]
        public async Task<PagedResultDto<MassNotificationOrganizationUnitLookupTableDto>>
            GetAllOrganizationUnitForLookupTable(GetAllForLookupTableInput input)
        {
            var query = _organizationUnitRepository.GetAll()
                .WhereIf(!string.IsNullOrWhiteSpace(input.Filter),
                    e => e.DisplayName != null && e.DisplayName.Contains(input.Filter));

            var totalCount = await query.CountAsync();

            var organizationUnitList = await query
                .PageBy(input)
                .ToListAsync();

            var lookupTableDtoList = new List<MassNotificationOrganizationUnitLookupTableDto>();
            foreach (var organizationUnit in organizationUnitList)
            {
                lookupTableDtoList.Add(new MassNotificationOrganizationUnitLookupTableDto
                {
                    Id = organizationUnit.Id,
                    DisplayName = organizationUnit.DisplayName
                });
            }

            return new PagedResultDto<MassNotificationOrganizationUnitLookupTableDto>(
                totalCount,
                lookupTableDtoList
            );
        }

        [AbpAuthorize(AppPermissions.Pages_Administration_MassNotification_Create)]
        public async Task CreateAlarm(CreateAlarmInput input)
        {
            //if (input.TargetNotifiers.IsNullOrEmpty())
            //{
            //    throw new UserFriendlyException(L("MassNotificationTargetNotifiersFieldIsRequiredMessage"));
            //}

            //var userIds = new List<UserIdentifier>();

            //if (!input.UserIds.IsNullOrEmpty())
            //{
            //    userIds.AddRange(input.UserIds.Select(i => new UserIdentifier(AbpSession.TenantId, i)));
            //}

            //if (!input.OrganizationUnitIds.IsNullOrEmpty())
            //{
            //    userIds.AddRange(
            //        await _userOrganizationUnitRepository.GetAllUsersInOrganizationUnitHierarchical(
            //            input.OrganizationUnitIds)
            //    );
            //}

            //if (userIds.Count == 0)
            //{
            //    if (input.OrganizationUnitIds.IsNullOrEmpty())
            //    {
            //        // tried to get users from organization, but could not find any user
            //        throw new UserFriendlyException(L("MassNotificationNoUsersFoundInOrganizationUnitMessage"));
            //    }

            //    throw new UserFriendlyException(L("MassNotificationUserOrOrganizationUnitFieldIsRequiredMessage"));
            //}

            //var targetNotifiers = new List<Type>();

            //foreach (var notifier in _notificationConfiguration.Notifiers)
            //{
            //    if (input.TargetNotifiers.Contains(notifier.FullName))
            //    {
            //        targetNotifiers.Add(notifier);
            //    }
            //}
            var list = await _userAppService.GetListOfUsers(new GetUsersInput() { MaxResultCount = int.MaxValue });
            var users = list.Items.Select(i => new UserIdentifier(AbpSession.TenantId, i.Id));

            await _appNotifier.SendMassNotificationAsync(
                input.Message,
                users.DistinctBy(u => u.UserId).ToArray(),
                NotificationSeverity.Info,
                null
            );
        }

        [AbpAuthorize(AppPermissions.Pages_Administration_MassNotification_Create)]
        public async Task CreateMassNotification(CreateMassNotificationInput input)
        {
            if (input.TargetNotifiers.IsNullOrEmpty())
            {
                throw new UserFriendlyException(L("MassNotificationTargetNotifiersFieldIsRequiredMessage"));
            }

            var userIds = new List<UserIdentifier>();

            if (!input.UserIds.IsNullOrEmpty())
            {
                userIds.AddRange(input.UserIds.Select(i => new UserIdentifier(AbpSession.TenantId, i)));
            }

            if (!input.OrganizationUnitIds.IsNullOrEmpty())
            {
                userIds.AddRange(
                    await _userOrganizationUnitRepository.GetAllUsersInOrganizationUnitHierarchical(
                        input.OrganizationUnitIds)
                );
            }

            if (userIds.Count == 0)
            {
                if (input.OrganizationUnitIds.IsNullOrEmpty())
                {
                    // tried to get users from organization, but could not find any user
                    throw new UserFriendlyException(L("MassNotificationNoUsersFoundInOrganizationUnitMessage"));
                }

                throw new UserFriendlyException(L("MassNotificationUserOrOrganizationUnitFieldIsRequiredMessage"));
            }

            var targetNotifiers = new List<Type>();

            foreach (var notifier in _notificationConfiguration.Notifiers)
            {
                if (input.TargetNotifiers.Contains(notifier.FullName))
                {
                    targetNotifiers.Add(notifier);
                }
            }

            await _appNotifier.SendMassNotificationAsync(
                input.Message,
                userIds.DistinctBy(u => u.UserId).ToArray(),
                input.Severity,
                targetNotifiers.ToArray()
            );
        }

        [AbpAuthorize(AppPermissions.Pages_Administration_NewVersion_Create)]
        public async Task CreateNewVersionReleasedNotification()
        {
            var args = new SendNotificationToAllUsersArgs
            {
                NotificationName = AppNotificationNames.NewVersionAvailable,
                Message = L("NewVersionAvailableNotificationMessage")
            };

            await _backgroundJobManager.EnqueueAsync<SendNotificationToAllUsersBackgroundJob, SendNotificationToAllUsersArgs>(args);
        }

        public List<string> GetAllNotifiers()
        {
            return _notificationConfiguration.Notifiers.Select(n => n.FullName).ToList();
        }

        [AbpAuthorize(AppPermissions.Pages_Administration_MassNotification)]
        public async Task<GetPublishedNotificationsOutput> GetNotificationsPublishedByUser(
            GetPublishedNotificationsInput input)
        {
            return new GetPublishedNotificationsOutput(
                await _notificationStore.GetNotificationsPublishedByUserAsync(AbpSession.ToUserIdentifier(),
                    AppNotificationNames.MassNotification, input.StartDate, input.EndDate)
            );
        }
    }
}