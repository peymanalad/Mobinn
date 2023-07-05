namespace Chamran.Deed.Authorization
{
    /// <summary>
    /// Defines string constants for application's permission names.
    /// <see cref="AppAuthorizationProvider"/> for permission definitions.
    /// </summary>
    public static class AppPermissions
    {
        public const string Pages_Reports = "Pages.Reports";
        public const string Pages_Reports_Create = "Pages.Reports.Create";
        public const string Pages_Reports_Edit = "Pages.Reports.Edit";
        public const string Pages_Reports_Delete = "Pages.Reports.Delete";

        public const string Pages_CommentLikes = "Pages.CommentLikes";
        public const string Pages_CommentLikes_Create = "Pages.CommentLikes.Create";
        public const string Pages_CommentLikes_Edit = "Pages.CommentLikes.Edit";
        public const string Pages_CommentLikes_Delete = "Pages.CommentLikes.Delete";

        public const string Pages_PostLikes = "Pages.PostLikes";
        public const string Pages_PostLikes_Create = "Pages.PostLikes.Create";
        public const string Pages_PostLikes_Edit = "Pages.PostLikes.Edit";
        public const string Pages_PostLikes_Delete = "Pages.PostLikes.Delete";

        public const string Pages_SoftwareUpdates = "Pages.SoftwareUpdates";
        public const string Pages_SoftwareUpdates_Create = "Pages.SoftwareUpdates.Create";
        public const string Pages_SoftwareUpdates_Edit = "Pages.SoftwareUpdates.Edit";
        public const string Pages_SoftwareUpdates_Delete = "Pages.SoftwareUpdates.Delete";

        public const string Pages_Comments = "Pages.Comments";
        public const string Pages_Comments_Create = "Pages.Comments.Create";
        public const string Pages_Comments_Edit = "Pages.Comments.Edit";
        public const string Pages_Comments_Delete = "Pages.Comments.Delete";

        public const string Pages_Seens = "Pages.Seens";
        public const string Pages_Seens_Create = "Pages.Seens.Create";
        public const string Pages_Seens_Edit = "Pages.Seens.Edit";
        public const string Pages_Seens_Delete = "Pages.Seens.Delete";

        public const string Pages_Administration_Hashtags = "Pages.Administration.Hashtags";
        public const string Pages_Administration_Hashtags_Create = "Pages.Administration.Hashtags.Create";
        public const string Pages_Administration_Hashtags_Edit = "Pages.Administration.Hashtags.Edit";
        public const string Pages_Administration_Hashtags_Delete = "Pages.Administration.Hashtags.Delete";

        public const string Pages_PostGroups = "Pages.PostGroups";
        public const string Pages_PostGroups_Create = "Pages.PostGroups.Create";
        public const string Pages_PostGroups_Edit = "Pages.PostGroups.Edit";
        public const string Pages_PostGroups_Delete = "Pages.PostGroups.Delete";

        public const string Pages_Posts = "Pages.Posts";
        public const string Pages_Posts_Create = "Pages.Posts.Create";
        public const string Pages_Posts_Edit = "Pages.Posts.Edit";
        public const string Pages_Posts_Delete = "Pages.Posts.Delete";

        public const string Pages_GroupMembers = "Pages.GroupMembers";
        public const string Pages_GroupMembers_Create = "Pages.GroupMembers.Create";
        public const string Pages_GroupMembers_Edit = "Pages.GroupMembers.Edit";
        public const string Pages_GroupMembers_Delete = "Pages.GroupMembers.Delete";

        public const string Pages_OrganizationGroups = "Pages.OrganizationGroups";
        public const string Pages_OrganizationGroups_Create = "Pages.OrganizationGroups.Create";
        public const string Pages_OrganizationGroups_Edit = "Pages.OrganizationGroups.Edit";
        public const string Pages_OrganizationGroups_Delete = "Pages.OrganizationGroups.Delete";

        public const string Pages_Organizations = "Pages.Organizations";
        public const string Pages_Organizations_Create = "Pages.Organizations.Create";
        public const string Pages_Organizations_Edit = "Pages.Organizations.Edit";
        public const string Pages_Organizations_Delete = "Pages.Organizations.Delete";

        //COMMON PERMISSIONS (FOR BOTH OF TENANTS AND HOST)

        public const string Pages = "Pages";

        public const string Pages_DemoUiComponents = "Pages.DemoUiComponents";
        public const string Pages_Administration = "Pages.Administration";

        public const string Pages_Administration_Roles = "Pages.Administration.Roles";
        public const string Pages_Administration_Roles_Create = "Pages.Administration.Roles.Create";
        public const string Pages_Administration_Roles_Edit = "Pages.Administration.Roles.Edit";
        public const string Pages_Administration_Roles_Delete = "Pages.Administration.Roles.Delete";

        public const string Pages_Administration_Users = "Pages.Administration.Users";
        public const string Pages_Administration_Users_Create = "Pages.Administration.Users.Create";
        public const string Pages_Administration_Users_Edit = "Pages.Administration.Users.Edit";
        public const string Pages_Administration_Users_Delete = "Pages.Administration.Users.Delete";
        public const string Pages_Administration_Users_ChangePermissions = "Pages.Administration.Users.ChangePermissions";
        public const string Pages_Administration_Users_Impersonation = "Pages.Administration.Users.Impersonation";
        public const string Pages_Administration_Users_Unlock = "Pages.Administration.Users.Unlock";
        public const string Pages_Administration_Users_ChangeProfilePicture = "Pages.Administration.Users.ChangeProfilePicture";

        public const string Pages_Administration_Languages = "Pages.Administration.Languages";
        public const string Pages_Administration_Languages_Create = "Pages.Administration.Languages.Create";
        public const string Pages_Administration_Languages_Edit = "Pages.Administration.Languages.Edit";
        public const string Pages_Administration_Languages_Delete = "Pages.Administration.Languages.Delete";
        public const string Pages_Administration_Languages_ChangeTexts = "Pages.Administration.Languages.ChangeTexts";
        public const string Pages_Administration_Languages_ChangeDefaultLanguage = "Pages.Administration.Languages.ChangeDefaultLanguage";

        public const string Pages_Administration_AuditLogs = "Pages.Administration.AuditLogs";

        public const string Pages_Administration_OrganizationUnits = "Pages.Administration.OrganizationUnits";
        public const string Pages_Administration_OrganizationUnits_ManageOrganizationTree = "Pages.Administration.OrganizationUnits.ManageOrganizationTree";
        public const string Pages_Administration_OrganizationUnits_ManageMembers = "Pages.Administration.OrganizationUnits.ManageMembers";
        public const string Pages_Administration_OrganizationUnits_ManageRoles = "Pages.Administration.OrganizationUnits.ManageRoles";

        public const string Pages_Administration_HangfireDashboard = "Pages.Administration.HangfireDashboard";

        public const string Pages_Administration_UiCustomization = "Pages.Administration.UiCustomization";

        public const string Pages_Administration_WebhookSubscription = "Pages.Administration.WebhookSubscription";
        public const string Pages_Administration_WebhookSubscription_Create = "Pages.Administration.WebhookSubscription.Create";
        public const string Pages_Administration_WebhookSubscription_Edit = "Pages.Administration.WebhookSubscription.Edit";
        public const string Pages_Administration_WebhookSubscription_ChangeActivity = "Pages.Administration.WebhookSubscription.ChangeActivity";
        public const string Pages_Administration_WebhookSubscription_Detail = "Pages.Administration.WebhookSubscription.Detail";
        public const string Pages_Administration_Webhook_ListSendAttempts = "Pages.Administration.Webhook.ListSendAttempts";
        public const string Pages_Administration_Webhook_ResendWebhook = "Pages.Administration.Webhook.ResendWebhook";

        public const string Pages_Administration_DynamicProperties = "Pages.Administration.DynamicProperties";
        public const string Pages_Administration_DynamicProperties_Create = "Pages.Administration.DynamicProperties.Create";
        public const string Pages_Administration_DynamicProperties_Edit = "Pages.Administration.DynamicProperties.Edit";
        public const string Pages_Administration_DynamicProperties_Delete = "Pages.Administration.DynamicProperties.Delete";

        public const string Pages_Administration_DynamicPropertyValue = "Pages.Administration.DynamicPropertyValue";
        public const string Pages_Administration_DynamicPropertyValue_Create = "Pages.Administration.DynamicPropertyValue.Create";
        public const string Pages_Administration_DynamicPropertyValue_Edit = "Pages.Administration.DynamicPropertyValue.Edit";
        public const string Pages_Administration_DynamicPropertyValue_Delete = "Pages.Administration.DynamicPropertyValue.Delete";

        public const string Pages_Administration_DynamicEntityProperties = "Pages.Administration.DynamicEntityProperties";
        public const string Pages_Administration_DynamicEntityProperties_Create = "Pages.Administration.DynamicEntityProperties.Create";
        public const string Pages_Administration_DynamicEntityProperties_Edit = "Pages.Administration.DynamicEntityProperties.Edit";
        public const string Pages_Administration_DynamicEntityProperties_Delete = "Pages.Administration.DynamicEntityProperties.Delete";

        public const string Pages_Administration_DynamicEntityPropertyValue = "Pages.Administration.DynamicEntityPropertyValue";
        public const string Pages_Administration_DynamicEntityPropertyValue_Create = "Pages.Administration.DynamicEntityPropertyValue.Create";
        public const string Pages_Administration_DynamicEntityPropertyValue_Edit = "Pages.Administration.DynamicEntityPropertyValue.Edit";
        public const string Pages_Administration_DynamicEntityPropertyValue_Delete = "Pages.Administration.DynamicEntityPropertyValue.Delete";

        public const string Pages_Administration_MassNotification = "Pages.Administration.MassNotification";
        public const string Pages_Administration_MassNotification_Create = "Pages.Administration.MassNotification.Create";

        public const string Pages_Administration_NewVersion_Create = "Pages_Administration_NewVersion_Create";

        //TENANT-SPECIFIC PERMISSIONS

        public const string Pages_Tenant_Dashboard = "Pages.Tenant.Dashboard";

        public const string Pages_Administration_Tenant_Settings = "Pages.Administration.Tenant.Settings";

        public const string Pages_Administration_Tenant_SubscriptionManagement = "Pages.Administration.Tenant.SubscriptionManagement";

        //HOST-SPECIFIC PERMISSIONS

        public const string Pages_Editions = "Pages.Editions";
        public const string Pages_Editions_Create = "Pages.Editions.Create";
        public const string Pages_Editions_Edit = "Pages.Editions.Edit";
        public const string Pages_Editions_Delete = "Pages.Editions.Delete";
        public const string Pages_Editions_MoveTenantsToAnotherEdition = "Pages.Editions.MoveTenantsToAnotherEdition";

        public const string Pages_Tenants = "Pages.Tenants";
        public const string Pages_Tenants_Create = "Pages.Tenants.Create";
        public const string Pages_Tenants_Edit = "Pages.Tenants.Edit";
        public const string Pages_Tenants_ChangeFeatures = "Pages.Tenants.ChangeFeatures";
        public const string Pages_Tenants_Delete = "Pages.Tenants.Delete";
        public const string Pages_Tenants_Impersonation = "Pages.Tenants.Impersonation";

        public const string Pages_Administration_Host_Maintenance = "Pages.Administration.Host.Maintenance";
        public const string Pages_Administration_Host_Settings = "Pages.Administration.Host.Settings";
        public const string Pages_Administration_Host_Dashboard = "Pages.Administration.Host.Dashboard";
    }
}