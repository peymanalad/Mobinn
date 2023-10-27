using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Abp;
using Abp.AspNetCore.SignalR.Hubs;
using Abp.Localization;
using Abp.RealTime;
using Abp.Runtime.Session;
using Abp.UI;
using Castle.Core.Logging;
using Castle.Windsor;
using Chamran.Deed.Chat;
using Chamran.Deed.Web.Xss;

namespace Chamran.Deed.Web.Chat.SignalR
{
    public class ChatHub : OnlineClientHubBase
    {
        private readonly IChatMessageManager _chatMessageManager;
        private readonly ILocalizationManager _localizationManager;
        private readonly IWindsorContainer _windsorContainer;
        private readonly IHtmlSanitizer _htmlSanitizer;
        private bool _isCallByRelease;
        private IAbpSession ChatAbpSession { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChatHub"/> class.
        /// </summary>
        public ChatHub(
            IChatMessageManager chatMessageManager,
            ILocalizationManager localizationManager,
            IWindsorContainer windsorContainer,
            IOnlineClientManager<ChatChannel> onlineClientManager,
            IOnlineClientInfoProvider clientInfoProvider,
            IHtmlSanitizer htmlSanitizer) : base(onlineClientManager, clientInfoProvider)
        {
            _chatMessageManager = chatMessageManager;
            _localizationManager = localizationManager;
            _windsorContainer = windsorContainer;
            _htmlSanitizer = htmlSanitizer;

            Logger = NullLogger.Instance;
            ChatAbpSession = NullAbpSession.Instance;
        }

        public async Task<string> SendMessage(SendChatMessageInput input)
        {
            input.Message = _htmlSanitizer.Sanitize(input.Message);
            var sender = Context.ToUserIdentifier();
            var receiver = new UserIdentifier(input.TenantId, input.UserId);

            try
            {
                using (ChatAbpSession.Use(Context.GetTenantId(), Context.GetUserId()))
                {
                    await _chatMessageManager.SendMessageAsync(sender, receiver, input.Message, input.TenancyName, input.UserName, input.ProfilePictureId);
                    return string.Empty;
                }
            }
            catch (UserFriendlyException ex)
            {
                Logger.Warn("Could not send chat message to user: " + receiver);
                Logger.Warn(ex.ToString(), ex);
                return ex.Message;
            }
            catch (Exception ex)
            {
                Logger.Warn("Could not send chat message to user: " + receiver);
                Logger.Warn(ex.ToString(), ex);
                return _localizationManager.GetSource("AbpWeb").GetString("InternalServerError");
            }
        }

        public async Task<string> ForwardMessage(ForwardChatMessageInput input)
        {
            input.Message = _htmlSanitizer.Sanitize(input.Message);
            var sender = Context.ToUserIdentifier();
            var receiver = new UserIdentifier(input.TenantId, input.UserId);

            try
            {
                using (ChatAbpSession.Use(Context.GetTenantId(), Context.GetUserId()))
                {
                    await _chatMessageManager.ForwardMessageAsync(sender, receiver, input.Message, input.TenancyName, input.UserName,input.ForwardedFromName);
                    return string.Empty;
                }
            }
            catch (UserFriendlyException ex)
            {
                Logger.Warn("Could not send chat message to user: " + receiver);
                Logger.Warn(ex.ToString(), ex);
                return ex.Message;
            }
            catch (Exception ex)
            {
                Logger.Warn("Could not send chat message to user: " + receiver);
                Logger.Warn(ex.ToString(), ex);
                return _localizationManager.GetSource("AbpWeb").GetString("InternalServerError");
            }
        }

        public async Task<string> ReplyMessage(ReplyMessageInput input)
        {
            input.Message = _htmlSanitizer.Sanitize(input.Message);
            var sender = Context.ToUserIdentifier();
            var receiver = new UserIdentifier(input.TenantId, input.UserId);

            try
            {
                using (ChatAbpSession.Use(Context.GetTenantId(), Context.GetUserId()))
                {
                    await _chatMessageManager.ReplyMessageAsync(sender, receiver, input.Message, input.TenancyName, input.UserName, input.ReplyMessageId);
                    return string.Empty;
                }
            }
            catch (UserFriendlyException ex)
            {
                Logger.Warn("Could not send chat message to user: " + receiver);
                Logger.Warn(ex.ToString(), ex);
                return ex.Message;
            }
            catch (Exception ex)
            {
                Logger.Warn("Could not send chat message to user: " + receiver);
                Logger.Warn(ex.ToString(), ex);
                return _localizationManager.GetSource("AbpWeb").GetString("InternalServerError");
            }
        }


        public async Task<string> EditMessage(EditChatMessageInput input)
        {
            input.Message = _htmlSanitizer.Sanitize(input.Message);
            var sender = Context.ToUserIdentifier();
            var receiver = new UserIdentifier(input.TenantId, input.UserId);

            try
            {
                using (ChatAbpSession.Use(Context.GetTenantId(), Context.GetUserId()))
                {
                    await _chatMessageManager.EditMessageAsync(sender, receiver, input.SharedMessageId, input.Message, input.TenancyName, input.UserName, input.ProfilePictureId);
                    return string.Empty;
                }
            }
            catch (UserFriendlyException ex)
            {
                Logger.Warn("Could not edit chat message to user: " + receiver);
                Logger.Warn(ex.ToString(), ex);
                return ex.Message;
            }
            catch (Exception ex)
            {
                Logger.Warn("Could not edit chat message to user: " + receiver);
                Logger.Warn(ex.ToString(), ex);
                return _localizationManager.GetSource("AbpWeb").GetString("InternalServerError");
            }
        }

        public async Task<string> DeleteMessage(DeleteChatMessageInput input)
        {
            var sender = Context.ToUserIdentifier();
            try
            {
                using (ChatAbpSession.Use(Context.GetTenantId(), Context.GetUserId()))
                {
                    await _chatMessageManager.DeleteMessageAsync(sender, input.SharedMessageId);
                    return string.Empty;
                }
            }
            catch (UserFriendlyException ex)
            {
                Logger.Warn("Could not delete chat message with id: " + input.SharedMessageId);
                Logger.Warn(ex.ToString(), ex);
                return ex.Message;
            }
            catch (Exception ex)
            {
                Logger.Warn("Could not delete chat message with id: " + input.SharedMessageId);
                Logger.Warn(ex.ToString(), ex);
                return _localizationManager.GetSource("AbpWeb").GetString("InternalServerError");
            }
        }


        public void Register()
        {
            Logger.Debug("A client is registered: " + Context.ConnectionId);
        }

        protected override void Dispose(bool disposing)
        {
            if (_isCallByRelease)
            {
                return;
            }
            base.Dispose(disposing);
            if (disposing)
            {
                _isCallByRelease = true;
                _windsorContainer.Release(this);
            }
        }
    }
}