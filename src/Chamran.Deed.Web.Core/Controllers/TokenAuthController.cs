﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Abp;
using Abp.AspNetCore.Mvc.Authorization;
using Abp.AspNetZeroCore.Web.Authentication.External;
using Abp.Authorization;
using Abp.Authorization.Users;
using Abp.MultiTenancy;
using Abp.Configuration;
using Abp.Domain.Repositories;
using Abp.Extensions;
using Abp.Net.Mail;
using Abp.Notifications;
using Abp.Runtime.Caching;
using Abp.Runtime.Security;
using Abp.Runtime.Session;
using Abp.Timing;
using Abp.UI;
using Abp.Zero.Configuration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Chamran.Deed.Authentication.TwoFactor;
using Chamran.Deed.Authentication.TwoFactor.Google;
using Chamran.Deed.Authorization;
using Chamran.Deed.Authorization.Accounts.Dto;
using Chamran.Deed.Authorization.Users;
using Chamran.Deed.MultiTenancy;
using Chamran.Deed.Web.Authentication.JwtBearer;
using Chamran.Deed.Web.Authentication.TwoFactor;
using Chamran.Deed.Web.Models.TokenAuth;
using Chamran.Deed.Authorization.Impersonation;
using Chamran.Deed.Authorization.Roles;
using Chamran.Deed.Configuration;
using Chamran.Deed.Identity;
using Chamran.Deed.Net.Sms;
using Chamran.Deed.Notifications;
using Chamran.Deed.Security.Recaptcha;
using Chamran.Deed.Web.Authentication.External;
using Chamran.Deed.Web.Common;
using Chamran.Deed.Authorization.Delegation;
using Chamran.Deed.Authorization.Users.Profile.Cache;
using Chamran.Deed.People;
using Microsoft.EntityFrameworkCore;
using IdentityModel.OidcClient;
using System.Security.Cryptography;
using Org.BouncyCastle.Crypto;

namespace Chamran.Deed.Web.Controllers
{
    [IgnoreAntiforgeryToken]
    [Route("api/[controller]/[action]")]
    public class TokenAuthController : DeedControllerBase

    {
        private readonly LogInManager _logInManager;
        private readonly ITenantCache _tenantCache;
        private readonly AbpLoginResultTypeHelper _abpLoginResultTypeHelper;
        private readonly TokenAuthConfiguration _configuration;
        private readonly UserManager _userManager;
        private readonly ICacheManager _cacheManager;
        private readonly IOptions<AsyncJwtBearerOptions> _jwtOptions;
        private readonly IExternalAuthConfiguration _externalAuthConfiguration;
        private readonly IExternalAuthManager _externalAuthManager;
        private readonly UserRegistrationManager _userRegistrationManager;
        private readonly IImpersonationManager _impersonationManager;
        private readonly IUserLinkManager _userLinkManager;
        private readonly IAppNotifier _appNotifier;
        private readonly ISmsSender _smsSender;
        private readonly IEmailSender _emailSender;
        private readonly IdentityOptions _identityOptions;
        private readonly GoogleAuthenticatorProvider _googleAuthenticatorProvider;
        private readonly ExternalLoginInfoManagerFactory _externalLoginInfoManagerFactory;
        private readonly ISettingManager _settingManager;
        private readonly IJwtSecurityStampHandler _securityStampHandler;
        private readonly AbpUserClaimsPrincipalFactory<User, Role> _claimsPrincipalFactory;
        public IRecaptchaValidator RecaptchaValidator { get; set; }
        private readonly IUserDelegationManager _userDelegationManager;
        private readonly IRepository<User, long> _userRepository;
        private readonly IRepository<Tenant, int> _tenantRepository;
        private readonly IRepository<GroupMember> _groupMemberRepository;
        private readonly UserClaimsPrincipalFactory _principalFactory;
        public TokenAuthController(LogInManager logInManager,
            ITenantCache tenantCache,
            AbpLoginResultTypeHelper abpLoginResultTypeHelper,
            TokenAuthConfiguration configuration,
            UserManager userManager,
            ICacheManager cacheManager,
            IOptions<AsyncJwtBearerOptions> jwtOptions,
            IExternalAuthConfiguration externalAuthConfiguration,
            IExternalAuthManager externalAuthManager,
            UserRegistrationManager userRegistrationManager,
            IImpersonationManager impersonationManager,
            IUserLinkManager userLinkManager,
            IAppNotifier appNotifier,
            ISmsSender smsSender,
            IEmailSender emailSender,
            IOptions<IdentityOptions> identityOptions,
            GoogleAuthenticatorProvider googleAuthenticatorProvider,
            ExternalLoginInfoManagerFactory externalLoginInfoManagerFactory,
            ISettingManager settingManager,
            IJwtSecurityStampHandler securityStampHandler,
            AbpUserClaimsPrincipalFactory<User, Role> claimsPrincipalFactory,
            IUserDelegationManager userDelegationManager,
            IRepository<User, long> userRepository,
            UserClaimsPrincipalFactory principalFactory,
            IRepository<GroupMember> groupMember,
            IRepository<Tenant, int> tenantRepository)
        {
            _logInManager = logInManager;
            _tenantCache = tenantCache;
            _abpLoginResultTypeHelper = abpLoginResultTypeHelper;
            _configuration = configuration;
            _userManager = userManager;
            _cacheManager = cacheManager;
            _jwtOptions = jwtOptions;
            _externalAuthConfiguration = externalAuthConfiguration;
            _externalAuthManager = externalAuthManager;
            _userRegistrationManager = userRegistrationManager;
            _impersonationManager = impersonationManager;
            _userLinkManager = userLinkManager;
            _appNotifier = appNotifier;
            _smsSender = smsSender;
            _emailSender = emailSender;
            _googleAuthenticatorProvider = googleAuthenticatorProvider;
            _externalLoginInfoManagerFactory = externalLoginInfoManagerFactory;
            _settingManager = settingManager;
            _securityStampHandler = securityStampHandler;
            _identityOptions = identityOptions.Value;
            _claimsPrincipalFactory = claimsPrincipalFactory;
            RecaptchaValidator = NullRecaptchaValidator.Instance;
            _userDelegationManager = userDelegationManager;
            _userRepository = userRepository;
            _tenantRepository = tenantRepository;
            _principalFactory = principalFactory;
            _groupMemberRepository = groupMember;

        }

