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

        public bool IsSpecial { get; set; }

        public bool IsPublished { get; set; }

        public string PostTitle { get; set; }

        public int? GroupMemberId { get; set; }

        public int? PostGroupId { get; set; }

        public Guid? PostFile2 { get; set; }

        public string PostFileToken2 { get; set; }

        public Guid? PostFile3 { get; set; }

        public string PostFileToken3 { get; set; }

        public string PostRefLink { get; set; }

    }
}