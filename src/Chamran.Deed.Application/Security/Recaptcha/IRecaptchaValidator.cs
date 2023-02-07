using System.Threading.Tasks;

namespace Chamran.Deed.Security.Recaptcha
{
    public interface IRecaptchaValidator
    {
        Task ValidateAsync(string captchaResponse);
    }
}