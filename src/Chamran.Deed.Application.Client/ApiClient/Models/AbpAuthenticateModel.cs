using Abp.Dependency;

namespace Chamran.Deed.ApiClient.Models
{
    public class AbpAuthenticateModel : ISingletonDependency
    {
        public string PhoneNumber { get; set; }

        public string Otp1 { get; set; }
        public string Otp2 { get; set; }
        public string Otp3 { get; set; }
        public string Otp4 { get; set; }
        public string Otp5 { get; set; }
        public string Otp6 { get; set; }

        public string UserNameOrEmailAddress { get; set; }

        public string Password { get; set; }

        public string TwoFactorVerificationCode { get; set; }

        public bool RememberClient { get; set; }

        public string TwoFactorRememberClientToken { get; set; }

        public bool? SingleSignIn { get; set; }

        public string ReturnUrl { get; set; }

        public bool IsTwoFactorVerification => !string.IsNullOrEmpty(TwoFactorVerificationCode);
    }
}