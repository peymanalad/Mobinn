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
        public string PostGroupPostSubGroupDescription { get; set; }

        public string PostFileFileName { get; set; }
        public string PostFile2FileName { get; set; }
        public string PostFile3FileName { get; set; }
        public string PostFile4FileName { get; set; }
        public string PostFile5FileName { get; set; }
        public string PostFile6FileName { get; set; }
        public string PostFile7FileName { get; set; }
        public string PostFile8FileName { get; set; }
        public string PostFile9FileName { get; set; }
        public string PostFile10FileName { get; set; }
        public int? OrganizationId { get; set; }
        public string OrganizationName { get; set; }

    }
}