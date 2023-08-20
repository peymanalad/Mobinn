using System;
using System.IO;
using System.Linq;
using Abp.IO.Extensions;
using Abp.UI;
using Abp.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Chamran.Deed.Storage;

namespace Chamran.Deed.Web.Controllers
{
    [Authorize]
    public class SoftwareUpdatesController : DeedControllerBase
    {
        private readonly ITempFileCacheManager _tempFileCacheManager;

        private const long MaxUpdateFileLength = 524288000; //500MB
        private const string MaxUpdateFileLengthUserFriendlyValue = "500MB"; //5MB
        private readonly string[] UpdateFileAllowedFileTypes = { "apk"};

        public SoftwareUpdatesController(ITempFileCacheManager tempFileCacheManager)
        {
            _tempFileCacheManager = tempFileCacheManager;
        }

        public FileUploadCacheOutput UploadUpdateFileFile()
        {
            try
            {
                Console.WriteLine(Request);
                //Check input
                if (Request.Form.Files.Count == 0)
                {
                    throw new UserFriendlyException(L("NoFileFoundError"));
                }

                var file = Request.Form.Files.First();
                if (file.Length > MaxUpdateFileLength)
                {
                    throw new UserFriendlyException(L("Warn_File_SizeLimit", MaxUpdateFileLengthUserFriendlyValue));
                }

                var fileType = Path.GetExtension(file.FileName).Substring(1);
                if (UpdateFileAllowedFileTypes != null && UpdateFileAllowedFileTypes.Length > 0 && !UpdateFileAllowedFileTypes.Contains(fileType))
                {
                    throw new UserFriendlyException(L("FileNotInAllowedFileTypes", UpdateFileAllowedFileTypes));
                }

                byte[] fileBytes;
                using (var stream = file.OpenReadStream())
                {
                    fileBytes = stream.GetAllBytes();
                }

                var fileToken = Guid.NewGuid().ToString("N");
                _tempFileCacheManager.SetFile(fileToken, new TempFileInfo(file.FileName, fileType, fileBytes));

                return new FileUploadCacheOutput(fileToken);
            }
            catch (UserFriendlyException ex)
            {
                return new FileUploadCacheOutput(new ErrorInfo(ex.Message));
            }
        }

        public string[] GetUpdateFileFileAllowedTypes()
        {
            return UpdateFileAllowedFileTypes;
        }

    }
}