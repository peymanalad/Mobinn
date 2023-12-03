using System;

namespace Chamran.Deed.Info.Dtos
{

    public class SendPostNotificationDto : PostDto
    {
        public Guid? GroupFile { get; set; }
        public string GroupDescription { get; set; }
    }
}