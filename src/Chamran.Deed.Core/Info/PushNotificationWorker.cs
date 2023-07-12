using System;
using System.Collections.Generic;
using System.Linq;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Threading.BackgroundWorkers;
using Abp.Threading.Timers;
using Abp.Timing;
using Chamran.Deed.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Chamran.Deed.Info;

public class PushNotificationWorker : PeriodicBackgroundWorkerBase, ISingletonDependency
{
    private readonly ILogger<PushNotificationWorker> _logger;
    private readonly IRepository<FCMQueue> _fcmQueueRepository;
    private readonly IPushNotificationService _pushNotificationService;
    private readonly IUnitOfWorkManager _unitOfWorkManager;

    public PushNotificationWorker(
        AbpTimer timer,
        ILogger<PushNotificationWorker> logger,
        IRepository<FCMQueue> fcmQueueRepository,
        IPushNotificationService pushNotificationService,
        IUnitOfWorkManager unitOfWorkManager) : base(timer)
    {
        Timer.Period = 60000;
        _logger = logger;
        _fcmQueueRepository = fcmQueueRepository;
        this._pushNotificationService = pushNotificationService;
        _unitOfWorkManager = unitOfWorkManager;
    }


    protected override async void DoWork()
    {

        try
        {
            await _unitOfWorkManager.WithUnitOfWork(async () =>
            {
                var notifications = _fcmQueueRepository.GetAll().Where(x => !x.IsSent);
                foreach (var notification in notifications)
                {
                    try
                    {
                        // Send push notification using pushNotificationService
                        await _pushNotificationService.SendPushNotifications(new List<string> { notification.DeviceToken }, notification.PushTitle, notification.PushBody);

                        // Mark the notification as sent in the repository
                        notification.IsSent = true;
                        notification.SentTime = Clock.Now; // Update with the current timestamp
                        await _fcmQueueRepository.UpdateAsync(notification);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error sending push notification with ID {notification.Id}: {ex.Message}");
                    }
                }
            });

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error retrieving pending push notifications: {ex.Message}");
        }
    }
}