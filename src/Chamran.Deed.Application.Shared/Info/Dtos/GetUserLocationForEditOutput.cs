using System;
using Abp.Application.Services.Dto;
using System.ComponentModel.DataAnnotations;

namespace Chamran.Deed.Info.Dtos
{
    public class GetUserLocationForEditOutput
    {
        public CreateOrEditUserLocationDto UserLocation { get; set; }

        public string UserName { get; set; }

    }
}