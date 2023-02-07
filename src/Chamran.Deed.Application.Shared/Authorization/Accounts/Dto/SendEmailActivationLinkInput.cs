using System.ComponentModel.DataAnnotations;

namespace Chamran.Deed.Authorization.Accounts.Dto
{
    public class SendEmailActivationLinkInput
    {
        [Required]
        public string EmailAddress { get; set; }
    }
}