        private async Task<string> EncryptQueryParameters(long userId, string passwordResetCode)
        {
            var expirationHours = await _settingManager.GetSettingValueAsync<int>(
                AppSettings.UserManagement.Password.PasswordResetCodeExpirationHours
            );

            var expireDate = Uri.EscapeDataString(Clock.Now.AddHours(expirationHours)
                .ToString(DeedConsts.DateTimeOffsetFormat));

            var query = $"userId={userId}&resetCode={passwordResetCode}&expireDate={expireDate}";

            return SimpleStringCipher.Instance.Encrypt(query);
        }


        [HttpPost]
        public async Task<bool> SendOtp([FromBody] AuthenticateModel model)
        {
            //return await _smsSender.SendAsyncResult("09122800039", "رمز یکبار مصرف شما:123456");
            var code = RandomHelper.GetRandom(100000, 999999).ToString();
            //var cacheKey = AbpSession.ToUserIdentifier().ToString();
            var cacheItem = new SmsVerificationCodeCacheItem
            {
                Code = code
            };

            await _cacheManager.GetSmsVerificationCodeCache().SetAsync(
                model.UserNameOrEmailAddress,
                cacheItem
            );

            return await _smsSender.SendAsyncResult(model.UserNameOrEmailAddress,
                //$"رمز یکبار مصرف شما:{code}\r\nسامانه دید");
                code);

        }


        [HttpPost]
        public async Task<AuthenticateResultModel> OtpAuthenticate([FromBody] AuthenticateModel model)
        {

            if (UseCaptchaOnLogin())
            {
                await ValidateReCaptcha(model.CaptchaResponse);
            }

            try
            {
                if (model == null || string.IsNullOrEmpty(model.UserNameOrEmailAddress) || string.IsNullOrEmpty(model.Password))
                {
                    throw new Exception("Model یا فیلدهای کلیدی آن مقدار null یا خالی دارند.");
                }

                //var correctKey = await _cacheManager.GetSmsVerificationCodeCache().GetOrDefaultAsync(model.UserNameOrEmailAddress);
                var correctKey = await _cacheManager.GetSmsVerificationCodeCache().GetOrDefaultAsync(model.UserNameOrEmailAddress);

                if (correctKey == null || string.IsNullOrEmpty(correctKey.Code))
                {
                    throw _abpLoginResultTypeHelper.CreateExceptionForFailedLoginAttempt(
                        AbpLoginResultType.InvalidPassword,
                        model.UserNameOrEmailAddress,
                        GetTenancyNameOrNull());
                }

                if (!model.Password.Equals(correctKey.Code))
                {
                    throw _abpLoginResultTypeHelper.CreateExceptionForFailedLoginAttempt(
                        AbpLoginResultType.InvalidPassword,
                        model.UserNameOrEmailAddress,
                        GetTenancyNameOrNull());
                }
            }
            catch (Exception)
            {
                throw _abpLoginResultTypeHelper.CreateExceptionForFailedLoginAttempt(
                    AbpLoginResultType.InvalidPassword,
                    model?.UserNameOrEmailAddress ?? "UnknownUser",
                    GetTenancyNameOrNull());
            }



            //var loginResult = await GetLoginResultAsync(
            //    model.UserNameOrEmailAddress,
            //    model.Password,
            //    GetTenancyNameOrNull()
            //);

            var returnUrl = model.ReturnUrl;

            //if (model.SingleSignIn.HasValue && model.SingleSignIn.Value &&
            //    loginResult.Result == AbpLoginResultType.Success)
            //{
            //    loginResult.User.SetSignInToken();
            //    returnUrl = AddSingleSignInParametersToReturnUrl(model.ReturnUrl, loginResult.User.SignInToken,
            //        loginResult.User.Id, loginResult.User.TenantId);
            //}

            //Password reset
            //if (loginResult.User.ShouldChangePasswordOnNextLogin)
            //{
            //    loginResult.User.SetNewPasswordResetCode();
            //    return new AuthenticateResultModel
            //    {
            //        ShouldResetPassword = true,
            //        ReturnUrl = returnUrl,
            //        c = await EncryptQueryParameters(loginResult.User.Id, loginResult.User.PasswordResetCode)
            //    };
            //}

            //Two factor auth
            //await _userManager.InitializeOptionsAsync(loginResult.Tenant?.Id);

            //string twoFactorRememberClientToken = null;
            //if (await IsTwoFactorAuthRequiredAsync(loginResult, model))
            //{
            //    if (model.TwoFactorVerificationCode.IsNullOrEmpty())
            //    {
            //        //Add a cache item which will be checked in SendTwoFactorAuthCode to prevent sending unwanted two factor code to users.
            //        await _cacheManager
            //            .GetTwoFactorCodeCache()
            //            .SetAsync(
            //                loginResult.User.ToUserIdentifier().ToString(),
            //                new TwoFactorCodeCacheItem()
            //            );

            //        return new AuthenticateResultModel
            //        {
            //            RequiresTwoFactorVerification = true,
            //            UserId = loginResult.User.Id,
            //            TwoFactorAuthProviders = await _userManager.GetValidTwoFactorProvidersAsync(loginResult.User),
            //            ReturnUrl = returnUrl
            //        };
            //    }

            //    twoFactorRememberClientToken = await TwoFactorAuthenticateAsync(loginResult, model);
            //}

            // One Concurrent Login 
            //if (AllowOneConcurrentLoginPerUser())
            //{
            //    await ResetSecurityStampForLoginResult(loginResult);
            //}
            var loginResult = await _userRepository.FirstOrDefaultAsync(
                user => (user.PhoneNumber == model.UserNameOrEmailAddress) && user.TenantId == AbpSession.TenantId
            );
            if (loginResult == null)
            {
                throw _abpLoginResultTypeHelper.CreateExceptionForFailedLoginAttempt(AbpLoginResultType.UserPhoneNumberIsNotConfirmed, model.UserNameOrEmailAddress, GetTenancyNameOrNull());
            }
            if (loginResult.IsSuperUser)
                throw new UserFriendlyException("SuperUser Can't Login to the App!");
            var identity = (ClaimsIdentity)(await _principalFactory.CreateAsync(loginResult)).Identity;

            var refreshToken = CreateRefreshToken(
                await CreateJwtClaims(
                    identity,
                    loginResult,
                    tokenType: TokenType.RefreshToken
                )
            );
            if (model.ExpireDays <= 0)
            {
                model.ExpireDays = 93;
            }
            var accessToken = CreateAccessToken(
                await CreateJwtClaims(
                    identity,
                    loginResult,
                    refreshTokenKey: refreshToken.key
                ), TimeSpan.FromDays(model.ExpireDays)
            );

            var result = new AuthenticateResultModel
            {
                AccessToken = accessToken,
                ExpireInSeconds = Convert.ToInt32(TimeSpan.FromDays(model.ExpireDays).TotalSeconds),
                RefreshToken = refreshToken.token,
                RefreshTokenExpireInSeconds = (int)_configuration.RefreshTokenExpiration.TotalSeconds,
                EncryptedAccessToken = GetEncryptedAccessToken(accessToken),
                TwoFactorRememberClientToken = null,//twoFactorRememberClientToken,
                UserId = loginResult.Id,
                ReturnUrl = returnUrl,
                JoinedOrganizations = new List<JoinedOrganizationDto>(),
                UserType = loginResult.UserType,
            };
            var query = _groupMemberRepository.GetAll().Include(x => x.UserFk).Include(x => x.OrganizationFk).Where(x => x.UserId == loginResult.Id);
            foreach (var groupMember in query)
            {
                result.JoinedOrganizations.Add(new JoinedOrganizationDto()
                {
                    OrganizationId = groupMember.OrganizationId,
                    OrganizationName = groupMember.OrganizationFk?.OrganizationName ?? "",
                    OrganizationPicture = groupMember.OrganizationFk?.OrganizationLogo
                });
            }
            return result;
            //(int)_configuration.AccessTokenExpiration.TotalSeconds,
        }


