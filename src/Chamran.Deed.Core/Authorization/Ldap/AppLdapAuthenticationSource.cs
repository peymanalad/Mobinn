﻿using Abp.Zero.Ldap.Authentication;
using Abp.Zero.Ldap.Configuration;
using Chamran.Deed.Authorization.Users;
using Chamran.Deed.MultiTenancy;

namespace Chamran.Deed.Authorization.Ldap
{
    public class AppLdapAuthenticationSource : LdapAuthenticationSource<Tenant, User>
    {
        public AppLdapAuthenticationSource(ILdapSettings settings, IAbpZeroLdapModuleConfig ldapModuleConfig)
            : base(settings, ldapModuleConfig)
        {
        }
    }
}