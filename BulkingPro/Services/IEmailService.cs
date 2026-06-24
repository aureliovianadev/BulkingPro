using System.Threading.Tasks;

namespace BulkingPro.Services;

public interface IEmailService
{
    Task<bool> SendPasswordResetEmailAsync(string email, string name, string code);
}