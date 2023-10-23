using System.ComponentModel.DataAnnotations;

namespace Chamran.Deed.Authorization.Users.Dto
{
    public class CreateNodeDto
    {
        [Required]
        public NodeUserDto User { get; set; }
        
        [Required]
        public NodeOrganizationDto Organization { get; set; }
        [Required]

        public int OrganizationChartId { get; set; }
    }
}