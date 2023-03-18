using System.Threading.Tasks;

namespace Chamran.Deed.Net.Sms
{
    public interface ISmsSender
    {
        Task SendAsync(string number, string message);
        Task<bool> SendAsyncResult(string number, string message);
    }
}