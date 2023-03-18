using System;
using System.Threading.Tasks;
using Abp.Dependency;
using Abp.Logging;
using Castle.Core.Logging;
using Chamran.Deed.Identity;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace Chamran.Deed.Net.Sms
{
    public class TwilioSmsSender : ISmsSender, ITransientDependency
    {
        private TwilioSmsSenderConfiguration _twilioSmsSenderConfiguration;

        public TwilioSmsSender(TwilioSmsSenderConfiguration twilioSmsSenderConfiguration)
        {
            _twilioSmsSenderConfiguration = twilioSmsSenderConfiguration;
        }
        public ILogger Logger { get; set; }
        public async Task SendAsync(string number, string message)
        {
            TwilioClient.Init(_twilioSmsSenderConfiguration.AccountSid, _twilioSmsSenderConfiguration.AuthToken);

            MessageResource resource = await MessageResource.CreateAsync(
                body: message,
                @from: new Twilio.Types.PhoneNumber(_twilioSmsSenderConfiguration.SenderNumber),
                to: new Twilio.Types.PhoneNumber(number)
            );
        }

        public async Task<bool> SendAsyncResult(string number, string message)
        {
            try
            {

                TwilioClient.Init(_twilioSmsSenderConfiguration.AccountSid, _twilioSmsSenderConfiguration.AuthToken);

                MessageResource resource = await MessageResource.CreateAsync(
                    body: message,
                    @from: new Twilio.Types.PhoneNumber(_twilioSmsSenderConfiguration.SenderNumber),
                    to: new Twilio.Types.PhoneNumber(number)
                );
                return true;
            }
            catch (Exception ex)
            {
                Logger.Log(LogSeverity.Error,
                    $"An error occured while Sending Message. Number: {number}", ex);

            }

            return false;
        }
    }
}
