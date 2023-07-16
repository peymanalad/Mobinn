using System;
using Abp.Application.Services.Dto;
using System.ComponentModel.DataAnnotations;

namespace Chamran.Deed.Info.Dtos
{
    public class GetUserPostGroupForEditOutput
    {
        public CreateOrEditUserPostGroupDto UserPostGroup { get; set; }

        public string UserName { get; set; }

        public string PostGroupPostGroupDescription { get; set; }

    }
}