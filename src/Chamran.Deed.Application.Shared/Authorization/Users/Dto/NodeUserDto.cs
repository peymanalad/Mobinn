using System.ComponentModel.DataAnnotations;
using Abp.Auditing;
using Abp.Authorization.Users;
using Abp.Domain.Entities;

namespace Chamran.Deed.Authorization.Users.Dto
{
    public class NodeUserDto : IPassivable
    {
        /// <summary>
        /// Set null to create a new user. Set user's Id to update a user
        /// </summary>
        public long? Id { get; set; }
        public string NationalId { get; set; }
        [Required]
        [StringLength(AbpUserBase.MaxNameLength)]
        public string Name { get; set; }

        [Required]
        [StringLength(AbpUserBase.MaxSurnameLength)]
        public string Surname { get; set; }

     
        [StringLength(UserConsts.MaxPhoneNumberLength)]
        public string PhoneNumber { get; set; }

        // Not used "Required" attribute since empty value is used to 'not change password'
        [StringLength(AbpUserBase.MaxPlainPasswordLength)]
        [DisableAuditing]
        public string Password { get; set; }


        public bool IsActive { get; set; }
    }

    public class NodeOrganizationDto
    {
        public string NationalId { get; set; }
        public string Name { get; set; }
        public bool IsGovernment { get; set; }
        public string OrganizationLogoToken { get; set; }
        public string Comment { get; set; }
    }
}