using Relativity.API;
using System;

namespace AdminMigrationUtility.Emailer.Helpers
{
	public interface ISQLHelper
	{
		String RetrieveConfigurationValue(IDBContext eddsDbContext, String sectionName, String name);
	}
}