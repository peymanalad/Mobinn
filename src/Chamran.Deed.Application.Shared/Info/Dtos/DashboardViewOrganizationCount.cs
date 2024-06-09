using System;
using System.Collections.Generic;

namespace Chamran.Deed.Info.Dtos
{
    public class DashboardViewOrganizationCount
    {
        public DashboardViewOrganizationCount(int? organizationId, string organizationName, List<DashboardViewDate> countInfo)
        {
            OrganizationId = organizationId;
            OrganizationName = organizationName;
            CountInfo = countInfo;
        }

        public int? OrganizationId { get; set; }
        public string OrganizationName { get; set; }
        public List<DashboardViewDate> CountInfo { get; set; }
    }
}
