using System.Threading.Tasks;
using Chamran.Deed.Security.Recaptcha;

namespace Chamran.Deed.Test.Base.Web
{
    public class FakeRecaptchaValidator : IRecaptchaValidator
    {
        public Task ValidateAsync(string captchaResponse)
        {
            return Task.CompletedTask;
        }
    }
}
