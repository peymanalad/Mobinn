﻿using System;
using System.Threading.Tasks;
using Abp;
using Abp.Auditing;
using Abp.Authorization;
using Abp.BackgroundJobs;
using Abp.Configuration;
using Abp.Extensions;
using Abp.Localization;
using Abp.Runtime.Caching;
using Abp.Runtime.Session;
using Abp.Timing;
using Abp.UI;
using Abp.Zero.Configuration;
using Microsoft.AspNetCore.Identity;
using Chamran.Deed.Authentication.TwoFactor.Google;
using Chamran.Deed.Authorization.Users.Dto;
using Chamran.Deed.Authorization.Users.Profile.Cache;
using Chamran.Deed.Authorization.Users.Profile.Dto;
using Chamran.Deed.Common;
using Chamran.Deed.Configuration;
using Chamran.Deed.Friendships;
using Chamran.Deed.Gdpr;
using Chamran.Deed.Net.Sms;
using Chamran.Deed.Security;
using Chamran.Deed.Storage;
using Chamran.Deed.Timing;
using Abp.Domain.Repositories;
using Chamran.Deed.UiCustomization.Dto;
using System.Collections.Generic;
using System.Linq;
using Chamran.Deed.People;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;



namespace Chamran.Deed.Authorization.Users.Profile
{
    [AbpAuthorize]
    public class ProfileAppService : DeedAppServiceBase, IProfileAppService
    {
        private const int MaxProfilePictureBytes = 5242880; //5MB
        private readonly IBinaryObjectManager _binaryObjectManager;
        private readonly ITimeZoneService _timeZoneService;
        private readonly IFriendshipManager _friendshipManager;
        private readonly GoogleTwoFactorAuthenticateService _googleTwoFactorAuthenticateService;
        private readonly ISmsSender _smsSender;
        private readonly ICacheManager _cacheManager;
        private readonly ITempFileCacheManager _tempFileCacheManager;
        private readonly IBackgroundJobManager _backgroundJobManager;
        private readonly ProfileImageServiceFactory _profileImageServiceFactory;
        private readonly IUserAppService _userAppService;
        private readonly IRepository<User, long> _userRepository;
        private readonly IRepository<GroupMember> _groupMemberRepository;

        public ProfileAppService(
            IBinaryObjectManager binaryObjectManager,
            ITimeZoneService timezoneService,
            IFriendshipManager friendshipManager,
            GoogleTwoFactorAuthenticateService googleTwoFactorAuthenticateService,
            ISmsSender smsSender,
            ICacheManager cacheManager,
            ITempFileCacheManager tempFileCacheManager,
            IBackgroundJobManager backgroundJobManager,
            ProfileImageServiceFactory profileImageServiceFactory, IUserAppService userAppService, IRepository<User, long> userRepository, IRepository<GroupMember> groupMemberRepository)
        {
            _binaryObjectManager = binaryObjectManager;
            _timeZoneService = timezoneService;
            _friendshipManager = friendshipManager;
            _googleTwoFactorAuthenticateService = googleTwoFactorAuthenticateService;
            _smsSender = smsSender;
            _cacheManager = cacheManager;
            _tempFileCacheManager = tempFileCacheManager;
            _backgroundJobManager = backgroundJobManager;
            _profileImageServiceFactory = profileImageServiceFactory;
            _userAppService = userAppService;
            _userRepository = userRepository;
            _groupMemberRepository = groupMemberRepository;
        }