        [HttpPost]
        public async Task<AuthenticateResultModel> Authenticate([FromBody] AuthenticateModel model)
        {
            if (UseCaptchaOnLogin())
            {
                await ValidateReCaptcha(model.CaptchaResponse);
            }


            string privateKeyInput = Environment.GetEnvironmentVariable("PRIVATE_KEY_PATH");
            if (string.IsNullOrWhiteSpace(privateKeyInput))
            {
                throw new UserFriendlyException("PRIVATE_KEY_PATH environment variable is not set or empty.");
            }
            string privateKeyPem;

            if (System.IO.File.Exists(privateKeyInput))
            {
                privateKeyPem = System.IO.File.ReadAllText(privateKeyInput);
            }
            else if (IsPemFormat(privateKeyInput))
            {
                privateKeyPem = privateKeyInput;
            }
            else
            {
                throw new UserFriendlyException($"Private key file not found and content is not a valid PEM format: {privateKeyInput}");
            }
            var loginResult = await GetLoginResultAsync(
                model.UserNameOrEmailAddress,
                DecryptWithPrivateKey(model.Password, privateKeyPem),
                GetTenancyNameOrNull()
            );
            if (loginResult.Result == AbpLoginResultType.InvalidPassword ||
                loginResult.Result == AbpLoginResultType.InvalidUserNameOrEmailAddress)
            {
                var user = await _userManager.FindByNameOrEmailAsync(model.UserNameOrEmailAddress);
                if (user != null)
                {
                    user.AccessFailedCount = 0;
                    user.LockoutEndDateUtc = null;
                    await _userManager.UpdateAsync(user);
                }
            }
            if (loginResult.User.UserType == AccountUserType.Normal)
            {
                throw new AbpAuthorizationException("ورود کاربر عادی مجاز نیست.");
            }

            var returnUrl = model.ReturnUrl;

            if (model.SingleSignIn.HasValue && model.SingleSignIn.Value &&
                loginResult.Result == AbpLoginResultType.Success)
            {
                loginResult.User.SetSignInToken();
                returnUrl = AddSingleSignInParametersToReturnUrl(model.ReturnUrl, loginResult.User.SignInToken,
                    loginResult.User.Id, loginResult.User.TenantId);
            }

            ////Password reset
            //if (loginResult.User.ShouldChangePasswordOnNextLogin)
            //{
            //    loginResult.User.SetNewPasswordResetCode();
            //    return new AuthenticateResultModel
            //    {
            //        ShouldResetPassword = true,
            //        ReturnUrl = returnUrl,
            //        c = await EncryptQueryParameters(loginResult.User.Id, loginResult.User.PasswordResetCode)
            //    };
            //}

            //Two factor auth
            await _userManager.InitializeOptionsAsync(loginResult.Tenant?.Id);

            string twoFactorRememberClientToken = null;
            if (await IsTwoFactorAuthRequiredAsync(loginResult, model))
            {
                if (model.TwoFactorVerificationCode.IsNullOrEmpty())
                {
                    //Add a cache item which will be checked in SendTwoFactorAuthCode to prevent sending unwanted two factor code to users.
                    await _cacheManager
                        .GetTwoFactorCodeCache()
                        .SetAsync(
                            loginResult.User.ToUserIdentifier().ToString(),
                            new TwoFactorCodeCacheItem()
                        );

                    return new AuthenticateResultModel
                    {
                        RequiresTwoFactorVerification = true,
                        UserId = loginResult.User.Id,
                        TwoFactorAuthProviders = await _userManager.GetValidTwoFactorProvidersAsync(loginResult.User),
                        ReturnUrl = returnUrl
                    };
                }

                twoFactorRememberClientToken = await TwoFactorAuthenticateAsync(loginResult, model);
            }

            // One Concurrent Login 
            if (AllowOneConcurrentLoginPerUser())
            {
                await ResetSecurityStampForLoginResult(loginResult);
            }

            var refreshToken = CreateRefreshToken(
                await CreateJwtClaims(
                    loginResult.Identity,
                    loginResult.User,
                    tokenType: TokenType.RefreshToken
                )
            );


            if (model.ExpireDays <= 0)
            {
                model.ExpireDays = 93;
            }
            var accessToken = CreateAccessToken(
                await CreateJwtClaims(
                    loginResult.Identity,
                    loginResult.User,
                    refreshTokenKey: refreshToken.key
                ), TimeSpan.FromDays(model.ExpireDays)
            );


            var result = new AuthenticateResultModel
            {
                AccessToken = accessToken,
                ExpireInSeconds = Convert.ToInt32(TimeSpan.FromDays(model.ExpireDays).TotalSeconds),//(int)_configuration.AccessTokenExpiration.TotalSeconds,
                RefreshToken = refreshToken.token,
                RefreshTokenExpireInSeconds = (int)_configuration.RefreshTokenExpiration.TotalSeconds,
                EncryptedAccessToken = GetEncryptedAccessToken(accessToken),
                TwoFactorRememberClientToken = twoFactorRememberClientToken,
                UserId = loginResult.User.Id,
                ReturnUrl = returnUrl,
                JoinedOrganizations = new List<JoinedOrganizationDto>(),
                UserType = loginResult.User.UserType,
            };

            var query = _groupMemberRepository.GetAll().Include(x => x.UserFk).Include(x => x.OrganizationFk).Where(x => x.UserId == loginResult.User.Id);
            foreach (var groupMember in query)
            {
                result.JoinedOrganizations.Add(new JoinedOrganizationDto()
                {
                    OrganizationId = groupMember.OrganizationId,
                    OrganizationName = groupMember.OrganizationFk?.OrganizationName ?? "",
                    OrganizationPicture = groupMember.OrganizationFk?.OrganizationLogo
                });
            }

            return result;

        }

