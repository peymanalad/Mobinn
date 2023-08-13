using System;

namespace Chamran.Deed.Web.Chat.SignalR;

public class DeleteChatMessageInput
{
    public Guid SharedMessageId { get; set; }
}