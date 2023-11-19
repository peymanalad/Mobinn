using System;

namespace Chamran.Deed.Info.Dtos
{
    public class GetPostForViewDto
    {
        public PostDto Post { get; set; }
        public string GroupMemberMemberPosition { get; set; }
        public string PostGroupPostGroupDescription { get; set; }
        public int TotalVisits { get; set; }
        public int TotalLikes { get; set; }
        public Guid? GroupFile { get; set; }
        public string PersianCreationTime { get; set; }
        public int? OrganizationId { get; set; }
        public string OrganizationName { get; set; }
    }

   
}