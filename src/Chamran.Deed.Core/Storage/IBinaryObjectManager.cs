using System;
using System.IO;
using System.Threading.Tasks;

namespace Chamran.Deed.Storage
{
    public interface IBinaryObjectManager
    {
        Task<BinaryObject> GetOrNullAsync(Guid id);
        Task<Stream> GetStreamAsync(Guid id);

        Task SaveAsync(BinaryObject file);
        
        Task DeleteAsync(Guid id);
    }
}