        static string DecryptWithPrivateKey(string encryptedText, string privateKeyPem)
        {
            if (string.IsNullOrWhiteSpace(privateKeyPem))
            {
                throw new ArgumentException("Private key is empty", nameof(privateKeyPem));
            }

            try
            {
                using (RSA rsa = RSA.Create())
                {
                    rsa.ImportFromPem(privateKeyPem);
                    byte[] decryptedBytes = rsa.Decrypt(Convert.FromBase64String(encryptedText), RSAEncryptionPadding.Pkcs1);
                    return Encoding.UTF8.GetString(decryptedBytes);
                }
            }
            catch (Exception ex) when (ex is ArgumentException || ex is CryptographicException)
            {
                throw new UserFriendlyException("Failed to read RSA private key. Check PRIVATE_KEY_PATH configuration.", ex);
            }
        }

        static bool IsPemFormat(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            return value.Contains("BEGIN") && value.Contains("END");
        }


        [HttpPost]
        public async Task<RefreshTokenResult> RefreshToken(string refreshToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                throw new ArgumentNullException(nameof(refreshToken));
            }

            var (isRefreshTokenValid, principal) = await IsRefreshTokenValid(refreshToken);
            if (!isRefreshTokenValid)
            {
                throw new ValidationException("Refresh token is not valid!");
            }

            try
            {
                var user = await _userManager.GetUserAsync(
                    UserIdentifier.Parse(principal.Claims.First(x => x.Type == AppConsts.UserIdentifier).Value)
                );

                if (user == null)
                {
                    throw new UserFriendlyException("Unknown user or user identifier");
                }

                if (AllowOneConcurrentLoginPerUser())
                {
                    await _userManager.UpdateSecurityStampAsync(user);
                    await _securityStampHandler.SetSecurityStampCacheItem(user.TenantId, user.Id, user.SecurityStamp);
                }

                principal = await _claimsPrincipalFactory.CreateAsync(user);

                var accessToken = CreateAccessToken(
                    await CreateJwtClaims(principal.Identity as ClaimsIdentity, user)
                );

                return await Task.FromResult(new RefreshTokenResult(
                    accessToken,
                    GetEncryptedAccessToken(accessToken),
                    (int)_configuration.AccessTokenExpiration.TotalSeconds)
                );
            }
            catch (UserFriendlyException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new ValidationException("Refresh token is not valid!", e);
            }
        }

        private bool UseCaptchaOnLogin()
        {
            return SettingManager.GetSettingValue<bool>(AppSettings.UserManagement.UseCaptchaOnLogin);
        }

        [HttpGet]
        [AbpMvcAuthorize]
        public async Task<string> GetLatestOtp(string phoneNumber)
        {
            try
            {

                var result = await _cacheManager.GetSmsVerificationCodeCache().GetOrDefaultAsync(phoneNumber);
                if (result == null)
                {
                    return await Task.FromResult(@"");
                }
                return result.Code;
            }
            catch (Exception ex)
            {
                Logger.Debug(ex.ToString(), ex);
            }

            return await Task.FromResult(@"");
        }

        [HttpGet]
        [AbpMvcAuthorize]
        public async Task LogOut()
        {
            if (AbpSession.UserId != null)
            {
                //var tokenValidityKeyInClaims = User.Claims.First(c => c.Type == AppConsts.TokenValidityKey);
                //await RemoveTokenAsync(tokenValidityKeyInClaims.Value);

                //var refreshTokenValidityKeyInClaims =
                //    User.Claims.FirstOrDefault(c => c.Type == AppConsts.RefreshTokenValidityKey);
                //if (refreshTokenValidityKeyInClaims != null)
                //{
                //    await RemoveTokenAsync(refreshTokenValidityKeyInClaims.Value);
                //}

                //if (AllowOneConcurrentLoginPerUser())
                //{
                //    await _securityStampHandler.RemoveSecurityStampCacheItem(
                //        AbpSession.TenantId,
                //        AbpSession.GetUserId()
                //    );
                //}
                await RemoveTokensAsync(AbpSession.TenantId,
                    AbpSession.GetUserId());
                await _securityStampHandler.RemoveSecurityStampCacheItem(
                        AbpSession.TenantId,
                        AbpSession.GetUserId()
                    );
            }
        }

