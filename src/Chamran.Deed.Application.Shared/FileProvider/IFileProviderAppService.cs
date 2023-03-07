using Abp.Application.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Chamran.Deed.FileProvider
{
    public interface IFileProviderAppService : IApplicationService
    {
        Task<string> GetImageSourceBase64(string fileId);

    }
}
