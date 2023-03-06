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
    public class PostsController : DeedControllerBase
    {
        private readonly ITempFileCacheManager _tempFileCacheManager;

        private const long MaxPostFileLength = 5242880; //5MB
        private const string MaxPostFileLengthUserFriendlyValue = "5MB"; //5MB
        private readonly string[] PostFileAllowedFileTypes = { "jpeg", "jpg", "png","mp4" };

        public PostsController(ITempFileCacheManager tempFileCacheManager)
        {
            _tempFileCacheManager = tempFileCacheManager;
        }

        public FileUploadCacheOutput UploadPostFileFile()
        {
            try
            {
                //Check input
                if (Request.Form.Files.Count == 0)
                {
                    throw new UserFriendlyException(L("NoFileFoundError"));
                }

                var file = Request.Form.Files.First();
                if (file.Length > MaxPostFileLength)
                {
                    throw new UserFriendlyException(L("Warn_File_SizeLimit", MaxPostFileLengthUserFriendlyValue));
                }

                var fileType = Path.GetExtension(file.FileName).Substring(1);
                if (PostFileAllowedFileTypes != null && PostFileAllowedFileTypes.Length > 0 && !PostFileAllowedFileTypes.Contains(fileType))
                {
                    throw new UserFriendlyException(L("FileNotInAllowedFileTypes", PostFileAllowedFileTypes));
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

        public string[] GetPostFileFileAllowedTypes()
        {
            return PostFileAllowedFileTypes;
        }

    }
}