        [DisableAuditing]
        public async Task<CurrentUserProfileEditDto> GetCurrentUserProfileForEdit()
        {
            var user = await GetCurrentUserAsync();
            var userProfileEditDto = ObjectMapper.Map<CurrentUserProfileEditDto>(user);

            userProfileEditDto.QrCodeSetupImageUrl = user.GoogleAuthenticatorKey != null
                ? _googleTwoFactorAuthenticateService.GenerateSetupCode("Chamran.Deed",
                    user.EmailAddress, user.GoogleAuthenticatorKey, 300, 300).QrCodeSetupImageUrl
                : "";
            userProfileEditDto.IsGoogleAuthenticatorEnabled = user.GoogleAuthenticatorKey != null;

            if (!Clock.SupportsMultipleTimezone)
            {
                return userProfileEditDto;
            }

            userProfileEditDto.Timezone = await SettingManager.GetSettingValueAsync(TimingSettingNames.TimeZone);

            var defaultTimeZoneId = await _timeZoneService.GetDefaultTimezoneAsync(
                SettingScopes.User,
                AbpSession.TenantId
            );

            if (userProfileEditDto.Timezone == defaultTimeZoneId)
            {
                userProfileEditDto.Timezone = string.Empty;
            }
            userProfileEditDto.JoinedOrganizations = new List<CurrentOrganizationDto>();
            var query = _groupMemberRepository.GetAll().Include(x => x.UserFk).Include(x => x.OrganizationFk).Where(x => x.UserId == AbpSession.UserId.Value);

            foreach (var groupMember in query)
            {
                if (groupMember.OrganizationFk?.OrganizationName != "")
                    userProfileEditDto.JoinedOrganizations.Add(new CurrentOrganizationDto()
                    {
                        OrganizationId = groupMember.OrganizationId,
                        OrganizationName = groupMember.OrganizationFk?.OrganizationName ?? "",
                        OrganizationPicture = groupMember.OrganizationFk?.OrganizationLogo
                    });
            }
            return userProfileEditDto;
        }

        public async Task DisableGoogleAuthenticator(VerifyAuthenticatorCodeInput input)
        {
            var result = await VerifyAuthenticatorCode(input);

            if (!result)
            {
                throw new UserFriendlyException(L("InvalidVerificationCode"));
            }

            var user = await GetCurrentUserAsync();

            user.GoogleAuthenticatorKey = null;
            user.RecoveryCode = null;
        }

        public async Task<UpdateGoogleAuthenticatorKeyOutput> ViewRecoveryCodes(VerifyAuthenticatorCodeInput input)
        {
            var verified = await VerifyAuthenticatorCodeInternal(input);

            if (!verified)
            {
                throw new UserFriendlyException(L("InvalidVerificationCode"));
            }

            var user = await GetCurrentUserAsync();

            var mergedCodes = user.RecoveryCode ?? "";
            var splitCodes = mergedCodes.Split(';');

            return new UpdateGoogleAuthenticatorKeyOutput
            {
                RecoveryCodes = splitCodes
            };
        }

        public async Task<GenerateGoogleAuthenticatorKeyOutput> GenerateGoogleAuthenticatorKey()
        {
            var user = await GetCurrentUserAsync();
            var googleAuthenticatorKey = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 10);

            return new GenerateGoogleAuthenticatorKeyOutput
            {
                GoogleAuthenticatorKey = googleAuthenticatorKey,
                QrCodeSetupImageUrl = _googleTwoFactorAuthenticateService.GenerateSetupCode(
                    "Chamran.Deed",
                    user.EmailAddress, googleAuthenticatorKey, 195, 195).QrCodeSetupImageUrl
            };
        }

        public async Task<UpdateGoogleAuthenticatorKeyOutput> UpdateGoogleAuthenticatorKey(
            UpdateGoogleAuthenticatorKeyInput input)
        {
            var verified = await VerifyAuthenticatorCodeInternal(new VerifyAuthenticatorCodeInput
            {
                Code = input.AuthenticatorCode,
                GoogleAuthenticatorKey = input.GoogleAuthenticatorKey
            });

            if (!verified)
            {
                throw new UserFriendlyException(L("InvalidVerificationCode"));
            }

            var user = await GetCurrentUserAsync();
            user.GoogleAuthenticatorKey = input.GoogleAuthenticatorKey;

            var recoveryCodes = await UserManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);

            CheckErrors(await UserManager.UpdateAsync(user));

