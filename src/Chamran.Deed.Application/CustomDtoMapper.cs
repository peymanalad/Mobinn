using Chamran.Deed.Common.Dtos;
using Chamran.Deed.Common;
using Chamran.Deed.Info.Dtos;
using Chamran.Deed.Info;
using Chamran.Deed.People.Dtos;
using Chamran.Deed.People;
using Abp.Application.Editions;
using Abp.Application.Features;
using Abp.Auditing;
using Abp.Authorization;
using Abp.Authorization.Users;
using Abp.DynamicEntityProperties;
using Abp.EntityHistory;
using Abp.Localization;
using Abp.Notifications;
using Abp.Organizations;
using Abp.UI.Inputs;
using Abp.Webhooks;
using AutoMapper;
using IdentityServer4.Extensions;
using Chamran.Deed.Auditing.Dto;
using Chamran.Deed.Authorization.Accounts.Dto;
using Chamran.Deed.Authorization.Delegation;
using Chamran.Deed.Authorization.Permissions.Dto;
using Chamran.Deed.Authorization.Roles;
using Chamran.Deed.Authorization.Roles.Dto;
using Chamran.Deed.Authorization.Users;
using Chamran.Deed.Authorization.Users.Delegation.Dto;
using Chamran.Deed.Authorization.Users.Dto;
using Chamran.Deed.Authorization.Users.Importing.Dto;
using Chamran.Deed.Authorization.Users.Profile.Dto;
using Chamran.Deed.Chat;
using Chamran.Deed.Chat.Dto;
using Chamran.Deed.DynamicEntityProperties.Dto;
using Chamran.Deed.Editions;
using Chamran.Deed.Editions.Dto;
using Chamran.Deed.Friendships;
using Chamran.Deed.Friendships.Cache;
using Chamran.Deed.Friendships.Dto;
using Chamran.Deed.Localization.Dto;
using Chamran.Deed.MultiTenancy;
using Chamran.Deed.MultiTenancy.Dto;
using Chamran.Deed.MultiTenancy.HostDashboard.Dto;
using Chamran.Deed.MultiTenancy.Payments;
using Chamran.Deed.MultiTenancy.Payments.Dto;
using Chamran.Deed.Notifications.Dto;
using Chamran.Deed.Organizations.Dto;
using Chamran.Deed.Sessions.Dto;
using Chamran.Deed.WebHooks.Dto;

