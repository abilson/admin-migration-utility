using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;

namespace AdminMigrationUtility.Emailer
{
	public class Email
	{
		public String From { get; set; }
		public String To { get; set; }
		public String Subject { get; set; }
		public String Body { get; set; }
		public MailMessage MailMessage { get; set; }
		public List<Attachment> Attachments { get; set; }

		public Email(string from, List<String> toList, String subject, String body)
		{
			From = from;
			Subject = subject;
			Body = body;
			To = GetEmailToList(toList);
		}

		public Email(string from, String semiColonDelimitedToList, String subject, String body)
		{
			From = from;
			Subject = subject;
			Body = body;
			To = semiColonDelimitedToList;
		}

		public Email(string from, List<String> toList, String subject, String body, List<Attachment> filesToAttach)
		{
			From = from;
			Subject = subject;
			Body = body;
			To = GetEmailToList(toList);
			Attachments = filesToAttach;
		}

		public Email(string from, String semiColonDelimitedToList, String subject, String body, List<Attachment> filesToAttach)
		{
			From = from;
			Subject = subject;
			Body = body;
			To = semiColonDelimitedToList;
			Attachments = filesToAttach;
		}

		public Response Send(IEmailService emailService)
		{
			var sendResponse = new Response(true, String.Empty);

			try
			{
				MailMessage = new MailMessage
				{
					From = new MailAddress(From),
					Subject = Subject,
					Body = Body,
					IsBodyHtml = true
				};

				if (Attachments != null && Attachments.Any())
				{
					foreach (Attachment file in Attachments)
					{
						MailMessage.Attachments.Add(file);
					}
				}

				MailMessage.To.Add(To);
				sendResponse = emailService.Send(MailMessage);
			}
			catch (Exception ex)
			{
				sendResponse.Success = false;
				sendResponse.Message = ex.ToString();
			}

			return sendResponse;
		}

		private static String GetEmailToList(List<String> toList)
		{
			return String.Join(",", toList);
		}
	}
}