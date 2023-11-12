using System;

namespace Chamran.Deed.Info.Dtos
{
    public class LeafUserDto
    {
        public long UserId { get; set; }

        public string UserName { get; set; }

        public int TenantId { get; set; }
        
                public string FirstName { get; set; }
        
                public string LastName { get; set; }
        
                public string MemberPosition { get; set; }
                /// <summary>
                /// 0: هم سطح 
                /// 1: ارشد
                /// 2: کارمند
                /// </summary>
                public int LevelType { get; set; }
             
 
                public Guid? ProfilePictureId { get; set; }
                public int OrganizationUserId { get; set; }

    }

    public class SameLeafDto
    {
        public long UserId { get; set; }

        public string UserName { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string MemberPosition { get; set; }
        public int OrganizationUserId { get; set; }
    }
}