using System;
using System.IO;
using System.Linq.Dynamic.Core;
using System.Net;
using System.Threading.Tasks;
using Abp.Auditing;
using Abp.Extensions;
using Abp.MimeTypes;
using Microsoft.AspNetCore.Mvc;
using Chamran.Deed.Dto;
using Chamran.Deed.Storage;
using Microsoft.AspNetCore.OutputCaching;
using Twilio.TwiML.Voice;
using Abp.AspNetZeroCore.Net;
using Abp.Domain.Repositories;
using Abp.Web.Models;
using Chamran.Deed.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Chamran.Deed.Web.Controllers
{
    public class FileController : DeedControllerBase
    {
        private readonly ITempFileCacheManager _tempFileCacheManager;
        private readonly IBinaryObjectManager _binaryObjectManager;
        private readonly IMimeTypeMap _mimeTypeMap;
        private readonly IRepository<SoftwareUpdate> _softwareUpdateRepository;

        public FileController(
            ITempFileCacheManager tempFileCacheManager,
            IBinaryObjectManager binaryObjectManager,
            IMimeTypeMap mimeTypeMap,
            IRepository<SoftwareUpdate> softwareUpdateRepository
        )
        {
            _tempFileCacheManager = tempFileCacheManager;
            _binaryObjectManager = binaryObjectManager;
            _mimeTypeMap = mimeTypeMap;
            _softwareUpdateRepository = softwareUpdateRepository;
        }

        [DisableAuditing]
        public ActionResult DownloadTempFile(FileDto file)
        {
            var fileBytes = _tempFileCacheManager.GetFile(file.FileToken);
            if (fileBytes == null)
            {
                return NotFound(L("RequestedFileDoesNotExists"));
            }

            return File(fileBytes, file.FileType, file.FileName);
        }

        [DisableAuditing]
        public async Task<ActionResult> DownloadBinaryFile(Guid id, string contentType, string fileName)
        {
            var fileObject = await _binaryObjectManager.GetOrNullAsync(id);
            if (fileObject == null)
            {
                return StatusCode((int)HttpStatusCode.NotFound);
            }

            if (fileName.IsNullOrEmpty())
            {
                if (!fileObject.Description.IsNullOrEmpty() &&
                    !Path.GetExtension(fileObject.Description).IsNullOrEmpty())
                {
                    fileName = fileObject.Description;
                }
                else
                {
                    return StatusCode((int)HttpStatusCode.BadRequest);
                }
            }

            if (contentType.IsNullOrEmpty())
            {
                if (!Path.GetExtension(fileName).IsNullOrEmpty())
                {
                    contentType = _mimeTypeMap.GetMimeType(fileName);
                }
                else
                {
                    return StatusCode((int)HttpStatusCode.BadRequest);
                }
            }

            return File(fileObject.Bytes, contentType, fileName);
        }

        [DisableAuditing]
        [AllowAnonymous]
        public async Task<IActionResult> LatestBuild()
        {
            var latestUpdate = _softwareUpdateRepository.GetAll()
                .OrderByDescending(m => m.CreationTime)
                .FirstOrDefault();

            if (latestUpdate == null || latestUpdate.UpdateFile == null)
            {
                return NotFound("File not found");
            }

            var fileObject = await _binaryObjectManager.GetOrNullAsync(latestUpdate.UpdateFile.Value);

            if (fileObject == null)
            {
                return NotFound();
            }

            var contentType = "application/octet-stream";
            var fileName = "Deed" + latestUpdate.BuildNo + ".apk";

            // Create a stream from the byte array
            var stream = new MemoryStream(fileObject.Bytes);

            // Return the stream as a FileStreamResult
            return File(stream, contentType, fileName);
        }


        [DisableAuditing]
        [OutputCache(Duration = 0, NoStore = true)]
        [ResponseCache(Duration = 0, NoStore = true)]
        public async Task<FileResult> GetContent(Guid id, string contentType, string fileName)
        {
            var fileObject = await _binaryObjectManager.GetOrNullAsync(id);
            if (fileObject == null)
            {
                return null; //StatusCode((int)HttpStatusCode.NotFound);
            }

            if (fileName.IsNullOrEmpty())
            {
                if (!fileObject.Description.IsNullOrEmpty() &&
                    !Path.GetExtension(fileObject.Description).IsNullOrEmpty())
                {
                    fileName = fileObject.Description;
                }
                else
                {
                    return null; //StatusCode((int)HttpStatusCode.BadRequest);
                }
            }

            if (contentType.IsNullOrEmpty())
            {
                if (!Path.GetExtension(fileName).IsNullOrEmpty())
                {
                    contentType = _mimeTypeMap.GetMimeType(fileName);
                }
                else
                {
                    return null; //StatusCode((int)HttpStatusCode.BadRequest);
                }
            }

            //var data = Convert.ToBase64String(fileObject.Bytes);
            //return File(Convert.FromBase64String(data), MimeTypeNames.ImageJpeg);
            return File(fileObject.Bytes, MimeTypeNames.ImageJpeg);
            //http://192.168.1.89:8089/File/GetContent?id=6FBAE131-2182-87FC-B93A-3A09C55AD962


        }

        [DisableAuditing]
        [OutputCache(Duration = 0, NoStore = true)]
        [ResponseCache(Duration = 0, NoStore = true)]
        [DontWrapResult]
        public async Task<string> GetBase64ImageSource(Guid id, string contentType, string fileName)
        {
            var fileObject = await _binaryObjectManager.GetOrNullAsync(id);
            if (fileObject == null)
            {
                return null; //StatusCode((int)HttpStatusCode.NotFound);
            }

            if (fileName.IsNullOrEmpty())
            {
                if (!fileObject.Description.IsNullOrEmpty() &&
                    !Path.GetExtension(fileObject.Description).IsNullOrEmpty())
                {
                    fileName = fileObject.Description;
                }
                else
                {
                    return null; //StatusCode((int)HttpStatusCode.BadRequest);
                }
            }

            if (contentType.IsNullOrEmpty())
            {
                if (!Path.GetExtension(fileName).IsNullOrEmpty())
                {
                    contentType = _mimeTypeMap.GetMimeType(fileName);
                }
                else
                {
                    return null; //StatusCode((int)HttpStatusCode.BadRequest);
                }
            }

            var data = Convert.ToBase64String(fileObject.Bytes);
            //return File(Convert.FromBase64String(data), MimeTypeNames.ImageJpeg);
            return "data:image/png;base64," + data;
            //http://192.168.1.89:8089/File/GetBase64ImageSource?id=6FBAE131-2182-87FC-B93A-3A09C55AD962


        }

    }
}
