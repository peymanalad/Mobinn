using System;
using Abp.Application.Services.Dto;
using System.ComponentModel.DataAnnotations;

namespace Chamran.Deed.Info.Dtos
{
    public class GetCommentForEditOutput
    {
        public CreateOrEditCommentDto Comment { get; set; }

        public string PostPostTitle { get; set; }

        public string UserName { get; set; }

        public string CommentCommentCaption { get; set; }

    }
}