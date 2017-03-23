using System;
using System.Net;
using System.Net.Mail;

namespace AdminMigrationUtility.Emailer
{
	public class EmailService : IEmailService
	{
		public SmtpClient Client { get; set; }

		public EmailService(String smtpServer, String smtpUsername, String smtpPassword, Int32? smtpPort)
		{
			Client = new SmtpClient(smtpServer);
			if (!String.IsNullOrEmpty(smtpUsername) && !String.IsNullOrEmpty(smtpPassword))
			{
				Client.Credentials = new NetworkCredential(smtpUsername, smtpPassword);
			}
			if (smtpPort != null)
			{
				Client.Port = smtpPort.Value;
			}
		}

		public Response Send(MailMessage mailMessage)
		{
			var response = new Response(true, String.Empty);

			try
			{
				Client.Send(mailMessage);
			}
			catch (Exception ex)
			{
				response.Success = false;
				response.Message = ex.ToString();
			}

			return response;
		}
	}
}