using Abp.Application.Services.Dto;
using Chamran.Deed.Info.Dtos;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Chamran.Deed.FileProvider
{
    public class ProxyFileProviderAppService : ProxyAppServiceBase, IFileProviderAppService
    {
        public async Task<string> GetImageSourceBase64(string fileId)
        {
            //return await ApiClient.GetStringAsync<string>();
            return await ApiClient.GetNoWrapAsync<string>("File/GetBase64ImageSource?id=" + fileId);
        }

        public Task<string> GetLocalImageSourceBase64(string path)
        {
            if (File.Exists(path))
                return Task.FromResult(File.ReadAllText(path));
            return Task.FromResult("");
        }


    }
}
