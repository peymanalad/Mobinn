﻿using System;
using Abp.Application.Services.Dto;

namespace Chamran.Deed.Info.Dtos
{
    public class PostDto : EntityDto
    {

        public Guid? PostFile { get; set; }

        public string PostFileFileName { get; set; }

        public string PostCaption { get; set; }

        public bool IsSpecial { get; set; }

        public string PostTitle { get; set; }

        public int? GroupMemberId { get; set; }

        public int? PostGroupId { get; set; }

        public string PostRefLink { get; set; }

        public DateTime CreationTime { get; set; }
        public DateTime? LastModificationTime { get; set; }
    }
}