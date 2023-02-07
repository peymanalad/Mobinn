namespace Chamran.Deed.Services.Permission
{
    public interface IPermissionService
    {
        bool HasPermission(string key);
    }
}