        private async Task RemoveTokensAsync(int? tenantId, long userId)
        {
            await _userManager.RemoveTokens(tenantId, userId);
        }

        private async Task RemoveTokenAsync(string tokenKey)
        {
            await _userManager.RemoveTokenValidityKeyAsync(
                await _userManager.GetUserAsync(AbpSession.ToUserIdentifier()), tokenKey
            );

            await _cacheManager.GetCache(AppConsts.TokenValidityKey).RemoveAsync(tokenKey);
        }

        private async Task RemoveTokensAsync(string tokenKey)
        {
            await _userManager.RemoveTokenValidityKeyAsync(
                await _userManager.GetUserAsync(AbpSession.ToUserIdentifier()), tokenKey
            );

            await _cacheManager.GetCache(AppConsts.TokenValidityKey).RemoveAsync(tokenKey);
        }

        [HttpPost]
        public async Task SendTwoFactorAuthCode([FromBody] SendTwoFactorAuthCodeModel model)
        {
            var cacheKey = new UserIdentifier(AbpSession.TenantId, model.UserId).ToString();

            var cacheItem = await _cacheManager
                .GetTwoFactorCodeCache()
                .GetOrDefaultAsync(cacheKey);

            if (cacheItem == null)
            {
                //There should be a cache item added in Authenticate method! This check is needed to prevent sending unwanted two factor code to users.
                throw new UserFriendlyException(L("SendSecurityCodeErrorMessage"));
            }

            var user = await _userManager.FindByIdAsync(model.UserId.ToString());

            if (model.Provider != GoogleAuthenticatorProvider.Name)
            {
                cacheItem.Code = await _userManager.GenerateTwoFactorTokenAsync(user, model.Provider);
                var message = L("EmailSecurityCodeBody", cacheItem.Code);

                if (model.Provider == "Email")
                {
                    await _emailSender.SendAsync(await _userManager.GetEmailAsync(user), L("EmailSecurityCodeSubject"),
                        message);
                }
                else if (model.Provider == "Phone")
                {
                    await _smsSender.SendAsync(await _userManager.GetPhoneNumberAsync(user), message);
                }
            }

            await _cacheManager.GetTwoFactorCodeCache().SetAsync(
                cacheKey,
                cacheItem
            );

            await _cacheManager.GetCache("ProviderCache").SetAsync(
                "Provider",
                model.Provider
            );
        }

        [HttpPost]
        public async Task<ImpersonatedAuthenticateResultModel> ImpersonatedAuthenticate(string impersonationToken)
        {
            var result = await _impersonationManager.GetImpersonatedUserAndIdentity(impersonationToken);
            var accessToken = CreateAccessToken(await CreateJwtClaims(result.Identity, result.User));

            return new ImpersonatedAuthenticateResultModel
            {
                AccessToken = accessToken,
                EncryptedAccessToken = GetEncryptedAccessToken(accessToken),
                ExpireInSeconds = (int)_configuration.AccessTokenExpiration.TotalSeconds
            };
        }

        [HttpPost]
        public async Task<ImpersonatedAuthenticateResultModel> DelegatedImpersonatedAuthenticate(long userDelegationId,
            string impersonationToken)
        {
            var result = await _impersonationManager.GetImpersonatedUserAndIdentity(impersonationToken);
            var userDelegation = await _userDelegationManager.GetAsync(userDelegationId);

            if (!userDelegation.IsCreatedByUser(result.User.Id))
            {
                throw new UserFriendlyException("User delegation error...");
            }

            var expiration = userDelegation.EndTime.Subtract(Clock.Now);
            var accessToken = CreateAccessToken(await CreateJwtClaims(result.Identity, result.User, expiration),
                expiration);

            return new ImpersonatedAuthenticateResultModel
            {
                AccessToken = accessToken,
                EncryptedAccessToken = GetEncryptedAccessToken(accessToken),
                ExpireInSeconds = (int)expiration.TotalSeconds
            };
        }

        [HttpPost]
        public async Task<SwitchedAccountAuthenticateResultModel> LinkedAccountAuthenticate(string switchAccountToken)
        {
            var result = await _userLinkManager.GetSwitchedUserAndIdentity(switchAccountToken);
            var accessToken = CreateAccessToken(await CreateJwtClaims(result.Identity, result.User));

            return new SwitchedAccountAuthenticateResultModel
            {
                AccessToken = accessToken,
                EncryptedAccessToken = GetEncryptedAccessToken(accessToken),
                ExpireInSeconds = (int)_configuration.AccessTokenExpiration.TotalSeconds
            };
        }

        [HttpGet]
        public List<ExternalLoginProviderInfoModel> GetExternalAuthenticationProviders()
        {
            var allProviders = _externalAuthConfiguration.ExternalLoginInfoProviders
                .Select(infoProvider => infoProvider.GetExternalLoginInfo())
                .Where(IsSchemeEnabledOnTenant)
                .ToList();
            return ObjectMapper.Map<List<ExternalLoginProviderInfoModel>>(allProviders);
        }

        private bool IsSchemeEnabledOnTenant(ExternalLoginProviderInfo scheme)
        {
            if (!AbpSession.TenantId.HasValue)
            {
                return true;
            }

            switch (scheme.Name)
            {
                case "OpenIdConnect":
                    return !_settingManager.GetSettingValueForTenant<bool>(
                        AppSettings.ExternalLoginProvider.Tenant.OpenIdConnect_IsDeactivated, AbpSession.GetTenantId());
                case "Microsoft":
                    return !_settingManager.GetSettingValueForTenant<bool>(
                        AppSettings.ExternalLoginProvider.Tenant.Microsoft_IsDeactivated, AbpSession.GetTenantId());
                case "Google":
                    return !_settingManager.GetSettingValueForTenant<bool>(
                        AppSettings.ExternalLoginProvider.Tenant.Google_IsDeactivated, AbpSession.GetTenantId());
                case "Twitter":
                    return !_settingManager.GetSettingValueForTenant<bool>(
                        AppSettings.ExternalLoginProvider.Tenant.Twitter_IsDeactivated, AbpSession.GetTenantId());
                case "Facebook":
                    return !_settingManager.GetSettingValueForTenant<bool>(
                        AppSettings.ExternalLoginProvider.Tenant.Facebook_IsDeactivated, AbpSession.GetTenantId());
                case "WsFederation":
                    return !_settingManager.GetSettingValueForTenant<bool>(
                        AppSettings.ExternalLoginProvider.Tenant.WsFederation_IsDeactivated, AbpSession.GetTenantId());
                default: return true;
            }
        }

