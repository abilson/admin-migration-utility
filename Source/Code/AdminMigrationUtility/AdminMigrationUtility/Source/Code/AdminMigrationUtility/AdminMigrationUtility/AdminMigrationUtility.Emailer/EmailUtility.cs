using AdminMigrationUtility.Helpers.Models;
using Relativity.API;
using System;
using System.Threading.Tasks;

namespace AdminMigrationUtility.Emailer
{
	public class EmailUtility
	{
		public static async Task SendEmail(IDBContext eddsdbContext, String emailTo, String emailSubject, String emailBody, SmtpClientSettings smtpSettings)
		{
			smtpSettings.GetSettings();

            if(String.IsNullOrWhiteSpace(smtpSettings.EncryptedSmtpPassword)) //Only unencrypted SMTP Passwords are supported
            {
                EmailService emailService = new EmailService(smtpSettings.SmtpServer, smtpSettings.SmtpUsername, smtpSettings.SmtpPassword, smtpSettings.SmtpPort);
                Email email = new Email(smtpSettings.EmailFrom, emailTo, emailSubject, emailBody);
                await Task.Run(() => email.Send(emailService));
            }
		}
	}
}