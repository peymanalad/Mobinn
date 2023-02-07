using Abp.AutoMapper;
using Chamran.Deed.Authorization.Users.Dto;

namespace Chamran.Deed.Mobile.MAUI.Models.User
{
    [AutoMapFrom(typeof(UserListDto))]
    public class UserListModel : UserListDto
    {
        public string Photo { get; set; }

        public string FullName => Name + " " + Surname;
    }
}
