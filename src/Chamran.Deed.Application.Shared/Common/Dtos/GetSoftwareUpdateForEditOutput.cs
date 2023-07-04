using System;
using Abp.Application.Services.Dto;
using System.ComponentModel.DataAnnotations;

namespace Chamran.Deed.Common.Dtos
{
    public class GetSoftwareUpdateForEditOutput
    {
        public CreateOrEditSoftwareUpdateDto SoftwareUpdate { get; set; }

    }
}