namespace Chamran.Deed
{
    internal static class CustomDtoMapper
    {
        public static void CreateMappings(IMapperConfigurationExpression configuration)
        {
            configuration.CreateMap<CreateOrEditOrganizationUserDto, OrganizationUser>().ReverseMap();
            configuration.CreateMap<OrganizationUserDto, OrganizationUser>().ReverseMap();
            configuration.CreateMap<CreateOrEditOrganizationChartDto, OrganizationChart>().ReverseMap();
            configuration.CreateMap<OrganizationChartDto, OrganizationChart>().ReverseMap();
            configuration.CreateMap<CreateOrEditUserPostGroupDto, UserPostGroup>().ReverseMap();
            configuration.CreateMap<UserPostGroupDto, UserPostGroup>().ReverseMap();
            configuration.CreateMap<CreateOrEditUserLocationDto, UserLocation>().ReverseMap();
            configuration.CreateMap<UserLocationDto, UserLocation>().ReverseMap();
            configuration.CreateMap<CreateOrEditUserTokenDto, Common.UserToken>().ReverseMap();
            configuration.CreateMap<UserTokenDto, Common.UserToken>().ReverseMap();
            configuration.CreateMap<CreateOrEditFCMQueueDto, FCMQueue>().ReverseMap();
            configuration.CreateMap<FCMQueueDto, FCMQueue>().ReverseMap();
            configuration.CreateMap<CreateOrEditReportDto, Report>().ReverseMap();
            configuration.CreateMap<ReportDto, Report>().ReverseMap();
            configuration.CreateMap<CreateOrEditCommentLikeDto, CommentLike>().ReverseMap();
            configuration.CreateMap<CommentLikeDto, CommentLike>().ReverseMap();
            configuration.CreateMap<CreateOrEditPostLikeDto, PostLike>().ReverseMap();
            configuration.CreateMap<PostLikeDto, PostLike>().ReverseMap();
            configuration.CreateMap<CreateOrEditSoftwareUpdateDto, SoftwareUpdate>().ReverseMap();
            configuration.CreateMap<SoftwareUpdateDto, SoftwareUpdate>().ReverseMap();
            configuration.CreateMap<CreateOrEditCommentDto, Comment>().ReverseMap();
            configuration.CreateMap<CommentDto, Comment>().ReverseMap();
            configuration.CreateMap<CreateCommentDto, Comment>().ReverseMap();
            configuration.CreateMap<CreateOrEditSeenDto, Seen>().ReverseMap();
            configuration.CreateMap<SeenDto, Seen>().ReverseMap();
            configuration.CreateMap<CreateOrEditHashtagDto, Hashtag>().ReverseMap();
            configuration.CreateMap<HashtagDto, Hashtag>().ReverseMap();
            configuration.CreateMap<CreateOrEditPostGroupDto, PostGroup>().ReverseMap();
            configuration.CreateMap<PostGroupDto, PostGroup>().ReverseMap();
            configuration.CreateMap<CreateOrEditPostDto, Post>().ReverseMap();
            configuration.CreateMap<PostDto, Post>().ReverseMap();
            configuration.CreateMap<CreateOrEditGroupMemberDto, GroupMember>().ReverseMap();
            configuration.CreateMap<GroupMemberDto, GroupMember>().ReverseMap();
            //configuration.CreateMap<CreateOrEditOrganizationGroupDto, OrganizationGroup>().ReverseMap();
            //configuration.CreateMap<OrganizationGroupDto, OrganizationGroup>().ReverseMap();
            configuration.CreateMap<CreateOrEditOrganizationDto, Organization>().ReverseMap();
            configuration.CreateMap<OrganizationDto, Organization>().ReverseMap();
            //Inputs
            configuration.CreateMap<CheckboxInputType, FeatureInputTypeDto>();
            configuration.CreateMap<SingleLineStringInputType, FeatureInputTypeDto>();
            configuration.CreateMap<ComboboxInputType, FeatureInputTypeDto>();
            configuration.CreateMap<IInputType, FeatureInputTypeDto>()
                .Include<CheckboxInputType, FeatureInputTypeDto>()
                .Include<SingleLineStringInputType, FeatureInputTypeDto>()
                .Include<ComboboxInputType, FeatureInputTypeDto>();
            configuration.CreateMap<StaticLocalizableComboboxItemSource, LocalizableComboboxItemSourceDto>();
            configuration.CreateMap<ILocalizableComboboxItemSource, LocalizableComboboxItemSourceDto>()
                .Include<StaticLocalizableComboboxItemSource, LocalizableComboboxItemSourceDto>();
            configuration.CreateMap<LocalizableComboboxItem, LocalizableComboboxItemDto>();
            configuration.CreateMap<ILocalizableComboboxItem, LocalizableComboboxItemDto>()
                .Include<LocalizableComboboxItem, LocalizableComboboxItemDto>();

            //Chat
            configuration.CreateMap<ChatMessage, ChatMessageDto>();
            configuration.CreateMap<ChatMessage, ChatMessageExportDto>();

            //Feature
            configuration.CreateMap<FlatFeatureSelectDto, Feature>().ReverseMap();
            configuration.CreateMap<Feature, FlatFeatureDto>();

            //Role
            configuration.CreateMap<RoleEditDto, Role>().ReverseMap();
            configuration.CreateMap<Role, RoleListDto>();
            configuration.CreateMap<UserRole, UserListRoleDto>();

            //Edition
            configuration.CreateMap<EditionEditDto, SubscribableEdition>().ReverseMap();
            configuration.CreateMap<EditionCreateDto, SubscribableEdition>();
            configuration.CreateMap<EditionSelectDto, SubscribableEdition>().ReverseMap();
            configuration.CreateMap<SubscribableEdition, EditionInfoDto>();

            configuration.CreateMap<Edition, EditionInfoDto>().Include<SubscribableEdition, EditionInfoDto>();

            configuration.CreateMap<SubscribableEdition, EditionListDto>();
            configuration.CreateMap<Edition, EditionEditDto>();
            configuration.CreateMap<Edition, SubscribableEdition>();
            configuration.CreateMap<Edition, EditionSelectDto>();

            //Payment
            configuration.CreateMap<SubscriptionPaymentDto, SubscriptionPayment>().ReverseMap();
            configuration.CreateMap<SubscriptionPaymentListDto, SubscriptionPayment>().ReverseMap();
            configuration.CreateMap<SubscriptionPayment, SubscriptionPaymentInfoDto>();

            //Permission
            configuration.CreateMap<Permission, FlatPermissionDto>();
            configuration.CreateMap<Permission, FlatPermissionWithLevelDto>();

            //Language
            configuration.CreateMap<ApplicationLanguage, ApplicationLanguageEditDto>();
            configuration.CreateMap<ApplicationLanguage, ApplicationLanguageListDto>();
            configuration.CreateMap<NotificationDefinition, NotificationSubscriptionWithDisplayNameDto>();
            configuration.CreateMap<ApplicationLanguage, ApplicationLanguageEditDto>()
                .ForMember(ldto => ldto.IsEnabled, options => options.MapFrom(l => !l.IsDisabled));

            //Tenant
            configuration.CreateMap<Tenant, RecentTenant>();
            configuration.CreateMap<Tenant, TenantLoginInfoDto>();
            configuration.CreateMap<Tenant, TenantListDto>();
            configuration.CreateMap<TenantEditDto, Tenant>().ReverseMap();
            configuration.CreateMap<CurrentTenantInfoDto, Tenant>().ReverseMap();

            //User
            configuration.CreateMap<User, UserEditDto>()
                .ForMember(dto => dto.Password, options => options.Ignore())
                .ReverseMap()
                .ForMember(user => user.Password, options => options.Ignore());
            configuration.CreateMap<User, UserLoginInfoDto>();
            configuration.CreateMap<User, UserListDto>();
            configuration.CreateMap<User, ChatUserDto>();
            configuration.CreateMap<User, OrganizationUnitUserListDto>();
            configuration.CreateMap<Role, OrganizationUnitRoleListDto>();
            configuration.CreateMap<CurrentUserProfileEditDto, User>().ReverseMap();
            configuration.CreateMap<UserLoginAttemptDto, UserLoginAttempt>().ReverseMap();
            configuration.CreateMap<ImportUserDto, User>();

            //AuditLog
            configuration.CreateMap<AuditLog, AuditLogListDto>();
            configuration.CreateMap<EntityChange, EntityChangeListDto>();
            configuration.CreateMap<EntityPropertyChange, EntityPropertyChangeDto>();

            //Friendship
            configuration.CreateMap<Friendship, FriendDto>();
            configuration.CreateMap<FriendCacheItem, FriendDto>();

            //OrganizationUnit
            configuration.CreateMap<OrganizationUnit, OrganizationUnitDto>();

            //Webhooks
            configuration.CreateMap<WebhookSubscription, GetAllSubscriptionsOutput>();
            configuration.CreateMap<WebhookSendAttempt, GetAllSendAttemptsOutput>()
                .ForMember(webhookSendAttemptListDto => webhookSendAttemptListDto.WebhookName,
                    options => options.MapFrom(l => l.WebhookEvent.WebhookName))
                .ForMember(webhookSendAttemptListDto => webhookSendAttemptListDto.Data,
                    options => options.MapFrom(l => l.WebhookEvent.Data));

            configuration.CreateMap<WebhookSendAttempt, GetAllSendAttemptsOfWebhookEventOutput>();

            configuration.CreateMap<DynamicProperty, DynamicPropertyDto>().ReverseMap();
            configuration.CreateMap<DynamicPropertyValue, DynamicPropertyValueDto>().ReverseMap();
            configuration.CreateMap<DynamicEntityProperty, DynamicEntityPropertyDto>()
                .ForMember(dto => dto.DynamicPropertyName,
                    options => options.MapFrom(entity => entity.DynamicProperty.DisplayName.IsNullOrEmpty() ? entity.DynamicProperty.PropertyName : entity.DynamicProperty.DisplayName));
            configuration.CreateMap<DynamicEntityPropertyDto, DynamicEntityProperty>();

            configuration.CreateMap<DynamicEntityPropertyValue, DynamicEntityPropertyValueDto>().ReverseMap();

            //User Delegations
            configuration.CreateMap<CreateUserDelegationDto, UserDelegation>();

            /* ADD YOUR OWN CUSTOM AUTOMAPPER MAPPINGS HERE */
        }
    }
}