using System;
using Chamran.Deed.Configuration.Dto;

namespace Chamran.Deed.UiCustomization.Dto
{
    public class UiCustomizationSettingsDto
    {
        public ThemeSettingsDto BaseSettings { get; set; }

        public bool IsLeftMenuUsed { get; set; }

        public bool IsTopMenuUsed { get; set; }

        public bool IsTabMenuUsed { get; set; }

        public bool AllowMenuScroll { get; set; } = true;
    }

    public class CurrentOrganizationDto
    {
        public int? OrganizationId { get; set; }
        public string OrganizationName { get; set; }
        public Guid? OrganizationPicture { get; set; }
    }
}
