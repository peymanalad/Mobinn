using System;
using Abp.Application.Services.Dto;

namespace Chamran.Deed.Common.Dtos
{
    public class SoftwareUpdateDto : EntityDto
    {
        public string SoftwareVersion { get; set; }

        public bool ForceUpdate { get; set; }

        public string UpdatePath { get; set; }

        public string WhatsNew { get; set; }

        public string Platform { get; set; }

        public int BuildNo { get; set; }

    }
}