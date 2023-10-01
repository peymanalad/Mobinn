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
    public class OrganizationsController : DeedControllerBase
    {
        private readonly ITempFileCacheManager _tempFileCacheManager;

        private const long MaxOrganizationLogoLength = 5242880; //5MB
        private const string MaxOrganizationLogoLengthUserFriendlyValue = "5MB"; //5MB
        private readonly string[] OrganizationLogoAllowedFileTypes = { "jpeg", "jpg", "png" };

        public OrganizationsController(ITempFileCacheManager tempFileCacheManager)
        {
            _tempFileCacheManager = tempFileCacheManager;
        }

        public FileUploadCacheOutput UploadOrganizationLogoFile()
        {
            try
            {
                //Check input
                if (Request.Form.Files.Count == 0)
                {
                    throw new UserFriendlyException(L("NoFileFoundError"));
                }

                var file = Request.Form.Files.First();
                if (file.Length > MaxOrganizationLogoLength)
                {
                    throw new UserFriendlyException(L("Warn_File_SizeLimit", MaxOrganizationLogoLengthUserFriendlyValue));
                }

                var fileType = Path.GetExtension(file.FileName).Substring(1);
                if (OrganizationLogoAllowedFileTypes != null && OrganizationLogoAllowedFileTypes.Length > 0 && !OrganizationLogoAllowedFileTypes.Contains(fileType))
                {
                    throw new UserFriendlyException(L("FileNotInAllowedFileTypes", OrganizationLogoAllowedFileTypes));
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

        public string[] GetOrganizationLogoFileAllowedTypes()
        {
            return OrganizationLogoAllowedFileTypes;
        }

    }
}