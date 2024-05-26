using System;
using System.Collections.Generic;
using System.Text;

namespace Chamran.Deed.Info.Dtos
{
    public class OrganizationDashboardViewDto
    {
        public int TotalUserCount { get; set; }
        public int TotalPostCount { get; set; }
        public int TotalCommentCount { get; set; }
        public int TotalPostViewCount { get; set; }
        public List<DashboardViewDate> PostCountPerDay { get; set; }
        public List<DashboardViewDate> ViewCountPerDay { get; set; }
        public List<DashboardViewDate> CommentCountPerDay { get; set; }
        public List<DashboardViewDate> LikeCountPerDay{ get; set; }
        public List<DashboardViewCategoryInfo> CategoryCount { get; set; }


       
    }
}