        [HttpPost]
        public async Task<ExternalAuthenticateResultModel> ExternalAuthenticate(
            [FromBody] ExternalAuthenticateModel model)
        {
            var externalUser = await GetExternalUserInfo(model);

            var loginResult = await _logInManager.LoginAsync(
                new UserLoginInfo(model.AuthProvider, externalUser.ProviderKey, model.AuthProvider),
                GetTenancyNameOrNull()
            );

            switch (loginResult.Result)
            {
                case AbpLoginResultType.Success:
                    {
                        // One Concurrent Login 
                        if (AllowOneConcurrentLoginPerUser())
                        {
                            await ResetSecurityStampForLoginResult(loginResult);
                        }

                        var refreshToken = CreateRefreshToken(
                            await CreateJwtClaims(
                                loginResult.Identity,
                                loginResult.User,
                                tokenType: TokenType.RefreshToken
                            )
                        );

                        var accessToken = CreateAccessToken(
                            await CreateJwtClaims(
                                loginResult.Identity,
                                loginResult.User,
                                refreshTokenKey: refreshToken.key
                            )
                        );

                        var returnUrl = model.ReturnUrl;

                        if (model.SingleSignIn.HasValue && model.SingleSignIn.Value &&
                            loginResult.Result == AbpLoginResultType.Success)
                        {
                            loginResult.User.SetSignInToken();
                            returnUrl = AddSingleSignInParametersToReturnUrl(
                                model.ReturnUrl,
                                loginResult.User.SignInToken,
                                loginResult.User.Id,
                                loginResult.User.TenantId
                            );
                        }

                        return new ExternalAuthenticateResultModel
                        {
                            AccessToken = accessToken,
                            EncryptedAccessToken = GetEncryptedAccessToken(accessToken),
                            ExpireInSeconds = (int)_configuration.AccessTokenExpiration.TotalSeconds,
                            ReturnUrl = returnUrl,
                            RefreshToken = refreshToken.token,
                            RefreshTokenExpireInSeconds = (int)_configuration.RefreshTokenExpiration.TotalSeconds
                        };
                    }
                case AbpLoginResultType.UnknownExternalLogin:
                    {
                        var newUser = await RegisterExternalUserAsync(externalUser);
                        if (!newUser.IsActive)
                        {
                            return new ExternalAuthenticateResultModel
                            {
                                WaitingForActivation = true
                            };
                        }

                        //Try to login again with newly registered user!
                        loginResult = await _logInManager.LoginAsync(
                            new UserLoginInfo(model.AuthProvider, model.ProviderKey, model.AuthProvider),
                            GetTenancyNameOrNull()
                        );

                        if (loginResult.Result != AbpLoginResultType.Success)
                        {
                            throw _abpLoginResultTypeHelper.CreateExceptionForFailedLoginAttempt(
                                loginResult.Result,
                                externalUser.EmailAddress,
                                GetTenancyNameOrNull()
                            );
                        }

                        var refreshToken = CreateRefreshToken(await CreateJwtClaims(loginResult.Identity,
                            loginResult.User, tokenType: TokenType.RefreshToken)
                        );

                        var accessToken = CreateAccessToken(await CreateJwtClaims(loginResult.Identity,
                            loginResult.User, refreshTokenKey: refreshToken.key));

                        return new ExternalAuthenticateResultModel
                        {
                            AccessToken = accessToken,
                            EncryptedAccessToken = GetEncryptedAccessToken(accessToken),
                            ExpireInSeconds = (int)_configuration.AccessTokenExpiration.TotalSeconds,
                            RefreshToken = refreshToken.token,
                            RefreshTokenExpireInSeconds = (int)_configuration.RefreshTokenExpiration.TotalSeconds
                        };
                    }
                default:
                    {
                        throw _abpLoginResultTypeHelper.CreateExceptionForFailedLoginAttempt(
                            loginResult.Result,
                            externalUser.EmailAddress,
                            GetTenancyNameOrNull()
                        );
                    }
            }
        }

        private async Task ResetSecurityStampForLoginResult(AbpLoginResult<Tenant, User> loginResult)
        {
            await _userManager.UpdateSecurityStampAsync(loginResult.User);
            await _securityStampHandler.SetSecurityStampCacheItem(loginResult.User.TenantId, loginResult.User.Id,
                loginResult.User.SecurityStamp);
            loginResult.Identity.ReplaceClaim(new Claim(AppConsts.SecurityStampKey, loginResult.User.SecurityStamp));
        }

        #region Etc

        [AbpMvcAuthorize]
        [HttpGet]
        public async Task<ActionResult> TestNotification(string message = "", string severity = "info")
        {
            if (message.IsNullOrEmpty())
            {
                message = "This is a test notification, created at " + Clock.Now;
            }

            await _appNotifier.SendMessageAsync(
                AbpSession.ToUserIdentifier(),
                message,
                severity.ToPascalCase().ToEnum<NotificationSeverity>()
            );

            return Content("Sent notification: " + message);
        }

        #endregion

