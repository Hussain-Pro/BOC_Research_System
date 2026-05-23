using System.Threading;
using System.Threading.Tasks;

namespace BOC.Application.Common.Interfaces;

public interface IEmailSender
{
    /// <summary>Simple send without cancellation support.</summary>
    Task SendEmailAsync(string toEmail, string subject, string body);

    /// <summary>Send with cancellation token for background job usage.</summary>
    Task SendEmailAsync(string toEmail, string subject, string body, CancellationToken cancellationToken);
}
