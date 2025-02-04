using System.ComponentModel.DataAnnotations;
using Abp.Auditing;
using Chamran.Deed.Authorization.Accounts.Validation;

namespace Chamran.Deed.Authorization.Users.Profile.Dto
{
    public class ChangePasswordInput
    {
        [Required]
        [DisableAuditing]
        public string CurrentPassword { get; set; }

        [Required]
        [DisableAuditing]
        [StrongPassword]
        public string NewPassword { get; set; }
    }
}