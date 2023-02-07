using System.Threading.Tasks;

namespace Chamran.Deed.Net.Sms
{
    public interface ISmsSender
    {
        Task SendAsync(string number, string message);
    }
}