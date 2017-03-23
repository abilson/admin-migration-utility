using Relativity.API;
using System;
using System.Data;
using System.Data.SqlClient;

namespace AdminMigrationUtility.Emailer.Helpers
{
	public class SQLHelper : ISQLHelper
	{
		public String RetrieveConfigurationValue(IDBContext eddsDbContext, String sectionName, String name)
		{
			const String sql = @"SELECT Value
					FROM EDDSDBO.Configuration
					WHERE Section = @sectionName
						AND Name = @name";

			var sqlParams = new SqlParameter[2];
			sqlParams[0] = new SqlParameter("@sectionName", SqlDbType.VarChar) { Value = sectionName };
			sqlParams[1] = new SqlParameter("@name", SqlDbType.VarChar) { Value = name };

			var result = eddsDbContext.ExecuteSqlStatementAsScalar(sql, sqlParams);

			if ((result != null) && (result != DBNull.Value))
			{
				return result.ToString();
			}
			return String.Empty;
		}
	}
}