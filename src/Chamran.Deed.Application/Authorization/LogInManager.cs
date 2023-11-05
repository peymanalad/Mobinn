using System;
using System.Threading.Tasks;
using Abp.Authorization;
using Abp.Authorization.Users;
using Abp.Configuration;
using Abp.Configuration.Startup;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Zero.Configuration;
using Microsoft.AspNetCore.Identity;
using Chamran.Deed.Authorization.Roles;
using Chamran.Deed.Authorization.Users;
using Chamran.Deed.MultiTenancy;

namespace Chamran.Deed.Authorization
{
    public class LogInManager : AbpLogInManager<Tenant, Role, User>
    {
        public LogInManager(
            UserManager userManager,
            IMultiTenancyConfig multiTenancyConfig,
            IRepository<Tenant> tenantRepository,
            IUnitOfWorkManager unitOfWorkManager,
            ISettingManager settingManager,
            IRepository<UserLoginAttempt, long> userLoginAttemptRepository,
            IUserManagementConfig userManagementConfig,
            IIocResolver iocResolver,
            RoleManager roleManager,
            IPasswordHasher<User> passwordHasher,
            UserClaimsPrincipalFactory claimsPrincipalFactory)
            : base(
                  userManager,
                  multiTenancyConfig,
                  tenantRepository,
                  unitOfWorkManager,
                  settingManager,
                  userLoginAttemptRepository,
                  userManagementConfig,
                  iocResolver,
                  passwordHasher,
                  roleManager,
                  claimsPrincipalFactory)
        {

        }




        public async Task<AbpLoginResult<Tenant, User>> OtpLoginAsync(string phoneNumber, string otp, string tenancyName, bool shouldLockout)
        {
            return await UnitOfWorkManager.WithUnitOfWorkAsync(async () =>
            {
                var result = await OtpLoginAsyncInternal(phoneNumber, otp, tenancyName);
                await SaveLoginAttemptAsync(result, tenancyName, phoneNumber + "@" + otp);
                return result;
            });
        }


        private async Task<AbpLoginResult<Tenant, User>> OtpLoginAsyncInternal(string phoneNumber, string otp, string tenancyName)
        {
            if (string.IsNullOrEmpty(phoneNumber))
            {
                throw new ArgumentException("login");
            }

            //Get and check tenant
            Tenant tenant = null;
            if (!MultiTenancyConfig.IsEnabled)
            {
                tenant = await GetDefaultTenantAsync();
            }
            else if (!string.IsNullOrWhiteSpace(tenancyName))
            {
                tenant = await TenantRepository.FirstOrDefaultAsync(t => t.TenancyName == tenancyName);
                if (tenant == null)
                {
                    return new AbpLoginResult<Tenant, User>(AbpLoginResultType.InvalidTenancyName);
                }

                if (!tenant.IsActive)
                {
                    return new AbpLoginResult<Tenant, User>(AbpLoginResultType.TenantIsNotActive, tenant);
                }
            }

            var tenantId = tenant?.Id;
            using (UnitOfWorkManager.Current.SetTenantId(tenantId))
            {
                //var user = await UserManager.FindAsync(tenantId, login);
                //if (user == null)
                {
                    return new AbpLoginResult<Tenant, User>(AbpLoginResultType.UnknownExternalLogin, tenant);
                }

                //return await CreateLoginResultAsync(user, tenant);
            }
        }
    }
}