using System;
using Abp.Application.Services.Dto;
using System.ComponentModel.DataAnnotations;

namespace Chamran.Deed.Info.Dtos
{
    public class CreateOrEditPostDto : EntityDto<int?>
    {

        public Guid? PostFile { get; set; }

        public string PostFileToken { get; set; }

        public string PostCaption { get; set; }

        public DateTime PostTime { get; set; }

        public bool IsSpecial { get; set; }

        public string PostTitle { get; set; }

        public int? GroupMemberId { get; set; }

        public int? PostGroupId { get; set; }

    }
}