            return new UpdateGoogleAuthenticatorKeyOutput
            {
                RecoveryCodes = recoveryCodes
            };
        }

        public async Task SendVerificationSms(SendVerificationSmsInputDto input)
        {
            var code = RandomHelper.GetRandom(100000, 999999).ToString();
            var cacheKey = AbpSession.ToUserIdentifier().ToString();
            var cacheItem = new SmsVerificationCodeCacheItem
            {
                Code = code
            };

            await _cacheManager.GetSmsVerificationCodeCache().SetAsync(
                cacheKey,
                cacheItem
            );

            await _smsSender.SendAsync(input.PhoneNumber, L("SmsVerificationMessage", code));
        }

        public async Task VerifySmsCode(VerifySmsCodeInputDto input)
        {
            var cacheKey = AbpSession.ToUserIdentifier().ToString();
            var cash = await _cacheManager.GetSmsVerificationCodeCache().GetOrDefaultAsync(cacheKey);

            if (cash == null)
            {
                throw new UserFriendlyException("Phone number confirmation code is not found in cache !");
            }

            if (input.Code != cash.Code)
            {
                throw new UserFriendlyException(L("WrongSmsVerificationCode"));
            }

            var user = await UserManager.GetUserAsync(AbpSession.ToUserIdentifier());
            if (user.IsSuperUser || user.UserType == Accounts.Dto.AccountUserType.SuperAdmin)
                throw new UserFriendlyException("SuperUser Can't Login to the App!");

            user.IsPhoneNumberConfirmed = true;
            user.PhoneNumber = input.PhoneNumber;
            await UserManager.UpdateAsync(user);
        }

        public async Task PrepareCollectedData()
        {
            await _backgroundJobManager.EnqueueAsync<UserCollectedDataPrepareJob, UserIdentifier>(
                AbpSession.ToUserIdentifier()
            );
        }

        public async Task UpdateCurrentUserProfile(CurrentUserProfileEditDto input)
        {
            var user = await GetCurrentUserAsync();
            if (user.UserType == Accounts.Dto.AccountUserType.SuperAdmin || user.UserType == Accounts.Dto.AccountUserType.Admin)
            {
                if (user.PhoneNumber != input.PhoneNumber)
                {
                    input.IsPhoneNumberConfirmed = false;
                }
                else if (user.IsPhoneNumberConfirmed)
                {
                    input.IsPhoneNumberConfirmed = true;
                }

                ObjectMapper.Map(input, user);
               
                CheckErrors(await UserManager.UpdateAsync(user));

                if (Clock.SupportsMultipleTimezone)
                {
                    if (input.Timezone.IsNullOrEmpty())
                    {
                        var defaultValue =
                            await _timeZoneService.GetDefaultTimezoneAsync(SettingScopes.User, AbpSession.TenantId);
                        await SettingManager.ChangeSettingForUserAsync(AbpSession.ToUserIdentifier(),
                            TimingSettingNames.TimeZone, defaultValue);
                    }
                    else
                    {
                        await SettingManager.ChangeSettingForUserAsync(AbpSession.ToUserIdentifier(),
                            TimingSettingNames.TimeZone, input.Timezone);
                    }
                }
            }
            else
            {
                throw new UserFriendlyException("فقط کاربر مدیر امکان ویرایش اطلاعات کاربری دارد");
            }
        }

        public async Task ChangePassword(ChangePasswordInput input)
        {
            await UserManager.InitializeOptionsAsync(AbpSession.TenantId);

            var user = await GetCurrentUserAsync();

            string decryptedCurrentPassword = DecryptWithPrivateKey(input.CurrentPassword);
            string decryptedNewPassword = DecryptWithPrivateKey(input.NewPassword);

            if (await UserManager.CheckPasswordAsync(user, decryptedCurrentPassword))
            {
                var result = await UserManager.ChangePasswordAsync(user, decryptedNewPassword);
                CheckErrors(result);
            }
            else
            {
                CheckErrors(IdentityResult.Failed(new IdentityError
                {
                    Description = "Incorrect password."
                }));
            }
        }

        static string DecryptWithPrivateKey(string encryptedText)
        {
            string privateKeyInput = Environment.GetEnvironmentVariable("PRIVATE_KEY_PATH");
            if (!File.Exists(privateKeyInput))
            {
                throw new FileNotFoundException("Private key file not found.", privateKeyInput);
            }

            string privateKeyPem = File.ReadAllText(privateKeyInput);
            using (RSA rsa = RSA.Create())
            {
                rsa.ImportFromPem(privateKeyPem);
                byte[] decryptedBytes = rsa.Decrypt(Convert.FromBase64String(encryptedText), RSAEncryptionPadding.Pkcs1);
                return Encoding.UTF8.GetString(decryptedBytes);
            }
        }


        public async Task UpdateProfilePicture(UpdateProfilePictureInput input)
        {
            var userId = AbpSession.GetUserId();
            if (input.UserId.HasValue && input.UserId.Value != userId)
            {
                await CheckUpdateUsersProfilePicturePermission();
                userId = input.UserId.Value;
            }

            await UpdateProfilePictureForUser(userId, input);
        }

        public async Task<bool> VerifyAuthenticatorCode(VerifyAuthenticatorCodeInput input)
        {
            var result = await VerifyAuthenticatorCodeInternal(input);
            return result;
        }

        private async Task<bool> VerifyAuthenticatorCodeInternal(VerifyAuthenticatorCodeInput input)
        {
            var user = await GetCurrentUserAsync();

            var isValid = _googleTwoFactorAuthenticateService.ValidateTwoFactorPin(
                user.GoogleAuthenticatorKey ?? input.GoogleAuthenticatorKey,
                input.Code
            );

            if (isValid)
            {
                return true;
            }

            isValid = (await UserManager.RedeemTwoFactorRecoveryCodeAsync(user, input.Code)).Succeeded;

            return isValid;
        }

        private async Task CheckUpdateUsersProfilePicturePermission()
        {
            var permissionToChangeAnotherUsersProfilePicture = await PermissionChecker.IsGrantedAsync(
                AppPermissions.Pages_Administration_Users_ChangeProfilePicture
            );

            if (!permissionToChangeAnotherUsersProfilePicture)
            {
                var localizedPermissionName = L("UpdateUsersProfilePicture");
                throw new AbpAuthorizationException(
                    string.Format(
                        L("AllOfThesePermissionsMustBeGranted"),
                        localizedPermissionName
                    )
                );
            }
        }

        private async Task UpdateProfilePictureForUser(long userId, UpdateProfilePictureInput input)
        {
            var userIdentifier = new UserIdentifier(AbpSession.TenantId, userId);
            var allowToUseGravatar = await SettingManager.GetSettingValueForUserAsync<bool>(
                AppSettings.UserManagement.AllowUsingGravatarProfilePicture,
                user: userIdentifier
            );

            if (!allowToUseGravatar)
            {
                input.UseGravatarProfilePicture = false;
            }

            await SettingManager.ChangeSettingForUserAsync(
                userIdentifier,
                AppSettings.UserManagement.UseGravatarProfilePicture,
                input.UseGravatarProfilePicture.ToString().ToLowerInvariant()
            );

            if (input.UseGravatarProfilePicture)
            {
                return;
            }

            var imageBytes = _tempFileCacheManager.GetFile(input.FileToken);

            if (imageBytes == null)
            {
                //throw new UserFriendlyException("There is no such image file with the token: " + input.FileToken);
            }

            var byteArray = imageBytes;

            if (byteArray?.Length > MaxProfilePictureBytes)
            {
                throw new UserFriendlyException(L("ResizedProfilePicture_Warn_SizeLimit",
                    AppConsts.ResizedMaxProfilePictureBytesUserFriendlyValue));
            }

            var user = await UserManager.GetUserByIdAsync(userIdentifier.UserId);

            if (user.ProfilePictureId.HasValue)
            {
                await _binaryObjectManager.DeleteAsync(user.ProfilePictureId.Value);
            }

            if (byteArray != null)
            {
                var storedFile = new BinaryObject(userIdentifier.TenantId, byteArray,
                    BinarySourceType.ProfilePicture,
                    //$"Profile picture of user {userIdentifier.UserId}. {DateTime.UtcNow}");
                    $"{userIdentifier.UserId}.png");
                await _binaryObjectManager.SaveAsync(storedFile);
                storedFile.SourceId = (int?)user.Id;
                await CurrentUnitOfWork.SaveChangesAsync(); //It's done to get Id of the role.
                await _userAppService.UpdateProfilePictureId(user.Id, storedFile.Id);


            }
            else
            {
                await _userAppService.RemoveProfilePicture(user.Id);

            }

        }


        [AbpAllowAnonymous]
        public async Task<GetPasswordComplexitySettingOutput> GetPasswordComplexitySetting()
        {
            var passwordComplexitySetting = new PasswordComplexitySetting
            {
                RequireDigit =
                    await SettingManager.GetSettingValueAsync<bool>(AbpZeroSettingNames.UserManagement
                        .PasswordComplexity.RequireDigit),
                RequireLowercase =
                    await SettingManager.GetSettingValueAsync<bool>(AbpZeroSettingNames.UserManagement
                        .PasswordComplexity.RequireLowercase),
                RequireNonAlphanumeric =
                    await SettingManager.GetSettingValueAsync<bool>(AbpZeroSettingNames.UserManagement
                        .PasswordComplexity.RequireNonAlphanumeric),
                RequireUppercase =
                    await SettingManager.GetSettingValueAsync<bool>(AbpZeroSettingNames.UserManagement
                        .PasswordComplexity.RequireUppercase),
                RequiredLength =
                    await SettingManager.GetSettingValueAsync<int>(AbpZeroSettingNames.UserManagement.PasswordComplexity
                        .RequiredLength)
            };

            return new GetPasswordComplexitySettingOutput
            {
                Setting = passwordComplexitySetting
            };
        }

        [DisableAuditing]
        public async Task<GetProfilePictureOutput> GetProfilePicture()
        {
            using (var profileImageService = await _profileImageServiceFactory.Get(AbpSession.ToUserIdentifier()))
            {
                var profilePictureContent = await profileImageService.Object.GetProfilePictureContentForUser(
                    AbpSession.ToUserIdentifier()
                );

                return new GetProfilePictureOutput(profilePictureContent);
            }
        }

        [AbpAllowAnonymous]
        public async Task<GetProfilePictureOutput> GetProfilePictureByUserName(string username)
        {
            var user = await UserManager.FindByNameAsync(username);
            if (user == null)
            {
                return new GetProfilePictureOutput(string.Empty);
            }

            var userIdentifier = new UserIdentifier(AbpSession.TenantId, user.Id);
            using (var profileImageService = await _profileImageServiceFactory.Get(userIdentifier))
            {
                var profileImage = await profileImageService.Object.GetProfilePictureContentForUser(userIdentifier);
                return new GetProfilePictureOutput(profileImage);
            }
        }

        public async Task<GetProfilePictureOutput> GetFriendProfilePicture(GetFriendProfilePictureInput input)
        {
            var friendUserIdentifier = input.ToUserIdentifier();
            var friendShip = await _friendshipManager.GetFriendshipOrNullAsync(
                AbpSession.ToUserIdentifier(),
                friendUserIdentifier
            );

            if (friendShip == null)
            {
                return new GetProfilePictureOutput(string.Empty);
            }


            using (var profileImageService = await _profileImageServiceFactory.Get(friendUserIdentifier))
            {
                var image = await profileImageService.Object.GetProfilePictureContentForUser(friendUserIdentifier);
                return new GetProfilePictureOutput(image);
            }
        }

        [AbpAllowAnonymous]
        public async Task<GetProfilePictureOutput> GetProfilePictureByUser(long userId)
        {
            var userIdentifier = new UserIdentifier(AbpSession.TenantId, userId);
            using (var profileImageService = await _profileImageServiceFactory.Get(userIdentifier))
            {
                var profileImage = await profileImageService.Object.GetProfilePictureContentForUser(userIdentifier);
                return new GetProfilePictureOutput(profileImage);
            }
        }

        public async Task ChangeLanguage(ChangeUserLanguageDto input)
        {
            await SettingManager.ChangeSettingForUserAsync(
                AbpSession.ToUserIdentifier(),
                LocalizationSettingNames.DefaultLanguage,
                input.LanguageName
            );
        }

        private async Task<byte[]> GetProfilePictureByIdOrNull(Guid profilePictureId)
        {
            var file = await _binaryObjectManager.GetOrNullAsync(profilePictureId);
            if (file == null)
            {
                return null;
            }

            return file.Bytes;
        }

        private async Task<GetProfilePictureOutput> GetProfilePictureByIdInternal(Guid profilePictureId)
        {
            var bytes = await GetProfilePictureByIdOrNull(profilePictureId);
            if (bytes == null)
            {
                return new GetProfilePictureOutput(string.Empty);
            }

            return new GetProfilePictureOutput(Convert.ToBase64String(bytes));
        }
    }
}