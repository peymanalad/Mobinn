using System;
using System.Collections.Generic;

namespace Chamran.Deed.Web.Models.TokenAuth
{
    public class AuthenticateResultModel
    {
        public string AccessToken { get; set; }

        public string EncryptedAccessToken { get; set; }

        public int ExpireInSeconds { get; set; }

        public bool ShouldResetPassword { get; set; }

        public string PasswordResetCode { get; set; }

        public long UserId { get; set; }

        public bool RequiresTwoFactorVerification { get; set; }

        public IList<string> TwoFactorAuthProviders { get; set; }

        public string TwoFactorRememberClientToken { get; set; }

        public string ReturnUrl { get; set; }

        public string RefreshToken { get; set; }

        public int RefreshTokenExpireInSeconds { get; set; }
        public string c { get; set; }

        public List<JoinedOrganizationDto> JoinedOrganizations { get; set; }
    }

    public class JoinedOrganizationDto
    {
        public string OrganizationName { get; set; }
        public Guid? OrganizationPicture { get; set; }
        public int? OrganizationId { get; set; }
    }
}