        private async Task<User> RegisterExternalUserAsync(ExternalAuthUserInfo externalLoginInfo)
        {
            string username;

            using (var providerManager =
                   _externalLoginInfoManagerFactory.GetExternalLoginInfoManager(externalLoginInfo.Provider))
            {
                username = providerManager.Object.GetUserNameFromExternalAuthUserInfo(externalLoginInfo);
            }

            var user = await _userRegistrationManager.RegisterAsync(
                externalLoginInfo.Name,
                externalLoginInfo.Surname,
                externalLoginInfo.EmailAddress,
                username,
                await _userManager.CreateRandomPassword(),
                true,
                null
            );

            user.Logins = new List<UserLogin>
            {
                new UserLogin
                {
                    LoginProvider = externalLoginInfo.Provider,
                    ProviderKey = externalLoginInfo.ProviderKey,
                    TenantId = user.TenantId
                }
            };

            await CurrentUnitOfWork.SaveChangesAsync();

            return user;
        }

        private async Task<ExternalAuthUserInfo> GetExternalUserInfo(ExternalAuthenticateModel model)
        {
            var userInfo = await _externalAuthManager.GetUserInfo(model.AuthProvider, model.ProviderAccessCode);
            if (!ProviderKeysAreEqual(model, userInfo))
            {
                throw new UserFriendlyException(L("CouldNotValidateExternalUser"));
            }

            return userInfo;
        }

        private bool ProviderKeysAreEqual(ExternalAuthenticateModel model, ExternalAuthUserInfo userInfo)
        {
            if (userInfo.ProviderKey == model.ProviderKey)
            {
                return true;
            }

            ;

            return userInfo.ProviderKey == model.ProviderKey.Replace("-", "").TrimStart('0');
        }

        private async Task<bool> IsTwoFactorAuthRequiredAsync(AbpLoginResult<Tenant, User> loginResult,
            AuthenticateModel authenticateModel)
        {
            if (!await SettingManager.GetSettingValueAsync<bool>(AbpZeroSettingNames.UserManagement.TwoFactorLogin
                    .IsEnabled))
            {
                return false;
            }

            if (!loginResult.User.IsTwoFactorEnabled)
            {
                return false;
            }

            if ((await _userManager.GetValidTwoFactorProvidersAsync(loginResult.User)).Count <= 0)
            {
                return false;
            }

            if (await TwoFactorClientRememberedAsync(loginResult.User.ToUserIdentifier(), authenticateModel))
            {
                return false;
            }

            return true;
        }

        private async Task<bool> TwoFactorClientRememberedAsync(UserIdentifier userIdentifier,
            AuthenticateModel authenticateModel)
        {
            if (!await SettingManager.GetSettingValueAsync<bool>(
                    AbpZeroSettingNames.UserManagement.TwoFactorLogin.IsRememberBrowserEnabled)
               )
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(authenticateModel.TwoFactorRememberClientToken))
            {
                return false;
            }

