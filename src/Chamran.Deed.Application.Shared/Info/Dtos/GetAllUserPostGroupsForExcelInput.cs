using Abp.Application.Services.Dto;
using System;

namespace Chamran.Deed.Info.Dtos
{
    public class GetAllUserPostGroupsForExcelInput
    {
        public string Filter { get; set; }

        public string UserNameFilter { get; set; }

        public string PostGroupPostGroupDescriptionFilter { get; set; }

    }

    public class GetAllAllowedUserPostGroupsForExcelInput
    {
        public string Filter { get; set; }

        public string UserNameFilter { get; set; }

        public string PostGroupPostGroupDescriptionFilter { get; set; }

    }
}