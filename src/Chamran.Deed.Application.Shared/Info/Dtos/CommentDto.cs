﻿using System;
using Abp.Application.Services.Dto;

namespace Chamran.Deed.Info.Dtos
{
    public class CommentDto : EntityDto
    {
        public string CommentCaption { get; set; }

        public DateTime InsertDate { get; set; }

        public int PostId { get; set; }

        public long UserId { get; set; }

        public int? CommentId { get; set; }

    }
}