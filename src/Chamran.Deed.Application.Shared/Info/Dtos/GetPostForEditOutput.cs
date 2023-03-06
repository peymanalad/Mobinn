using System;
using Abp.Application.Services.Dto;
using System.ComponentModel.DataAnnotations;

namespace Chamran.Deed.Info.Dtos
{
    public class GetPostForEditOutput
    {
        public CreateOrEditPostDto Post { get; set; }

        public string GroupMemberMemberPosition { get; set; }

        public string PostGroupPostGroupDescription { get; set; }

        public string PostFileFileName { get; set; }

    }
}