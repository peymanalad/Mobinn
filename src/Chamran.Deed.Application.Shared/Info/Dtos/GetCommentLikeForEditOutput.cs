using System;
using Abp.Application.Services.Dto;
using System.ComponentModel.DataAnnotations;

namespace Chamran.Deed.Info.Dtos
{
    public class GetCommentLikeForEditOutput
    {
        public CreateOrEditCommentLikeDto CommentLike { get; set; }

        public string CommentCommentCaption { get; set; }

        public string UserName { get; set; }

    }
}