using System.Collections.Generic;
using MvvmHelpers;
using Chamran.Deed.Models.NavigationMenu;

namespace Chamran.Deed.Services.Navigation
{
    public interface IMenuProvider
    {
        ObservableRangeCollection<NavigationMenuItem> GetAuthorizedMenuItems(Dictionary<string, string> grantedPermissions);
    }
}