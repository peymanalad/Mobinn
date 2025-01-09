using System;
using Abp.Application.Services.Dto;
using System.ComponentModel.DataAnnotations;

namespace Chamran.Deed.Info.Dtos
{
    public class CreateOrEditPostDto : EntityDto<int?>
    {
        [Required]
        public Guid PostKey { get; set; }

        public Guid? PostFile { get; set; }

        public string PostFileToken { get; set; }

        public string PostCaption { get; set; }

        public bool IsSpecial { get; set; }

        public bool IsPublished { get; set; }

        public string PostTitle { get; set; }

        public int? GroupMemberId { get; set; }

        public int? PostGroupId { get; set; }
        public int? PostSubGroupId { get; set; }

        public Guid? PostFile2 { get; set; }

        public string PostFileToken2 { get; set; }

        public Guid? PostFile3 { get; set; }

        public string PostFileToken3 { get; set; }  
        
        public Guid? PostFile4 { get; set; }

        public string PostFileToken4 { get; set; }   
        
        public Guid? PostFile5 { get; set; }

        public string PostFileToken5 { get; set; } 
        
        public Guid? PostFile6 { get; set; }

        public string PostFileToken6 { get; set; }
        
        public Guid? PostFile7 { get; set; }

        public string PostFileToken7 { get; set; } 
        
        public Guid? PostFile8 { get; set; }

        public string PostFileToken8 { get; set; }   
        
        public Guid? PostFile9 { get; set; }

        public string PostFileToken9 { get; set; }
        
        public Guid? PostFile10 { get; set; }

        public string PostFileToken10 { get; set; }

        public string PostRefLink { get; set; }

        public long? PublisherUserId { get; set; }
        public DateTime? DatePublished { get; set; }
        public PostStatus CurrentPostStatus { get; set; }
        public string PostComment { get; set; }

        //
        public int OrganizationId { get; set; }

    }
}