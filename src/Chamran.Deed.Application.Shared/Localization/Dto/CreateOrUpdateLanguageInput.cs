using System.ComponentModel.DataAnnotations;

namespace Chamran.Deed.Localization.Dto
{
    public class CreateOrUpdateLanguageInput
    {
        [Required]
        public ApplicationLanguageEditDto Language { get; set; }
    }
}