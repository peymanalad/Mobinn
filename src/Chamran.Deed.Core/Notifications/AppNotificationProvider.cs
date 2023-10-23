using Abp.Authorization;
using Abp.Localization;
using Abp.Notifications;
using Chamran.Deed.Authorization;

namespace Chamran.Deed.Notifications
{
    public class AppNotificationProvider : NotificationProvider
    {
        public override void SetNotifications(INotificationDefinitionContext context)
        {
            context.Manager.Add(
                new NotificationDefinition(
                    AppNotificationNames.MassNotification,
                    displayName: L("MassNotification"),
                    permissionDependency: new SimplePermissionDependency(AppPermissions.Pages)
                    )
                );
            context.Manager.Add(
                new NotificationDefinition(
                    AppNotificationNames.ChatMessage,
                    displayName: L("ChatMessage"),
                    permissionDependency: new SimplePermissionDependency(AppPermissions.Pages)
                    )
                );
            context.Manager.Add(
                           new NotificationDefinition(
                               AppNotificationNames.NewPost,
                               displayName: L("NewPost"),
                               permissionDependency: new SimplePermissionDependency(AppPermissions.Pages)
                               )
                           );
            context.Manager.Add(
                new NotificationDefinition(
                    AppNotificationNames.NewTask,
                    displayName: L("NewTask"),
                    permissionDependency: new SimplePermissionDependency(AppPermissions.Pages)
                )
            );

            //context.Manager.Add(
            //    new NotificationDefinition(
            //        AppNotificationNames.NewUserRegistered,
            //        displayName: L("NewUserRegisteredNotificationDefinition"),
            //        permissionDependency: new SimplePermissionDependency(AppPermissions.Pages_Administration_Users)
            //    )
            //);

            //context.Manager.Add(
            //    new NotificationDefinition(
            //        AppNotificationNames.NewTenantRegistered,
            //        displayName: L("NewTenantRegisteredNotificationDefinition"),
            //        permissionDependency: new SimplePermissionDependency(AppPermissions.Pages_Tenants)
            //        )
            //    );
        }

        private static ILocalizableString L(string name)
        {
            return new LocalizableString(name, DeedConsts.LocalizationSourceName);
        }
    }
}
