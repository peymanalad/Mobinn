using System;
using System.Collections.Generic;
using Abp.Runtime.Validation;
using Chamran.Deed.Dto;

namespace Chamran.Deed.Authorization.Users.Dto
{
    public class GetUsersInput : PagedAndSortedInputDto, IShouldNormalize, IGetUsersInput
    {
        public string Filter { get; set; }

        public DateTime? FromCreationDate { get; set; }
        public DateTime? ToCreationDate { get; set; }
        public DateTime? FromLastLoginDate { get; set; }
        public DateTime? ToLastLoginDate { get; set; }
        public string NationalIdFilter { get; set; }
        public string NameFilter { get; set; }
        public string SurNameFilter { get; set; }
        public string UserNameFilter { get; set; }
        public string PhoneNumberFilter { get; set; }
        public bool? IsActiveFilter { get; set; }
        public int? UserType{ get; set; }

        public List<string> Permissions { get; set; }

        public int? Role { get; set; }

        public bool OnlyLockedUsers { get; set; }

        public int? OrganizationId { get; set; }

        public void Normalize()
        {
            if (string.IsNullOrEmpty(Sorting))
            {
                Sorting = "Name,Surname";
            }

            Filter = Filter?.Trim();
        }
    }
}
