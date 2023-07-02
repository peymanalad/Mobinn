using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Abp.Dependency;
using Abp.Logging;
using Castle.Core.Logging;
using Chamran.Deed.Identity;
using SmsBehestan;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace Chamran.Deed.Net.Sms
{
    public class DeedSmsSender : ISmsSender, ITransientDependency
    {
        private readonly DeedSmsSenderConfiguration _deedSmsSenderConfiguration;

        public DeedSmsSender(DeedSmsSenderConfiguration DeedSmsSenderConfiguration)
        {
            _deedSmsSenderConfiguration = DeedSmsSenderConfiguration;
            this.Logger = (ILogger)NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public async Task SendAsync(string number, string message)
        {
            try
            {
                var msg = new List<MessageDTO> { new() { Recipient = number, Text = message, Messages = new List<MessageDTO>() } };

                var res = await new SmsBehestan.Client(_deedSmsSenderConfiguration).EnqueueAsync(new EnqueueRequestDTO()
                {
                    Messages = msg,
                });

            }
            catch (Exception ex)
            {
                Logger.Log(LogSeverity.Error,
                    $"An error occured while Sending Message. Number: {number}", ex);

            }

        }

        public async Task<bool> SendAsyncResult(string number, string message)
        {
            try
            {
                var msg = new List<MessageDTO> { new() { Recipient = number, Text = message, Messages = new List<MessageDTO>() } };

                var res = await new SmsBehestan.Client(_deedSmsSenderConfiguration).EnqueueAsync(new EnqueueRequestDTO()
                {
                    Messages = msg,
                });
                Logger.Log(LogSeverity.Info,
                    $"Sms Result for Number: {number} :" + res);

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