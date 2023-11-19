using Abp.Application.Services.Dto;

namespace Chamran.Deed.Info.Dtos
{
    public class PostPostGroupLookupTableDto
    {
        public int Id { get; set; }

        public string DisplayName { get; set; }
        public string OrganizationName { get; set; }
        public int? OrganizationId { get; set; }
    }
}