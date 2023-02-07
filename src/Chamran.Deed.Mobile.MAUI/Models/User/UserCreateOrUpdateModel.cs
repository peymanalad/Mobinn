using Abp.AutoMapper;
using Chamran.Deed.Authorization.Users.Dto;

namespace Chamran.Deed.Mobile.MAUI.Models.User
{
    [AutoMapFrom(typeof(CreateOrUpdateUserInput))]
    public class UserCreateOrUpdateModel : CreateOrUpdateUserInput
    {

    }
}
