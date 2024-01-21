using System;
using Abp.Application.Services.Dto;
using JetBrains.Annotations;

namespace Chamran.Deed.Chat.Dto
{
    public class ChatMessageDto : EntityDto
    {
        public long UserId { get; set; }

        public int? TenantId { get; set; }

        public long TargetUserId { get; set; }

        public int? TargetTenantId { get; set; }

        public ChatSide Side { get; set; }

        public ChatMessageReadState ReadState { get; set; }

        public ChatMessageReadState ReceiverReadState { get; set; }

        public string Message { get; set; }
        
        public DateTime CreationTime { get; set; }

        public Guid? SharedMessageId { get; set; }

        [CanBeNull] public string  ForwardedFromName { get; set; }
        public long ReplyMessageId { get; set; }
        public string ReplyMessage { get; set; }

        public Guid? FriendProfilePictureId { get; set; }
        public string FriendName { get; set; }
        public string FriendSurName { get; set; }

    }
}