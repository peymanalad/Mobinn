using System.ComponentModel.DataAnnotations;

namespace Chamran.Deed.Authorization.Users.Dto
{
    public class ChangeUserLanguageDto
    {
        [Required]
        public string LanguageName { get; set; }
    }
}
