﻿using System;
using Abp.Application.Services.Dto;
using System.ComponentModel.DataAnnotations;

namespace Chamran.Deed.Info.Dtos
{
    public class CreateOrEditPostGroupDto : EntityDto<int?>
    {

        public string PostGroupDescription { get; set; }

        public int Ordering { get; set; }

        public Guid? GroupFile { get; set; }

        public string GroupFileToken { get; set; }

        public int? OrganizationId { get; set; }

    }

    public class CreateOrEditPostSubGroupDto : EntityDto<int?>
    {

        public string PostSubGroupDescription { get; set; }

        public int PostGroupId { get; set; }

        public Guid? SubGroupFile { get; set; }

        public string SubGroupFileToken { get; set; }


    }
}