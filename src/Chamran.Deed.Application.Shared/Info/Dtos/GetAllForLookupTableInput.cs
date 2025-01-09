using System.Collections.Generic;
using Abp.Application.Services.Dto;
using JetBrains.Annotations;

namespace Chamran.Deed.Info.Dtos
{
    public class GetAllForLookupTableInput : PagedAndSortedResultRequestDto
    {
        public string Filter { get; set; }
        //public int? OrganizationId { get; set; }
        public int? OrganizationId { get; set; }

    }
}