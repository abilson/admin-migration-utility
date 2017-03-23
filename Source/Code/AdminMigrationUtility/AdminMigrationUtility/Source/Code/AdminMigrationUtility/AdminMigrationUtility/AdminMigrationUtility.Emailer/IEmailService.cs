using System.Net.Mail;

namespace AdminMigrationUtility.Emailer
{
	public interface IEmailService
	{
		Response Send(MailMessage mailMessage);
	}
}