using Abp.AutoMapper;
using Chamran.Deed.Authorization.Users.Dto;
using Chamran.Deed.Info.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chamran.Deed.Mobile.MAUI.Models.Info
{
    [AutoMapFrom(typeof(GetPostCategoriesForViewDto))]
    public class PostCategoriesModel
    {
        public int Id { get; set; }
        public string PostGroupDescription { get; set; }
        public string Base64Image { get; set; }
    }
}
