using System;

namespace Chamran.Deed.Info.Dtos
{
    public class GetPostForViewDto
    {
        public PostDto Post { get; set; }

        public string GroupMemberMemberPosition { get; set; }

        public string PostGroupPostGroupDescription { get; set; }

        public Guid? GroupFile { get; set; }
    }
}