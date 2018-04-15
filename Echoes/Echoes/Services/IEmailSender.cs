using System.Threading.Tasks;

namespace Echoes.Services
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string email, string subject, string message);
    }
}
