using Abp.Authorization;
using Abp.Configuration.Startup;
using Abp.Localization;
using Abp.MultiTenancy;

namespace Chamran.Deed.Authorization
{
    /// <summary>
    /// Application's authorization provider.
    /// Defines permissions for the application.
    /// See <see cref="AppPermissions"/> for all permission names.
    /// </summary>
    public class AppAuthorizationProvider : AuthorizationProvider
    {
        private readonly bool _isMultiTenancyEnabled;

        public AppAuthorizationProvider(bool isMultiTenancyEnabled)
        {
            _isMultiTenancyEnabled = isMultiTenancyEnabled;
        }

        public AppAuthorizationProvider(IMultiTenancyConfig multiTenancyConfig)
        {
            _isMultiTenancyEnabled = multiTenancyConfig.IsEnabled;
        }

        public override void SetPermissions(IPermissionDefinitionContext context)
        {
            //COMMON PERMISSIONS (FOR BOTH OF TENANTS AND HOST)

            var pages = context.GetPermissionOrNull(AppPermissions.Pages) ?? context.CreatePermission(AppPermissions.Pages, L("Pages"));

            var instagramCrawlerPosts = pages.CreateChildPermission(AppPermissions.Pages_InstagramCrawlerPosts, L("InstagramCrawlerPosts"));
            instagramCrawlerPosts.CreateChildPermission(AppPermissions.Pages_InstagramCrawlerPosts_Create, L("CreateNewInstagramCrawlerPost"));
            instagramCrawlerPosts.CreateChildPermission(AppPermissions.Pages_InstagramCrawlerPosts_Edit, L("EditInstagramCrawlerPost"));
            instagramCrawlerPosts.CreateChildPermission(AppPermissions.Pages_InstagramCrawlerPosts_Delete, L("DeleteInstagramCrawlerPost"));

            var deedCharts = pages.CreateChildPermission(AppPermissions.Pages_DeedCharts, L("DeedCharts"));
            deedCharts.CreateChildPermission(AppPermissions.Pages_DeedCharts_Create, L("CreateNewDeedChart"));
            deedCharts.CreateChildPermission(AppPermissions.Pages_DeedCharts_Edit, L("EditDeedChart"));
            deedCharts.CreateChildPermission(AppPermissions.Pages_DeedCharts_Delete, L("DeleteDeedChart"));

            var taskStats = pages.CreateChildPermission(AppPermissions.Pages_TaskStats, L("TaskStats"));
            taskStats.CreateChildPermission(AppPermissions.Pages_TaskStats_Create, L("CreateNewTaskStat"));
            taskStats.CreateChildPermission(AppPermissions.Pages_TaskStats_Edit, L("EditTaskStat"));
            taskStats.CreateChildPermission(AppPermissions.Pages_TaskStats_Delete, L("DeleteTaskStat"));

            var taskEntries = pages.CreateChildPermission(AppPermissions.Pages_TaskEntries, L("TaskEntries"));
            taskEntries.CreateChildPermission(AppPermissions.Pages_TaskEntries_Create, L("CreateNewTaskEntry"));
            taskEntries.CreateChildPermission(AppPermissions.Pages_TaskEntries_Edit, L("EditTaskEntry"));
            taskEntries.CreateChildPermission(AppPermissions.Pages_TaskEntries_Delete, L("DeleteTaskEntry"));

            var organizationUsers = pages.CreateChildPermission(AppPermissions.Pages_OrganizationUsers, L("OrganizationUsers"));
            organizationUsers.CreateChildPermission(AppPermissions.Pages_OrganizationUsers_Create, L("CreateNewOrganizationUser"));
            organizationUsers.CreateChildPermission(AppPermissions.Pages_OrganizationUsers_Edit, L("EditOrganizationUser"));
            organizationUsers.CreateChildPermission(AppPermissions.Pages_OrganizationUsers_Delete, L("DeleteOrganizationUser"));

            var organizationCharts = pages.CreateChildPermission(AppPermissions.Pages_OrganizationCharts, L("OrganizationCharts"));
            organizationCharts.CreateChildPermission(AppPermissions.Pages_OrganizationCharts_Create, L("CreateNewOrganizationChart"));
            organizationCharts.CreateChildPermission(AppPermissions.Pages_OrganizationCharts_Edit, L("EditOrganizationChart"));
            organizationCharts.CreateChildPermission(AppPermissions.Pages_OrganizationCharts_Delete, L("DeleteOrganizationChart"));

            var userPostGroups = pages.CreateChildPermission(AppPermissions.Pages_UserPostGroups, L("UserPostGroups"));
            userPostGroups.CreateChildPermission(AppPermissions.Pages_UserPostGroups_Create, L("CreateNewUserPostGroup"));
            userPostGroups.CreateChildPermission(AppPermissions.Pages_UserPostGroups_Edit, L("EditUserPostGroup"));
            userPostGroups.CreateChildPermission(AppPermissions.Pages_UserPostGroups_Delete, L("DeleteUserPostGroup"));

            var userLocations = pages.CreateChildPermission(AppPermissions.Pages_UserLocations, L("UserLocations"));
            userLocations.CreateChildPermission(AppPermissions.Pages_UserLocations_Create, L("CreateNewUserLocation"));
            userLocations.CreateChildPermission(AppPermissions.Pages_UserLocations_Edit, L("EditUserLocation"));
            userLocations.CreateChildPermission(AppPermissions.Pages_UserLocations_Delete, L("DeleteUserLocation"));

            var userTokens = pages.CreateChildPermission(AppPermissions.Pages_UserTokens, L("UserTokens"));
            userTokens.CreateChildPermission(AppPermissions.Pages_UserTokens_Create, L("CreateNewUserToken"));
            userTokens.CreateChildPermission(AppPermissions.Pages_UserTokens_Edit, L("EditUserToken"));
            userTokens.CreateChildPermission(AppPermissions.Pages_UserTokens_Delete, L("DeleteUserToken"));

            var fcmQueues = pages.CreateChildPermission(AppPermissions.Pages_FCMQueues, L("FCMQueues"));
            fcmQueues.CreateChildPermission(AppPermissions.Pages_FCMQueues_Create, L("CreateNewFCMQueue"));
            fcmQueues.CreateChildPermission(AppPermissions.Pages_FCMQueues_Edit, L("EditFCMQueue"));
            fcmQueues.CreateChildPermission(AppPermissions.Pages_FCMQueues_Delete, L("DeleteFCMQueue"));

            var reports = pages.CreateChildPermission(AppPermissions.Pages_Reports, L("Reports"));
            reports.CreateChildPermission(AppPermissions.Pages_Reports_Create, L("CreateNewReport"));
            reports.CreateChildPermission(AppPermissions.Pages_Reports_Edit, L("EditReport"));
            reports.CreateChildPermission(AppPermissions.Pages_Reports_Delete, L("DeleteReport"));

            var commentLikes = pages.CreateChildPermission(AppPermissions.Pages_CommentLikes, L("CommentLikes"));
            commentLikes.CreateChildPermission(AppPermissions.Pages_CommentLikes_Create, L("CreateNewCommentLike"));
            commentLikes.CreateChildPermission(AppPermissions.Pages_CommentLikes_Edit, L("EditCommentLike"));
            commentLikes.CreateChildPermission(AppPermissions.Pages_CommentLikes_Delete, L("DeleteCommentLike"));

            var postLikes = pages.CreateChildPermission(AppPermissions.Pages_PostLikes, L("PostLikes"));
            postLikes.CreateChildPermission(AppPermissions.Pages_PostLikes_Create, L("CreateNewPostLike"));
            postLikes.CreateChildPermission(AppPermissions.Pages_PostLikes_Edit, L("EditPostLike"));
            postLikes.CreateChildPermission(AppPermissions.Pages_PostLikes_Delete, L("DeletePostLike"));

            var softwareUpdates = pages.CreateChildPermission(AppPermissions.Pages_SoftwareUpdates, L("SoftwareUpdates"));
            softwareUpdates.CreateChildPermission(AppPermissions.Pages_SoftwareUpdates_Create, L("CreateNewSoftwareUpdate"));
            softwareUpdates.CreateChildPermission(AppPermissions.Pages_SoftwareUpdates_Edit, L("EditSoftwareUpdate"));
            softwareUpdates.CreateChildPermission(AppPermissions.Pages_SoftwareUpdates_Delete, L("DeleteSoftwareUpdate"));

            var comments = pages.CreateChildPermission(AppPermissions.Pages_Comments, L("Comments"));
            comments.CreateChildPermission(AppPermissions.Pages_Comments_Create, L("CreateNewComment"));
            comments.CreateChildPermission(AppPermissions.Pages_Comments_Edit, L("EditComment"));
            comments.CreateChildPermission(AppPermissions.Pages_Comments_Delete, L("DeleteComment"));

            var seens = pages.CreateChildPermission(AppPermissions.Pages_Seens, L("Seens"));
            seens.CreateChildPermission(AppPermissions.Pages_Seens_Create, L("CreateNewSeen"));
            seens.CreateChildPermission(AppPermissions.Pages_Seens_Edit, L("EditSeen"));
            seens.CreateChildPermission(AppPermissions.Pages_Seens_Delete, L("DeleteSeen"));

            var postGroups = pages.CreateChildPermission(AppPermissions.Pages_PostGroups, L("PostGroups"));
            postGroups.CreateChildPermission(AppPermissions.Pages_PostGroups_Create, L("CreateNewPostGroup"));
            postGroups.CreateChildPermission(AppPermissions.Pages_PostGroups_Edit, L("EditPostGroup"));
            postGroups.CreateChildPermission(AppPermissions.Pages_PostGroups_Delete, L("DeletePostGroup"));

            var posts = pages.CreateChildPermission(AppPermissions.Pages_Posts, L("Posts"));
            posts.CreateChildPermission(AppPermissions.Pages_Posts_Create, L("CreateNewPost"));
            posts.CreateChildPermission(AppPermissions.Pages_Posts_Edit, L("EditPost"));
            posts.CreateChildPermission(AppPermissions.Pages_Posts_Delete, L("DeletePost"));

            var groupMembers = pages.CreateChildPermission(AppPermissions.Pages_GroupMembers, L("GroupMembers"));
            groupMembers.CreateChildPermission(AppPermissions.Pages_GroupMembers_Create, L("CreateNewGroupMember"));
            groupMembers.CreateChildPermission(AppPermissions.Pages_GroupMembers_Edit, L("EditGroupMember"));
            groupMembers.CreateChildPermission(AppPermissions.Pages_GroupMembers_Delete, L("DeleteGroupMember"));

            var organizationGroups = pages.CreateChildPermission(AppPermissions.Pages_OrganizationGroups, L("OrganizationGroups"));
            organizationGroups.CreateChildPermission(AppPermissions.Pages_OrganizationGroups_Create, L("CreateNewOrganizationGroup"));
            organizationGroups.CreateChildPermission(AppPermissions.Pages_OrganizationGroups_Edit, L("EditOrganizationGroup"));
            organizationGroups.CreateChildPermission(AppPermissions.Pages_OrganizationGroups_Delete, L("DeleteOrganizationGroup"));

            var organizations = pages.CreateChildPermission(AppPermissions.Pages_Organizations, L("Organizations"));
            organizations.CreateChildPermission(AppPermissions.Pages_Organizations_Create, L("CreateNewOrganization"));
            organizations.CreateChildPermission(AppPermissions.Pages_Organizations_Edit, L("EditOrganization"));
            organizations.CreateChildPermission(AppPermissions.Pages_Organizations_Delete, L("DeleteOrganization"));

            pages.CreateChildPermission(AppPermissions.Pages_DemoUiComponents, L("DemoUiComponents"));

            var administration = pages.CreateChildPermission(AppPermissions.Pages_Administration, L("Administration"));

            var hashtags = administration.CreateChildPermission(AppPermissions.Pages_Administration_Hashtags, L("Hashtags"));
            hashtags.CreateChildPermission(AppPermissions.Pages_Administration_Hashtags_Create, L("CreateNewHashtag"));
            hashtags.CreateChildPermission(AppPermissions.Pages_Administration_Hashtags_Edit, L("EditHashtag"));
            hashtags.CreateChildPermission(AppPermissions.Pages_Administration_Hashtags_Delete, L("DeleteHashtag"));

            var roles = administration.CreateChildPermission(AppPermissions.Pages_Administration_Roles, L("Roles"));
            roles.CreateChildPermission(AppPermissions.Pages_Administration_Roles_Create, L("CreatingNewRole"));
            roles.CreateChildPermission(AppPermissions.Pages_Administration_Roles_Edit, L("EditingRole"));
            roles.CreateChildPermission(AppPermissions.Pages_Administration_Roles_Delete, L("DeletingRole"));

            var users = administration.CreateChildPermission(AppPermissions.Pages_Administration_Users, L("Users"));
            users.CreateChildPermission(AppPermissions.Pages_Administration_Users_Create, L("CreatingNewUser"));
            users.CreateChildPermission(AppPermissions.Pages_Administration_Users_Edit, L("EditingUser"));
            users.CreateChildPermission(AppPermissions.Pages_Administration_Users_Delete, L("DeletingUser"));
            users.CreateChildPermission(AppPermissions.Pages_Administration_Users_ChangePermissions, L("ChangingPermissions"));
            users.CreateChildPermission(AppPermissions.Pages_Administration_Users_Impersonation, L("LoginForUsers"));
            users.CreateChildPermission(AppPermissions.Pages_Administration_Users_Unlock, L("Unlock"));
            users.CreateChildPermission(AppPermissions.Pages_Administration_Users_ChangeProfilePicture, L("UpdateUsersProfilePicture"));

            var languages = administration.CreateChildPermission(AppPermissions.Pages_Administration_Languages, L("Languages"));
            languages.CreateChildPermission(AppPermissions.Pages_Administration_Languages_Create, L("CreatingNewLanguage"), multiTenancySides: _isMultiTenancyEnabled ? MultiTenancySides.Host : MultiTenancySides.Tenant);
            languages.CreateChildPermission(AppPermissions.Pages_Administration_Languages_Edit, L("EditingLanguage"), multiTenancySides: _isMultiTenancyEnabled ? MultiTenancySides.Host : MultiTenancySides.Tenant);
            languages.CreateChildPermission(AppPermissions.Pages_Administration_Languages_Delete, L("DeletingLanguages"), multiTenancySides: _isMultiTenancyEnabled ? MultiTenancySides.Host : MultiTenancySides.Tenant);
            languages.CreateChildPermission(AppPermissions.Pages_Administration_Languages_ChangeTexts, L("ChangingTexts"));
            languages.CreateChildPermission(AppPermissions.Pages_Administration_Languages_ChangeDefaultLanguage, L("ChangeDefaultLanguage"));

            administration.CreateChildPermission(AppPermissions.Pages_Administration_AuditLogs, L("AuditLogs"));

            var organizationUnits = administration.CreateChildPermission(AppPermissions.Pages_Administration_OrganizationUnits, L("OrganizationUnits"));
            organizationUnits.CreateChildPermission(AppPermissions.Pages_Administration_OrganizationUnits_ManageOrganizationTree, L("ManagingOrganizationTree"));
            organizationUnits.CreateChildPermission(AppPermissions.Pages_Administration_OrganizationUnits_ManageMembers, L("ManagingMembers"));
            organizationUnits.CreateChildPermission(AppPermissions.Pages_Administration_OrganizationUnits_ManageRoles, L("ManagingRoles"));

            administration.CreateChildPermission(AppPermissions.Pages_Administration_UiCustomization, L("VisualSettings"));

            var webhooks = administration.CreateChildPermission(AppPermissions.Pages_Administration_WebhookSubscription, L("Webhooks"));
            webhooks.CreateChildPermission(AppPermissions.Pages_Administration_WebhookSubscription_Create, L("CreatingWebhooks"));
            webhooks.CreateChildPermission(AppPermissions.Pages_Administration_WebhookSubscription_Edit, L("EditingWebhooks"));
            webhooks.CreateChildPermission(AppPermissions.Pages_Administration_WebhookSubscription_ChangeActivity, L("ChangingWebhookActivity"));
            webhooks.CreateChildPermission(AppPermissions.Pages_Administration_WebhookSubscription_Detail, L("DetailingSubscription"));
            webhooks.CreateChildPermission(AppPermissions.Pages_Administration_Webhook_ListSendAttempts, L("ListingSendAttempts"));
            webhooks.CreateChildPermission(AppPermissions.Pages_Administration_Webhook_ResendWebhook, L("ResendingWebhook"));

            var dynamicProperties = administration.CreateChildPermission(AppPermissions.Pages_Administration_DynamicProperties, L("DynamicProperties"));
            dynamicProperties.CreateChildPermission(AppPermissions.Pages_Administration_DynamicProperties_Create, L("CreatingDynamicProperties"));
            dynamicProperties.CreateChildPermission(AppPermissions.Pages_Administration_DynamicProperties_Edit, L("EditingDynamicProperties"));
            dynamicProperties.CreateChildPermission(AppPermissions.Pages_Administration_DynamicProperties_Delete, L("DeletingDynamicProperties"));

            var dynamicPropertyValues = dynamicProperties.CreateChildPermission(AppPermissions.Pages_Administration_DynamicPropertyValue, L("DynamicPropertyValue"));
            dynamicPropertyValues.CreateChildPermission(AppPermissions.Pages_Administration_DynamicPropertyValue_Create, L("CreatingDynamicPropertyValue"));
            dynamicPropertyValues.CreateChildPermission(AppPermissions.Pages_Administration_DynamicPropertyValue_Edit, L("EditingDynamicPropertyValue"));
            dynamicPropertyValues.CreateChildPermission(AppPermissions.Pages_Administration_DynamicPropertyValue_Delete, L("DeletingDynamicPropertyValue"));

            var dynamicEntityProperties = dynamicProperties.CreateChildPermission(AppPermissions.Pages_Administration_DynamicEntityProperties, L("DynamicEntityProperties"));
            dynamicEntityProperties.CreateChildPermission(AppPermissions.Pages_Administration_DynamicEntityProperties_Create, L("CreatingDynamicEntityProperties"));
            dynamicEntityProperties.CreateChildPermission(AppPermissions.Pages_Administration_DynamicEntityProperties_Edit, L("EditingDynamicEntityProperties"));
            dynamicEntityProperties.CreateChildPermission(AppPermissions.Pages_Administration_DynamicEntityProperties_Delete, L("DeletingDynamicEntityProperties"));

            var dynamicEntityPropertyValues = dynamicProperties.CreateChildPermission(AppPermissions.Pages_Administration_DynamicEntityPropertyValue, L("EntityDynamicPropertyValue"));
            dynamicEntityPropertyValues.CreateChildPermission(AppPermissions.Pages_Administration_DynamicEntityPropertyValue_Create, L("CreatingDynamicEntityPropertyValue"));
            dynamicEntityPropertyValues.CreateChildPermission(AppPermissions.Pages_Administration_DynamicEntityPropertyValue_Edit, L("EditingDynamicEntityPropertyValue"));
            dynamicEntityPropertyValues.CreateChildPermission(AppPermissions.Pages_Administration_DynamicEntityPropertyValue_Delete, L("DeletingDynamicEntityPropertyValue"));

            var massNotification = administration.CreateChildPermission(AppPermissions.Pages_Administration_MassNotification, L("MassNotifications"));
            massNotification.CreateChildPermission(AppPermissions.Pages_Administration_MassNotification_Create, L("MassNotificationCreate"));

            //TENANT-SPECIFIC PERMISSIONS

            pages.CreateChildPermission(AppPermissions.Pages_Tenant_Dashboard, L("Dashboard"), multiTenancySides: MultiTenancySides.Tenant);

            administration.CreateChildPermission(AppPermissions.Pages_Administration_Tenant_Settings, L("Settings"), multiTenancySides: MultiTenancySides.Tenant);
            administration.CreateChildPermission(AppPermissions.Pages_Administration_Tenant_SubscriptionManagement, L("Subscription"), multiTenancySides: MultiTenancySides.Tenant);

            //HOST-SPECIFIC PERMISSIONS

            var editions = pages.CreateChildPermission(AppPermissions.Pages_Editions, L("Editions"), multiTenancySides: MultiTenancySides.Host);
            editions.CreateChildPermission(AppPermissions.Pages_Editions_Create, L("CreatingNewEdition"), multiTenancySides: MultiTenancySides.Host);
            editions.CreateChildPermission(AppPermissions.Pages_Editions_Edit, L("EditingEdition"), multiTenancySides: MultiTenancySides.Host);
            editions.CreateChildPermission(AppPermissions.Pages_Editions_Delete, L("DeletingEdition"), multiTenancySides: MultiTenancySides.Host);
            editions.CreateChildPermission(AppPermissions.Pages_Editions_MoveTenantsToAnotherEdition, L("MoveTenantsToAnotherEdition"), multiTenancySides: MultiTenancySides.Host);

            var tenants = pages.CreateChildPermission(AppPermissions.Pages_Tenants, L("Tenants"), multiTenancySides: MultiTenancySides.Host);
            tenants.CreateChildPermission(AppPermissions.Pages_Tenants_Create, L("CreatingNewTenant"), multiTenancySides: MultiTenancySides.Host);
            tenants.CreateChildPermission(AppPermissions.Pages_Tenants_Edit, L("EditingTenant"), multiTenancySides: MultiTenancySides.Host);
            tenants.CreateChildPermission(AppPermissions.Pages_Tenants_ChangeFeatures, L("ChangingFeatures"), multiTenancySides: MultiTenancySides.Host);
            tenants.CreateChildPermission(AppPermissions.Pages_Tenants_Delete, L("DeletingTenant"), multiTenancySides: MultiTenancySides.Host);
            tenants.CreateChildPermission(AppPermissions.Pages_Tenants_Impersonation, L("LoginForTenants"), multiTenancySides: MultiTenancySides.Host);

            administration.CreateChildPermission(AppPermissions.Pages_Administration_Host_Settings, L("Settings"), multiTenancySides: MultiTenancySides.Host);

            var maintenance = administration.CreateChildPermission(AppPermissions.Pages_Administration_Host_Maintenance, L("Maintenance"), multiTenancySides: _isMultiTenancyEnabled ? MultiTenancySides.Host : MultiTenancySides.Tenant);
            maintenance.CreateChildPermission(AppPermissions.Pages_Administration_NewVersion_Create, L("SendNewVersionNotification"));

            administration.CreateChildPermission(AppPermissions.Pages_Administration_HangfireDashboard, L("HangfireDashboard"), multiTenancySides: _isMultiTenancyEnabled ? MultiTenancySides.Host : MultiTenancySides.Tenant);
            administration.CreateChildPermission(AppPermissions.Pages_Administration_Host_Dashboard, L("Dashboard"), multiTenancySides: MultiTenancySides.Host);
        }

        private static ILocalizableString L(string name)
        {
            return new LocalizableString(name, DeedConsts.LocalizationSourceName);
        }
    }
}