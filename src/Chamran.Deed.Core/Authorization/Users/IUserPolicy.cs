using System.Threading.Tasks;
using Abp.Domain.Policies;

namespace Chamran.Deed.Authorization.Users
{
    public interface IUserPolicy : IPolicy
    {
        Task CheckMaxUserCountAsync(int tenantId);
    }
}
