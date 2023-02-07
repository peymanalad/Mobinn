using Chamran.Deed.Models.NavigationMenu;

namespace Chamran.Deed.Services.Navigation
{
    public interface IMenuProvider
    {
        List<NavigationMenuItem> GetAuthorizedMenuItems(Dictionary<string, string> grantedPermissions);
    }
}