using System.Threading.Tasks;
using Chamran.Deed.Sessions.Dto;

namespace Chamran.Deed.Web.Session
{
    public interface IPerRequestSessionCache
    {
        Task<GetCurrentLoginInformationsOutput> GetCurrentLoginInformationsAsync();
    }
}
