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
    public class PostGroupsController : DeedControllerBase
    {
        private readonly ITempFileCacheManager _tempFileCacheManager;

        private const long MaxGroupFileLength = 5242880; //5MB
        private const string MaxGroupFileLengthUserFriendlyValue = "5MB"; //5MB
        private readonly string[] GroupFileAllowedFileTypes = { "jpeg", "jpg", "png" };

        public PostGroupsController(ITempFileCacheManager tempFileCacheManager)
        {
            _tempFileCacheManager = tempFileCacheManager;
        }

        public FileUploadCacheOutput UploadGroupFileFile()
        {
            try
            {
                //Check input
                if (Request.Form.Files.Count == 0)
                {
                    throw new UserFriendlyException(L("NoFileFoundError"));
                }

                var file = Request.Form.Files.First();
                if (file.Length > MaxGroupFileLength)
                {
                    throw new UserFriendlyException(L("Warn_File_SizeLimit", MaxGroupFileLengthUserFriendlyValue));
                }

                var fileType = Path.GetExtension(file.FileName).Substring(1);
                if (GroupFileAllowedFileTypes != null && GroupFileAllowedFileTypes.Length > 0 && !GroupFileAllowedFileTypes.Contains(fileType))
                {
                    throw new UserFriendlyException(L("FileNotInAllowedFileTypes", GroupFileAllowedFileTypes));
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

        public string[] GetGroupFileFileAllowedTypes()
        {
            return GroupFileAllowedFileTypes;
        }

    }
}