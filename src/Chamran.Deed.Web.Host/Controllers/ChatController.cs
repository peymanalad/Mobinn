using System;
using System.Net;
using System.Threading.Tasks;
using Abp.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc;
using Chamran.Deed.Chat;
using Chamran.Deed.Storage;

namespace Chamran.Deed.Web.Controllers
{
    [AbpMvcAuthorize]
    public class ChatController : ChatControllerBase
    {
        public ChatController(IBinaryObjectManager binaryObjectManager, IChatMessageManager chatMessageManager) : 
            base(binaryObjectManager, chatMessageManager)
        {
        }

        public async Task<ActionResult> GetUploadedObject(Guid fileId, string fileName, string contentType)
        {
            using (CurrentUnitOfWork.SetTenantId(null))
            {
                var fileObject = await BinaryObjectManager.GetOrNullAsync(fileId);
                if (fileObject == null)
                {
                    return StatusCode((int)HttpStatusCode.NotFound);
                }

                return File(fileObject.Bytes, contentType, fileName);
            }
        }

        public async Task<ActionResult> GetUploadedObjectSize(Guid fileId)
        {
            using (CurrentUnitOfWork.SetTenantId(null))
            {
                var fileObject = await BinaryObjectManager.GetOrNullAsync(fileId);
                if (fileObject == null)
                {
                    return StatusCode((int)HttpStatusCode.NotFound);
                }
                const int scale = 1024;
                string[] sizeUnits = { "Bytes", "KB", "MB" };
                long size = fileObject.Bytes.Length;
                int unitIndex = 0;
                while (size >= scale && unitIndex < sizeUnits.Length - 1)
                {
                    size /= scale;
                    unitIndex++;
                }
                var data = new { SizeInByte= size, SizeInText= $"{size} {sizeUnits[unitIndex]}" };

                return Json(data);
            }
        }
    }
}