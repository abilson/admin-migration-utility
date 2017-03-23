using AdminMigrationUtility.Helpers.SQL;
using Relativity.API;
using System;
using System.Threading.Tasks;

namespace AdminMigrationUtility.Helpers.Models
{
	public class SmtpClientSettings
	{
		public String EmailFrom { get; set; }
		public String SmtpServer { get; set; }
		public String SmtpUsername { get; set; }
		public String SmtpPassword { get; set; }
        public String EncryptedSmtpPassword { get; set; }
        public Int32? SmtpPort { get; set; }
		public IDBContext EddsDbContext { get; set; }
		private ISqlQueryHelper SqlQueryHelper { get; set; }

		public SmtpClientSettings(IDBContext eddsDbContext, ISqlQueryHelper sqlQueryHelper)
		{
			EddsDbContext = eddsDbContext;
			SqlQueryHelper = sqlQueryHelper;
		}

		public void GetSettings()
		{
			EmailFrom = GetEmailFrom(EddsDbContext).Result;
			SmtpServer = GetSmtpServer(EddsDbContext).Result;
			SmtpUsername = GetSmtpUserName(EddsDbContext).Result;
			SmtpPassword = GetSmtpPassword(EddsDbContext).Result;
            EncryptedSmtpPassword = GetEncryptedSmtpPassword(EddsDbContext).Result;
			SmtpPort = GetSmtpPort(EddsDbContext).Result;
		}

		private async Task<String> GetEmailFrom(IDBContext eddsDbContext)
		{
			return await SqlQueryHelper.RetrieveConfigurationValue(eddsDbContext, Constant.Configuration.Section.Notification, Constant.Configuration.Name.EmailFrom);
		}

		private async Task<String> GetSmtpServer(IDBContext eddsDbContext)
		{
			return await SqlQueryHelper.RetrieveConfigurationValue(eddsDbContext, Constant.Configuration.Section.Notification, Constant.Configuration.Name.SmtpServer);
		}

		private async Task<String> GetSmtpUserName(IDBContext eddsDbContext)
		{
			return await SqlQueryHelper.RetrieveConfigurationValue(eddsDbContext, Constant.Configuration.Section.Notification, Constant.Configuration.Name.SmtpUserName);
		}

		private async Task<String> GetSmtpPassword(IDBContext eddsDbContext)
		{
			return await SqlQueryHelper.RetrieveConfigurationValue(eddsDbContext, Constant.Configuration.Section.Notification, Constant.Configuration.Name.SmtpPassword);
		}

        private async Task<String> GetEncryptedSmtpPassword(IDBContext eddsDbContext)
        {
            return await SqlQueryHelper.RetrieveConfigurationValue(eddsDbContext, Constant.Configuration.Section.Notification, Constant.Configuration.Name.EncryptedSmtpPassword);
        }

        private async Task<Int32?> GetSmtpPort(IDBContext eddsDbContext)
		{
			var result = await SqlQueryHelper.RetrieveConfigurationValue(eddsDbContext, Constant.Configuration.Section.Notification, Constant.Configuration.Name.SmtpPort);
			if (String.IsNullOrEmpty(result))
			{
				return null;
			}
			return Convert.ToInt32(result);
		}
	}
}