using System;
using System.Collections.Generic;
using System.Text;

namespace Chamran.Deed.Info.Dtos
{
    public class SuperUserDashboardViewDto
    {
        public int TotalUserCount { get; set; }
        public int TotalPostCount { get; set; }
        public int TotalCommentCount { get; set; }
        public int TotalPostViewCount { get; set; }
        public List<DashboardViewOrganizationCount> Top5PostCountPerDay { get; set; }
        public List<DashboardViewOrganizationCount> Top5ViewCountPerDay { get; set; }
        public List<DashboardViewOrganizationCount> Top5CommentCountPerDay { get; set; }
        public List<DashboardViewOrganizationCount> Top5LikeCountPerDay { get; set; }
        public List<DashboardViewCategoryInfo> CategoryCount { get; set; }

    }
}
