using System.Collections.Generic;
using System.Threading.Tasks;
using Abp;
using Chamran.Deed.Dto;

namespace Chamran.Deed.Gdpr
{
    public interface IUserCollectedDataProvider
    {
        Task<List<FileDto>> GetFiles(UserIdentifier user);
    }
}
