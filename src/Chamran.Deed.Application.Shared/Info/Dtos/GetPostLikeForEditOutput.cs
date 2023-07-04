using System;
using Abp.Application.Services.Dto;
using System.ComponentModel.DataAnnotations;

namespace Chamran.Deed.Info.Dtos
{
    public class GetPostLikeForEditOutput
    {
        public CreateOrEditPostLikeDto PostLike { get; set; }

        public string PostPostTitle { get; set; }

        public string UserName { get; set; }

    }
}