            try
            {
                var validationParameters = new TokenValidationParameters
                {
                    ValidAudience = _configuration.Audience,
                    ValidIssuer = _configuration.Issuer,
                    IssuerSigningKey = _configuration.SecurityKey
                };

                foreach (var validator in _jwtOptions.Value.AsyncSecurityTokenValidators)
                {
                    if (validator.CanReadToken(authenticateModel.TwoFactorRememberClientToken))
                    {
                        try
                        {
                            var (principal, _) = await validator.ValidateToken(
                                authenticateModel.TwoFactorRememberClientToken,
                                validationParameters
                            );

                            var userIdentifierClaim = principal.FindFirst(c => c.Type == AppConsts.UserIdentifier);
                            if (userIdentifierClaim == null)
                            {
                                return false;
                            }

                            return userIdentifierClaim.Value == userIdentifier.ToString();
                        }
                        catch (Exception ex)
                        {
                            Logger.Debug(ex.ToString(), ex);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Debug(ex.ToString(), ex);
            }

            return false;
        }

        /* Checkes two factor code and returns a token to remember the client (browser) if needed */
        private async Task<string> TwoFactorAuthenticateAsync(AbpLoginResult<Tenant, User> loginResult,
            AuthenticateModel authenticateModel)
        {
            var twoFactorCodeCache = _cacheManager.GetTwoFactorCodeCache();
            var userIdentifier = loginResult.User.ToUserIdentifier().ToString();
            var cachedCode = await twoFactorCodeCache.GetOrDefaultAsync(userIdentifier);
            var provider = _cacheManager.GetCache("ProviderCache").Get("Provider", cache => cache).ToString();

            if (provider == GoogleAuthenticatorProvider.Name)
            {
                if (!await _googleAuthenticatorProvider.ValidateAsync("TwoFactor",
                        authenticateModel.TwoFactorVerificationCode, _userManager, loginResult.User))
                {
                    throw new UserFriendlyException(L("InvalidSecurityCode"));
                }
            }
            else if (cachedCode?.Code == null || cachedCode.Code != authenticateModel.TwoFactorVerificationCode)
            {
                throw new UserFriendlyException(L("InvalidSecurityCode"));
            }

            //Delete from the cache since it was a single usage code
            await twoFactorCodeCache.RemoveAsync(userIdentifier);

            if (authenticateModel.RememberClient)
            {
                if (await SettingManager.GetSettingValueAsync<bool>(AbpZeroSettingNames.UserManagement.TwoFactorLogin
                        .IsRememberBrowserEnabled))
                {
                    return CreateAccessToken(
                        await CreateJwtClaims(
                            loginResult.Identity,
                            loginResult.User
                        )
                    );
                }
            }

            return null;
        }

        private string GetTenancyNameOrNull()
        {
            if (!AbpSession.TenantId.HasValue)
            {
                return null;
            }

            return _tenantCache.GetOrNull(AbpSession.TenantId.Value)?.TenancyName;
        }

        private async Task<AbpLoginResult<Tenant, User>> GetLoginResultAsync(string usernameOrEmailAddress,
            string password, string tenancyName)
        {
            var shouldLockout = false;


            AbpLoginResult<Tenant, User> loginResult =
                await _logInManager.LoginAsync(usernameOrEmailAddress, password, tenancyName, shouldLockout);
            switch (loginResult.Result)
            {
                case AbpLoginResultType.Success:
                    return loginResult;
                default:
                    throw _abpLoginResultTypeHelper.CreateExceptionForFailedLoginAttempt(
                        loginResult.Result,
                        usernameOrEmailAddress,
                        tenancyName
                    );
            }
        }

        private async Task<AbpLoginResult<Tenant, User>> GetOtpLoginResultAsync(string phoneNumber,
            string otp, string tenancyName)
        {
            var shouldLockout = await SettingManager.GetSettingValueAsync<bool>(
                AbpZeroSettingNames.UserManagement.UserLockOut.IsEnabled
            );
            var loginResult = await _logInManager.OtpLoginAsync(phoneNumber, otp, tenancyName, shouldLockout);

            switch (loginResult.Result)
            {
                case AbpLoginResultType.Success:
                    return loginResult;
                default:
                    throw _abpLoginResultTypeHelper.CreateExceptionForFailedLoginAttempt(
                        loginResult.Result,
                        phoneNumber,
                        tenancyName
                    );
            }
        }


        private string CreateAccessToken(IEnumerable<Claim> claims, TimeSpan? expiration = null)
        {
            return CreateToken(claims, expiration ?? _configuration.AccessTokenExpiration);
        }

        private (string token, string key) CreateRefreshToken(IEnumerable<Claim> claims)
        {
            var claimsList = claims.ToList();
            return (CreateToken(claimsList, AppConsts.RefreshTokenExpiration),
                claimsList.First(c => c.Type == AppConsts.TokenValidityKey).Value);
        }

        private string CreateToken(IEnumerable<Claim> claims, TimeSpan? expiration = null)
        {
            var now = DateTime.UtcNow;

            var jwtSecurityToken = new JwtSecurityToken(
                issuer: _configuration.Issuer,
                audience: _configuration.Audience,
                claims: claims,
                notBefore: now,
                signingCredentials: _configuration.SigningCredentials,
                expires: expiration == null ? (DateTime?)null : now.Add(expiration.Value)
            );

            return new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
        }

        private static string GetEncryptedAccessToken(string accessToken)
        {
            return SimpleStringCipher.Instance.Encrypt(accessToken, AppConsts.DefaultPassPhrase);
        }

        private async Task<IEnumerable<Claim>> CreateJwtClaims(
            ClaimsIdentity identity, User user,
            TimeSpan? expiration = null,
            TokenType tokenType = TokenType.AccessToken,
            string refreshTokenKey = null)
        {
            var tokenValidityKey = Guid.NewGuid().ToString();
            var claims = identity.Claims.ToList();
            var nameIdClaim = claims.First(c => c.Type == _identityOptions.ClaimsIdentity.UserIdClaimType);

            if (_identityOptions.ClaimsIdentity.UserIdClaimType != JwtRegisteredClaimNames.Sub)
            {
                claims.Add(new Claim(JwtRegisteredClaimNames.Sub, nameIdClaim.Value));
            }

            claims.AddRange(new[]
            {
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.Now.ToUnixTimeSeconds().ToString(),
                    ClaimValueTypes.Integer64),
                new Claim(AppConsts.TokenValidityKey, tokenValidityKey),
                new Claim(AppConsts.UserIdentifier, user.ToUserIdentifier().ToUserIdentifierString()),
                new Claim(AppConsts.TokenType, tokenType.To<int>().ToString())
            });

            if (!string.IsNullOrEmpty(refreshTokenKey))
            {
                claims.Add(new Claim(AppConsts.RefreshTokenValidityKey, refreshTokenKey));
            }

            if (!expiration.HasValue)
            {
                expiration = tokenType == TokenType.AccessToken
                    ? _configuration.AccessTokenExpiration
                    : _configuration.RefreshTokenExpiration;
            }

            var expirationDate = DateTime.UtcNow.Add(expiration.Value);

            await _cacheManager
                .GetCache(AppConsts.TokenValidityKey)
                .SetAsync(tokenValidityKey, "", absoluteExpireTime: new DateTimeOffset(expirationDate));

            await _userManager.AddTokenValidityKeyAsync(
                user,
                tokenValidityKey,
                expirationDate
            );

            return claims;
        }

        private static string AddSingleSignInParametersToReturnUrl(string returnUrl, string signInToken, long userId,
            int? tenantId)
        {
            returnUrl += (returnUrl.Contains("?") ? "&" : "?") +
                         "accessToken=" + signInToken +
                         "&userId=" + Convert.ToBase64String(Encoding.UTF8.GetBytes(userId.ToString()));
            if (tenantId.HasValue)
            {
                returnUrl += "&tenantId=" + Convert.ToBase64String(Encoding.UTF8.GetBytes(tenantId.Value.ToString()));
            }

            return returnUrl;
        }


        private async Task<(bool isValid, ClaimsPrincipal principal)> IsRefreshTokenValid(string refreshToken)
        {
            ClaimsPrincipal principal = null;

            try
            {
                var validationParameters = new TokenValidationParameters
                {
                    ValidAudience = _configuration.Audience,
                    ValidIssuer = _configuration.Issuer,
                    IssuerSigningKey = _configuration.SecurityKey
                };

                foreach (var validator in _jwtOptions.Value.AsyncSecurityTokenValidators)
                {
                    if (!validator.CanReadToken(refreshToken))
                    {
                        continue;
                    }

                    try
                    {
                        (principal, _) = await validator.ValidateRefreshToken(refreshToken, validationParameters);
                        return (true, principal);
                    }
                    catch (Exception ex)
                    {
                        Logger.Debug(ex.ToString(), ex);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Debug(ex.ToString(), ex);
            }

            return (false, principal);
        }


        private bool AllowOneConcurrentLoginPerUser()
        {
            return _settingManager.GetSettingValue<bool>(AppSettings.UserManagement.AllowOneConcurrentLoginPerUser);
        }

        private async Task ValidateReCaptcha(string captchaResponse)
        {
            var requestUserAgent = Request.Headers["User-Agent"].ToString();
            if (!requestUserAgent.IsNullOrWhiteSpace() &&
                WebConsts.ReCaptchaIgnoreWhiteList.Contains(requestUserAgent.Trim()))
            {
                return;
            }

            await RecaptchaValidator.ValidateAsync(captchaResponse);
        }
    }
}