﻿using System.Collections.Generic;
using Abp.Application.Services.Dto;
using Chamran.Deed.Editions.Dto;

namespace Chamran.Deed.MultiTenancy.Dto
{
    public class GetTenantFeaturesEditOutput
    {
        public List<NameValueDto> FeatureValues { get; set; }

        public List<FlatFeatureDto> Features { get; set; }
    }
}